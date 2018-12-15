using SevenZip.Utils;
using SevenZip.Utils.lzma;
using System.IO;
using static SevenZip.Utils.CoderPropID;
using static System.IO.SeekOrigin;

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

        public static void Compress(Stream inStream, Stream outStream)
        {
            inStream.Seek(0, Begin);
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(new CoderPropID[] { DictionarySize,
                PosStateBits, LitContextBits, LitPosBits, Algorithm, NumFastBytes,
                MatchFinder, EndMarker }, new object[] { 1 << 16, 2, 3, 0, 2, 128,
                    "bt4", false });
            encoder.WriteCoderProperties(outStream);
            outStream.Write(bytes(inStream.Length), 0, 8);
            encoder.Code(inStream, outStream, -1, -1, null);
        }

        public static void Decompress(Stream inStream, Stream outStream)
        {
            Decoder decoder = new Decoder();
            byte[] props = new byte[5];
            inStream.Read(props, 0, 5);
            long outSize = 0;
            for (int i = 0; i < 8; i++)
                outSize |= (long)(byte)inStream.ReadByte() << (8 * i);
            decoder.SetDecoderProperties(props);
            long compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, compressedSize, outSize, null);
        }
    }
}