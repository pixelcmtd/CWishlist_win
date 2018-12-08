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

        public static int[] parse_ints(this string[] strs)
        {
            int[] i = new int[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = int.Parse(strs[j]);
            return i;
        }

        public static uint[] parse_uints(this string[] strs)
        {
            uint[] i = new uint[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = uint.Parse(strs[j]);
            return i;
        }

        public static short[] parse_shorts(this string[] strs)
        {
            short[] i = new short[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = short.Parse(strs[j]);
            return i;
        }

        public static ushort[] parse_ushorts(this string[] strs)
        {
            ushort[] i = new ushort[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = ushort.Parse(strs[j]);
            return i;
        }

        public static long[] parse_longs(this string[] strs)
        {
            long[] i = new long[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = long.Parse(strs[j]);
            return i;
        }

        public static ulong[] parse_ulongs(this string[] strs)
        {
            ulong[] i = new ulong[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = ulong.Parse(strs[j]);
            return i;
        }

        public static byte[] parse_bytes(this string[] strs)
        {
            byte[] i = new byte[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = byte.Parse(strs[j]);
            return i;
        }

        public static sbyte[] parse_sbytes(this string[] strs)
        {
            sbyte[] i = new sbyte[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = sbyte.Parse(strs[j]);
            return i;
        }

        public static decimal[] parse_decimals(this string[] strs)
        {
            decimal[] i = new decimal[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = decimal.Parse(strs[j]);
            return i;
        }

        public static bool[] parse_bools(this string[] strs)
        {
            bool[] i = new bool[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = bool.Parse(strs[j]);
            return i;
        }

        public static char[] parse_chars(this string[] strs)
        {
            char[] i = new char[strs.Length];
            for (int j = 0; j < i.Length; j++)
                i[j] = char.Parse(strs[j]);
            return i;
        }
    }
}
