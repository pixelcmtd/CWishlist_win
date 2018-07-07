using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System;
using System.Net;

namespace CWishlist_win
{
    static class IO
    {
        static byte[] cwld_header { get; } = new byte[8] { 67, 87, 76, 68, 13, 10, 26, 10 }; //C W L D CR LF EOF LF

        delegate void wl_save(WL wl, string file);

        delegate WL wl_load(string file);

        public static string tinyurl_create(string url) => new WebClient().DownloadString("http://tinyurl.com/api-create.php?url=" + url);

        public static bool valid_url(string url) => Uri.TryCreate(url, UriKind.Absolute, out Uri u);

        public static WL load(string f) => f == "" ? WL.NEW : ((f.cose(4, '.') && f.cose(3, 'c') && f.cose(2, 'w') && f.cose(1, 'l')) ? cwl_load(f) : f.cose(1, 'b') ? cwlb_load(f) : cwlu_load(f));

        public static void cwld_save(WL wl, string file)
        {
            List<byte> u = new List<byte>();

            foreach (Item i in wl)
            {
                u.AddRange(Encoding.Unicode.GetBytes(i.name));
                u.Add(10);
                u.Add(13);
                u.AddRange(Encoding.Unicode.GetBytes(i.url));
                u.Add(10);
                u.Add(13);
            }

            byte[] c = Deflate.compress(u.ToArray());
            Stream s = File.Open(file, FileMode.Create, FileAccess.Write);

            s.write(cwld_header);
            s.write(4, 1);
            s.write(c);

            s.Close();
            s.Dispose();
        }

        public static WL cwld_load(string file)
        {
            byte[] raw = File.ReadAllBytes(file);

            byte[] h = new byte[8];
            Array.Copy(raw, h, 8);

            byte[] c = new byte[raw.Length - 10];
            Array.Copy(raw, 10, c, 0, c.Length);

            if (!h.arr_equal(cwld_header))
                throw new InvalidHeaderException("CWLD", cwld_header, h);

            if (raw[8] != 4 || raw[9] != 1)
                throw new NotSupportedFileVersionException();

            byte[] u = Deflate.decompress(c);
            //MessageBox.Show(u.ToString(NumberFormat.HEX));
            List<Item> items = new List<Item>();
            string str = "";
            bool nus = false;
            Item itm = new Item();

            for (int i = 0; i < u.Length; i += 2)
                if (u[i] == 10 && u[i + 1] == 13) //10 is LF and 13 is CR
                {
                    if (nus)
                    {
                        itm.url = str;
                        items.Add(itm);
                        itm = new Item();
                    }
                    else
                        itm.name = str;
                    nus = !nus;
                    str = "";
                }
                else
                    str += to_unicode(u[i], u[i + 1]);

            return new WL(items.ToArray());
        }

        /// <summary>
        /// Save func for the CWLU-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        public static void cwlu_save(WL wl, string file)
        {
            string xml = "<c>";

            foreach (Item i in wl)
                xml += string.Format($"<i n=\"{i.name.xml_esc()}\" u=\"{i.url.xml_esc()}\" />");

            xml += "</c>";

            if (File.Exists(file))
                File.Delete(file);

            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Create, Encoding.ASCII);

            zip.add_entry("V", 1, CompressionLevel.Fastest);
            zip.add_entry("F", 3, CompressionLevel.Fastest);
            zip.add_entry("W", Encoding.Unicode.GetBytes(xml), CompressionLevel.Optimal);

            zip.Dispose();
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

            Stream s = zip.GetEntry("F").Open();
            if (s.ReadByte() > 3)
                throw new NotSupportedFileVersionException();
            s.Close();

            s = zip.GetEntry("V").Open();
            if (s.ReadByte() > 1)
                throw new NotSupportedFileVersionException();
            s.Close();

            XmlReader xml = XmlReader.Create(new StreamReader(zip.GetEntry("W").Open(), Encoding.Unicode));
            List<Item> itms = new List<Item>();

            while (xml.Read())
                if (xml.Name == "i")
                    itms.Add(new Item(xml.GetAttribute("n"), xml.GetAttribute("u")));

