using System;
using System.Collections;
using System.Collections.Generic;

namespace CWishlist_win
{
    public struct WL : IEnumerable
    {
        public WL(params Item[] items)
        {
            this.items = items;
        }

        public WL(List<Item> items)
        {
            this.items = items.ToArray();
        }

        public Item[] items;

        public static WL NEW { get; } = new WL(new Item[0]);

        public static bool operator ==(WL first, WL second)
        {
            if (first.Length != second.Length)
                return false;
            for (int i = 0; i < first.Length; i++)
                if (first[i] != second[i])
                    return false;
            return true;
        }

        public static bool operator !=(WL first, WL second) => !(first == second);

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

        public static implicit operator int(WL wl) => wl.Length;

        public static implicit operator long(WL wl)
        {
            return wl.items.LongLength;
        }

        public override bool Equals(object obj) => (obj is WL) ? ((WL)obj) == this : false;
		
		public bool Equals(WL wl) => wl == this;

        public override int GetHashCode()
		{
			unchecked
			{
				int hc = 0;
				foreach(Item i in items)
					hc += i.GetHashCode();
				return hc;
			}
		}

        public override string ToString() => items.ToString();

        public IEnumerator GetEnumerator() => items.GetEnumerator();

        public int Length
        {
            get => items.Length;
        }

        public long LongLength
        {
            get => items.LongLength;
        }

        public Item[] SearchItems(Predicate<Item> predicate) => Array.FindAll(items, predicate);

        public int GetFirstIndex(Predicate<Item> predicate) => Array.FindIndex(items, predicate);
    }
}
