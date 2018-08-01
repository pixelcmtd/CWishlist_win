using System;

namespace CWishlist_win
{
    enum NumberFormat : int
    {
        BIN = 2,
        OCT = 8,
        DEC = 10,
        HEX = 16
    }

    static class NFUtils
    {
        /// <summary>
        /// Gets the number of digits this format at max needs to represent the bits.
        /// </summary>
        /// <param name="bits">8, 16, 24, 32, 48 or 64</param>
        public static int get_digit_count(this NumberFormat nf, int bits)
        {
            return nf == NumberFormat.BIN ? bits : nf == NumberFormat.HEX ? bits / 4 : nf == NumberFormat.OCT ? oct_digits(bits) : dec_digits(bits);
        }

        static int oct_digits(int bits)
        {
            return bits == 8 ? 3 : bits == 16 ? 6 : bits == 24 ? 8 : bits == 32 ? 11 : bits == 48 ? 16 : 22;
        }

        static int dec_digits(int bits)
        {
            return bits == 8 ? 3 : bits == 16 ? 5 : bits == 24 ? 8 : bits == 32 ? 10 : bits == 48 ? 14 : 19;
        }

        /// <summary>
        /// True if the format uses padding, false if not.
        /// </summary>
        public static bool padding(this NumberFormat nf)
        {
            return nf != NumberFormat.DEC;
        }

        public static string ToString(this sbyte i, NumberFormat nf)
        {
            if (nf.padding())
            {
                return Convert.ToString(i, (int)nf).PadLeft(nf.get_digit_count(8), '0');
            }
            else
            {
                return Convert.ToString(i, (int)nf);
            }
        }

        public static string ToString(this byte i, NumberFormat nf)
        {
            if (nf.padding())
            {
                return Convert.ToString(i, (int)nf).PadLeft(nf.get_digit_count(8), '0');
            }
            else
            {
                return Convert.ToString(i, (int)nf);
            }
        }

        public static string ToString(this short i, NumberFormat nf)
        {
            if (nf.padding())
            {
                return Convert.ToString(i, (int)nf).PadLeft(nf.get_digit_count(16), '0');
            }
            else
            {
                return Convert.ToString(i, (int)nf);
            }
        }

        public static string ToString(this ushort i, NumberFormat nf)
        {
            if (nf.padding())
            {
                return Convert.ToString(i, (int)nf).PadLeft(nf.get_digit_count(16), '0');
            }
            else
            {
                return Convert.ToString(i, (int)nf);
            }
        }

        public static string ToString(this int i, NumberFormat nf)
        {
            if(nf.padding())
            {
                return Convert.ToString(i, (int)nf).PadLeft(nf.get_digit_count(32), '0');
            }
            else
            {
                return Convert.ToString(i, (int)nf);
            }
        }

        public static string ToString(this uint i, NumberFormat nf)
        {
            if (nf.padding())
            {
                return Convert.ToString(i, (int)nf).PadLeft(nf.get_digit_count(32), '0');
            }
            else
            {
                return Convert.ToString(i, (int)nf);
            }
        }

        public static string ToString(this long i, NumberFormat nf)
        {
            if (nf.padding())
            {
                return Convert.ToString(i, (int)nf).PadLeft(nf.get_digit_count(64), '0');
            }
            else
            {
                return Convert.ToString(i, (int)nf);
            }
        }
    }
}
