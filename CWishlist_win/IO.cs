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

namespace CWishlist_win
{
    static class IO
    {
        public static string tinyurl_create(string url)
        {
            return new WebClient().DownloadString(tinyurl_api + url);
        }

        //fastcharactercontains
        static bool fccontains(string s, char c)
        {
            foreach (char d in s)
                if (c == d)
                    return true;
            return false;
        }

        public static bool valid_url(string url)
        {
            string s = url.ToLower();
            return s.StartsWith(http) || s.StartsWith(https) || s.StartsWith(ftp) && fccontains(s, '.');
        }

        public static WL load(string f)
        {
            return f == "" ? WL.NEW : f.le('l') ? cwll_load(f) : f.le('d') ? cwld_load(f) : f.le('u') ? cwlu_load(f)
                : throw new Exception("Only CWLL, CWLD and CWLU files are supported by this version of CWL.");
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
        /// Save func for the CWLL-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        public static void cwll_save(WL wl, string file)
        {
            FileStream fs = File.Open(file, Create, FileAccess.Write);
            fs.Write(cwll_header, 0, 4);
            fs.write(1);
            MemoryStream ms = new MemoryStream();
            foreach (Item i in wl)
                i.write_bytes(ms, "L1");
            Compress(ms, fs);
            ms.Close();
            fs.Close();
        }

        /// <summary>
        /// Read func for the CWLL-format<para />
        /// Name: CWishlistLZMA (LZMA compressed binary+UTF8 format)<para />
        /// File version: 4 (not saved)<para />
        /// Format versions: 1(saved, checked)
        /// </summary>
        public static WL cwll_load(string file)
        {
            FileStream fs = File.Open(file, Open, FileAccess.Read);
            byte[] hdr = new byte[4];
            fs.Read(hdr, 0, 4);
            if (!arrequ(hdr, cwll_header))
            {
                fs.Close();
                throw new InvalidHeaderException("CWLL", cwll_header, hdr);
            }
            if (fs.ReadByte() != 1)
            {
                fs.Close();
                throw new Exception("This CWL version only supports v1 of the CWLL standard.");
            }
            MemoryStream ms = new MemoryStream();
            Decompress(fs, ms);
            fs.Close();
            List<Item> items = new List<Item>();
            int j;
            List<byte> bfr = new List<byte>();
            while ((j = ms.ReadByte()) != -1)
            {
                while (j != 11 && j != 8)
                {
                    bfr.Add((byte)j);
                    j = ms.ReadByte();
                }
                string name = utf8(bfr.ToArray());
                string url;
                if (j == 11)
                {
                    byte[] b = new byte[6];
                    ms.Read(b, 0, 6);
                    url = tinyurl + b64(b);
                }
                else if (j == 8)
                {
                    bfr.Clear();
                    while ((j = ms.ReadByte()) != 11)
                        bfr.Add((byte)j);
                    url = utf8(bfr.ToArray());
                }
                else
                    throw new Exception("CWLL reading seems to be broken.");
                items.Add(new Item(name, url));
            }
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
                throw new Exception("This CWLD file is invalid.");
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
                            tu = d.ReadByte() != 0;
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
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, ASCII);
            if (zip.read_entry_byte("F") != 3 || zip.read_entry_byte("V") != 1)
                throw new Exception("Invalid CWLU file.");
            XmlReader xml = XmlReader.Create(new StreamReader(zip.GetEntry("W").Open(), Unicode));
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "i")
                    items.Add(new Item(xml.GetAttribute("n"), xml.GetAttribute("u")));
            xml.Close();
            zip.Dispose();
            return new WL(items);
        }

        /// <summary>
        /// Write func for the CWLS-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        public static void write_recents(string file, IEnumerable<string> recents)
        {
#if DEBUG
            Console.WriteLine("[CWLS]Writing file...");
#endif
            Stream fs = File.Open(file, Create, FileAccess.Write);
            fs.write(cwls_header);
            fs.WriteByte(6);
#if DEBUG
            Console.WriteLine("[CWLS]Wrote header.");
#endif
            MemoryStream ms = new MemoryStream();
            foreach (string r in recents)
            {
                ms.write(utf8(r));
                ms.WriteByte(11);
#if DEBUG
                Console.WriteLine("[CWLS]Wrote \"" + r + "\".");
#endif
            }
            ms.Position = 0;
#if DEBUG
            Console.WriteLine("[CWLS]Compressing to file...");
#endif
            Compress(ms, fs);
#if DEBUG
            Console.WriteLine("[CWLS]Compressed to file.");
#endif
            fs.Close();
#if DEBUG
            Console.WriteLine("[CWLS]Finished.");
#endif
        }

