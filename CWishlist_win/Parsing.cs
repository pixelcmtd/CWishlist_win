using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWishlist_win
{
    static class Parsing
    {
        public static int[] parse_ints(this string[] strs)
        {
            int[] i = new int[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = int.Parse(strs[j]);
            return i;
        }

        public static uint[] parse_uints(this string[] strs)
        {
            uint[] i = new uint[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = uint.Parse(strs[j]);
            return i;
        }

        public static short[] parse_shorts(this string[] strs)
        {
            short[] i = new short[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = short.Parse(strs[j]);
            return i;
        }

        public static ushort[] parse_ushorts(this string[] strs)
        {
            ushort[] i = new ushort[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = ushort.Parse(strs[j]);
            return i;
        }

        public static long[] parse_longs(this string[] strs)
        {
            long[] i = new long[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = long.Parse(strs[j]);
            return i;
        }

        public static ulong[] parse_ulongs(this string[] strs)
        {
            ulong[] i = new ulong[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = ulong.Parse(strs[j]);
            return i;
        }

        public static byte[] parse_bytes(this string[] strs)
        {
            byte[] i = new byte[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = byte.Parse(strs[j]);
            return i;
        }

        public static sbyte[] parse_sbytes(this string[] strs)
        {
            sbyte[] i = new sbyte[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = sbyte.Parse(strs[j]);
            return i;
        }

        public static decimal[] parse_decimals(this string[] strs)
        {
            decimal[] i = new decimal[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = decimal.Parse(strs[j]);
            return i;
        }

        public static bool[] parse_bools(this string[] strs)
        {
            bool[] i = new bool[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = bool.Parse(strs[j]);
            return i;
        }

        public static char[] parse_chars(this string[] strs)
        {
            char[] i = new char[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = char.Parse(strs[j]);
            return i;
        }
    }
}
