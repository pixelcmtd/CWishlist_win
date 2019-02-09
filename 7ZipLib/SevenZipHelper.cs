using SevenZip.Utils;
using SevenZip.Utils.lzma;
using System.IO;
using static binutils.io;

namespace SevenZip
{
    public static class SevenZipHelper
    {
        static byte[] bytes(long i)
        {
            return new byte[] { (byte)(i >> 56), (byte)(i >> 48), (byte)(i >> 40),
                (byte)(i >> 32), (byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8),
                (byte)i};
        }

        static long i64(byte[] b)
        {
            return (b[0] << 56) | (b[1] << 48) | (b[2] << 40) | (b[3] << 32) |
                   (b[4] << 24) | (b[5] << 16) | (b[6] << 8)  |  b[7];
        }

        const uint dicSize = 1 << 16;

        public static void Compress(Stream inStream, Stream outStream)
        {
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(dicSize, 2, 3, 0, 128, false);
            encoder.WriteCoderProperties(outStream);
            outStream.Write(bytes(inStream.Length), 0, 8);
            encoder.Code(inStream, outStream, -1, -1, null);
        }

        public static void Decompress(Stream inStream, Stream outStream)
        {
            Decoder decoder = new Decoder();
            decoder.SetDecoderProperties(inStream, dicSize);
            byte[] ros = new byte[8]; //raw outSize
            inStream.Read(ros, 0, 8);
            long outSize = i64(ros);
            long compressedSize = inStream.Length - inStream.Position;
            dbg("[LZMA]Decompressing {0} bytes to {1} bytes.", compressedSize, outSize);
            decoder.Code(inStream, outStream, compressedSize, outSize, null);
            dbg("[LZMA]Decompressed {0} bytes.", outStream.Length);
        }
    }
}