            xml.Close();
            xml.Dispose();
            zip.Dispose();

            return new WL(itms.ToArray());
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
            Stream s = zip.GetEntry("W").Open();
            XmlReader xml = XmlReader.Create(s);
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "i")
                    items.Add(new Item(Encoding.UTF32.GetString(Convert.FromBase64String(xml.GetAttribute("n"))), Encoding.UTF32.GetString(Convert.FromBase64String(xml.GetAttribute("u")))));
            xml.Close();
            xml.Dispose();
            s.Close();
            s.Dispose();
            zip.Dispose();
            return new WL(items.ToArray());
        }

        /// <summary>
        /// Read func for the original CWL-format<para />
        /// Name: CWishlist<para />
        /// File version 1 (not saved)<para />
        /// Format versions: 1 (not saved)
        /// </summary>
        static WL cwl_load(string file)
        {
            string tmp = Path.GetTempFileName();
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.UTF8);
            Stream s = zip.Entries[0].Open();
            XmlReader xml = XmlReader.Create(s);
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "item")
                    items.Add(new Item(xml.GetAttribute(0), xml.GetAttribute(1)));
            xml.Close();
            xml.Dispose();
            s.Close();
            s.Dispose();
            zip.Dispose();
            return new WL(items.ToArray());
        }

        /// <summary>
        /// Write func for the CWLS-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        public static void write_recent(string file, string[] recents)
        {
            string xml = "<r>";
            foreach (string r in recents)
                xml += string.Format("<f f=\"{0}\" />", r.xml_esc());
            xml += "</r>";
            if (File.Exists(file))
                File.Delete(file);
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Create, Encoding.ASCII);
            Stream s = zip.CreateEntry("V", CompressionLevel.Fastest).Open();
            s.WriteByte(2);
            s.Close();
            s = zip.CreateEntry("R", CompressionLevel.Optimal).Open();
            s.Write(Encoding.UTF8.GetBytes(xml), 0, xml.Length);
            s.Close();
            zip.Dispose();
        }

        /// <summary>
        /// Read func for the CWLS-format<para />
        /// Name: CWishlists<para />
        /// File version not set yet (not saved)<para />
        /// Format versions: 1, 2
        /// </summary>
        public static string[] load_recent(string file)
        {
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);
            Stream s = zip.GetEntry("V").Open();
            int v = s.ReadByte();
            s.Close();
            if (v > 2)
                throw new TooNewRecentsFileException();
            else if(v == 1)
            {
                s = zip.GetEntry("R").Open();
                XmlReader x = XmlReader.Create(s);
                List<string> r = new List<string>();
                while (x.Read())
                    if (x.Name == "r")
                        r.Add(Encoding.UTF32.GetString(Convert.FromBase64String(x.GetAttribute("f"))));
                x.Close();
                x.Dispose();
                zip.Dispose();
                return r.ToArray();
            }
            else
            {
                s = zip.GetEntry("R").Open();
                XmlReader x = XmlReader.Create(s);
                List<string> r = new List<string>();
                while (x.Read())
                    if (x.Name == "f")
                        r.Add(x.GetAttribute(0));
                x.Close();
                x.Dispose();
                zip.Dispose();
                return r.ToArray();
            }
        }

        static bool cose(this string s, byte o, char c) => s[s.Length - o] == c;

        static string xml_esc(this string s) => s.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;");

        static void add_entry(this ZipArchive zip, string entry_name, byte[] contents, CompressionLevel comp_lvl = CompressionLevel.Optimal)
        {
            Stream s = zip.CreateEntry(entry_name, comp_lvl).Open();
            s.write(contents);
            s.Close();
            s.Dispose();
        }

        static void add_entry(this ZipArchive zip, string entry_name, byte content, CompressionLevel comp_lvl = CompressionLevel.Fastest)
        {
            Stream s = zip.CreateEntry(entry_name, comp_lvl).Open();
            s.write(content);
            s.Close();
            s.Dispose();
        }

        static void write(this Stream s, params byte[] b) => s.Write(b, 0, b.Length);

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
