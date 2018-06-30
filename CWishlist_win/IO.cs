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
        static byte[] cwlc_header { get; } = new byte[8] { 67, 87, 76, 67, 13, 10, 26, 10 }; //C W L C CR LF EOF LF

        delegate void wl_save(WL wl, string file);

        delegate WL wl_load(string file);

        public static string tinyurl_create(string url) => new WebClient().DownloadString("http://tinyurl.com/api-create.php?url=" + url);

        public static bool valid_url(string url) => Uri.TryCreate(url, UriKind.Absolute, out Uri u);

        public static WL load(string f) => f == "" ? WL.New : ((f.cose(4, '.') && f.cose(3, 'c') && f.cose(2, 'w') && f.cose(1, 'l')) ? cwl_load(f) : f.cose(1, 'b') ? cwlb_load(f) : cwlu_load(f));

        public static void experimental_save(WL wl, string file)
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

            s.write(cwlc_header);
            s.write(4, 1);
            s.write(c);

            s.Close();
            s.Dispose();
        }

        public static WL experimental_load(string file)
        {
            byte[] raw = File.ReadAllBytes(file);

            byte[] h = new byte[8];
            Array.Copy(raw, h, 8);

            byte[] c = new byte[raw.Length - 10];
            Array.Copy(raw, 10, c, 0, c.Length);

            if (!h.arr_equal(cwlc_header))
                throw new InvalidHeaderException("CWLC", cwlc_header, h);

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
                }
                else
                    str += Encoding.Unicode.GetChars(new byte[] { u[i], u[i + 1] })[0];

            return new WL(items.ToArray());
        }

        //Save func for the CWLU-format
        //For information on the format check the load/read func
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

        //Read func for the CWLU-format
        //Name: CWishlistUncde (UTF16/Unicode and no longer useless UTF32 in Base64)
        //File version: 3 (saved, checked)
        //Format versions: 1 (saved, checked)
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

        //Read func for the CWLB-format
        //Name: CWishlistBase[64] (64 is not included in the name, but Base64 is the reason for the name)
        //File version 2 (should be saved, not checked)
        //Format versions: 1 (should be saved, not checked)
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

        //Read func for the original CWL-format
        //Name: CWishlist
        //File version 1 (not saved)
        //Format versions: 1 (not saved)
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
                XmlReader xml = XmlReader.Create(s);
                List<string> rcwls = new List<string>();
                while (xml.Read())
                    if (xml.NodeType == XmlNodeType.Element && xml.Name == "r")
                        rcwls.Add(Encoding.UTF32.GetString(Convert.FromBase64String(xml.GetAttribute("f"))));
                xml.Close();
                xml.Dispose();
                zip.Dispose();
                return rcwls.ToArray();
            }
            else
            {
                s = zip.GetEntry("R").Open();
                XmlReader xml = XmlReader.Create(s);
                List<string> r = new List<string>();
                while (xml.Read())
                    if (xml.NodeType == XmlNodeType.Element && xml.Name == "f")
                        r.Add(xml.GetAttribute(0));
                xml.Close();
                xml.Dispose();
                zip.Dispose();
                return r.ToArray();
            }
        }

        static bool cose(this string s, byte o, char c) => s[s.Length - o] == c;

        static string xml_esc(this string s) => s.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;");

        static void add_entry(this ZipArchive zip, string entry_name, byte[] contents, CompressionLevel comp_lvl)
        {
            Stream s = zip.CreateEntry(entry_name, comp_lvl).Open();
            s.Write(contents, 0, contents.Length);
            s.Close();
            s.Dispose();
        }

        static void add_entry(this ZipArchive zip, string entry_name, byte content, CompressionLevel comp_lvl)
        {
            Stream s = zip.CreateEntry(entry_name, comp_lvl).Open();
            s.Write(new byte[] { content }, 0, 1);
            s.Close();
            s.Dispose();
        }

        static void write(this Stream s, params byte[] b) => s.Write(b, 0, b.Length);
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
