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
    }
}
