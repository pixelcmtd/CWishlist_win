using System;
using System.Collections.Generic;

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

        public static bool arr_equal(this Array left, Array right)
        {
            if (left.Length != right.Length)
                return false;
            if (left.GetType() != right.GetType())
                return false;
            for (int i = 0; i < left.Length; i++)
                if (left.GetValue(i) != right.GetValue(i))
                    return false;
            return true;
        }

        public static string ToString(this byte[] bytes, NumberFormat format)
        {
            string s = "";
            string f = format == NumberFormat.DEC ? "D" : format == NumberFormat.HEX ? "X" : throw new NotSupportedNumberFormatException(format);
            foreach (byte b in bytes)
                s += b.ToString(f);
            return s;
        }
    }

    class NotSupportedNumberFormatException : Exception
    {
        public NotSupportedNumberFormatException(NumberFormat nf) : base($"The given NumberFormat {nf} is not supported.") { }
    }
}
