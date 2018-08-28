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
                s.write(Encoding.Unicode.GetBytes(name));
                s.write(10, 13);
                s.write(Encoding.Unicode.GetBytes(url));
                s.write(10, 13);
            }
            else if (format == "D2")
            {
                s.write(Encoding.Unicode.GetBytes(name));
                s.write(11);
                if (url.StartsWith("http://tinyurl.com/"))
                {
                    s.write(1);
                    s.write(Encoding.ASCII.GetBytes(url.Substring(19)));
                }
                else
                {
                    s.write(0);
                    s.write(Encoding.Unicode.GetBytes(url));
                }
                s.write(11);
            }
            else if (format == "L1")
            {
                s.write(Encoding.Unicode.GetBytes(name));
                if(url.StartsWith("http://tinyurl.com/") && url.Length == 27)
                {
                    s.write(13);
                    s.write(b64(url.Substring(19)));
                }
                else
                {
                    if(url.StartsWith("https://www."))
                    {
                        s.write(cwll_is_https_www);
                        s.write(Encoding.Unicode.GetBytes(url.Substring(12)));
                    }
                    else if(url.StartsWith("http://www."))
                    {
                        s.write(cwll_is_http_www);
                        s.write(Encoding.Unicode.GetBytes(url.Substring(11)));
                    }
                    else if (url.StartsWith("https://"))
                    {
                        s.write(cwll_is_https);
                        s.write(Encoding.Unicode.GetBytes(url.Substring(8)));
                    }
                    else if (url.StartsWith("http://"))
                    {
                        s.write(cwll_is_http);
                        s.write(Encoding.Unicode.GetBytes(url.Substring(7)));
                    }
                    else
                    {
                        s.write(cwll_no_protocol);
                        s.write(Encoding.Unicode.GetBytes(url));
                    }
                    s.write(cwll_item_end);
                }
            }
        }
    }
}
