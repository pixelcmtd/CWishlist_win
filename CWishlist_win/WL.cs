using System;
using System.Collections;
using System.Collections.Generic;
using static System.Array;

namespace CWishlist_win
{
    public class WL : IEnumerable
    {
        public WL(params Item[] items)
        {
            this.items = items;
        }

        public WL(List<Item> items) : this(items.ToArray()) { }

        public Item[] items;

        public static WL NEW { get; } = new WL(Item.EMPTY);

        public static bool operator ==(WL first, WL second)
        {
            long f = first.LongLength;
            if (f != second.LongLength)
                return false;
            for (long i = 0; i < f; i++)
                if (first[i] != second[i])
                    return false;
            return true;
        }

        public static bool operator !=(WL first, WL second)
        {
            long f = first.LongLength;
            if (f != second.LongLength)
                return true;
            for (long i = 0; i < f; i++)
                if (first[i] != second[i])
                    return true;
            return false;
        }

        public Item this[ulong index] => items[index];
        public Item this[uint index] => items[index];
        public Item this[int index] => items[index];
        public Item this[long index] => items[index];
        public Item this[short index] => items[index];
        public Item this[ushort index] => items[index];
        public Item this[byte index] => items[index];
        public Item this[sbyte index] => items[index];

        public static WL operator &(WL first, WL second)
        {
            Item[] items = new Item[first.Length + second.Length];
            for (int i = 0; i < first.Length; i++)
                items[i] = first[i];
            for (int i = first; i < first + second; i++)
                items[i] = second[i - first];
            return new WL(items);
        }

        public static implicit operator int(WL wl) => wl.items.Length;
        public static implicit operator long(WL wl) => wl.items.LongLength;
        public static implicit operator Item[](WL wl) => wl.items;

        public override bool Equals(object obj) => (obj is WL) ? ((WL)obj) == this : false;
		
		public bool Equals(WL wl) => wl == this;

        public override int GetHashCode()
		{
		    int hc = 0;
            foreach (Item i in items)
                hc = unchecked(hc * i.GetHashCode());
		    return hc;
		}

        public override string ToString() => items.ToString();

        public IEnumerator GetEnumerator() => items.GetEnumerator();

        public int Length => items.Length;
        public long LongLength => items.LongLength;

        public Item[] SearchItems(Predicate<Item> predicate) => FindAll(items, predicate);
        public int GetFirstIndex(Predicate<Item> predicate) => FindIndex(items, predicate);

        public int[] GetIndices(Predicate<Item> predicate)
        {
            List<int> i = new List<int>();
            for (int j = 0; j < items.Length; j++)
                if (predicate(items[j]))
                    i.Add(j);
            return i.ToArray();
        }
    }
}