        /// <summary>
        /// Read func for the CWLS-format<para />
        /// Name: CWishlists<para />
        /// File version 1 (since v2 more or less saved (magic string CWLS))<para />
        /// Format versions: 1, 2, 3, 4, 5, 6 (saved, checked)
        /// </summary>
        public static List<string> load_recents(string file)
        {
            int v = get_cwls_version(file);
            if (v > 6)
                throw new TooNewRecentsFileException();
            else if (v < 4)
                throw new Exception($"CWLSv{v} is deprecated, it's no longer supported by CWL.");
            else if (v == 4)
            {
                List<string> r = new List<string>();
                Stream rawfs = File.Open(file, Open, FileAccess.Read);
                rawfs.Seek(10, SeekOrigin.Begin);
                Stream s = new DeflateStream(rawfs, CompressionMode.Decompress, false);
                int i;
                byte[] bfr = new byte[131070]; //ushort.MaxValue * 2 (128KiB)
                while ((i = s.ReadByte()) != -1)
                {
                    int len = (i << 8) | s.ReadByte();
                    s.Read(bfr, 0, len * 2);
                    r.Add(utf16(bfr, len));
                }
                s.Close();
                return r;
            }
            else if (v == 5)
            {
                List<string> r = new List<string>();
                FileStream fs = File.Open(file, Open, FileAccess.Read);
                fs.Seek(5, SeekOrigin.Begin);
                MemoryStream ms = new MemoryStream();
                Decompress(fs, ms);
                int i;
                StringBuilder b = new StringBuilder();
                while ((i = ms.ReadByte()) != -1)
                {
                    b.Clear();
                    bool u8 = i != 0;
                    if (u8)
                        while ((i = ms.ReadByte()) != 8 && i != -1)
                            b.Append(utf8(i));
                    else
                        while ((i = ms.ReadByte()) != 0xe5 && i != -1)
                            b.Append(utf16(i, ms.ReadByte()));
                    r.Add(b.ToString());
                }
                return r;
            }
            else
            {
                List<string> r = new List<string>();
                FileStream fs = File.Open(file, Open, FileAccess.Read);
                fs.Seek(5, SeekOrigin.Begin);
                MemoryStream ms = new MemoryStream();
                Decompress(fs, ms);
                fs.Close();
                ms.Position = 0;
                int i;
                List<byte> bfr = new List<byte>();
                while ((i = ms.ReadByte()) != -1)
                {
                    if (i != 11)
                        bfr.Add((byte)i);
                    else
                    {
                        r.Add(utf8(bfr.ToArray()));
                        bfr.Clear();
                    }
                }
                ms.Dispose();
                return r;
            }
        }

        static int get_cwls_version(string f)
        {
            Stream s = File.Open(f, Open, FileAccess.Read);
            if (s.ReadByte() == 80 && s.ReadByte() == 75)
            {
                s.Close();
                using (ZipArchive z = ZipFile.Open(f, ZipArchiveMode.Read, ASCII))
                    return z.read_entry_byte("V");
            }
            else if (readequals(s, 8, cwls4_header))
            {
                s.Close();
                return 4;
            }
            else
            {
                s.Seek(4, SeekOrigin.Begin);
                int v = s.ReadByte();
                s.Close();
                return v;
            }
        }

        static bool readequals(Stream stream, int len, byte[] arr)
        {
            if (len != arr.Length)
                return false;
            byte[] bfr = new byte[len];
            stream.Read(bfr, 0, len);
            return arrequ(bfr, arr);
        }

        static bool le(this string s, char c) => s[s.Length - 1] == c;
    }

    class InvalidHeaderException : Exception
    {
        public InvalidHeaderException(string format, byte[] expected, byte[] invalid) :
            this(format, expected.ToHexString(), invalid.ToString()) { }

        public InvalidHeaderException(string format, string expected, string invalid) :
            base($"This {format}-File's header is not correct, it's expected to be {expected} by the standard, but it's {invalid}.") { }
    }

    class TooNewRecentsFileException : Exception
    {
        public TooNewRecentsFileException() :
            base("The recents-file saved in the AppData is too new for this version of the program, please update.") { }
    }
}
