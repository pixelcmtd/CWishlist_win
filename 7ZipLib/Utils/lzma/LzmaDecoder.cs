using System;
using System.IO;
using SevenZip.Utils.range;

namespace SevenZip.Utils.lzma
{
    public class Decoder : ICoder
	{
		class LenDecoder
		{
			BitDecoder m_Choice = new BitDecoder();
			BitDecoder m_Choice2 = new BitDecoder();
            readonly BitTreeDecoder[] m_LowCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
            readonly BitTreeDecoder[] m_MidCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
			BitTreeDecoder m_HighCoder = new BitTreeDecoder(Base.kNumHighLenBits);
			uint m_NumPosStates = 0;

			public void Create(uint numPosStates)
			{
				for (uint posState = m_NumPosStates; posState < numPosStates; posState++)
				{
					m_LowCoder[posState] = new BitTreeDecoder(Base.kNumLowLenBits);
					m_MidCoder[posState] = new BitTreeDecoder(Base.kNumMidLenBits);
				}
				m_NumPosStates = numPosStates;
			}

			public void Init()
			{
				m_Choice.Init();
				for (uint posState = 0; posState < m_NumPosStates; posState++)
				{
					m_LowCoder[posState].Init();
					m_MidCoder[posState].Init();
				}
				m_Choice2.Init();
				m_HighCoder.Init();
			}

			public uint Decode(range.Decoder rangeDecoder, uint posState)
			{
				if (m_Choice.Decode(rangeDecoder) == 0)
					return m_LowCoder[posState].Decode(rangeDecoder);
				else
				{
					uint symbol = Base.kNumLowLenSymbols;
					if (m_Choice2.Decode(rangeDecoder) == 0)
						symbol += m_MidCoder[posState].Decode(rangeDecoder);
					else symbol += Base.kNumMidLenSymbols + m_HighCoder.Decode(rangeDecoder);
					return symbol;
				}
			}
		}

		class LiteralDecoder
		{
			struct Decoder2
			{
				BitDecoder[] m_Decoders;
				public void Create() => m_Decoders = new BitDecoder[0x300];
				public void Init() { for (int i = 0; i < 0x300; i++) m_Decoders[i].Init(); }

				public byte DecodeNormal(range.Decoder rangeDecoder)
				{
					uint symbol = 1;
                    while (symbol < 0x100)
                        symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
					return (byte)symbol;
				}

				public byte DecodeWithMatchByte(range.Decoder rangeDecoder, byte matchByte)
				{
					uint symbol = 1;
                    while (symbol < 0x100)
                    {
						uint matchBit = (uint)(matchByte >> 7) & 1;
						matchByte <<= 1;
						uint bit = m_Decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
						symbol = (symbol << 1) | bit;
						if (matchBit != bit)
						{
							while (symbol < 0x100)
								symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
							break;
						}
					}
					return (byte)symbol;
				}
			}

			Decoder2[] m_Coders;
			int m_NumPrevBits;
			int m_NumPosBits;
			uint m_PosMask;

			public void Create(int numPosBits, int numPrevBits)
			{
				if (m_Coders != null && m_NumPrevBits == numPrevBits &&
					m_NumPosBits == numPosBits) return;
				m_NumPosBits = numPosBits;
				m_PosMask = ((uint)1 << numPosBits) - 1;
				m_NumPrevBits = numPrevBits;
				uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
				m_Coders = new Decoder2[numStates];
				for (uint i = 0; i < numStates; i++) m_Coders[i].Create();
			}

			public void Init()
			{
				uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
				for (uint i = 0; i < numStates; i++) m_Coders[i].Init();
			}

			uint GetState(uint pos, byte prevByte)
			=> ((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> (8 - m_NumPrevBits));

			public byte DecodeNormal(range.Decoder rangeDecoder, uint pos, byte prevByte)
			=> m_Coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder);

			public byte DecodeWithMatchByte(range.Decoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
			=> m_Coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte);
		}

		OutWindow m_OutWindow = new OutWindow();
		range.Decoder m_RangeDecoder = new range.Decoder();
        readonly BitDecoder[] m_IsMatchDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
        readonly BitDecoder[] m_IsRepDecoders = new BitDecoder[Base.kNumStates];
        readonly BitDecoder[] m_IsRepG0Decoders = new BitDecoder[Base.kNumStates];
        readonly BitDecoder[] m_IsRepG1Decoders = new BitDecoder[Base.kNumStates];
        readonly BitDecoder[] m_IsRepG2Decoders = new BitDecoder[Base.kNumStates];
        readonly BitDecoder[] m_IsRep0LongDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
        readonly BitTreeDecoder[] m_PosSlotDecoder = new BitTreeDecoder[Base.kNumLenToPosStates];
        readonly BitDecoder[] m_PosDecoders = new BitDecoder[Base.kNumFullDistances - Base.kEndPosModelIndex];

		BitTreeDecoder m_PosAlignDecoder = new BitTreeDecoder(Base.kNumAlignBits);

		LenDecoder m_LenDecoder = new LenDecoder();
		LenDecoder m_RepLenDecoder = new LenDecoder();

		LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

		uint m_DictionarySize;
		uint m_DictionarySizeCheck;

		uint m_PosStateMask;

		public Decoder()
		{
			m_DictionarySize = 0xFFFFFFFF;
			for (int i = 0; i < Base.kNumLenToPosStates; i++)
				m_PosSlotDecoder[i] = new BitTreeDecoder(Base.kNumPosSlotBits);
		}

