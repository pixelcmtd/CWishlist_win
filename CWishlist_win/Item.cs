using System.IO;
using System.Text;
using static CWishlist_win.CLinq;
using static CWishlist_win.Consts;

namespace CWishlist_win
{
    public class Item
    {
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

        public byte[] bytes(string format)
        {
            MemoryStream ms = new MemoryStream();
            write_bytes(ms, format);
            ms.Close();
            return ms.ToArray();
        }

        public void write_bytes(Stream s, string format)
        {
            if (format == "D1")
            {
                s.write(utf16(name));
                s.write(10, 13);
                s.write(utf16(url));
                s.write(10, 13);
            }
            else if (format == "D2")
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
            else if (format.StartsWith("L1"))
            {
                bool url_before_utf8 = format.Length > 2 && format[2] != 'a';
                bool name_utf8 = is_utf8_only(name);
                int name_sep = url_before_utf8 ? cwll_utf8_base : cwll_utf16_base;
                if (name_utf8)
                    name_sep |= cwll_utf8;
                s.WriteByte((byte)name_sep);
                s.write(name_utf8 ? utf8(name) : utf16(name));
                if(url.StartsWith("http://tinyurl.com/") && url.Length == 27)
                {
                    s.write((byte)(cwll_tinyurl | (name_utf8 ? cwll_utf8_base : cwll_utf16_base)));
                    s.write(b64(url.Substring(19)));
                }
                else
                {
                    int flags = 0;
                    if (url.StartsWith(https))
                    {
                        flags |= cwll_https;
                        url = url.Substring(8);
                    }
                    else if (url.StartsWith(http))
                    {
                        flags |= cwll_http;
                        url = url.Substring(7);
                    }
                    if (url.StartsWith(www))
                    {
                        flags |= cwll_www;
                        url = url.Substring(4);
                    }
                    write_str(s, url, flags, name_utf8);
                }
            }
        }

        void write_str(Stream s, string t, int flags, bool str_before_utf8)
        {
            bool b = is_utf8_only(t);
            if (b)
                flags |= cwll_utf8;
            s.write((byte)((str_before_utf8 ? cwll_utf8_base : cwll_utf16_base) | flags));
            s.write(b ? utf8(t) : utf16(t));
        }
    }
}
