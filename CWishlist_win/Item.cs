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

        public string dbgfmt()
        {
            return "{\"" + name + "\",\"" + url + "\"}";
        }

        public void write_bytes(Stream s, int format)
        {
            if (format == D1)
            {
                s.write(utf16(name));
                s.write(10, 13);
                s.write(utf16(url));
                s.write(10, 13);
            }
            else if (format == D2)
            {
                s.write(utf16(name));
                s.write(11);
                if (url.StartsWith("http://tinyurl.com/"))
                {
                    s.write(1);
                    s.write(ascii(url.Substring(19)));
                }
                else
                {
                    s.write(0);
                    s.write(utf16(url));
                }
                s.write(11);
            }
            else if (format == L1)
            {
                s.write(utf8(name));
                if (url.StartsWith(tinyurl))
                {
                    s.write(L1_TU);
                    s.write(b64(url.Substring(tinyurl_length)));
                }
                else
                {
                    s.write(L1_NOTU);
                    s.write(utf8(url.StartsWith(http) ? url.Substring(7) : url));
                    s.write(L1_ENDSTR);
                }
            }
        }
    }
}
