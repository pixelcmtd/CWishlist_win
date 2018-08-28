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
    }

    class NotSupportedNumberFormatException : Exception
    {
        public NotSupportedNumberFormatException(NumberFormat nf) : base($"The given NumberFormat {nf} is not supported.") { }
    }

    class Enumerable<T> : IEnumerable<T>
    {
        IEnumerator<T> ie;

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
