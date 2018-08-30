using SevenZip.Utils;
using SevenZip.Utils.lzma;
using System.IO;
using static CWishlist_win.CLinq;
using static SevenZip.Utils.CoderPropID;

namespace SevenZip
{
    public static class SevenZipHelper
    {
        static CoderPropID[] propIDs = { DictionarySize, PosStateBits, LitContextBits,
            LitPosBits, Algorithm, NumFastBytes, MatchFinder, EndMarker };
        static object[] properties = { 1 << 16, 2, 3, 0, 2, 128, "bt4", false };

        public static void Compress(Stream inStream, Stream outStream)
        {
            inStream.Seek(0, SeekOrigin.Begin);
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            outStream.Write(bytes(inStream.Length), 0, 8);
            encoder.Code(inStream, outStream, -1, -1, null);
        }

        public static void Decompress(Stream inStream, Stream outStream)
        {
            Decoder decoder = new Decoder();
            byte[] properties2 = new byte[5];
            inStream.Read(properties2, 0, 5);
            long outSize = 0;
            for (int i = 0; i < 8; i++)
                outSize |= (long)(byte)inStream.ReadByte() << (8 * i);
            decoder.SetDecoderProperties(properties2);
            long compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, compressedSize, outSize, null);
            outStream.Seek(0, SeekOrigin.Begin);
        }
    }
}