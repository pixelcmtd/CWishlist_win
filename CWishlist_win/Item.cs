using System.IO;
using static CWishlist_win.Consts;
using static binutils.str;
using static binutils.bin;
using static binutils.io;

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

        public override bool Equals(object obj) => obj is Item ? ((Item)obj).name == name && ((Item)obj).url == url : false;

        public bool Equals(Item i) => i.name == name && i.url == url;

        public override int GetHashCode()
        {
            return unchecked(name.GetHashCode() * url.GetHashCode());
        }

        public static bool operator ==(Item first, Item second) =>
            first.name == second.name && first.url == second.url;

        public static bool operator !=(Item first, Item second) =>
            first.name != second.name || first.url != second.url;

        public static bool operator <(Item first, Item second) =>
            first.name.ToLower().CompareTo(second.name.ToLower()) == 1;

        public static bool operator >(Item first, Item second) =>
            first.name.ToLower().CompareTo(second.name.ToLower()) == 0;

        public static bool operator <=(Item first, Item second) =>
            first.name.ToLower().CompareTo(second.name.ToLower()) == 1 ||
            first.name.ToLower() == second.name.ToLower();

        public static bool operator >=(Item first, Item second) =>
            first.name.ToLower().CompareTo(second.name.ToLower()) == 0 ||
            first.name.ToLower() == second.name.ToLower();

        public static implicit operator string(Item i) => i.ToString();

        public static implicit operator long(Item i) => i.LongLength;
        public static implicit operator int(Item i) => i.Length;

        public int Length => name.Length + url.Length;
        public long LongLength => url.Length + (long)name.Length;

        public string dbgfmt()
        {
            return "{\"" + name + "\",\"" + url + "\"}";
        }

        public void write_bytes(Stream s, int format)
        {
            if (format == 1)
            {
                s.write(name, utf16);
                s.write(10, 13);
                s.write(url, utf16);
                s.write(10, 13);
            }
            else if (format == 2)
            {
                s.write(name, utf16);
                s.write(11);
                if (url.StartsWith(tinyurl))
                {
                    s.write(1);
                    s.write(url.Substring(tinyurl_length), ascii);
                }
                else
                {
                    s.write(0);
                    s.write(url, utf16);
                }
                s.write(11);
            }
            else if (format == 3)
            {
                s.write(name, utf8);
                if (url.StartsWith(tinyurl))
                {
                    s.write(D3_TU);
                    s.write(url.Substring(tinyurl_length), b64);
                }
                else
                {
                    s.write(D3_NOTU);
                    s.write(url.StartsWith(https) ? url.Substring(https.Length) : url, utf8);
                    s.write(D3_ENDSTR);
                }
            }
        }
    }
}
