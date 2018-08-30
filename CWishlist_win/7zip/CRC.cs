namespace SevenZip
{
	public static class CRC
	{
		public static readonly uint[] Table = new uint[256];

		static CRC()
		{
			for (uint i = 0; i < 256; i++)
			{
                Table[i] = i;
				for (int j = 0; j < 8; j++)
					if ((Table[i] & 1) != 0)
						Table[i] = (Table[i] >> 1) ^ 0xEDB88320;
					else
                        Table[i] = Table[i] >> 1;
			}
		}

		static uint _value = uint.MaxValue;

		public static void Reset()
        {
            _value = uint.MaxValue;
        }

		public static void UpdateByte(byte b)
		{
			_value = Table[((byte)_value ^ b)] ^ (_value >> 8);
		}

		public static void Update(byte[] data, uint offset, uint size)
		{
			for (uint i = 0; i < size; i++)
				_value = Table[(((byte)(_value)) ^ data[offset + i])] ^ (_value >> 8);
		}

		public static uint GetDigest() { return _value ^ uint.MaxValue; }

		public static uint CalculateDigest(byte[] data, uint offset, uint size)
		{
            Reset();
			Update(data, offset, size);
			return GetDigest();
		}

		public static bool VerifyDigest(uint digest, byte[] data, uint offset, uint size)
		{
            Reset();
            Update(data, offset, size);
			return GetDigest() == digest;
		}

        public static uint CalculateDigest(byte[] data)
        {
            Reset();
            Update(data, 0, (uint)data.LongLength);
            return GetDigest();
        }

        public static bool VerifyDigest(uint digest, byte[] data)
        {
            Reset();
            Update(data, 0, (uint)data.LongLength);
            return GetDigest() == digest;
        }
    }
}
