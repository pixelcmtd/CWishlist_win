using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System;
using System.Net;
using static CWishlist_win.CLinq;
using static CWishlist_win.Consts;
using static System.IO.FileMode;
using static System.Text.Encoding;
using static binutils.bin;
using static binutils.c;
using static binutils.io;
using static binutils.str;

namespace CWishlist_win
{
    static class IO
    {
        public static string tinyurl_create(string url)
        {
            return new WebClient().DownloadString(tinyurl_api + Uri.EscapeDataString(url));
        }

        public static string tinyurl_resolve(string tinyurl)
        {
            WebRequest req = WebRequest.Create(tinyurl);
            req.Method = WebRequestMethods.Http.Head;
            WebResponse resp = req.GetResponse();
            return resp.ResponseUri.ToString();
        }

        public static bool valid_url(string url)
        {
            string s = url.ToLower();
            return s.StartsWith(http) || s.StartsWith(https) && fccontains(s, '.');
        }

        /// <summary>
        /// Loads the given file with the format recognized from the extention.
        /// </summary>
        public static WL load(string f)
        {
            char c = f[f.Length - 1];
            return f == "" ? WL.NEW : c == 'd' ? cwld_load(f) : c == 'u' ? cwlu_load(f) : throw new
                Exception("Only CWLL, CWLD and CWLU files are supported by this version of CWL.");
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
            dbg("[CWLD]Saving file...");
            Stream s = File.Open(file, Create, FileAccess.Write);
            s.write(cwld_header);
            s.write(4, 3);
            dbg("[CWLD]Wrote header...");
            DeflateStream d = new DeflateStream(s, CompressionLevel.Optimal, false);
            foreach (Item i in wl)
            {
                i.write_bytes(d, L1);
                dbg("[CWLD]Wrote {0}...", i.dbgfmt());
            }
            d.Close();
            dbg("[CWLD]Saved file.");
        }

        /// <summary>
        /// Read func for the CWLD-format<para />
        /// Name: CWishlistDeflate (A custom binary format compressed with Deflate)<para />
        /// File version: 4 (saved, checked)<para />
        /// Format versions: 1, 2(saved, checked)
        /// </summary>
        public static WL cwld_load(string file)
        {
            dbg("[CWLD]Reading file...");
            Stream raw = File.Open(file, Open, FileAccess.Read);

            byte[] h = new byte[8]; //header
            raw.Read(h, 0, 8);
            int v = -2;

            if (!arrequ(h, cwld_header))
            {
                raw.Close();
                throw new InvalidHeaderException("CWLD", cwld_header, h);
            }
            if (raw.ReadByte() != 4 || (v = raw.ReadByte()) > 3 || v < 1)
            {
                raw.Close();
                throw new Exception(
                    $"This CWLD file is invalid.{(v != -2 ? " (v is " + v + ")" : "")})");
            }

            DeflateStream d = new DeflateStream(raw, CompressionMode.Decompress, false);
            List<Item> itms = new List<Item>();
            StringBuilder s = new StringBuilder();

            dbg($"[CWLD]Initialized, checked header, continuing with v{v}.");

            if (v == 1)
            {
                bool nus = false; //Name Url Switch
                Item i = new Item();
                char c;
                int j;

                while ((j = d.ReadByte()) != -1)
                    if ((c = utf16(j, d.ReadByte())) == '\u0d0a')
                    {
                        if (nus)
                        {
                            i.url = s.ToString();
                            itms.Add(i);
                            dbg("[CWLD]Read {0}...", i.dbgfmt());
                            i = new Item();
                        }
                        else i.name = s.ToString();
                        s.Clear();
                        nus = !nus;
                    }
                    else s.Append(c);
                d.Close();
            }
            else if (v == 2)
            {
                bool cs = false; //char switch
                bool nus = false; //name url switch
                bool tu = false; //tinyurl
                Item i = new Item();
                int j;
                byte b = 0;

                while((j = d.ReadByte()) != -1)
                    if (j == 11 && !cs)
                    {
                        tu = false;
                        if (!nus)
                        {
                            i.name = s.ToString();
                            nus = true;
                            tu = d.ReadByte() != 0;
                        }
                        else
                        {
                            i.url = s.ToString();
                            itms.Add(i);
                            dbg("[CWLD]Read {0}...", i.dbgfmt());
                            i = new Item();
                            nus = false;
                        }
                        s.Clear();
                        if (tu) s.Append("http://tinyurl.com/");
                    }
                    else if (tu) s.Append(ascii(j));
                    else
                    {
                        if (cs) s.Append(utf16(b, j));
                        else b = (byte)j;
                        cs = !cs;
                    }
            }
            else
            {
                int j;
                List<byte> bfr = new List<byte>();

                while ((j = d.ReadByte()) != -1)
                {
                    while (j != 11 && j != 8)
                    {
                        bfr.Add((byte)j);
                        j = d.ReadByte();
                    }
                    string name = utf8(bfr.ToArray());
                    bfr.Clear();
                    string url;
                    if (j == 11)
                    {
                        byte[] b = new byte[6];
                        d.Read(b, 0, 6);
                        url = tinyurl + b64(b);
                    }
                    else if (j == 8)
                    {
                        while ((j = d.ReadByte()) != 11) bfr.Add((byte)j);
                        url = utf8(bfr.ToArray());
                        if (!(url.StartsWith(http) || url.StartsWith(https)))
                            url = https + url;
                    }
                    else throw new Exception("CWLDv3 reading seems to be broken.");
                    Item itm = new Item(name, url);
                    dbg("[CWLD]Read {0}...", itm.dbgfmt());
                    itms.Add(itm);
                }
            }
            dbg("[CWLD]Read file.");
            return new WL(itms);
        }

