using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System;
using System.Net;
using static CWishlist_win.CLinq;
using static CWishlist_win.Consts;
using static SevenZip.SevenZipHelper;
using static System.IO.FileMode;
using static System.Text.Encoding;
using static System.IO.Compression.CompressionLevel;

namespace CWishlist_win
{
    static class IO
    {
        public static string tinyurl_create(string url) => new WebClient().DownloadString(tinyurl_api + url);

        public static bool valid_url(string url)
        {
            return url.ToLower().StartsWith("http://") || url.ToLower().StartsWith("https://") && url.Contains(".");
        }

        public static WL load(string f)
        {
            return f == "" ? WL.NEW : f.cose(1, 'd') ? cwld_load(f) : f.cose(1, 'u') ? cwlu_load(f) : f.cose(1, 'b') ? cwlb_load(f) : cwl_load(f);
        }

        public static WL backup_load(string f)
        {
            return cwld_load(f);
        }

        public static void backup_save(WL wl, string f)
        {
            cwld_save(wl, f);
        }

        static string cwll_url_str_read(Stream s, bool is_utf8)
        {
            StringBuilder b = new StringBuilder();
            int i;
            while ((i = s.ReadByte()) != -1)
                if ((is_utf8 && i == cwll_item_end_utf8) || (!is_utf8 && i == cwll_item_end_utf16))
                    break;
                else if (is_utf8)
                    b.Append(utf8(i));
                else
                    b.Append(utf16(i, s.ReadByte()));
            return b.ToString();
        }

        public static void cwll_save(WL wl, string file)
        {
            FileStream fs = File.Open(file, Create, FileAccess.Write);
            fs.Write(cwll_header, 0, 8);
            fs.write(5, 1);
            MemoryStream ms = new MemoryStream();
            foreach (Item i in wl)
                i.write_bytes(ms, "L1");
            Compress(ms, fs);
            ms.Close();
            fs.Close();
        }

        public static WL cwll_load(string file)
        {
            FileStream fs = File.Open(file, Open, FileAccess.Read);
            byte[] bfr = new byte[8];
            fs.Read(bfr, 0, 8);
            if(!arrequ(bfr, cwll_header))
            {
                fs.Close();
                throw new InvalidHeaderException("CWLL", cwll_header, bfr);
            }
            if(fs.ReadByte() != 5 || fs.ReadByte() > 1)
            {
                fs.Close();
                throw new NotSupportedFileVersionException();
            }
            MemoryStream ms = new MemoryStream();
            Decompress(fs, ms);
            fs.Close();
            List<Item> items = new List<Item>();
            Item i = new Item();
            int j;
            StringBuilder b = new StringBuilder();
            while ((j = ms.ReadByte()) != -1)
            {
                if (j > 0xdf && j < 0xf9) //the private area of utf16 (values that are not typable or displayable)
                {
                    if (j == cwll_is_tinyurl)
                    {
                        i.name = b.ToString();
                        b.Clear();
                        i.url = "http://tinyurl.com/" + b64(ms, 6);
                        items.Add(i);
                        i = new Item();
                    }
                    else
                    {
                        i.name = b.ToString();
                        b.Clear();
                        bool c = j == cwll_is_https_utf8;
                        bool f = j == cwll_is_https_www_utf8;
                        bool h = j == cwll_is_http_utf8;
                        bool m = j == cwll_is_http_www_utf8;
                        if (c || j == cwll_is_https_utf16)
                            i.url = cwll_url_str_read(ms, c);
                        else if (j == cwll_is_https_www_utf16 || f)
                            i.url = cwll_url_str_read(ms, f);
                        else if (j == cwll_is_http_utf16 || h)
                            i.url = cwll_url_str_read(ms, h);
                        else if (j == cwll_is_http_www_utf16 || m)
                            i.url = cwll_url_str_read(ms, m);
                        else
                            i.url = cwll_url_str_read(ms, j == cwll_no_protocol_utf8);
                        items.Add(i);
                        i = new Item();
                    }
                }
                else
                    b.Append(utf16(j, ms.ReadByte()));
            }
            ms.Close();
            return new WL(items);
        }

        /// <summary>
        /// Save func for the CWLD-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        public static void cwld_save(WL wl, string file)
        {
            Stream s = File.Open(file, Create, FileAccess.Write);

            s.write(cwld_header);
            s.write(4, 2);

            DeflateStream d = new DeflateStream(s, CompressionLevel.Optimal, false);

            foreach (Item i in wl)
                i.write_bytes(d, "D2");

            d.Close();
        }

