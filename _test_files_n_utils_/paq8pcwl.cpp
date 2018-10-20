/* paq8pcwl/cwla file compressor/archiver.  Release by Andreas Morphis, Aug. 22, 2008

    Copyright (C) 2018 Christian Haeussler, Matt Mahoney, Serge Osnach, Alexander Ratushnyak,
    Bill Pettis, Przemyslaw Skibinski, Matthew Fite, wowtiger, Andrew Paterson,
    Jan Ondrus, Andreas Morphis, Pavel L. Holoborodko, KZ.

    LICENSE

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License as
    published by the Free Software Foundation; either version 2 of
    the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    General Public License for more details at
    Visit <http://www.gnu.org/copyleft/gpl.html>.

TO COMPILE

Recommended compiler commands and optimizations:

  MINGW g++:
    nasm paq7asm.asm -f win32 --prefix _
    g++ paq8p.cpp -std=c++14 -O3 -s -march=i686 -fomit-frame-pointer -o paq8p.exe paq7asm.obj

  Borland:
    nasm paq7asm.asm -f obj --prefix _
    bcc32 -O -w-8027 paq8p.cpp paq7asm.obj

  Mars:
    nasm paq7asm.asm -f obj --prefix _
    dmc -Ae -O paq8p.cpp paq7asm.obj

MinGW produces faster executables than Borland or Mars, but Intel C++
is even faster than MinGW).
*/

#define PROGNAME "cwla"  // Please change this if you change the program.

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <math.h>
#include <ctype.h>
#define NDEBUG  // remove for debugging (turns on Array bound checks)
#include <assert.h>

#include <windows.h>

#ifndef DEFAULT_OPTION
#define DEFAULT_OPTION 7
#endif

// 8, 16, 32 bit unsigned types (adjust as appropriate)
typedef unsigned char  U8;
typedef unsigned short U16;
typedef unsigned int   U32;

// Error handler: print message if any, and exit
void quit(const char* message = 0) {
  throw message;
}

// string.equalsignorecase
int equals(const char* a, const char* b) {
  assert(a && b);
  while (*a && *b) {
    int c1=*a;
    if (c1 >= 'A' && c1 <= 'Z') c1 += 'a' - 'A';
    int c2=*b;
    if (c2>='A'&&c2<='Z') c2+='a'-'A';
    if (c1!=c2) return 0;
    ++a;
    ++b;
  }
  return *a==*b;
}

//////////////////////// Program Checker /////////////////////

// Track time and memory used
class ProgramChecker {
  int memused;  // bytes allocated by Array<T> now
  int maxmem;   // most bytes allocated ever
  clock_t start_time;  // in ticks
public:
  void alloc(int n) {  // report memory allocated, may be negative
    memused+=n;
    if (memused>maxmem) maxmem=memused;
  }
  ProgramChecker(): memused(0), maxmem(0) {
    start_time=clock();
    assert(sizeof(U8)==1);
    assert(sizeof(U16)==2);
    assert(sizeof(U32)==4);
    assert(sizeof(short)==2);
    assert(sizeof(int)==4);
  }
  void print() {}
} programChecker;

//////////////////////////// Array ////////////////////////////

template <class T, int ALIGN=0> class Array {
private:
  int n;     // user size
  int reserved;  // actual size
  char *ptr; // allocated memory, zeroed
  T* data;   // start of n elements of aligned data
  void create(int i);  // create with size i
public:
  explicit Array(int i=0) {create(i);}
  ~Array();
  T& operator[](int i) {
#ifndef NDEBUG
    if (i<0 || i>=n) fprintf(stderr, "%d out of bounds %d\n", i, n), quit();
#endif
    return data[i];
  }
  const T& operator[](int i) const {
#ifndef NDEBUG
    if (i<0 || i>=n) fprintf(stderr, "%d out of bounds %d\n", i, n), quit();
#endif
    return data[i];
  }
  int size() const {return n;}
  void resize(int i);  // change size to i
  void pop_back() {if (n>0) --n;}  // decrement size
  void push_back(const T& x);  // increment size, append x
private:
  Array(const Array&);  // no copy or assignment
  Array& operator=(const Array&);
};

template<class T, int ALIGN> void Array<T, ALIGN>::resize(int i) {
  if (i<=reserved) {
    n=i;
    return;
  }
  char *saveptr=ptr;
  T *savedata=data;
  int saven=n;
  create(i);
  if (saveptr) {
    if (savedata) {
      memcpy(data, savedata, sizeof(T)*min(i, saven));
      programChecker.alloc(-ALIGN-n*sizeof(T));
    }
    free(saveptr);
  }
}

template<class T, int ALIGN> void Array<T, ALIGN>::create(int i) {
  n=reserved=i;
  if (i<=0) {
    data=0;
    ptr=0;
    return;
  }
  const int sz=ALIGN+n*sizeof(T);
  programChecker.alloc(sz);
  ptr = (char*)calloc(sz, 1);
  if (!ptr) quit("Out of memory!");
  data = (ALIGN ? (T*)(ptr + ALIGN - (((long)ptr)&(ALIGN - 1))) : (T*)ptr);
  assert((char*)data >= ptr && (char*)data <= ptr + ALIGN);
}

template<class T, int ALIGN> Array<T, ALIGN>::~Array() {
  programChecker.alloc(-ALIGN-n*sizeof(T));
  free(ptr);
}

template<class T, int ALIGN> void Array<T, ALIGN>::push_back(const T& x) {
  if (n==reserved) {
    int saven=n;
    resize(max(1, n*2));
    n=saven;
  }
  data[n++]=x;
}

/////////////////////////// String /////////////////////////////

// A tiny subset of std::string
// size() includes NUL terminator.

class String: public Array<char> {
public:
  const char* c_str() const {return &(*this)[0];}
  void operator=(const char* s) {
    resize(strlen(s)+1);
    strcpy(&(*this)[0], s);
  }
  void operator+=(const char* s) {
    assert(s);
    pop_back();
    while (*s) push_back(*s++);
    push_back(0);
  }
  String(const char* s=""): Array<char>(1) {
    (*this)+=s;
  }
};


//////////////////////////// rnd ///////////////////////////////

// 32-bit pseudo random number generator
class Random{
  Array<U32> table;
  int i;
public:
  Random(): table(64) {
	  table[0] = 123456789;
	  table[1] = 987654321;
	  for (int j = 0; j < 62; j++) table[j + 2] = table[j + 1] * 11 + table[j] * 23 / 16;
	  i = 0;
  }
  U32 operator()() {
	  return ++i, table[i & 63] = table[i - 24 & 63] ^ table[i - 55 & 63];
  }
} rnd;

////////////////////////////// Buf /////////////////////////////

int pos;  // Number of input bytes in buf (not wrapped)

class Buf {
  Array<U8> b;
public:
  Buf(int i=0): b(i) {}
  void setsize(int i) {
    if (!i) return;
    assert(i > 0 && !(i & (i - 1)));
    b.resize(i);
  }
  U8& operator[](int i) {
	  return b[i&b.size() - 1];
  }
  int operator()(int i) const {
	  assert(i > 0);
	  return b[pos - i & b.size() - 1];
  }
  int size() const {
    return b.size();
  }
};

// IntBuf(n) is a buffer of n int (must be a power of 2).
// intBuf[i] returns a reference to i'th element with wrap.
class IntBuf {
  Array<int> b;
public:
  IntBuf(int i=0): b(i) {}
  int& operator[](int i) {
	  return b[i&b.size() - 1];
  }
};

/////////////////////// Global context /////////////////////////

constexpr int MEM  = 0x10000 << DEFAULT_OPTION
int y = 0;  // Last bit, 0 or 1, set by encoder

// Global context set by Predictor and available to all models.
int c0 = 1; // Last 0-7 bits of the partial byte with a leading 1 bit (1-255)
U32 c4 = 0; // Last 4 whole bytes, packed.  Last byte is bits 0-7.
int bpos = 0; // bits in c0 (0 to 7)
Buf buf;  // Rotating input queue set by Predictor

///////////////////////////// ilog //////////////////////////////

// ilog(x) = round(log2(x) * 16), 0 <= x < 64K
class Ilog {
  Array<U8> t;
public:
  int operator()(U16 x) const {return t[x];}
  Ilog();
} ilog;

Ilog::Ilog(): t(0x10000) {
	U32 x = 14155776;
	for (int i = 2; i < 0x10000; ++i)
		t[i] = (x += 774541002 / (i * 2 - 1)) >> 24;
}

// llog(x) accepts 32 bits
inline int llog(U32 x) {
	if (x >= 0x1000000)
		return 256 + ilog(x >> 16);
	else if (x >= 0x10000)
		return 128 + ilog(x >> 8);
	else
		return ilog(x);
}

///////////////////////// state table ////////////////////////

