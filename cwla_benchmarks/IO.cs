using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System;
using static System.IO.FileMode;
using static System.Text.Encoding;

namespace CWishlist_win
{
    static class IO
    {
        public static WL load(string f)
        {
            return f == "" ? WL.NEW : f.le('d') ? cwld_load(f) : f.le('u') ? cwlu_load(f)
                : throw new Exception("Only CWLD and CWLU files are supported by this version of CWL.");
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
                    if ((chr = Unicode.GetChars(new byte[] { (byte)j, (byte)d.ReadByte() })[0]) == '\u0d0a')
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
                        s.Append(ASCII.GetString(new byte[] { (byte)j }));
                    else
                    {
                        if (cs)
                            s.Append(Unicode.GetString(new byte[] { b, (byte)j }));
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
            XmlReader xml = XmlReader.Create(new StreamReader(zip.GetEntry("W").Open(), Unicode));
            List<Item> items = new List<Item>();
            while (xml.Read())
                if (xml.Name == "i")
                    items.Add(new Item(xml.GetAttribute("n"), xml.GetAttribute("u")));
            xml.Close();
            zip.Dispose();
            return new WL(items);
        }

        static bool le(this string s, char c) => s[s.Length - 1] == c;
    }
}