        /// <summary>
        /// Read func for the CWLD-format<para />
        /// Name: CWishlistDeflate (A custom binary format compressed with Deflate)<para />
        /// File version: 4 (saved, checked)<para />
        /// Format versions: 1, 2(saved, checked)
        /// </summary>
        public static WL cwld_load(string file)
        {
            Stream raw = File.Open(file, Open, FileAccess.Read);

            byte[] h = new byte[8]; //header
            raw.Read(h, 0, 8);
            int v = -1;

            if (!arrequ(h, cwld_header))
            {
                raw.Close();
                throw new InvalidHeaderException("CWLD", cwld_header, h);
            }
            if (raw.ReadByte() != 4 || (v = raw.ReadByte()) > 2)
            {
                raw.Close();
                throw new NotSupportedFileVersionException();
            }

            if(v == 1)
            {
                DeflateStream d = new DeflateStream(raw, CompressionMode.Decompress, false);
                List<Item> items = new List<Item>();
                StringBuilder s = new StringBuilder();
                bool nus = false; //Name Url Switch
                Item i = new Item();
                char chr;
                int j = -1;

                while ((j = d.ReadByte()) != -1)
                    if ((chr = utf16(j, d.ReadByte())) == '\u0d0a')
                    {
                        if (nus)
                        {
                            i.url = s.ToString();
                            items.Add(i);
                            i = new Item();
                        }
                        else
                            i.name = s.ToString();
                        s.Clear();
                        nus = !nus;
                    }
                    else
                        s.Append(chr);

                d.Close();

                return new WL(items);
            }
            else
            {
                DeflateStream d = new DeflateStream(raw, CompressionMode.Decompress, false);
                List<Item> itms = new List<Item>();
                StringBuilder s = new StringBuilder();
                bool cs = false; //char switch
                bool nus = false; //name url switch
                bool tu = false; //tinyurl
                Item i = new Item();
                int j = -1;
                byte b = 0;

                while((j = d.ReadByte()) != -1)
                    if (j == 11 && !cs)
                    {
                        tu = false;
                        if (!nus)
                        {
                            i.name = s.ToString();
                            nus = true;
                            tu = d.ReadByte() == 1;
                        }
                        else
                        {
                            i.url = s.ToString();
                            itms.Add(i);
                            i = new Item();
                            nus = false;
                        }
                        s.Clear();
                        if (tu)
                            s.Append("http://tinyurl.com/");
                    }
                    else if (tu)
                        s.Append(ascii(j));
                    else
                    {
                        if (cs)
                            s.Append(utf16(b, j));
                        else
                            b = (byte)j;
                        cs = !cs;
                    }
                return new WL(itms);
            }
        }

        /// <summary>
        /// Read func for the CWLU-format<para />
        /// Name: CWishlistUncde (UTF16/Unicode and no longer useless UTF32 in Base64)<para />
        /// File version: 3 (saved, checked)<para />
        /// Format versions: 1 (saved, checked)
        /// </summary>
        static WL cwlu_load(string file)
        {
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);

            //check major & minor
            if (zip.read_entry_byte("F") != 3 || zip.read_entry_byte("V") != 1)
                throw new NotSupportedFileVersionException();

            //start reading W
            XmlReader xml = XmlReader.Create(new StreamReader(zip.GetEntry("W").Open(), Encoding.Unicode));
            List<Item> items = new List<Item>();

            while (xml.Read())
                if (xml.Name == "i")
                    items.Add(new Item(xml.GetAttribute("n"), xml.GetAttribute("u")));

            xml.Close();
            zip.Dispose();

            return new WL(items);
        }