static const U8 State_table[256][4] = {
  {  1,  2, 0, 0},{  3,  5, 1, 0},{  4,  6, 0, 1},{  7, 10, 2, 0}, // 0-3
  {  8, 12, 1, 1},{  9, 13, 1, 1},{ 11, 14, 0, 2},{ 15, 19, 3, 0}, // 4-7
  { 16, 23, 2, 1},{ 17, 24, 2, 1},{ 18, 25, 2, 1},{ 20, 27, 1, 2}, // 8-11
  { 21, 28, 1, 2},{ 22, 29, 1, 2},{ 26, 30, 0, 3},{ 31, 33, 4, 0}, // 12-15
  { 32, 35, 3, 1},{ 32, 35, 3, 1},{ 32, 35, 3, 1},{ 32, 35, 3, 1}, // 16-19
  { 34, 37, 2, 2},{ 34, 37, 2, 2},{ 34, 37, 2, 2},{ 34, 37, 2, 2}, // 20-23
  { 34, 37, 2, 2},{ 34, 37, 2, 2},{ 36, 39, 1, 3},{ 36, 39, 1, 3}, // 24-27
  { 36, 39, 1, 3},{ 36, 39, 1, 3},{ 38, 40, 0, 4},{ 41, 43, 5, 0}, // 28-31
  { 42, 45, 4, 1},{ 42, 45, 4, 1},{ 44, 47, 3, 2},{ 44, 47, 3, 2}, // 32-35
  { 46, 49, 2, 3},{ 46, 49, 2, 3},{ 48, 51, 1, 4},{ 48, 51, 1, 4}, // 36-39
  { 50, 52, 0, 5},{ 53, 43, 6, 0},{ 54, 57, 5, 1},{ 54, 57, 5, 1}, // 40-43
  { 56, 59, 4, 2},{ 56, 59, 4, 2},{ 58, 61, 3, 3},{ 58, 61, 3, 3}, // 44-47
  { 60, 63, 2, 4},{ 60, 63, 2, 4},{ 62, 65, 1, 5},{ 62, 65, 1, 5}, // 48-51
  { 50, 66, 0, 6},{ 67, 55, 7, 0},{ 68, 57, 6, 1},{ 68, 57, 6, 1}, // 52-55
  { 70, 73, 5, 2},{ 70, 73, 5, 2},{ 72, 75, 4, 3},{ 72, 75, 4, 3}, // 56-59
  { 74, 77, 3, 4},{ 74, 77, 3, 4},{ 76, 79, 2, 5},{ 76, 79, 2, 5}, // 60-63
  { 62, 81, 1, 6},{ 62, 81, 1, 6},{ 64, 82, 0, 7},{ 83, 69, 8, 0}, // 64-67
  { 84, 71, 7, 1},{ 84, 71, 7, 1},{ 86, 73, 6, 2},{ 86, 73, 6, 2}, // 68-71
  { 44, 59, 5, 3},{ 44, 59, 5, 3},{ 58, 61, 4, 4},{ 58, 61, 4, 4}, // 72-75
  { 60, 49, 3, 5},{ 60, 49, 3, 5},{ 76, 89, 2, 6},{ 76, 89, 2, 6}, // 76-79
  { 78, 91, 1, 7},{ 78, 91, 1, 7},{ 80, 92, 0, 8},{ 93, 69, 9, 0}, // 80-83
  { 94, 87, 8, 1},{ 94, 87, 8, 1},{ 96, 45, 7, 2},{ 96, 45, 7, 2}, // 84-87
  { 48, 99, 2, 7},{ 48, 99, 2, 7},{ 88,101, 1, 8},{ 88,101, 1, 8}, // 88-91
  { 80,102, 0, 9},{103, 69,10, 0},{104, 87, 9, 1},{104, 87, 9, 1}, // 92-95
  {106, 57, 8, 2},{106, 57, 8, 2},{ 62,109, 2, 8},{ 62,109, 2, 8}, // 96-99
  { 88,111, 1, 9},{ 88,111, 1, 9},{ 80,112, 0,10},{113, 85,11, 0}, // 100-103
  {114, 87,10, 1},{114, 87,10, 1},{116, 57, 9, 2},{116, 57, 9, 2}, // 104-107
  { 62,119, 2, 9},{ 62,119, 2, 9},{ 88,121, 1,10},{ 88,121, 1,10}, // 108-111
  { 90,122, 0,11},{123, 85,12, 0},{124, 97,11, 1},{124, 97,11, 1}, // 112-115
  {126, 57,10, 2},{126, 57,10, 2},{ 62,129, 2,10},{ 62,129, 2,10}, // 116-119
  { 98,131, 1,11},{ 98,131, 1,11},{ 90,132, 0,12},{133, 85,13, 0}, // 120-123
  {134, 97,12, 1},{134, 97,12, 1},{136, 57,11, 2},{136, 57,11, 2}, // 124-127
  { 62,139, 2,11},{ 62,139, 2,11},{ 98,141, 1,12},{ 98,141, 1,12}, // 128-131
  { 90,142, 0,13},{143, 95,14, 0},{144, 97,13, 1},{144, 97,13, 1}, // 132-135
  { 68, 57,12, 2},{ 68, 57,12, 2},{ 62, 81, 2,12},{ 62, 81, 2,12}, // 136-139
  { 98,147, 1,13},{ 98,147, 1,13},{100,148, 0,14},{149, 95,15, 0}, // 140-143
  {150,107,14, 1},{150,107,14, 1},{108,151, 1,14},{108,151, 1,14}, // 144-147
  {100,152, 0,15},{153, 95,16, 0},{154,107,15, 1},{108,155, 1,15}, // 148-151
  {100,156, 0,16},{157, 95,17, 0},{158,107,16, 1},{108,159, 1,16}, // 152-155
  {100,160, 0,17},{161,105,18, 0},{162,107,17, 1},{108,163, 1,17}, // 156-159
  {110,164, 0,18},{165,105,19, 0},{166,117,18, 1},{118,167, 1,18}, // 160-163
  {110,168, 0,19},{169,105,20, 0},{170,117,19, 1},{118,171, 1,19}, // 164-167
  {110,172, 0,20},{173,105,21, 0},{174,117,20, 1},{118,175, 1,20}, // 168-171
  {110,176, 0,21},{177,105,22, 0},{178,117,21, 1},{118,179, 1,21}, // 172-175
  {110,180, 0,22},{181,115,23, 0},{182,117,22, 1},{118,183, 1,22}, // 176-179
  {120,184, 0,23},{185,115,24, 0},{186,127,23, 1},{128,187, 1,23}, // 180-183
  {120,188, 0,24},{189,115,25, 0},{190,127,24, 1},{128,191, 1,24}, // 184-187
  {120,192, 0,25},{193,115,26, 0},{194,127,25, 1},{128,195, 1,25}, // 188-191
  {120,196, 0,26},{197,115,27, 0},{198,127,26, 1},{128,199, 1,26}, // 192-195
  {120,200, 0,27},{201,115,28, 0},{202,127,27, 1},{128,203, 1,27}, // 196-199
  {120,204, 0,28},{205,115,29, 0},{206,127,28, 1},{128,207, 1,28}, // 200-203
  {120,208, 0,29},{209,125,30, 0},{210,127,29, 1},{128,211, 1,29}, // 204-207
  {130,212, 0,30},{213,125,31, 0},{214,137,30, 1},{138,215, 1,30}, // 208-211
  {130,216, 0,31},{217,125,32, 0},{218,137,31, 1},{138,219, 1,31}, // 212-215
  {130,220, 0,32},{221,125,33, 0},{222,137,32, 1},{138,223, 1,32}, // 216-219
  {130,224, 0,33},{225,125,34, 0},{226,137,33, 1},{138,227, 1,33}, // 220-223
  {130,228, 0,34},{229,125,35, 0},{230,137,34, 1},{138,231, 1,34}, // 224-227
  {130,232, 0,35},{233,125,36, 0},{234,137,35, 1},{138,235, 1,35}, // 228-231
  {130,236, 0,36},{237,125,37, 0},{238,137,36, 1},{138,239, 1,36}, // 232-235
  {130,240, 0,37},{241,125,38, 0},{242,137,37, 1},{138,243, 1,37}, // 236-239
  {130,244, 0,38},{245,135,39, 0},{246,137,38, 1},{138,247, 1,38}, // 240-243
  {140,248, 0,39},{249,135,40, 0},{250, 69,39, 1},{ 80,251, 1,39}, // 244-247
  {140,252, 0,40},{249,135,41, 0},{250, 69,40, 1},{ 80,251, 1,40}, // 248-251
  {140,252, 0,41} };  // 252, 253-255 are reserved

#define nex(state, sel) State_table[state][sel]

///////////////////////////// Squash //////////////////////////////

