using System;
using System.IO;
using System.IO.Compression;

namespace binutils
{
    public static class io
    {
        public delegate byte[] encoding_s2b(string s);

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

        public static void write(this Stream s, string t, encoding_s2b encoding)
        {
            s.write(encoding(t));
        }

        public static void dbg(string fmt, params object[] args)
        {
#if DEBUG
            if(fmt == null) dbg("[dbg()]fmt is null");
            else Console.WriteLine(DateTime.Now + fmt, args);
#endif
        }

        // for this to work you have to run the debug build in cmd
        public static void dbg(string fmt, ConsoleColor fg, params object[] arg)
        {
#if DEBUG
            ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = fg;
            dbg(fmt, arg);
            Console.ForegroundColor = c;
#endif
        }
    }
}
