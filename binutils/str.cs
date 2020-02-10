using System.Text;

namespace binutils
{
    public static class str
    {
        public static char ascii(int i)
        {
            return Encoding.ASCII.GetChars(new byte[] { (byte)i })[0];
        }

        public static char utf8(int i)
        {
            return Encoding.UTF8.GetChars(new byte[] { (byte)i })[0];
        }

        public static char utf16(int i, int j)
        {
            return Encoding.Unicode.GetChars(new byte[] { (byte)i, (byte)j })[0];
        }

        public static char utf32(int i, int j, int k, int l)
        {
            return Encoding.UTF32.GetChars(new byte[] { (byte)i, (byte)j, (byte)k, (byte)l })[0];
        }

        public static string ascii(byte[] b)
        {
            return Encoding.ASCII.GetString(b);
        }

        public static string utf8(byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }

        public static string utf16(byte[] b)
        {
            return Encoding.Unicode.GetString(b);
        }

        public static string utf32(byte[] b)
        {
            return Encoding.UTF32.GetString(b);
        }

        public static byte[] ascii(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        public static byte[] utf8(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static byte[] utf16(string s)
        {
            return Encoding.Unicode.GetBytes(s);
        }

        public static byte[] utf32(string s)
        {
            return Encoding.UTF32.GetBytes(s);
        }

        public static byte ascii(char c)
        {
            return Encoding.ASCII.GetBytes(new char[] { c })[0];
        }

        public static byte utf8(char c)
        {
            return Encoding.UTF8.GetBytes(new char[] { c })[0];
        }

        public static byte[] utf16(char c)
        {
            return Encoding.Unicode.GetBytes(new char[] { c });
        }

        public static byte[] utf32(char c)
        {
            return Encoding.UTF32.GetBytes(new char[] { c });
        }

        public static string ascii(byte[] b, int len)
        {
            return Encoding.ASCII.GetString(b, 0, len);
        }

        public static string utf8(byte[] b, int len)
        {
            return Encoding.UTF8.GetString(b, 0, len);
        }

        public static string utf16(byte[] b, int len)
        {
            return Encoding.Unicode.GetString(b, 0, len);
        }

        public static string utf32(byte[] b, int len)
        {
            return Encoding.UTF32.GetString(b, 0, len);
        }
    }
}