// return p = 1/(1 + exp(-d)), d scaled by 8 bits, p scaled by 12 bits
int squash(int d) {
	static const int t[33] = {
	  1,2,3,6,10,16,27,45,73,120,194,310,488,747,1101,
	  1546,2047,2549,2994,3348,3607,3785,3901,3975,4022,
	  4050,4068,4079,4085,4089,4092,4093,4094 };
	if (d > 2047) return 4095;
	if (d < -2047) return 0;
	int w = d & 0x7f; //zero first bit
	d = (d >> 7) + 16;
	return (t[d] * (128 - w) + t[(d + 1)] * w + 64) >> 7;
}

//////////////////////////// Stretch ///////////////////////////////

// Inverse of squash. d = ln(p/(1-p)), d scaled by 8 bits, p by 12 bits.
// d has range -2047 to 2047 representing -8 to 8.  p has range 0 to 4095.

class Stretch {
  Array<short> t;
public:
  Stretch();
  int operator()(int p) const {
    assert(p>=0 && p<4096);
    return t[p];
  }
} stretch;

Stretch::Stretch(): t(4096) {
	int pi = 0;
  for (int x=-2047; x<=2047; ++x) {  // invert squash()
	  int i = squash(x);
	  for (int j = pi; j <= i; ++j)
		  t[j] = x;
	  pi = i + 1;
  }
  t[4095] = 2047;
}

//////////////////////////// Mixer /////////////////////////////

extern "C" int dot_product(short *t, short *w, int n);  // in NASM

extern "C" void train(short *t, short *w, int n, int err);  // in NASM

class Mixer {
  const int N, M, S;   // max inputs, max contexts, max context sets
  Array<short, 16> tx; // N inputs from add()
  Array<short, 16> wx; // N*M weights
  Array<int> cxt;  // S contexts
  int ncxt;        // number of contexts (0 to S)
  int base;        // offset of next context
  int nx;          // Number of inputs in tx, 0 to N
  Array<int> pr;   // last result (scaled 12 bits)
  Mixer* mp;       // points to a Mixer to combine results
public:
  Mixer(int n, int m, int s=1, int w=0);

  // Adjust weights to minimize coding cost of last prediction
  void update() {
    for (int i=0; i<ncxt; ++i) {
		int err = ((y << 12) - pr[i]) * 7;
		assert(err >= -32768 && err < 32768);
		if (err) train(&tx[0], &wx[cxt[i] * N], nx, err);
    }
    nx=base=ncxt=0;
  }

  // Input x (call up to N times)
  void add(int x) {
    assert(nx<N);
    tx[nx++]=x;
  }

  // Set a context (call S times, sum of ranges <= M)
  void set(int cx, int range) {
	  assert(range >= 0);
	  assert(ncxt < S);
	  assert(cx >= 0);
	  assert(base + cx < M);
	  cxt[ncxt++] = base + cx;
	  base += range;
  }

  // predict next bit
  int p() {
	  while (nx & 7) tx[nx++] = 0;  // pad
    if (mp) {  // combine outputs
      mp->update();
      for (int i=0; i<ncxt; ++i) {
		  pr[i] = squash(dot_product(&tx[0], &wx[cxt[i] * N], nx) >> 5);
        mp->add(stretch(pr[i]));
      }
      mp->set(0, 1);
      return mp->p();
    }
    else {  // S=1 context
		return pr[0] = squash(dot_product(&tx[0], &wx[0], nx) >> 8);
    }
  }
  ~Mixer();
};

Mixer::~Mixer() {
  delete mp;
}


Mixer::Mixer(int n, int m, int s, int w):
    N((n+7)&-8), M(m), S(s), tx(N), wx(N*M),
    cxt(S), ncxt(0), base(0), nx(0), pr(S), mp(0) {
	assert(n > 0 && N > 0 && (N & 7) == 0 && M > 0);
  int i;
  for (i = 0; i < S; ++i)
	  pr[i] = 2048;
  for (i = 0; i < N*M; ++i)
	  wx[i] = w;
  if (S > 1) mp = new Mixer(S, 1, 1, 0x7fff);
}

//////////////////////////// APM1 //////////////////////////////

// APM1 maps a probability and a context into a new probability
// that bit y will next be 1.  After each guess it updates
// its state to improve future guesses.

class APM1 {
  int index;     // last p, context
  const int N;   // number of contexts
  Array<U16> t;  // [N][33]:  p, context -> p
public:
  APM1(int n);
  int p(int pr=2048, int cxt=0, int rate=7) {
    assert(pr>=0 && pr<4096 && cxt>=0 && cxt<N && rate>0 && rate<32);
    pr=stretch(pr);
    int g=(y<<16)+(y<<rate)-y-y;
    t[index] += g-t[index] >> rate;
    t[index+1] += g-t[index+1] >> rate;
    const int w=pr&127;  // interpolation weight (33 points)
    index=(pr+2048>>7)+cxt*33;
    return t[index]*(128-w)+t[index+1]*w >> 11;
  }
};

// maps p, cxt -> p initially
APM1::APM1(int n): index(0), N(n), t(n*33) {
	for (int i = 0; i < N; ++i)
		for (int j = 0; j < 33; ++j)
			t[i * 33 + j] = i == 0 ? squash((j - 16) * 128) * 16 : t[j];
}

//////////////////////////// StateMap, APM //////////////////////////

// A StateMap maps a context to a probability.

static int dt[1024];  // i -> 16K / (i + 3)

class StateMap {
protected:
  const int N;  // Number of contexts
  int cxt;      // Context of last prediction
  Array<U32> t;       // cxt -> prediction in high 22 bits, count in low 10 bits
  inline void update(int limit) {
	  assert(cxt >= 0 && cxt < N);
	  U32 *p = &t[cxt], p0 = p[0];
	  int n = p0 & 1023, pr = p0 >> 10;  // count, prediction
	  if (n < limit) ++p0;
	  else p0 = p0 & 0xfffffc00 | limit;
	  p0 += (((y << 22) - pr) >> 3)*dt[n] & 0xfffffc00;
	  p[0] = p0;
  }

public:
  StateMap(int n = 256);

  // update bit y (0..1), predict next bit in context cx
  int p(int cx, int limit=1023) {
	  assert(cx >= 0 && cx < N);
	  assert(limit > 0 && limit < 1024);
	  update(limit);
	  return t[cxt = cx] >> 20;
  }
};

StateMap::StateMap(int n): N(n), cxt(0), t(n) {
	for (int i = 0; i < N; ++i)
		t[i] = 1 << 31;
}

// An APM maps a probability and a context to a new probability.  Methods:
class APM: public StateMap {
public:
  APM(int n);
  int p(int pr, int cx, int limit=255) {
	  assert(pr >= 0 && pr < 4096);
	  assert(cx >= 0 && cx < N / 24);
	  assert(limit > 0 && limit < 1024);
	  update(limit);
	  pr = (stretch(pr) + 2048) * 23;
	  int wt = pr & 0xfff;  // interpolation weight of next element
	  cx = cx * 24 + (pr >> 12);
	  assert(cx >= 0 && cx < N - 1);
	  cxt = cx + (wt >> 11);
	  pr = (t[cx] >> 13)*(0x1000 - wt) + (t[cx + 1] >> 13)*wt >> 19;
    return pr;
  }
};

APM::APM(int n): StateMap(n*24) {
  for (int i=0; i<N; ++i) {
	  int p = ((i % 24 * 2 + 1) * 4096) / 48 - 2048;
	  t[i] = (U32(squash(p)) << 20) + 6;
  }
}


//////////////////////////// hash //////////////////////////////

// Hash 2-5 ints.
inline U32 hash(U32 a, U32 b, U32 c=0xffffffff, U32 d=0xffffffff, U32 e = 0xffffffff) {
	U32 h = a * 200002979u + b * 30005491u + c * 50004239u + d * 70004807u + e * 110002499u;
	return h ^ h >> 9 ^ a >> 2 ^ b >> 3 ^ c >> 4 ^ d >> 5 ^ e >> 6;
}

///////////////////////////// BH ////////////////////////////////

// A BH maps a 32 bit hash to an array of B bytes (checksum and B-2 values)

// 2 byte checksum with LRU replacement (except last 2 by priority)
template <int B> class BH {
  enum {M=8};  // search limit
  Array<U8, 64> t; // elements
  U32 n; // size-1
public:
  BH(int i): t(i*B), n(i-1) {
	  assert(B >= 2 && i > 0 && (i&(i - 1)) == 0); // size a power of 2?
  }
  U8* operator[](U32 i);
};

