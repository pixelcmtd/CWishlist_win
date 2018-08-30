using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CWishlist_win
{
    static class CLinq
    {
        public static K where<K, V>(this Dictionary<K, V>.KeyCollection keys, Predicate<K> predicate)
        {
            foreach (K key in keys)
                if (predicate(key))
                    return key;
            return default;
        }

        public static V where<K, V>(this Dictionary<K, V>.ValueCollection vals, Predicate<V> predicate)
        {
            foreach (V val in vals)
                if (predicate(val))
                    return val;
            return default;
        }

        public static T where<T>(this IEnumerable<T> ie, Predicate<T> p)
        {
            foreach (T t in ie)
                if (p(t))
                    return t;
            return default;
        }

        public static IEnumerable<T> to_enumerable<T>(this IEnumerator<T> ie)
        {
            return new Enumerable<T>(ie);
        }

        public static T where<T>(this IEnumerator<T> ie, Predicate<T> p)
        {
            return ie.to_enumerable().where(p);
        }

        public static bool arrequ<T>(T[] left, T[] right)
        {
            if (left.Length != right.Length)
                return false;
            for (int i = 0; i < left.Length; i++)
                if (!left[i].Equals(right[i]))
                    return false;
            return true;
        }

        public static string ToString(this byte[] bytes, NumberFormat format)
        {
            string s = "";
            if(format != NumberFormat.DEC)
            {
                foreach (byte b in bytes)
                    s += b.ToString(format);
                return s;
            }
            else
            {
                foreach (byte b in bytes)
                    s += b.ToString(format) + ", ";
                return s.Substring(0, s.Length - 2);
            }
        }

        public static string xml_esc(this string s)
        {
            return s.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

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
            s.write(Encoding.ASCII.GetBytes(t));
        }

        public static void write_utf8(this Stream s, string t)
        {
            s.write(Encoding.UTF8.GetBytes(t));
        }

        public static void write_utf16(this Stream s, string t)
        {
            s.write(Encoding.Unicode.GetBytes(t));
        }

        public static string pad_left_if(this string s, bool b, int digits, char c)
        {
            return b ? s.PadLeft(digits, c) : s;
        }

        public static void add<T>(this List<T> l, params T[] ts)
        {
            l.AddRange(ts);
        }

        public static void add(this List<byte> l, params byte[] ts)
        {
            l.AddRange(ts);
        }

        public static byte[] b64(string s)
        {
            return Convert.FromBase64String(s);
        }

        public static string b64(byte[] b)
        {
            return Convert.ToBase64String(b);
        }

        public static string b64(Stream s, int bytelen)
        {
            byte[] b = new byte[bytelen];
            s.Read(b, 0, bytelen);
            return Convert.ToBase64String(b);
        }

        public static short int16(byte[] b)
        {
            return (short)((b[0] << 8) | b[1]);
        }

        public static ushort uint16(byte[] b)
        {
            return (ushort)((b[0] << 8) | b[1]);
        }

        public static int int32(byte[] b)
        {
            return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
        }

        public static uint uint32(byte[] b)
        {
            return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }

        public static long int64(byte[] b)
        {
            return ((long)b[0] << 56) | ((long)b[1] << 48) | ((long)b[2] << 40) | ((long)b[3] << 32)
                | ((long)b[4] << 24) | ((long)b[5] << 16) | ((long)b[6] << 8) | b[7];
        }

        public static ulong uint64(byte[] b)
        {
            return ((ulong)b[0] << 56) | ((ulong)b[1] << 48) | ((ulong)b[2] << 40) | ((ulong)b[3] << 32)
                | ((ulong)b[4] << 24) | ((ulong)b[5] << 16) | ((ulong)b[6] << 8) | b[7];
        }
        //this piece of code is just to beautiful to delete xd
        //public static unsafe byte[] bytes(short i)
        //{
        //    short[] s = new short[] { i };
        //    byte[] b = new byte[2];
        //    fixed(short *t = s)
        //    {
        //        fixed(byte *c = b)
        //        {
        //            Buffer.MemoryCopy(t, c, 2, 2);
        //        }
        //    }
        //    return b;
        //}

        public static byte[] bytes(short i)
        {
            return new byte[] { (byte)i, (byte)(i >> 8) };
        }

        public static byte[] bytes(ushort i)
        {
            return new byte[] { (byte)i, (byte)(i >> 8) };
        }

        public static byte[] bytes(int i)
        {
            return new byte[] { (byte)i, (byte)(i >> 8), (byte)(i >> 16), (byte)(i >> 24) };
        }

        public static byte[] bytes(uint i)
        {
            return new byte[] { (byte)i, (byte)(i >> 8), (byte)(i >> 16), (byte)(i >> 24) };
        }

        public static byte[] bytes(long i)
        {
            return new byte[] { (byte)i, (byte)(i >> 8), (byte)(i >> 16), (byte)(i >> 24),
            (byte)(i >> 32),(byte)(i >> 40),(byte)(i >> 48),(byte)(i >> 56)};
        }

        public static byte[] bytes(ulong i)
        {
            return new byte[] { (byte)i, (byte)(i >> 8), (byte)(i >> 16), (byte)(i >> 24),
            (byte)(i >> 32),(byte)(i >> 40),(byte)(i >> 48),(byte)(i >> 56)};
        }

        public static bool is_utf8_only(string s)
        {
            foreach (char c in s)
                if (Encoding.Unicode.GetBytes(new char[] { c })[0] != 0)
                    return false;
            return true;
        }
    }

    class NotSupportedNumberFormatException : Exception
    {
        public NotSupportedNumberFormatException(NumberFormat nf) : base($"The given NumberFormat {nf} is not supported.") { }
    }

    class Enumerable<T> : IEnumerable<T>
    {
        readonly IEnumerator<T> ie;

        public Enumerable(IEnumerator<T> ie)
        {
            this.ie = ie;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ie;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ie;
        }
    }
}
