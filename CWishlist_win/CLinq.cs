using System;
using System.Collections.Generic;
using System.Text;

namespace CWishlist_win
{
    static class CLinq
    {
        public static T where<T>(this IEnumerable<T> ie, Predicate<T> p)
        {
            foreach (T t in ie)
                if (p(t))
                    return t;
            return default;
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
            int _len_div_2 = s.Length / 2; //saves idivs in the iterations
            byte[] b = new byte[_len_div_2];
            for (int i = 0; i < _len_div_2; i++)
                //                             imul is less
                //                              expensive
                //                              than idiv
                b[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
            return b;
        }

        //fast [unsafe] [and full] array copy for item arrays
        public static void farrcpyitm(Item[] src, Item[] dest)
        {
            for (long i = 0; i < src.LongLength; i++)
                dest[i] = new Item(src[i].name, src[i].url);
        }
    }
}