template <int B>
inline  U8* BH<B>::operator[](U32 i) {
	int chk = (i >> 16 ^ i) & 0xffff;
	i = i * M&n;
	U8 *p;
	U16 *cp;
	int j;
  for (j = 0; j < M; ++j) {
	  p = &t[(i + j)*B];
	  cp = (U16*)p;
	  if (p[2] == 0) { *cp = chk; break; }
	  if (*cp == chk) break;  // found
  }
  if (!j) return p + 1;  // front
  static U8 tmp[B];  // element to move to front
  if (j == M) {
    --j;
    memset(tmp, 0, B);
	*(U16*)tmp = chk;
	if (M > 2 && t[(i + j)*B + 2] > t[(i + j - 1)*B + 2]) --j;
  }
  else memcpy(tmp, cp, B);
  memmove(&t[(i + 1)*B], &t[i*B], j*B);
  memcpy(&t[i*B], tmp, B);
  return &t[i*B + 1];
}

/////////////////////////// ContextMap /////////////////////////

// A ContextMap maps contexts to a bit histories and makes predictions
// to a Mixer.


// Predict to mixer m from bit history state s, using sm to map s to
// a probability.
inline int mix2(Mixer& m, int s, StateMap& sm) {
  int p1=sm.p(s);
  int n0 = -!nex(s, 2);
  int n1 = -!nex(s, 3);
  int st = stretch(p1) >> 2;
  m.add(st);
  p1 >>= 4;
  int p0 = 255 - p1;
  m.add(p1 - p0);
  m.add(st*(n1 - n0));
  m.add((p1&n0) - (p0&n1));
  m.add((p1&n1) - (p0&n0));
  return s > 0;
}

// A RunContextMap maps a context into the next byte and a repeat
// count up to M.  Size should be a power of 2.  Memory usage is 3M/4.
class RunContextMap {
  BH<4> t;
  U8* cp;
public:
	RunContextMap(int m) : t(m / 4) { cp = t[0] + 1; }
  void set(U32 cx) {  // update count
	  if (cp[0] == 0 || cp[1] != buf(1)) cp[0] = 1, cp[1] = buf(1);
	else if (cp[0] < 255) ++cp[0];
	cp = t[cx] + 1;
  }
  int p() {  // predict next bit
	  if (cp[1] + 256 >> 8 - bpos == c0)
		  return ((cp[1] >> 7 - bpos & 1) * 2 - 1)*ilog(cp[0] + 1) * 8;
	  else
		  return 0;
  }
  int mix(Mixer& m) {  // return run length
    m.add(p());
	return cp[0] != 0;
  }
};

// Context is looked up directly.  m=size is power of 2 in bytes.
// Context should be < m/512.  High bits are discarded.
class SmallStationaryContextMap {
  Array<U16> t;
  int cxt;
  U16 *cp;
public:
  SmallStationaryContextMap(int m): t(m / 2), cxt(0) {
	  assert((m / 2 & m / 2 - 1) == 0); // power of 2?
	  for (int i = 0; i < t.size(); ++i)
		  t[i] = 32768;
	  cp = &t[0];
  }
  void set(U32 cx) {
	  cxt = cx * 256 & t.size() - 256;
  }
  void mix(Mixer& m, int rate = 7) {
	  *cp += (y << 16) - *cp + (1 << rate - 1) >> rate;
	  cp = &t[cxt + c0];
	  m.add(stretch(*cp >> 4));
  }
};

class ContextMap {
  const int C;  // max number of contexts
  class E {  // hash element, 64 bytes
    U16 chk[7];  // byte context checksums
    U8 last;     // last 2 accesses (0-6) in low, high nibble
  public:
    U8 bh[7][7]; // byte context, 3-bit context -> bit history state
      // bh[][0] = 1st bit, bh[][1,2] = 2nd bit, bh[][3..6] = 3rd bit
      // bh[][0] is also a replacement priority, 0 = empty
    U8* get(U16 chk);  // Find element (0-6) matching checksum.
      // If not found, insert or replace lowest priority (not last).
  };
  Array<E, 64> t;  // bit histories for bits 0-1, 2-4, 5-7
    // For 0-1, also contains a run count in bh[][4] and value in bh[][5]
    // and pending update count in bh[7]
  Array<U8*> cp;   // C pointers to current bit history
  Array<U8*> cp0;  // First element of 7 element array containing cp[i]
  Array<U32> cxt;  // C whole byte contexts (hashes)
  Array<U8*> runp; // C [0..3] = count, value, unused, unused
  StateMap *sm;    // C maps of state -> p
  int cn;          // Next context to set by set()
  void update(U32 cx, int c);  // train model that context cx predicts c
  int mix1(Mixer& m, int cc, int bp, int c1, int y1);
    // mix() with global context passed as arguments to improve speed.
public:
  ContextMap(int m, int c=1);  // m = memory in bytes, a power of 2, C = c
  ~ContextMap();
  void set(U32 cx, int next=-1);   // set next whole byte context to cx
    // if next is 0 then set order does not matter
  int mix(Mixer& m) {return mix1(m, c0, bpos, buf(1), y);}
};

// Find or create hash element matching checksum ch
inline U8* ContextMap::E::get(U16 ch) {
  if (chk[last&15]==ch) return &bh[last&15][0];
  int b=0xffff, bi=0;
  for (int i=0; i<7; ++i) {
    if (chk[i]==ch) return last=last<<4|i, (U8*)&bh[i][0];
    int pri=bh[i][0];
    if (pri<b && (last&15)!=i && last>>4!=i) b=pri, bi=i;
  }
  return last=0xf0|bi, chk[bi]=ch, (U8*)memset(&bh[bi][0], 0, 7);
}

// Construct using m bytes of memory for c contexts
ContextMap::ContextMap(int m, int c): C(c), t(m>>6), cp(c), cp0(c),
    cxt(c), runp(c), cn(0) {
	assert(m >= 64 && !(m&m - 1));  // power of 2?
	assert(sizeof(E) == 64);
	sm = new StateMap[C];
  for (int i = 0; i < C; ++i) {
	  cp0[i] = cp[i] = &t[0].bh[0][0];
	  runp[i] = cp[i] + 3;
  }
}

ContextMap::~ContextMap() {
  delete[] sm;
}

// Set the i'th context to cx
inline void ContextMap::set(U32 cx, int next) {
	int i = cn++;
	i &= next;
	assert(i >= 0 && i < C);
	cx = cx * 987654323 + i;  // permute (don't hash) cx to spread the distribution
	cx = cx << 16 | cx >> 16;
	cxt[i] = cx * 123456791 + i;
}

// Update the model with bit y1, and predict next bit to mixer m.
// Context: cc=c0, bp=bpos, c1=buf(1), y1=y.
int ContextMap::mix1(Mixer& m, int cc, int bp, int c1, int y1) {

  // Update model with y
  int result=0;
  for (int i=0; i<cn; ++i) {
    if (cp[i]) {
		assert(cp[i] >= &t[0].bh[0][0] && cp[i] <= &t[t.size() - 1].bh[6][6]);
	  assert((long(cp[i]) & 63) >= 15);
	  int ns = nex(*cp[i], y1);
	  if (ns >= 204 && rnd() << (452 - ns >> 3)) ns -= 4;  // probabilistic increment
      *cp[i]=ns;
    }

    // Update context pointers
    if (bpos > 1 && !runp[i][0])
		cp[i] = 0;
    else
    {
     switch(bpos)
     {
	 case 1: case 3: case 6: cp[i] = cp0[i] + 1 + (cc & 1); break;
	 case 4: case 7: cp[i] = cp0[i] + 3 + (cc & 3); break;
	 case 2: case 5: cp0[i] = cp[i] = t[cxt[i] + cc & t.size() - 1].get(cxt[i] >> 16); break;
      default:
      {
       cp0[i]=cp[i]=t[cxt[i]+cc&t.size()-1].get(cxt[i]>>16);
       // Update pending bit histories for bits 2-7
       if (cp0[i][3]==2) {
		   const int c = cp0[i][4] + 256;
		   U8 *p = t[cxt[i] + (c >> 6)&t.size() - 1].get(cxt[i] >> 16);
		   p[0] = 1 + ((c >> 5) & 1);
		   p[1 + ((c >> 5) & 1)] = 1 + ((c >> 4) & 1);
		   p[3 + ((c >> 4) & 3)] = 1 + ((c >> 3) & 1);
		   p = t[cxt[i] + (c >> 3)&t.size() - 1].get(cxt[i] >> 16);
		   p[0] = 1 + ((c >> 2) & 1);
		   p[1 + ((c >> 2) & 1)] = 1 + ((c >> 1) & 1);
		   p[3 + ((c >> 1) & 3)] = 1 + (c & 1);
		   cp0[i][6] = 0;
       }
       // Update run count of previous context
       if (runp[i][0]==0)  // new context
         runp[i][0]=2, runp[i][1]=c1;
       else if (runp[i][1]!=c1)  // different byte in context
         runp[i][0]=1, runp[i][1]=c1;
       else if (runp[i][0]<254)  // same byte in context
         runp[i][0]+=2;
       else if (runp[i][0]==255)
         runp[i][0]=128;
       runp[i]=cp0[i]+3;
      } break;
     }
    }

    // predict from last byte in context
    if (runp[i][1]+256>>8-bp==cc) {
      int rc=runp[i][0];  // count*2, +1 if 2 different bytes seen
      int b=(runp[i][1]>>7-bp&1)*2-1;  // predicted bit + for 1, - for 0
      int c=ilog(rc+1)<<2+(~rc&1);
      m.add(b*c);
    }
    else
      m.add(0);

   if(cp[i])
   {
    result+=mix2(m, *cp[i], sm[i]);
   }
   else
   {
    mix2(m, 0, sm[i]);
   }
  }
  if (bp==7) cn=0;
  return result;
}

