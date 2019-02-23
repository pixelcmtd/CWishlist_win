using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace LegacyFormats
{
    public class LegacyCwlFormats
    {
        //this piece of code is just too beautiful to delete xd
        public static unsafe byte[] bytes(short i)
        {
            short[] s = new short[] { i };
            byte[] b = new byte[2];
            fixed (short *t = s)
            {
                fixed (byte *c = b)
                {
                    Buffer.MemoryCopy(t, c, 2, 2);
                }
            }
            return b;
        }

        public static string[] load_cwls1(string file)
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

        public static string[] load_cwls2(string file)
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

        public static string[] load_cwls3(string file)
        {
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);
            List<string> r = new List<string>();
            Stream s = zip.GetEntry("R").Open();
            int i;
            byte[] bfr = new byte[131070]; //ushort.MaxValue * 2 (128KiB)
            while ((i = s.ReadByte()) != -1)
            {
                int len = (i << 8) | s.ReadByte();
                s.Read(bfr, 0, len * 2);
                r.Add(Encoding.Unicode.GetString(bfr, 0, len * 2));
            }
            s.Close();
            zip.Dispose();
            return r.ToArray();
        }

        static (string, string)[] cwl_load(string file)
        {
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read);
            XmlReader xml = XmlReader.Create(zip.Entries[0].Open());
            List<(string, string)> items = new List<(string, string)>();
            while (xml.Read())
                if (xml.Name == "item")
                    items.Add((xml.GetAttribute("name"), xml.GetAttribute("url")));
            xml.Close();
            zip.Dispose();
            return items.ToArray();
        }

        static (string, string)[] cwlb_load(string file)
        {
            ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.ASCII);
            XmlReader xml = XmlReader.Create(zip.GetEntry("W").Open());
            List<(string, string)> items = new List<(string, string)>();
            while (xml.Read())
                if (xml.Name == "i")
                    items.Add((Encoding.UTF32.GetString(Convert.FromBase64String(xml.GetAttribute("n"))),
                        Encoding.UTF32.GetString(Convert.FromBase64String(xml.GetAttribute("u")))));
            xml.Close();
            zip.Dispose();
            return items.ToArray();
        }

        /// <summary>
        /// Save func for the CWLL-format<para />
        /// For information on the format check the load/read func
        /// </summary>
        //public static void cwll_save(WL wl, string file)
        //{
        //    dbg("[CWLL]Saving file...");
        //    FileStream fs = File.Open(file, Create, FileAccess.Write);
        //    fs.write(cwll_header);
        //    fs.write(1);
        //    dbg("[CWLL]Wrote header...");
        //    MemoryStream ms = new MemoryStream();
        //    foreach (Item i in wl)
        //    {
        //        i.write_bytes(ms, L1);
        //        dbg("[CWLL]Wrote {0}...", i.dbgfmt());
        //    }
        //    ms.Seek(0, SeekOrigin.Begin);
        //    dbg("[CWLL]Compressing {0} bytes: {1}", ms.Length, hex(ms.ToArray()));
        //
        //    //Compress(ms, fs);
        //    ms.Close();
        //    fs.Close();
        //    dbg("[CWLL]Wrote file.");
        //}
        //
        ///// <summary>
        ///// Read func for the CWLL-format<para />
        ///// Name: CWishlistLZMA (LZMA compressed binary+UTF8 format)<para />
        ///// File version: 4 (not saved)<para />
        ///// Format versions: 1(saved, checked)
        ///// </summary>
        //public static WL cwll_load(string file)
        //{
        //    dbg("[CWLL]Loading file...");
        //    FileStream fs = File.Open(file, Open, FileAccess.Read);
        //    byte[] hdr = new byte[4];
        //    fs.Read(hdr, 0, 4);
        //    if (!arrequ(hdr, cwll_header))
        //    {
        //        fs.Close();
        //        throw new InvalidHeaderException("CWLL", cwll_header, hdr);
        //    }
        //    if (fs.ReadByte() != 1)
        //    {
        //        fs.Close();
        //        throw new Exception("This CWL version only supports v1 of the CWLL standard.");
        //    }
        //    dbg("[CWLL]Read header...");
        //    MemoryStream ms = new MemoryStream();
        //    //Decompress(fs, ms);
        //    fs.Close();
        //    List<Item> items = new List<Item>();
        //    int j;
        //    List<byte> bfr = new List<byte>();
        //    ms.Seek(0, SeekOrigin.Begin);
        //    dbg("[CWLL]Decompressed data is {0} bytes: {1}", ms.Length, hex(ms.ToArray()));
        //    while ((j = ms.ReadByte()) != -1)
        //    {
        //        while (j != 11 && j != 8)
        //        {
        //            bfr.Add((byte)j);
        //            j = ms.ReadByte();
        //        }
        //        string name = utf8(bfr.ToArray());
        //        string url;
        //        if (j == 11)
        //        {
        //            byte[] b = new byte[6];
        //            ms.Read(b, 0, 6);
        //            url = tinyurl + b64(b);
        //        }
        //        else if (j == 8)
        //        {
        //            bfr.Clear();
        //            while ((j = ms.ReadByte()) != 11) bfr.Add((byte)j);
        //            url = utf8(bfr.ToArray());
        //        }
        //        else throw new Exception("CWLL reading seems to be broken.");
        //        Item itm = new Item(name, url);
        //        dbg("[CWLL]Read {0}...", itm.dbgfmt());
        //        items.Add(itm);
        //    }
        //    dbg("[CWLL]Read file.");
        //    return new WL(items);
        //}
    }
}