		void SetDictionarySize(uint dictionarySize)
		{
			if (m_DictionarySize != dictionarySize)
			{
				m_DictionarySize = dictionarySize;
				m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1);
				uint blockSize = Math.Max(m_DictionarySizeCheck, 1 << 12);
				m_OutWindow.Create(blockSize);
			}
		}

		void SetPosBitsProperties(int pb)
		{
			uint numPosStates = (uint)1 << pb;
			m_LenDecoder.Create(numPosStates);
			m_RepLenDecoder.Create(numPosStates);
			m_PosStateMask = numPosStates - 1;
		}

		bool _solid = false;
		void Init(Stream inStream, Stream outStream)
		{
			m_RangeDecoder.Init(inStream);
			m_OutWindow.Init(outStream, _solid);

			uint i;
			for (i = 0; i < Base.kNumStates; i++)
			{
				for (uint j = 0; j <= m_PosStateMask; j++)
				{
					uint index = (i << Base.kNumPosStatesBitsMax) + j;
					m_IsMatchDecoders[index].Init();
					m_IsRep0LongDecoders[index].Init();
				}
				m_IsRepDecoders[i].Init();
				m_IsRepG0Decoders[i].Init();
				m_IsRepG1Decoders[i].Init();
				m_IsRepG2Decoders[i].Init();
			}

			m_LiteralDecoder.Init();
			for (i = 0; i < Base.kNumLenToPosStates; i++) m_PosSlotDecoder[i].Init();
			for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
				m_PosDecoders[i].Init();

			m_LenDecoder.Init();
			m_RepLenDecoder.Init();
			m_PosAlignDecoder.Init();
		}

		public void Code(Stream inStream, Stream outStream,
			long inSize, long outSize, ICodeProgress progress)
		{
			Init(inStream, outStream);

			Base.State state = new Base.State();
			state.Init();
			uint rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;

			ulong nowPos64 = 0, outSize64 = (ulong)outSize;
			if (nowPos64 < outSize64)
			{
				state.UpdateChar();
				byte b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, 0, 0);
				m_OutWindow.PutByte(b);
				nowPos64++;
			}
			while (nowPos64 < outSize64)
			{
				uint posState = (uint)nowPos64 & m_PosStateMask;
				if (m_IsMatchDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) == 0)
				{
					byte b;
					byte prevByte = m_OutWindow.GetByte(0);
					if (!state.IsCharState) b = m_LiteralDecoder.DecodeWithMatchByte(m_RangeDecoder,
                        (uint)nowPos64, prevByte, m_OutWindow.GetByte(rep0));
					else b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, (uint)nowPos64, prevByte);
					m_OutWindow.PutByte(b);
					state.UpdateChar();
					nowPos64++;
				}
				else
				{
					uint len;
					if (m_IsRepDecoders[state.Index].Decode(m_RangeDecoder) == 1)
					{
						if (m_IsRepG0Decoders[state.Index].Decode(m_RangeDecoder) == 0)
						{
							if (m_IsRep0LongDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) == 0)
							{
								state.UpdateShortRep();
								m_OutWindow.PutByte(m_OutWindow.GetByte(rep0));
								nowPos64++;
								continue;
							}
						}
						else
						{
                            uint distance;
							if (m_IsRepG1Decoders[state.Index].Decode(m_RangeDecoder) == 0)
								distance = rep1;
							else
							{
								if (m_IsRepG2Decoders[state.Index].Decode(m_RangeDecoder) == 0)
									distance = rep2;
								else
								{
									distance = rep3;
									rep3 = rep2;
								}
								rep2 = rep1;
							}
							rep1 = rep0;
							rep0 = distance;
						}
						len = m_RepLenDecoder.Decode(m_RangeDecoder, posState) + Base.kMatchMinLen;
						state.UpdateRep();
					}
					else
					{
						rep3 = rep2;
						rep2 = rep1;
						rep1 = rep0;
						len = Base.kMatchMinLen + m_LenDecoder.Decode(m_RangeDecoder, posState);
						state.UpdateMatch();
						uint posSlot = m_PosSlotDecoder[Base.GetLenToPosState(len)].Decode(m_RangeDecoder);
						if (posSlot >= Base.kStartPosModelIndex)
						{
							int numDirectBits = (int)((posSlot >> 1) - 1);
							rep0 = (2 | (posSlot & 1)) << numDirectBits;
							if (posSlot < Base.kEndPosModelIndex)
								rep0 += BitTreeDecoder.ReverseDecode(m_PosDecoders,
										rep0 - posSlot - 1, m_RangeDecoder, numDirectBits);
							else
							{
								rep0 += m_RangeDecoder.DecodeDirectBits(
									numDirectBits - Base.kNumAlignBits) << Base.kNumAlignBits;
								rep0 += m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
							}
						}
						else rep0 = posSlot;
					}
					if (rep0 >= m_OutWindow.TrainSize + nowPos64 ||
                           rep0 >= m_DictionarySizeCheck &&
                           rep0 == 0xFFFFFFFF)
                           break;
					m_OutWindow.CopyBlock(rep0, len);
					nowPos64 += len;
				}
			}
			m_OutWindow.Flush();
			m_OutWindow.ReleaseStream();
			m_RangeDecoder.ReleaseStream();
		}

		public void SetDecoderProperties(Stream s, uint dicSize)
		{
            int props = s.ReadByte();
			int lc = props % 9;
			int remainder = props / 9;
			int lp = remainder % 5;
			int pb = remainder / 5;
			SetDictionarySize(dicSize);
            m_LiteralDecoder.Create(lp, lc);
            SetPosBitsProperties(pb);
		}

		public bool Train(Stream stream)
		{
			_solid = true;
			return m_OutWindow.Train(stream);
		}
	}
}
