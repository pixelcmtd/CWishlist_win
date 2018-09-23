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
        //this piece of code is just to beautiful to delete xd
        public static unsafe byte[] bytes(short i)
        {
            short[] s = new short[] { i };
            byte[] b = new byte[2];
            fixed (short* t = s)
            {
                fixed (byte* c = b)
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
    }
}