//////////////////////////// Models //////////////////////////////

// All of the models below take a Mixer as a parameter and write
// predictions to it.

//////////////////////////// matchModel ///////////////////////////

// find the longest matching context and return its length

int matchModel(Mixer& m) {
  const int MAXLEN=65534;  // longest allowed match + 1
  static Array<int> t(MEM);  // hash table of pointers to contexts
  static int h=0;  // hash of last 7 bytes
  static int ptr=0;  // points to next byte of match if any
  static int len=0;  // length of match, or 0 if no match
  static int result=0;
  
  static SmallStationaryContextMap scm1(0x20000);

  if (!bpos) {
    h=h*997*8+buf(1)+1&t.size()-1;  // update context hash
    if (len) ++len, ++ptr;
    else {  // find match
      ptr=t[h];
      if (ptr && pos-ptr<buf.size())
        while (buf(len+1)==buf[ptr-len-1] && len<MAXLEN) ++len;
    }
    t[h]=pos;  // update hash table
    result=len;
    scm1.set(pos);
  }

  // predict
  if (len)
  {
   if (buf(1)==buf[ptr-1] && c0==buf[ptr]+256>>8-bpos)
   {
    if (len>MAXLEN) len=MAXLEN;
    if (buf[ptr]>>7-bpos&1)
    {
     m.add(ilog(len)<<2);
     m.add(min(len, 32)<<6);
    }
    else 
    {
     m.add(-(ilog(len)<<2));
     m.add(-(min(len, 32)<<6));
    }
   }
   else
   {
    len=0;
    m.add(0);
    m.add(0);
   }
  }
  else
  {
   m.add(0);
   m.add(0);
  }

  scm1.mix(m);
  return result;
}

//////////////////////////// wordModel /////////////////////////

// Model English text (words and columns/end of line)

void wordModel(Mixer& m) {
  static U32 word0=0, word1=0, word2=0, word3=0, word4=0, word5=0;  // hashes
  static U32 text0=0;  // hash stream of letters
  static ContextMap cm(MEM*16, 20);
  static int nl1=-3, nl=-2;  // previous, current newline position

  // Update word hashes
  if (bpos==0) {
    int c=c4&255;
    if (c>='A' && c<='Z')
      c += 'a' - 'A';
    if (c >= 'a' && c <= 'z' || c >= 128) {
      word0 = word0 * 263 * 32 + c;
      text0 = text0 * 997 * 16 + c;
    }
    else if (word0) {
		word5 = word4 * 23;
		word4 = word3 * 19;
		word3 = word2 * 17;
		word2 = word1 * 13;
		word1 = word0 * 11;
		word0 = 0;
    }
    if (c==10) nl1=nl, nl=pos-1;
    int col=min(255, pos-nl), above=buf[nl1+col]; // text column context
    U32 h=word0*271+buf(1);
    
    cm.set(h);
    cm.set(word0);
    cm.set(h+word1);
    cm.set(word0+word1*31);
    cm.set(h+word1+word2*29);
    cm.set(text0&0xffffff);
    cm.set(text0&0xfffff);

    cm.set(h+word2);
    cm.set(h+word3);
    cm.set(h+word4);
    cm.set(h+word5);
    cm.set(buf(1)|buf(3)<<8|buf(5)<<16);
    cm.set(buf(2)|buf(4)<<8|buf(6)<<16);

    cm.set(h+word1+word3);
    cm.set(h+word2+word3);

    cm.set(col<<16|buf(1)<<8|above);
    cm.set(buf(1)<<8|above);
    cm.set(col<<8|buf(1));
    cm.set(col);
  }
  cm.mix(m);
}

//////////////////////////// recordModel ///////////////////////

// Model 2-D data with fixed record length.  Also order 1-2 models
// that include the distance to the last match.

void recordModel(Mixer& m) {
  static int cpos1[256] , cpos2[256], cpos3[256], cpos4[256];
  static int wpos1[0x10000]; // buf(1..2) -> last position
  static int rlen=2, rlen1=3, rlen2=4;  // run length and 2 candidates
  static int rcount1=0, rcount2=0;  // candidate counts
  static ContextMap cm(32768, 3), cn(32768/2, 3), co(32768*2, 3), cp(MEM, 3);

  if (!bpos) {
    int w=c4&0xffff, c=w&255, d=w>>8;
    int r=pos-cpos1[c];
    if (r>1 && r==cpos1[c]-cpos2[c]
        && r==cpos2[c]-cpos3[c] && r==cpos3[c]-cpos4[c]
        && (r>15 || (c==buf(r*5+1)) && c==buf(r*6+1))) {
      if (r==rlen1) ++rcount1;
      else if (r==rlen2) ++rcount2;
      else if (rcount1>rcount2) rlen2=r, rcount2=1;
      else rlen1=r, rcount1=1;
    }
    if (rcount1>15 && rlen!=rlen1) rlen=rlen1, rcount1=rcount2=0;
    if (rcount2>15 && rlen!=rlen2) rlen=rlen2, rcount1=rcount2=0;

    assert(rlen>0);
    cm.set(c<<8| (min(255, pos-cpos1[c])/4) );
    cm.set(w<<9| llog(pos-wpos1[w])>>2);
    
    cm.set(rlen|buf(rlen)<<10|buf(rlen*2)<<18);
    cn.set(w|rlen<<8);
    cn.set(d|rlen<<16);
    cn.set(c|rlen<<8);

    co.set(buf(1) << 8 | min(255, pos-cpos1[buf(1)]));
    co.set(buf(1)<<17|buf(2)<<9|llog(pos-wpos1[w])>>2);
    int col = pos % rlen;
    co.set(buf(1)<<8|buf(rlen));

    cp.set(rlen|buf(rlen)<<10|col<<18);
    cp.set(rlen|buf(1)<<10|col<<18);
    cp.set(col|rlen<<12);

    cpos4[c]=cpos3[c];
    cpos3[c]=cpos2[c];
    cpos2[c]=cpos1[c];
    cpos1[c]=pos;
    wpos1[w]=pos;
  }
  cm.mix(m);
  cn.mix(m);
  co.mix(m);
  cp.mix(m);
}


//////////////////////////// sparseModel ///////////////////////

// Model order 1-2 contexts with gaps.

void sparseModel(Mixer& m, int seenbefore, int howmany) {
  static ContextMap cm(MEM*2, 48);
  static int mask = 0;

  if (bpos==0) {

    cm.set( c4 & 0x00f0f0f0);
    cm.set((c4 & 0xf0f0f0f0) + 1);
    cm.set((c4 & 0x00f8f8f8) + 2);
    cm.set((c4 & 0xf8f8f8f8) + 3);
    cm.set((c4 & 0x00e0e0e0) + 4);
    cm.set((c4 & 0xe0e0e0e0) + 5);
    cm.set((c4 & 0x00f0f0ff) + 6);

    cm.set(seenbefore);
    cm.set(howmany);
    cm.set(c4&0x00ff00ff);
    cm.set(c4&0xff0000ff);
    cm.set(buf(1)|buf(5)<<8);
    cm.set(buf(1)|buf(6)<<8);
    cm.set(buf(3)|buf(6)<<8);
    cm.set(buf(4)|buf(8)<<8);
    
    for (int i=1; i<8; ++i) {
      cm.set((buf(i+1)<<8)|buf(i+2));
      cm.set((buf(i+1)<<8)|buf(i+3));
      cm.set(seenbefore|buf(i)<<8);
    }

    int fl = 0;
    if(c4 & 0xff != 0){
           if(isalpha(c4 & 0xff)) fl = 1;
      else if(ispunct(c4 & 0xff)) fl = 2;
      else if(isspace(c4 & 0xff)) fl = 3;
      else if(c4 & 0xff == 0xff)  fl = 4;
      else if(c4 & 0xff < 16)     fl = 5;
      else if(c4 & 0xff < 64)     fl = 6;
      else fl = 7;
    }
    mask = (mask << 3) | fl;
    cm.set(mask);
    cm.set(mask << 8 | buf(1));
    cm.set(mask << 17 | buf(2) << 8 | buf(3));
    cm.set(mask & 0x1ff | ((c4 & 0xf0f0f0f0) << 9));
  }
  cm.mix(m);
}

