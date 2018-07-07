using System;
using System.Collections;

namespace CWishlist_win
{
    public struct WL : IEnumerable
    {
        public WL(params Item[] items) => this.items = items;

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

        public Item[] SearchItems(Predicate<Item> predicate) => Array.FindAll(items, predicate);

        public int GetFirstIndex(Predicate<Item> predicate) => Array.FindIndex(items, predicate);
    }

    public struct Item
    {
        public Item(string name, string url)
        {
            this.name = name;
            this.url = url;
        }

        public string name;
        public string url;

        public override string ToString() => name != "" ? name : @"[/unnamed item\]";

        public override bool Equals(object obj) => (obj is Item) ? ((Item) obj).name == name && ((Item) obj).url == url : false;
		
		public bool Equals(Item itm) => itm.name == name && itm.url == url;

        public override int GetHashCode()
		{
			unchecked
			{
				return name.GetHashCode() + url.GetHashCode();
			}
		}

        public static bool operator ==(Item first, Item second) => first.name == second.name && first.url == second.url;

        public static bool operator !=(Item first, Item second) => first.name != second.name || first.url != second.url;

        public static bool operator <(Item first, Item second) => first.name.CompareTo(second.name) == 1;

        public static bool operator >(Item first, Item second) => first.name.CompareTo(second.name) == 0;

        public static bool operator <=(Item first, Item second) => first.name.CompareTo(second.name) == 1 || first.name == second.name;

        public static bool operator >=(Item first, Item second) => first.name.CompareTo(second.name) == 0 || first.name == second.name;

        public static implicit operator string(Item item) => item.ToString();

        public static implicit operator long(Item item) => item.url.Length + item.name.Length;

        public static implicit operator uint(Item item) => (uint)(item.url.Length + item.name.Length);

        public int Length
        {
            get => name.Length;
        }
    }
}
