using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using static CWishlist_win.Encodings;

namespace CWishlist_win
{
    static class CLinq
    {
        public static K where<K, V>(this Dictionary<K, V>.KeyCollection keys, Predicate<K> predicate)
        {
            foreach (K key in keys)
                if (predicate(key))
                    return key;
            return default;
        }

        public static V where<K, V>(this Dictionary<K, V>.ValueCollection vals, Predicate<V> predicate)
        {
            foreach (V val in vals)
                if (predicate(val))
                    return val;
            return default;
        }

        public static T where<T>(this IEnumerable<T> ie, Predicate<T> p)
        {
            foreach (T t in ie)
                if (p(t))
                    return t;
            return default;
        }

        public static IEnumerable<T> to_enumerable<T>(this IEnumerator<T> ie)
        {
            return new Enumerable<T>(ie);
        }

        public static T where<T>(this IEnumerator<T> ie, Predicate<T> p)
        {
            return ie.to_enumerable().where(p);
        }

        public static bool arrequ<T>(T[] left, T[] right)
        {
            if (left.Length != right.Length)
                return false;
            for (int i = 0; i < left.Length; i++)
                if (!left[i].Equals(right[i]))
                    return false;
            return true;
        }

        public static string hex(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();
            foreach (byte b in bytes)
                s.Append(b.ToString("x2"));
            return s.ToString();
        }

