using System;
using System.IO;

namespace SevenZip.Utils
{
	public interface ICodeProgress
	{
		void SetProgress(long inSize, long outSize);
	};

	public interface ICoder
	{
		void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodeProgress progress);
	};

	public enum CoderPropID
	{
		DefaultProp = 0,
		DictionarySize,
		UsedMemorySize,
		Order,
		BlockSize,
		PosStateBits,
		LitContextBits,
		LitPosBits,
		NumFastBytes,
		MatchFinder,
		MatchFinderCycles,
		NumPasses,
		Algorithm,
		NumThreads,
		EndMarker
	};


	public interface ISetCoderProperties
	{
		void SetCoderProperties(CoderPropID[] propIDs, object[] properties);
	};

	public interface IWriteCoderProperties
	{
		void WriteCoderProperties(System.IO.Stream outStream);
	}

	public interface ISetDecoderProperties
	{
		void SetDecoderProperties(byte[] properties);
	}
}
