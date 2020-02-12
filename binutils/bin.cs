using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static binutils.str;

namespace binutils
{
    public static class bin
    {
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

        public static byte[] b64(string s) => Convert.FromBase64String(s);
        public static string b64(byte[] b) => Convert.ToBase64String(b);
        public static string b64(Exception e) => b64(utf8(e.ToString()));

        public static string b64(Stream s, int bytelen)
        {
            byte[] b = new byte[bytelen];
            s.Read(b, 0, bytelen);
            return Convert.ToBase64String(b);
        }
    }
}