        public static byte[] hex(string s)
        {
            int _len_div_2 = s.Length / 2; //saves idivs
            byte[] b = new byte[_len_div_2];
            for (int i = 0; i < _len_div_2; i++)
                //                             imul is less
                //                              expensive
                //                              than idiv
                b[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
            return b;
        }
        
        public static void add_entry(this ZipArchive zip, string entry_name, byte[] contents, CompressionLevel comp_lvl = CompressionLevel.Optimal)
        {
            Stream s = zip.CreateEntry(entry_name, comp_lvl).Open();
            s.write(contents);
            s.Close();
            s.Dispose();
        }

        public static void add_entry(this ZipArchive zip, string entry_name, byte content, CompressionLevel comp_lvl = CompressionLevel.Fastest)
        {
            Stream s = zip.CreateEntry(entry_name, comp_lvl).Open();
            s.write(content);
            s.Close();
            s.Dispose();
        }

        public static int read_entry_byte(this ZipArchive zip, string entry_name)
        {
            Stream s = zip.GetEntry(entry_name).Open();
            int b = s.ReadByte();
            s.Close();
            return b;
        }

        public static void write(this Stream s, params byte[] b)
        {
            s.Write(b, 0, b.Length);
        }

        public static void write_ascii(this Stream s, string t)
        {
            s.write(ascii(t));
        }

        public static void write_utf8(this Stream s, string t)
        {
            s.write(utf8(t));
        }

        public static void write_utf16(this Stream s, string t)
        {
            s.write(utf16(t));
        }

        public static byte[] b64(string s)
        {
            return Convert.FromBase64String(s);
        }

        public static string b64(byte[] b)
        {
            return Convert.ToBase64String(b);
        }

        public static string b64(Stream s, int bytelen)
        {
            byte[] b = new byte[bytelen];
            s.Read(b, 0, bytelen);
            return Convert.ToBase64String(b);
        }

        public static short int16(byte b, byte c)
        {
            return (short)((b << 8) | c);
        }

        public static ushort uint16(byte b, byte c)
        {
            return (ushort)((b << 8) | c);
        }

        public static int int32(byte b, byte c, byte d, byte e)
        {
            return (b << 24) | (c << 16) | (d << 8) | e;
        }

        public static uint uint32(byte b, byte c, byte d, byte e)
        {
            return ((uint)b << 24) | ((uint)c << 16) | ((uint)d << 8) | e;
        }

        public static long int64(byte b, byte c, byte d, byte e, byte f, byte g, byte h, byte i)
        {
            return ((long)b << 56) | ((long)c << 48) | ((long)d << 40) | ((long)e << 32)
                | ((long)f << 24) | ((long)g << 16) | ((long)h << 8) | (long)i;
        }

        public static ulong uint64(byte b, byte c, byte d, byte e, byte f, byte g, byte h, byte i)
        {
            return ((ulong)b << 56) | ((ulong)c << 48) | ((ulong)d << 40) | ((ulong)e << 32)
                | ((ulong)f << 24) | ((ulong)g << 16) | ((ulong)h << 8) | (ulong)i;
        }

        public static short int16(int i, int j)
        {
            return (short)((i << 8) | j);
        }

        public static ushort uint16(int i, int j)
        {
            return (ushort)((i << 8) | j);
        }

        public static int int32(int i, int j, int k, int l)
        {
            return (i << 24) | (j << 16) | (k << 8) | l;
        }

        public static uint uint32(int i, int j, int k, int l)
        {
            return ((uint)i << 24) | ((uint)j << 16) | ((uint)k << 8) | (uint)l;
        }

        public static long int64(int i, int j, int k, int l, int m, int n, int o, int p)
        {
            return ((long)i << 56) | ((long)j << 48) | ((long)k << 40) | ((long)l << 32)
                | ((long)m << 24) | ((long)n << 16) | ((long)o << 8) | (long)p;
        }

        public static ulong uint64(int i, int j, int k, int l, int m, int n, int o, int p)
        {
            return uint64(new byte[] { (byte)i, (byte)j, (byte)k, (byte)l, (byte)m, (byte)n, (byte)o, (byte)p });
        }

        public static short int16(byte[] b)
        {
            return (short)((b[0] << 8) | b[1]);
        }

        public static ushort uint16(byte[] b)
        {
            return (ushort)((b[0] << 8) | b[1]);
        }

        public static int int32(byte[] b)
        {
            return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
        }

        public static uint uint32(byte[] b)
        {
            return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }

        public static long int64(byte[] b)
        {
            return ((long)b[0] << 56) | ((long)b[1] << 48) | ((long)b[2] << 40) | ((long)b[3] << 32)
                | ((long)b[4] << 24) | ((long)b[5] << 16) | ((long)b[6] << 8) | b[7];
        }

        public static ulong uint64(byte[] b)
        {
            return ((ulong)b[0] << 56) | ((ulong)b[1] << 48) | ((ulong)b[2] << 40) | ((ulong)b[3] << 32)
                | ((ulong)b[4] << 24) | ((ulong)b[5] << 16) | ((ulong)b[6] << 8) | b[7];
        }

        public static byte[] bytes(short i)
        {
            return new byte[] { (byte)(i >> 8), (byte)i };
        }

        public static byte[] bytes(ushort i)
        {
            return new byte[] { (byte)(i >> 8), (byte)i };
        }

        public static byte[] bytes(int i)
        {
            return new byte[] { (byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8), (byte)i };
        }

        public static byte[] bytes(uint i)
        {
            return new byte[] { (byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8), (byte)i };
        }

        public static byte[] bytes(long i)
        {
            return new byte[] { (byte)(i >> 56), (byte)(i >> 48), (byte)(i >> 40), (byte)(i >> 32),
            (byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8), (byte)i};
        }

        public static byte[] bytes(ulong i)
        {
            return new byte[] { (byte)(i >> 56), (byte)(i >> 48), (byte)(i >> 40), (byte)(i >> 32),
            (byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8), (byte)i};
        }

        public static void arrcpy<T>(T[] arr1, T[] arr2, long len)
        {
            for (long i = 0; i < len; i++)
                arr2[i] = arr1[i];
        }

        //file compare
        public static bool fcmp(FileStream stream1, FileStream stream2)
        {
            if (stream1.Length - stream1.Position != stream2.Length - stream2.Position)
                return false;
            int i;
            while ((i = stream1.ReadByte()) != -1)
                if (i != stream2.ReadByte())
                    return false;
            return true;
        }

        //file compare
        public static bool fcmp(string f1, string f2)
        {
            FileStream s1 = File.Open(f1, FileMode.Open, FileAccess.Read);
            FileStream s2 = File.Open(f2, FileMode.Open, FileAccess.Read);
            bool b = fcmp(s1, s2);
            s1.Close();
            s2.Close();
            return b;
        }

        //writesinglebytefile
        public static void writesbf(string file, byte b)
        {
            FileStream fs = File.Open(file, FileMode.Create, FileAccess.Write);
            fs.WriteByte(b);
            fs.Close();
        }

        //fast [unsafe] [and full] array copy for item arrays
        public static void farrcpy(Item[] src, Item[] dest)
        {
            for (long i = 0; i < src.LongLength; i++)
                dest[i] = new Item(src[i].name, src[i].url);
        }

        //fastcharactercontains
        public static bool fccontains(string s, char c)
        {
            foreach (char d in s)
                if (c == d)
                    return true;
            return false;
        }
    }

    class Enumerable<T> : IEnumerable<T>
    {
        readonly IEnumerator<T> e;

        public Enumerable(IEnumerator<T> ie)
        {
            e = ie;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return e;
        }
    }
}
