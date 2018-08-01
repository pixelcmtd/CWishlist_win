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
            List<byte> u = new List<byte>(); //uncompressed
            foreach (Item i in wl)
            {
                u.AddRange(Encoding.Unicode.GetBytes(i.name));
                u.Add(10);
                u.Add(13);
                u.AddRange(Encoding.Unicode.GetBytes(i.url));
                u.Add(10);
                u.Add(13);
            }
            Stream s = File.Open(file, FileMode.Create, FileAccess.Write);

            s.write(cwld_header);
            s.write(4, 1);
            s.write(Deflate.compress(u.ToArray()));

            s.Close();
        }

        /// <summary>
        /// Read func for the CWLD-format<para />
        /// Name: CWishlistDeflate (A custom binary format compressed with Deflate)<para />
        /// File version: 4<para />
        /// Format version: 1
        /// </summary>
        public static WL cwld_load(string file)
        {
            byte[] raw = File.ReadAllBytes(file);

            byte[] h = new byte[8]; //header
            Array.Copy(raw, h, 8);

            byte[] c = new byte[raw.Length - 10]; //compressed
            Array.Copy(raw, 10, c, 0, c.Length);

            if (!h.arr_equal(cwld_header))
                throw new InvalidHeaderException("CWLD", cwld_header, h);
            if (raw[8] != 4 || raw[9] != 1)
                throw new NotSupportedFileVersionException();

            byte[] u = Deflate.decompress(c); //uncompressed
            List<Item> items = new List<Item>();
            string s = "";
            bool nus = false; //Name Url Switch
            Item itm = new Item();

            for (int i = 0; i < u.Length; i += 2)
                if (u[i] == 10 && u[i + 1] == 13) //10 is LF and 13 is CR
                {
                    if (nus)
                    {
                        itm.url = s;
                        items.Add(itm);
                        itm = new Item();
                    }
                    else
                        itm.name = s;
                    nus = !nus;
                    s = "";
                }
                else
                    s += to_unicode(u[i], u[i + 1]);

            return new WL(items.ToArray());
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

            return new WL(items.ToArray());
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
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.UTF8);
            XmlReader xml = XmlReader.Create(zip.Entries[0].Open());
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "item")
                    items.Add(new Item(xml.GetAttribute("name"), xml.GetAttribute("url")));
            xml.Close();
            zip.Dispose();
            return new WL(items.ToArray());
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