//////////////////////////// distanceModel ///////////////////////

// Model for modelling distances between symbols

void distanceModel(Mixer& m) {
  static ContextMap cr(MEM, 3);
  if( bpos == 0 ){
    static int pos00=0,pos20=0,posnl=0;
    int c=c4&0xff;
    if(c==0x00)pos00=pos;
    if(c==0x20)pos20=pos;
    if(c==0xff||c=='\r'||c=='\n')posnl=pos;
    cr.set(min(pos-pos00,255)|(c<<8));
    cr.set(min(pos-pos20,255)|(c<<8));
    cr.set(min(pos-posnl,255)|(c<<8)+234567);
  }
  cr.mix(m);
}

//////////////////////////// indirectModel /////////////////////

// The context is a byte string history that occurs within a
// 1 or 2 byte context.

void indirectModel(Mixer& m) {
  static ContextMap cm(MEM, 6);
  static U32 t1[256];
  static U16 t2[0x10000];

  if (!bpos) {
    U32 d=c4&0xffff, c=d&255;
    U32& r1=t1[d>>8];
    r1=r1<<8|c;
    U16& r2=t2[c4>>8&0xffff];
    r2=r2<<8|c;
    U32 t=c|t1[c]<<8;
    cm.set(t&0xffff);
    cm.set(t&0xffffff);
    cm.set(t);
    cm.set(t&0xff00);
    t=d|t2[d]<<16;
    cm.set(t&0xffffff);
    cm.set(t);

  }
  cm.mix(m);
}

//////////////////////////// dmcModel //////////////////////////

// Model using DMC.  The bitwise context is represented by a state graph,
// initilaized to a bytewise order 1 model as in 
// http://plg.uwaterloo.ca/~ftp/dmc/dmc.c but with the following difference:
// - It uses integer arithmetic.
// - The threshold for cloning a state increases as memory is used up.
// - Each state maintains both a 0,1 count and a bit history (as in a
//   context model).  The 0,1 count is best for stationary data, and the
//   bit history for nonstationary data.  The bit history is mapped to
//   a probability adaptively using a StateMap.  The two computed probabilities
//   are combined.
// - When memory is used up the state graph is reinitialized to a bytewise
//   order 1 context as in the original DMC.  However, the bit histories
//   are not cleared.

struct DMCNode {  // 12 bytes
  unsigned int nx[2];  // next pointers
  U8 state;  // bit history
  unsigned int c0:12, c1:12;  // counts * 256
};

void dmcModel(Mixer& m) {
  static int top=0, curr=0;  // allocated, current node
  static Array<DMCNode> t(MEM*2);  // state graph
  static StateMap sm;
  static int threshold=256;

  // clone next state
  if (top>0 && top<t.size()) {
    int next=t[curr].nx[y];
    int n=y?t[curr].c1:t[curr].c0;
    int nn=t[next].c0+t[next].c1;
    if (n>=threshold*2 && nn-n>=threshold*3) {
      int r=n*4096/nn;
      assert(r>=0 && r<=4096);
      t[next].c0 -= t[top].c0 = t[next].c0*r>>12;
      t[next].c1 -= t[top].c1 = t[next].c1*r>>12;
      t[top].nx[0]=t[next].nx[0];
      t[top].nx[1]=t[next].nx[1];
      t[top].state=t[next].state;
      t[curr].nx[y]=top;
      ++top;
      if (top==MEM*2) threshold=512;
      if (top==MEM*3) threshold=768;
    }
  }

  // Initialize to a bytewise order 1 model at startup or when flushing memory
  if (top==t.size() && bpos==1) top=0;
  if (top==0) {
    assert(t.size()>=65536);
    for (int i=0; i<256; ++i) {
      for (int j=0; j<256; ++j) {
        if (i<127) {
          t[j*256+i].nx[0]=j*256+i*2+1;
          t[j*256+i].nx[1]=j*256+i*2+2;
        }
        else {
          t[j*256+i].nx[0]=(i-127)*256;
          t[j*256+i].nx[1]=(i+1)*256;
        }
        t[j*256+i].c0=128;
        t[j*256+i].c1=128;
      }
    }
    top=65536;
    curr=0;
    threshold=256;
  }

  // update count, state
  if (y) {
    if (t[curr].c1<3800) t[curr].c1+=256;
  }
  else if (t[curr].c0<3800) t[curr].c0 += 256;
  t[curr].state = nex(t[curr].state, y);
  curr = t[curr].nx[y];

  // predict
  const int pr1 = sm.p(t[curr].state);
  const int n1 = t[curr].c1;
  const int n0 = t[curr].c0;
  const int pr2 = (n1 + 5) * 4096 / (n0 + n1 + 10);
  m.add(stretch(pr1));
  m.add(stretch(pr2));
}

//////////////////////////// contextModel //////////////////////

typedef enum {DEFAULT, TEXT} Filetype;

// This combines all the context models with a Mixer.
int contextModel2() {
  static ContextMap cm(MEM*32, 9);
  static RunContextMap rcm7(MEM), rcm9(MEM), rcm10(MEM);
  static Mixer m(800, 3088, 7, 128);
  static U32 cxt[16];  // order 0-11 contexts
  static Filetype filetype=DEFAULT;
  static int size=0;  // bytes remaining in block

  // Parse filetype and size
  if (bpos==0) {
    --size;
    if (size==-1) filetype=(Filetype)buf(1);
    if (size==-5) {
      size=buf(4)<<24|buf(3)<<16|buf(2)<<8|buf(1);
    }
  }

  m.update();
  m.add(256);

  // Test for special file types
  int ismatch=ilog(matchModel(m));  // Length of longest matching context

  // Normal model
  if (bpos == 0) {
    int i;
    for ( i=15; i>0; --i)  // update order 0-11 context hashes
      cxt[i]=cxt[i-1]*257+(c4&255)+1;
    for ( i=0; i<7; ++i)
      cm.set(cxt[i]);
    rcm7.set(cxt[7]);
    cm.set(cxt[8]);
    rcm9.set(cxt[10]);
    rcm10.set(cxt[12]);
    cm.set(cxt[14]);
  }
  int order = cm.mix(m);
  
  rcm7.mix(m);
  rcm9.mix(m);
  rcm10.mix(m);

  if (level>=4) {
    sparseModel(m,ismatch,order);
    distanceModel(m);
    picModel(m);
    recordModel(m);  
    wordModel(m);
    indirectModel(m);
    dmcModel(m);
  }

  order = order-2;
  if(order<0) order=0;

  U32 c1=buf(1), c2=buf(2), c3=buf(3), c;

  m.set(c1+8, 264);
  m.set(c0, 256);
  m.set(order+8*(c4>>5&7)+64*(c1==c2)+128*(filetype==EXE), 256);
  m.set(c2, 256);
  m.set(c3, 256);
  m.set(ismatch, 256);
  
  if(bpos)
  {	
    c=c0<<(8-bpos); if(bpos==1)c+=c3/2;
    c=(min(bpos,5))*256+c1/32+8*(c2/32)+(c&192);
  }
  else c=c3/128+(c4>>31)*2+4*(c2/64)+(c1&240);
  m.set(c, 1536);
  int pr=m.p();
  return pr;
}


//////////////////////////// Predictor /////////////////////////

// A Predictor estimates the probability that the next bit of
// uncompressed data is 1.  Methods:
// p() returns P(1) as a 12 bit number (0-4095).
// update(y) trains the predictor with the actual bit (0 or 1).

class Predictor {
  int pr;  // next prediction
public:
  Predictor();
  int p() const {assert(pr>=0 && pr<4096); return pr;}
  void update();
};

Predictor::Predictor(): pr(2048) {}

void Predictor::update() {
  static APM1 a(256), a1(0x10000), a2(0x10000), a3(0x10000),
                      a4(0x10000), a5(0x10000), a6(0x10000);

  // Update global context: pos, bpos, c0, c4, buf
  c0+=c0+y;
  if (c0>=256) {
    buf[pos++]=c0;
    c4=(c4<<8)+c0-256;
    c0=1;
  }
  bpos=(bpos+1)&7;

  // Filter the context model with APMs
  int pr0=contextModel2();

  pr=a.p(pr0, c0);
  
  int pr1=a1.p(pr0, c0+256*buf(1));
  int pr2=a2.p(pr0, c0^hash(buf(1), buf(2))&0xffff);
  int pr3=a3.p(pr0, c0^hash(buf(1), buf(2), buf(3))&0xffff);
  pr0=pr0+pr1+pr2+pr3+2>>2;
  
      pr1=a4.p(pr, c0+256*buf(1));
      pr2=a5.p(pr, c0^hash(buf(1), buf(2))&0xffff);
      pr3=a6.p(pr, c0^hash(buf(1), buf(2), buf(3))&0xffff);
  pr=pr+pr1+pr2+pr3+2>>2;

  pr=pr+pr0+1>>1;
}

