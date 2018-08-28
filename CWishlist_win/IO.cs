using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System;
using System.Net;
using static CWishlist_win.CLinq;
using static System.BitConverter;
using static CWishlist_win.Consts;

namespace CWishlist_win
{
    static class IO
    {
        public static string tinyurl_create(string url) => new WebClient().DownloadString(tinyurl_api + url);

        public static bool valid_url(string url) => Uri.TryCreate(url, UriKind.Absolute, out Uri u);

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

        public static void cwll_save(WL wl, string file)
        {

        }

        public static WL cwll_load(string file)
        {
            SevenZip.Compression.LZMA.Decoder d = new SevenZip.Compression.LZMA.Decoder();
            FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read);
            byte[] bfr = new byte[8];
            fs.Read(bfr, 0, 8);
            if(!arrequ(bfr, cwll_header))
            {
                fs.Close();
                throw new InvalidHeaderException("CWLL", cwll_header, bfr);
            }
            int file_ver = fs.ReadByte();
            int format_ver = fs.ReadByte();
            if(file_ver != 5 || format_ver > 1)
            {
                fs.Close();
                throw new NotSupportedFileVersionException();
            }
            fs.Read(bfr, 0, 8);
            if (!IsLittleEndian)
                Array.Reverse(bfr);
            long len = ToInt64(bfr, 0);
            bfr = new byte[5];
            fs.Read(bfr, 0, 5);
            d.SetDecoderProperties(bfr);
            MemoryStream ms = new MemoryStream();
            d.Code(fs, ms, fs.Length, len, null);
            List<Item> items = new List<Item>();
            Item i = new Item();
            int j = -1;
            int k = -1;
            bool us = false;
            StringBuilder b = new StringBuilder();
            while ((j = ms.ReadByte()) != -1)
            {
                if (!us)
                {
                    if (j == cwll_is_tinyurl)
                    {
                        i.name = b.ToString();
                        b.Clear();
                        i.url = "http://tinyurl.com/" + b64(ms, 6);
                    }
                    else if (j == cwll_is_http || j == cwll_is_https || j == cwll_is_https_www || j == cwll_is_http_www || j == cwll_no_protocol)
                    {
                        i.name = b.ToString();
                        b.Clear();
                        b = new StringBuilder(j == cwll_is_http ? http : j == cwll_is_https ? https :
                            j == cwll_is_http_www ? http_www : j == cwll_is_https_www ? https_www : "");
                        while ((j = ms.ReadByte()) != -1)
                        {
                            if (j == cwll_item_end && !us)
                                break;
                            else if (us)
                                b.Append(to_unicode((byte)k, (byte)j));
                            else
                                k = j;
                            us = !us;
                        }
                        i.url = b.ToString();
                        b.Clear();
                    }
                    else
                        k = j;
                    if (i.url != null)
                    {
                        j = k = -1;
                        us = false;
                        items.Add(i);
                        i = new Item();
                    }
                    else
                        us = true;
                }
                else
                {
                    b.Append(to_unicode((byte)k, (byte)j));
                    us = false;
                }
            }
            ms.Close();
            fs.Close();
            return new WL(items);
        }

        /// <summary>
        /// Save func for the CWLD-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        public static void cwld_save(WL wl, string file)
        {
            Stream s = File.Open(file, FileMode.Create, FileAccess.Write);

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
            Stream raw = File.Open(file, FileMode.Open, FileAccess.Read);

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
                    if ((chr = to_unicode((byte)j, (byte)d.ReadByte())) == '\u0d0a')
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
                        s.Append(Encoding.ASCII.GetChars(new byte[] { (byte)j }));
                    else
                    {
                        if (cs)
                            s.Append(to_unicode(b, (byte)j));
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

            if (zip.read_entry_byte("F") > 3)
                throw new NotSupportedFileVersionException();
            if (zip.read_entry_byte("V") > 1)
                throw new NotSupportedFileVersionException();

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
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);
            XmlReader xml = XmlReader.Create(zip.GetEntry("W").Open());
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "i")
                    items.Add(new Item(Encoding.UTF32.GetString(Convert.FromBase64String(xml.GetAttribute("n"))), Encoding.UTF32.GetString(Convert.FromBase64String(xml.GetAttribute("u")))));
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
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.UTF8);
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
            Stream fs = File.Open(file, FileMode.Create, FileAccess.Write);
            fs.write(cwls_header);
            fs.write(1, 4);
            DeflateStream ds = new DeflateStream(fs, CompressionLevel.Optimal, false);
            foreach (string r in recents)
            {
                byte[] l = BitConverter.GetBytes((ushort)r.Length);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(l);
                ds.write(l);
                ds.write(Encoding.Unicode.GetBytes(r));
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
                ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);
                XmlReader x = XmlReader.Create(zip.GetEntry("R").Open());
                List<string> r = new List<string>();
                while (x.Read())
                    if (x.Name == "r")
                        r.Add(Encoding.UTF32.GetString(Convert.FromBase64String(x.GetAttribute("f"))));
                x.Close();
                zip.Dispose();
                return r.ToArray();
            }
            else if (v == 2)
            {
                ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);
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
                ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);
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
                    r.Add(Encoding.Unicode.GetString(bfr, 0, len * 2));
                }
                s.Close();
                zip.Dispose();
                return r.ToArray();
            }
            else
            {
                List<string> r = new List<string>();
                Stream rawfs = File.Open(file, FileMode.Open, FileAccess.Read);
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
                    r.Add(Encoding.Unicode.GetString(bfr, 0, len * 2));
                }
                s.Close();
                return r.ToArray();
            }
        }

        static int get_cwls_version(string f)
        {
            Stream s = File.Open(f, FileMode.Open, FileAccess.Read);
            int v = -1;
            if (s.ReadByte() == 80 && s.ReadByte() == 75)
            {
                s.Close();
                ZipArchive z = ZipFile.Open(f, ZipArchiveMode.Read, Encoding.ASCII);
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

        static char to_unicode(byte one, byte two) => Encoding.Unicode.GetChars(new byte[] { one, two })[0];
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
