using System;
using System.IO;
using System.IO.Compression;
using static binutils.str;

namespace binutils
{
    public static class io
    {
        public static void add_entry(this ZipArchive zip, string entry_name, byte[] contents, CompressionLevel comp_lvl = CompressionLevel.Optimal)
        {
            Stream s = zip.CreateEntry(entry_name, comp_lvl).Open();
            s.write(contents);
            s.Close();
            s.Dispose();
        }

        public static void add_entry(this ZipArchive zip, string entry_name, byte content, CompressionLevel comp_lvl = CompressionLevel.Fastest)
        {
            Stream s = zip.CreateEntry(entry_name, comp_lvl).Open();
            s.write(content);
            s.Close();
            s.Dispose();
        }

        public static int read_entry_byte(this ZipArchive zip, string entry_name)
        {
            Stream s = zip.GetEntry(entry_name).Open();
            int b = s.ReadByte();
            s.Close();
            return b;
        }

        public static void write(this Stream s, params byte[] b)
        {
            s.Write(b, 0, b.Length);
        }

        public static void write_ascii(this Stream s, string t)
        {
            s.write(ascii(t));
        }

        public static void write_utf8(this Stream s, string t)
        {
            s.write(utf8(t));
        }

        public static void write_utf16(this Stream s, string t)
        {
            s.write(utf16(t));
        }

        public static void dbg(string fmt, params object[] arg)
        {
#if DEBUG
            Console.WriteLine(fmt, arg);
#endif
        }
    }
}