        /// <summary>
        /// Read func for the CWLU-format<para />
        /// Name: CWishlistUncde (UTF16/Unicode and no longer useless UTF32 in Base64)<para />
        /// File version: 3 (saved, checked)<para />
        /// Format versions: 1 (saved, checked)
        /// </summary>
        static WL cwlu_load(string file)
        {
            dbg("[CWLU]Reading file...");
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, ASCII);
            if (zip.read_entry_byte("F") != 3 || zip.read_entry_byte("V") != 1)
                throw new Exception("Invalid CWLU file.");
            XmlReader xml = XmlReader.Create(new StreamReader(zip.GetEntry("W").Open(), Unicode));
            dbg("[CWLU]Initialized ZIP and XML.");
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "i")
                {
                    Item i = new Item(xml.GetAttribute("n"), xml.GetAttribute("u"));
                    items.Add(i);
                    dbg($"[CWLU]Read {i.dbgfmt()}");
                }
            xml.Close();
            zip.Dispose();
            WL wl = new WL(items);
            dbg("[CWLU]Finished.");
            return wl;
        }

        /// <summary>
        /// Write func for the CWLS-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        public static void write_recents(string file, IEnumerable<string> recents)
        {
            dbg("[CWLS]Writing file...");
            Stream fs = File.Open(file, Create, FileAccess.Write);
            fs.write(cwls_header);
            fs.WriteByte(5);
            dbg("[CWLS]Wrote header.");
            DeflateStream d = new DeflateStream(fs, CompressionLevel.Optimal, false);
            foreach (string r in recents)
            {
                d.write(utf8(r));
                d.WriteByte(11);
                dbg("[CWLS]Wrote \"" + r + "\".");
            }
            d.Close();
            dbg("[CWLS]Finished.");
        }

        /// <summary>
        /// Read func for the CWLS-format<para />
        /// Name: CWishlists<para />
        /// File version 1 (since v2 more or less saved (magic string CWLS))<para />
        /// Format versions: 1, 2, 3, 4, 5, 6 (saved, checked)
        /// </summary>
        public static List<string> load_recents(string file)
        {
            dbg("[CWLS]Reading file...");
            int v;
            Stream s = File.Open(file, Open, FileAccess.Read);
            if (s.ReadByte() == 80 && s.ReadByte() == 75)
            {
                s.Close();
                using ZipArchive z = ZipFile.Open(file, ZipArchiveMode.Read, ASCII);
                v = z.read_entry_byte("V");
            }
            else
            {
                s.Seek(0, SeekOrigin.Begin);
                if (readequals(s, 8, cwls4_header))
                {
                    s.Close();
                    v = 4;
                }
                else
                {
                    s.Seek(4, SeekOrigin.Begin);
                    v = s.ReadByte();
                    s.Close();
                }
            }
            dbg($"[CWLS]Got version {v}.");
            if (v > 5) throw new TooNewRecentsFileException();
            if (v < 4) throw new Exception($"CWLSv{v} is deprecated, it's no longer supported by CWL.");
            dbg($"[CWLS]Starting reading with version {v}.");
            List<string> r = new List<string>();
            if (v == 4)
            {
                Stream rawfs = File.Open(file, Open, FileAccess.Read);
                rawfs.Seek(10, SeekOrigin.Begin);
                s = new DeflateStream(rawfs, CompressionMode.Decompress, false);
                int i;
                byte[] bfr = new byte[131070]; //ushort.MaxValue * 2 (128KiB)
                while ((i = s.ReadByte()) != -1)
                {
                    int len = (i << 8) | s.ReadByte();
                    s.Read(bfr, 0, len * 2);
                    r.Add(utf16(bfr, len));
                }
                s.Close();
            }
            else
            {
                FileStream fs = File.Open(file, Open, FileAccess.Read);
                fs.Position = 5;
                DeflateStream d = new DeflateStream(fs, CompressionMode.Decompress, false);
                int i;
                List<byte> bfr = new List<byte>();
                while ((i = d.ReadByte()) != -1)
                    if (i != 11) bfr.Add((byte)i);
                    else
                    {
                        string t = utf8(bfr.ToArray());
                        dbg("[CWLS]Read {0}.", t);
                        r.Add(t);
                        bfr.Clear();
                    }
                d.Close();
            }
            return r;
        }

        static bool readequals(Stream stream, int len, byte[] arr)
        {
            if (len != arr.Length)
                return false;
            byte[] bfr = new byte[len];
            stream.Read(bfr, 0, len);
            return arrequ(bfr, arr);
        }
    }

    class InvalidHeaderException : Exception
    {
        public InvalidHeaderException(string format, byte[] expected, byte[] invalid) :
            this(format, hex(expected), hex(invalid)) { }

        public InvalidHeaderException(string format, string expected, string invalid) :
            base($"This {format}-File's header is not correct, it's expected to be {expected} by the standard, but it's {invalid}.") { }
    }

    class TooNewRecentsFileException : Exception
    {
        public TooNewRecentsFileException() :
            base("The recents-file saved in the AppData is too new for this version of the program, please update.") { }
    }
}
