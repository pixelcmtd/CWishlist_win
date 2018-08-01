using System.IO;
using System.IO.Compression;

namespace CWishlist_win
{
    static class Deflate
    {
        public static byte[] compress(byte[] b)
        {
            MemoryStream s = new MemoryStream(b, false);
            MemoryStream d = new MemoryStream();
            DeflateStream ds = new DeflateStream(d, CompressionMode.Compress);
            s.CopyTo(ds, 32767); //short.MaxValue
            ds.Close();
            s.Close();
            d.Close();
            return d.ToArray();
        }

        public static byte[] decompress(byte[] b)
        {
            MemoryStream s = new MemoryStream(b, false);
            MemoryStream d = new MemoryStream();
            DeflateStream ds = new DeflateStream(s, CompressionMode.Decompress);
            ds.CopyTo(d, 32767); //short.MaxValue
            ds.Close();
            s.Close();
            d.Close();
            return d.ToArray();
        }
    }
}
