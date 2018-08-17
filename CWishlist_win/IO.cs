using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System;
using System.Net;
using static CWishlist_win.CLinq;
using System.Windows.Forms;

namespace CWishlist_win
{
    static class IO
    {
        static byte[] cwld_header { get; } = new byte[8] { 67, 87, 76, 68, 13, 10, 26, 10 }; //C W L D CR LF EOF LF

        public static string tinyurl_create(string url) => new WebClient().DownloadString("http://tinyurl.com/api-create.php?url=" + url);

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
                        {
                            i.name = s.ToString();
                        }
                        s.Clear();
                        nus = !nus;
                    }
                    else
                    {
                        s.Append(chr);
                    }

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
                {
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
                    {
                        s.Append(Encoding.ASCII.GetChars(new byte[] { (byte)j }));
                    }
                    else if (!cs)
                    {
                        b = (byte)j;
                        cs = true;
                    }
                    else
                    {
                        s.Append(to_unicode(b, (byte)j));
                        cs = false;
                    }
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
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Create, Encoding.ASCII);
            zip.add_entry("V", 2);
            zip.add_entry("F", 1);
            BufferedStream s = new BufferedStream(zip.CreateEntry("R", CompressionLevel.Optimal).Open(), 1048576);
            s.write_utf8("<r>");
            foreach (string r in recents)
                s.write_utf8($"<f f=\"{r.xml_esc()}\" />");
            s.write_utf8("</r>");
            s.Flush();
            s.Close();
            zip.Dispose();
        }

        /// <summary>
        /// Read func for the CWLS-format<para />
        /// Name: CWishlists<para />
        /// File version 1 (originally not, but later saved)<para />
        /// Format versions: 1, 2
        /// </summary>
        public static string[] load_recent(string file)
        {
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);
            int v = zip.read_entry_byte("V");
            if (v > 2)
                throw new TooNewRecentsFileException();
            else if (v == 1)
            {
                XmlReader x = XmlReader.Create(zip.GetEntry("R").Open());
                List<string> r = new List<string>();
                while (x.Read())
                    if (x.Name == "r")
                        r.Add(Encoding.UTF32.GetString(Convert.FromBase64String(x.GetAttribute("f"))));
                x.Close();
                zip.Dispose();
                return r.ToArray();
            }
            else
            {
                XmlReader x = XmlReader.Create(zip.GetEntry("R").Open());
                List<string> r = new List<string>();
                while (x.Read())
                    if (x.Name == "f")
                        r.Add(x.GetAttribute("f"));
                x.Close();
                zip.Dispose();
                return r.ToArray();
            }
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
