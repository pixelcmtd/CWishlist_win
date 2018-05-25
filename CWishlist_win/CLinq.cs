using System;
using System.Collections.Generic;

namespace CWishlist_win
{
    static class CLinq
    {
        public static TKey Where<TKey, TValue>(this Dictionary<TKey, TValue>.KeyCollection keys, Predicate<TKey> predicate)
        {
            foreach (TKey key in keys)
                if (predicate(key))
                    return key;
            return default(TKey);
        }

        public static bool ArrayEquals(this Array left, Array right)
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
    }
}