//////////////////////////// Encoder ////////////////////////////

// An Encoder does arithmetic encoding.  Methods:
// Encoder(COMPRESS, f) creates encoder for compression to archive f, which
//   must be open past any header for writing in binary mode.
// Encoder(DECOMPRESS, f) creates encoder for decompression from archive f,
//   which must be open past any header for reading in binary mode.
// code(i) in COMPRESS mode compresses bit i (0 or 1) to file f.
// code() in DECOMPRESS mode returns the next decompressed bit from file f.
//   Global y is set to the last bit coded or decoded by code().
// compress(c) in COMPRESS mode compresses one byte.
// decompress() in DECOMPRESS mode decompresses and returns one byte.
// flush() should be called exactly once after compression is done and
//   before closing f.  It does nothing in DECOMPRESS mode.
// size() returns current length of archive
// setFile(f) sets alternate source to FILE* f for decompress() in COMPRESS
//   mode (for testing transforms).
// If level (global) is 0, then data is stored without arithmetic coding.

typedef enum {COMPRESS, DECOMPRESS} Mode;
class Encoder {
private:
  Predictor predictor;
  const Mode mode;       // Compress or decompress?
  FILE* archive;         // Compressed data file
  U32 x1, x2;            // Range, initially [0, 1), scaled by 2^32
  U32 x;                 // Decompress mode: last 4 input bytes of archive
  FILE *alt;             // decompress() source in COMPRESS mode

  // Compress bit y or return decompressed bit
  int code(int i=0) {
    int p=predictor.p();
    assert(p>=0 && p<4096);
    p+=p<2048;
    U32 xmid=x1 + (x2-x1>>12)*p + ((x2-x1&0xfff)*p>>12);
    assert(xmid>=x1 && xmid<x2);
    if (mode==DECOMPRESS) y=x<=xmid; else y=i;
    y ? (x2=xmid) : (x1=xmid+1);
    predictor.update();
    while (((x1^x2)&0xff000000)==0) {  // pass equal leading bytes of range
      if (mode==COMPRESS) putc(x2>>24, archive);
      x1<<=8;
      x2=(x2<<8)+255;
      if (mode==DECOMPRESS) x=(x<<8)+(getc(archive)&255);  // EOF is OK
    }
    return y;
  }

public:
  Encoder(Mode m, FILE* f);
  Mode getMode() const {return mode;}
  long size() const {return ftell(archive);}  // length of archive so far
  void flush();  // call this when compression is finished
  void setFile(FILE* f) {alt=f;}

  // Compress one byte
  void compress(int c) {
    assert(mode==COMPRESS);
    if (level==0)
      putc(c, archive);
    else 
      for (int i=7; i>=0; --i)
        code((c>>i)&1);
  }

  // Decompress and return one byte
  int decompress() {
    if (mode==COMPRESS) {
      assert(alt);
      return getc(alt);
    }
    else if (level==0)
      return getc(archive);
    else {
      int c=0;
      for (int i=0; i<8; ++i)
        c+=c+code();
      return c;
    }
  }
};

Encoder::Encoder(Mode m, FILE* f): 
    mode(m), archive(f), x1(0), x2(0xffffffff), x(0), alt(0) {
  if (level>0 && mode==DECOMPRESS) {  // x = first 4 bytes of archive
    for (int i=0; i<4; ++i)
      x=(x<<8)+(getc(archive)&255);
  }
  for (int i=0; i<1024; ++i)
    dt[i]=16384/(i+i+3);

}

void Encoder::flush() {
  if (mode==COMPRESS && level>0)
    putc(x1>>24, archive);  // Flush first unequal byte of range
}

/////////////////////////// Filters /////////////////////////////////
//
// Before compression, data is encoded in blocks with the following format:
//
//   <type> <size> <encoded-data>
//
// Type is 1 byte (type Filetype): DEFAULT=0, JPEG, EXE
// Size is 4 bytes in big-endian format.
// Encoded-data decodes to <size> bytes.  The encoded size might be
// different.  Encoded data is designed to be more compressible.
//
//   void encode(FILE* in, FILE* out, int n);
//
// Reads n bytes of in (open in "rb" mode) and encodes one or
// more blocks to temporary file out (open in "wb+" mode).
// The file pointer of in is advanced n bytes.  The file pointer of
// out is positioned after the last byte written.
//
//   en.setFile(FILE* out);
//   int decode(Encoder& en);
//
// Decodes and returns one byte.  Input is from en.decompress(), which
// reads from out if in COMPRESS mode.  During compression, n calls
// to decode() must exactly match n bytes of in, or else it is compressed
// as type 0 without encoding.
//
//   Filetype detect(FILE* in, int n, Filetype type);
//
// Reads n bytes of in, and detects when the type changes to
// something else.  If it does, then the file pointer is repositioned
// to the start of the change and the new type is returned.  If the type
// does not change, then it repositions the file pointer n bytes ahead
// and returns the old type.
//
// For each type X there are the following 2 functions:
//
//   void encode_X(FILE* in, FILE* out, int n, ...);
//
// encodes n bytes from in to out.
//
//   int decode_X(Encoder& en);
//
// decodes one byte from en and returns it.  decode() and decode_X()
// maintain state information using static variables.
#define bswap(x)	\
+   ((((x) & 0xff000000) >> 24) | \
+    (((x) & 0x00ff0000) >>  8) | \
+    (((x) & 0x0000ff00) <<  8) | \
+    (((x) & 0x000000ff) << 24))

Filetype detect(FILE* in, int n, Filetype type) {
  U32 buf1=0, buf0=0;  // last 8 bytes
  long start=ftell(in);

  for (int i = 0; i < n; ++i) {
	  int c = getc(in);
	  if (c == EOF) return (Filetype)(-1);
	  buf1 = buf1 << 8 | buf0 >> 24;
	  buf0 = buf0 << 8 | c;
  }
  return type;
}

// Default encoding as self
void encode_default(FILE* in, FILE* out, int len) {
  while (len--) putc(getc(in), out);
}

int decode_default(Encoder& en) {
  return en.decompress();
}

// Split n bytes into blocks by type.  For each block, output
// <type> <size> and call encode_X to convert to type X.
void encode(FILE* in, FILE* out, int n) {
  Filetype type=DEFAULT;
  long begin=ftell(in);
  while (n>0) {
    Filetype nextType=detect(in, n, type);
    long end=ftell(in);
    fseek(in, begin, SEEK_SET);
    int len=int(end-begin);
    if (len>0) {
      fprintf(out, "%c%c%c%c%c", type, len>>24, len>>16, len>>8, len);
	  encode_default(in, out, len);
    }
    n-=len;
    type=nextType;
    begin=end;
  }
}

// Decode <type> <len> <data>...
int decode(Encoder& en) {
  static Filetype type=DEFAULT;
  static int len=0;
  while (len==0) {
    type=(Filetype)en.decompress();
    len=en.decompress()<<24;
    len|=en.decompress()<<16;
    len|=en.decompress()<<8;
    len|=en.decompress();
    if (len<0) len=1;
  }
  --len;
  return decode_default(en);
}

//////////////////// Compress, Decompress ////////////////////////////

// Compress a file
void compress(const char* filename, long filesize, Encoder& en) {
  assert(en.getMode()==COMPRESS);
  assert(filename && filename[0]);
  FILE *f=fopen(filename, "rb");
  if (!f) perror(filename), quit();
  long start=en.size();
  printf("%s %ld -> ", filename, filesize);

  // Transform and test in blocks
  const int BLOCK=MEM*64;
  for (int i=0; filesize>0; i+=BLOCK) {
    int size=BLOCK;
    if (size>filesize) size=filesize;
    FILE* tmp=tmpfile();
    if (!tmp) perror("tmpfile"), quit();
    long savepos=ftell(f);
    encode(f, tmp, size);

    // Test transform
    rewind(tmp);
    en.setFile(tmp);
    fseek(f, savepos, SEEK_SET);
    long j;
    int c1=0, c2=0;
    for (j=0; j<size; ++j)
      if ((c1=decode(en))!=(c2=getc(f))) break;

    // Test fails, compress without transform
    if (j!=size || getc(tmp)!=EOF) {
      printf("Transform fails at %ld, input=%d decoded=%d, skipping...\n", i+j, c2, c1);
      en.compress(0);
      en.compress(size>>24);
      en.compress(size>>16);
      en.compress(size>>8);
      en.compress(size);
      fseek(f, savepos, SEEK_SET);
      for (int j=0; j<size; ++j)
        en.compress(getc(f));
    }

    // Test succeeds, decode(encode(f)) == f, compress tmp
    else {
      rewind(tmp);
      int c;
      j=0;
      while ((c=getc(tmp))!=EOF)
        en.compress(c);
    }
    filesize-=size;
    fclose(tmp);  // deletes
  }
  if (f) fclose(f);
  printf("%-12ld\n", en.size()-start);
}