        /// <summary>
        /// Read func for the CWLB-format<para />
        /// Name: CWishlistBase[64] (64 is not included in the name, but Base64 is the reason for the name)<para />
        /// File version 2 (should be saved, not checked)<para />
        /// Format versions: 1 (should be saved, not checked)
        /// </summary>
        static WL cwlb_load(string file)
        {
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, ASCII);
            XmlReader xml = XmlReader.Create(zip.GetEntry("W").Open());
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "i")
                    items.Add(new Item(utf32(b64(xml.GetAttribute("n"))), utf32(b64(xml.GetAttribute("u")))));
            xml.Close();
            zip.Dispose();
            return new WL(items);
        }

        /// <summary>
        /// Read func for the original CWL-format<para />
        /// Name: CWishlist<para />
        /// File version 1 (not saved)<para />
        /// Format versions: 1 (not saved)
        /// </summary>
        static WL cwl_load(string file)
        {
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, UTF8);
            XmlReader xml = XmlReader.Create(zip.Entries[0].Open());
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "item")
                    items.Add(new Item(xml.GetAttribute("name"), xml.GetAttribute("url")));
            xml.Close();
            zip.Dispose();
            return new WL(items);
        }

        /// <summary>
        /// Write func for the CWLS-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        public static void write_recent(string file, string[] recents)
        {
            if (File.Exists(file))
                File.Delete(file);
            Stream fs = File.Open(file, Create, FileAccess.Write);
            fs.write(cwls_header);
            fs.write(1, 4);
            DeflateStream ds = new DeflateStream(fs, Optimal, false);
            foreach (string r in recents)
            {
                ds.write(bytes((ushort)r.Length));
                ds.write(utf16(r));
            }
            ds.Close();
        }

        /// <summary>
        /// Read func for the CWLS-format<para />
        /// Name: CWishlists<para />
        /// File version 1 (starting saving in 2, not checked)<para />
        /// Format versions: 1, 2, 3, 4 (saved, checked)
        /// </summary>
        public static string[] load_recent(string file)
        {
            int v = get_cwls_version(file);
            if (v > 4)
                throw new TooNewRecentsFileException();
            else if (v == 1)
            {
                ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, ASCII);
                XmlReader x = XmlReader.Create(zip.GetEntry("R").Open());
                List<string> r = new List<string>();
                while (x.Read())
                    if (x.Name == "r")
                        r.Add(utf32(b64(x.GetAttribute("f"))));
                x.Close();
                zip.Dispose();
                return r.ToArray();
            }
            else if (v == 2)
            {
                ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, ASCII);
                XmlReader x = XmlReader.Create(zip.GetEntry("R").Open());
                List<string> r = new List<string>();
                while (x.Read())
                    if (x.Name == "f")
                        r.Add(x.GetAttribute("f"));
                x.Close();
                zip.Dispose();
                return r.ToArray();
            }
            else if (v == 3)
            {
                ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, ASCII);
                List<string> r = new List<string>();
                Stream s = zip.GetEntry("R").Open();
                int i = -1;
                int j = -1;
                byte[] bfr = new byte[131070]; //ushort.MaxValue * 2 (128KiB)
                while ((i = s.ReadByte()) != -1)
                {
                    j = s.ReadByte();
                    int len = BitConverter.ToUInt16(BitConverter.IsLittleEndian ? new byte[] { (byte)i, (byte)j } : new byte[] { (byte)j, (byte)i }, 0);
                    s.Read(bfr, 0, len * 2);
                    r.Add(Unicode.GetString(bfr, 0, len * 2));
                }
                s.Close();
                zip.Dispose();
                return r.ToArray();
            }
            else
            {
                List<string> r = new List<string>();
                Stream rawfs = File.Open(file, Open, FileAccess.Read);
                rawfs.Seek(10, SeekOrigin.Begin);
                Stream s = new DeflateStream(rawfs, CompressionMode.Decompress, false);
                int i = -1;
                int j = -1;
                byte[] bfr = new byte[131070]; //ushort.MaxValue * 2 (128KiB)
                while ((i = s.ReadByte()) != -1)
                {
                    j = s.ReadByte();
                    int len = BitConverter.ToUInt16(BitConverter.IsLittleEndian ? new byte[] { (byte)i, (byte)j } : new byte[] { (byte)j, (byte)i }, 0);
                    s.Read(bfr, 0, len * 2);
                    r.Add(Unicode.GetString(bfr, 0, len * 2));
                }
                s.Close();
                return r.ToArray();
            }
        }

        static int get_cwls_version(string f)
        {
            Stream s = File.Open(f, Open, FileAccess.Read);
            int v = -1;
            if (s.ReadByte() == 80 && s.ReadByte() == 75)
            {
                s.Close();
                ZipArchive z = ZipFile.Open(f, ZipArchiveMode.Read, ASCII);
                v = z.read_entry_byte("V");
                z.Dispose();
            }
            else
            {
                s.Seek(9, SeekOrigin.Begin);
                v = s.ReadByte();
                s.Close();
            }
            return v;
        }

        static bool cose(this string s, byte o, char c) => s[s.Length - o] == c;
    }

    class InvalidHeaderException : Exception
    {
        public InvalidHeaderException(string format, byte[] expected, byte[] invalid) : this(format, expected.ToString(NumberFormat.HEX), invalid.ToString(NumberFormat.HEX)) { }

        public InvalidHeaderException(string format, string expected, string invalid) : base($"This {format}-File's header is not correct, it's expected to be {expected} by the standard, but it's {invalid}.") { }
    }

    class NotSupportedFileVersionException : Exception
    {
        public NotSupportedFileVersionException() : base("This CWishlist standard/version is not supported by this version of the program.") { }
    }

    class TooNewRecentsFileException : Exception
    {
        public TooNewRecentsFileException() : base("The recents-file saved in the AppData is too new for this version of the program, please update.") { }
    }
}
