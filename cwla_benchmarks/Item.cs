using System;
using System.IO;
using System.Text;

namespace CWishlist_win
{
    public class Item
    {
        public static Item[] EMPTY { get; } = { };

        public Item()
        {
            name = url = null;
        }

        public Item(string name, string url)
        {
            this.name = name;
            this.url = url;
        }

        public string name;
        public string url;

        public override string ToString() => name != "" ? name : @"[/unnamed item\]";

        public override bool Equals(object obj) => (obj is Item) ? ((Item)obj).name == name && ((Item)obj).url == url : false;

        public bool Equals(Item i) => i.name == name && i.url == url;

        public override int GetHashCode()
        {
            return unchecked(name.GetHashCode() * url.GetHashCode());
        }

        public static bool operator ==(Item first, Item second) => first.name == second.name && first.url == second.url;

        public static bool operator !=(Item first, Item second) => first.name != second.name || first.url != second.url;

        public static bool operator <(Item first, Item second) => first.name.CompareTo(second.name) == 1;

        public static bool operator >(Item first, Item second) => first.name.CompareTo(second.name) == 0;

        public static bool operator <=(Item first, Item second) => first.name.CompareTo(second.name) == 1 || first.name == second.name;

        public static bool operator >=(Item first, Item second) => first.name.CompareTo(second.name) == 0 || first.name == second.name;

        public static implicit operator string(Item i) => i.ToString();

        public static implicit operator long(Item i) => i.LongLength;

        public static implicit operator int(Item i) => i.Length;

        public int Length
        {
            get => name.Length + url.Length;
        }

        public long LongLength
        {
            get => (long)url.Length + (long)name.Length;
        }

        public long MemoryLength
        {
            get => LongLength * 2;
        }

        public void write(Stream s)
        {
            byte[] _name = Encoding.UTF8.GetBytes(name);
            s.Write(_name, 0, _name.Length);
            if (url.StartsWith("http://tinyurl.com/"))
            {
                s.WriteByte(11);
                s.Write(Convert.FromBase64String(url.Substring(19)), 0, 6);
            }
            else
            {
                s.WriteByte(8);
                byte[] _url = Encoding.UTF8.GetBytes(url);
                s.Write(_url, 0, _url.Length);
                s.WriteByte(11);
            }
        }
    }
}