// Try to make a directory, return true if successful
bool mkdir(const char* dir) {
  return CreateDirectory(dir, 0);
}

// Decompress a file
void decompress(const char* filename, long filesize, Encoder& en) {
  assert(en.getMode()==DECOMPRESS);
  assert(filename && filename[0]);

  // Test if output file exists.  If so, then compare.
  FILE* f=fopen(filename, "rb");
  if (f) {
    printf("Comparing %s %ld -> ", filename, filesize);
    bool found=false;  // mismatch?
    for (int i=0; i<filesize; ++i) {
      printStatus(i);
      int c1=found?EOF:getc(f);
      int c2=decode(en);
      if (c1!=c2 && !found) {
        printf("differ at %d: file=%d archive=%d\n", i, c1, c2);
        found=true;
      }
    }
    if (!found && getc(f)!=EOF)
      printf("file is longer\n");
    else if (!found)
      printf("identical   \n");
    fclose(f);
  }

  // Create file
  else {
    f=fopen(filename, "wb");
    if (!f) {  // Try creating directories in path and try again
      String path(filename);
      for (int i=0; path[i]; ++i) {
        if (path[i]=='/' || path[i]=='\\') {
          char savechar=path[i];
          path[i]=0;
          if (makedir(path.c_str()))
            printf("Created directory %s\n", path.c_str());
          path[i]=savechar;
        }
      }
      f=fopen(filename, "wb");
    }

    // Decompress
    if (f) {
      for (int i=0; i<filesize; ++i)
        putc(decode(en), f);
      fclose(f);
    }
    // Can't create, discard data
    else {
      perror(filename);
      for (int i=0; i<filesize; ++i)
        decode(en);
    }
  }
}

//////////////////////// "User" (the CWL app) Interface ///////////////////////

const char* getline(FILE *f) {
  static String s;
  int len = 0, c;
  while ((c = getc(f)) != EOF && c != 11 && c != '\n') {
    if (len >= s.size()) s.resize(len * 2 + 1);
    s[len++] = c;
  }
  if (len>=s.size()) s.resize(len+1);
  s[len]=0;
  if (c == EOF || c == 11)
    return 0;
  else
    return s.c_str();
}

// Same as expand() except fname is an ordinary file
int putsize(String& archive, String& s, const char* fname, int base) {
  int result = 0;
  FILE *f = fopen(fname, "rb");
  if (f) {
    fseek(f, 0, SEEK_END);
    long len = ftell(f);
    if (len > -1) {
      static char blk[24];
      sprintf(blk, "%lx\n", len);
      archive += blk;
      archive += (fname + base);
      archive += "\n";
      s += fname;
      s += "\n";
      ++result;
    }
    fclose(f);
  }
  return result;
}

int expand(String& archive, String& s, const char* fname, int base) {
  int result = 0;
  DWORD attr = GetFileAttributes(fname);
  if ((attr != 0xFFFFFFFF) && (attr & FILE_ATTRIBUTE_DIRECTORY)) {
    WIN32_FIND_DATA ffd;
    String fdir(fname);
    fdir += "/*";
    HANDLE h = FindFirstFile(fdir.c_str(), &ffd);
    while (h != INVALID_HANDLE_VALUE) {
      if (!equals(ffd.cFileName, ".") && !equals(ffd.cFileName, "..")) {
        String d(fname);
        d += "/";
        d += ffd.cFileName;
        result += expand(archive, s, d.c_str(), base);
      }
      if (!FindNextFile(h, &ffd)) break;
    }
    FindClose(h);
  }
  else
    result = putsize(archive, s, fname, base);
  return result;
}

int main(int argc, char **argv) {
	if (argc < 4)
		quit("Invalid call: not enough arguments!");
	char *arg1 = argv[1];
	char *arg2 = argv[2];
	char *arg3 = argv[3];
	if (arg1[0] != '-')
		quit("Invalid call: argv[1] does not start with '-'!");
	if (!arg1[1])
		quit("Invalid call: argv[1] doesn't contain a character!");
	if (arg1[2])
		quit("Invalid call: argv[1] is longer than 2!");
  try {
	  char arg11 = arg1[1];
	Mode mode;
	if (arg11 == 'd')
		mode = DECOMPRESS;
	else if (arg11 == 'c')
		mode = COMPRESS;
	else
		quit("Invalid call: mode is neither 'd' nor 'c'.");

    FILE* archive = 0;  // compressed file
    int files = 0;  // number of files to compress/decompress
    Array<char*> fname(1);  // file names (resized to files)
    Array<long> fsize(1);   // file lengths (resized to files)

    String archiveName(arg2);
   
    String filenames;
    if (mode == COMPRESS) {
      String header_string;
      int i;
      for (i = 1; i < argc; ++i) {
        String name(argv[i]);
        int len = name.size() - 1;
        int base = len - 1;
        while (base >= 0 && name[base] != '\\') --base;
        ++base;
        int expanded = expand(header_string, filenames, name.c_str(), base);
        files += expanded;
      }

      archive = fopen(archiveName.c_str(), "wb+");
      if (!archive) perror(archiveName.c_str()), quit();
      fprintf(archive, PROGNAME "%c\n%s%c", 1, header_string.c_str(), 11);

      fname.resize(files);
      fsize.resize(files);
      char *p = &filenames[0];
      rewind(archive);
      getline(archive);
      for (i = 0; i < files; ++i) {
        const char *num = getline(archive);
        assert(num);
        fsize[i] = atol(num);
        assert(fsize[i] >= 0);
        fname[i] = p;
        while (*p != '\n') ++p;
        assert(p - filenames.c_str() < filenames.size());
        *p++ = 0;
      }
      fseek(archive, 0, SEEK_END);
    }

    if (mode == DECOMPRESS) {
      archive = fopen(archiveName.c_str(), "rb+");
      if (!archive) perror(archiveName.c_str()), quit();
	  getline(archive);
	  while (getline(archive)) ++files;  // count files
      long header_size=ftell(archive);
      filenames.resize(header_size+4);  // copy of header
      rewind(archive);
      fread(&filenames[0], 1, header_size, archive);
      fname.resize(files);
      fsize.resize(files);
      char* p=&filenames[0];
      while (*p && *p!='\r') ++p;  // skip first line
      ++p;
      for (int i=0; i<files; ++i) {
        fsize[i]=atol(p+1);
        while (*p && *p!='\t') ++p;
        fname[i]=p+1;
        while (*p && *p!='\r') ++p;
        if (!*p) printf("%s: header corrupted at %d\n", archiveName.c_str(),
          p-&filenames[0]), quit();
        assert(p-&filenames[0]<header_size);
        *p++=0;
      }
    }
    buf.setsize(MEM * 8);
    // Compress or decompress files
    assert(fname.size() == files && fsize.size() == files);
    long total_size = 0;  // sum of file sizes
    for (int i = 0; i < files; ++i) total_size+=fsize[i];
    Encoder en(mode, archive);
    if (mode==COMPRESS) {
      for (int i=0; i<files; ++i)
        compress(fname[i], fsize[i], en);
      en.flush();
      printf("%ld -> %ld\n", total_size, en.size());
    }

    // Decompress files to dir2: paq8p -d dir1/archive.paq8p dir2
    // If there is no dir2, then extract to dir1
    // If there is no dir1, then extract to .
    else {
      assert(argc >= 2);
      String dir(argc > 2 ? argv[2] : argv[1]);
      if (argc==2) {  // chop "/archive.paq8p"
        int i;
        for (i=dir.size()-2; i>=0; --i) {
          if (dir[i]=='/' || dir[i]=='\\') {
            dir[i]=0;
            break;
          }
          if (i==1 && dir[i]==':') {  // leave "C:"
            dir[i+1]=0;
            break;
          }
        }
        if (i==-1) dir=".";  // "/" not found
      }
      dir=dir.c_str();
      if (dir[0] && (dir.size()!=3 || dir[1]!=':')) dir+="/";
      for (int i=0; i<files; ++i) {
        String out(dir.c_str());
        out+=fname[i];
        decompress(out.c_str(), fsize[i], en);
      }
    }
    fclose(archive);
  }
  catch(const char* s) {
    if (s) printf("%s\n", s);
  }
  return 0;
}
