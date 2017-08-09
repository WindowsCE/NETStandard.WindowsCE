using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Decimal2
    {
        public static decimal Add(decimal d1, decimal d2)
            => decimal.Add(d1, d2);

        public static decimal Ceiling(decimal d)
            => -(decimal.Floor(-d));

        public static int Compare(decimal d1, decimal d2)
            => decimal.Compare(d1, d2);

        public static decimal Divide(decimal d1, decimal d2)
            => decimal.Divide(d1, d2);

        public static bool Equals(decimal d1, decimal d2)
            => decimal.Equals(d1, d2);

        public static decimal Floor(decimal d)
            => decimal.Floor(d);

        public static int[] GetBits(decimal d)
            => decimal.GetBits(d);

        public static decimal Multiply(decimal d1, decimal d2)
            => decimal.Multiply(d1, d2);

        public static decimal Negate(decimal d)
            => decimal.Negate(d);

        public static decimal Parse(string s)
            => decimal.Parse(s);

        public static decimal Parse(string s, IFormatProvider provider)
            => decimal.Parse(s, provider);

        public static decimal Parse(string s, NumberStyles style)
            => decimal.Parse(s, style);

        public static decimal Parse(string s, NumberStyles style, IFormatProvider provider)
            => decimal.Parse(s, style, provider);

        public static decimal Remainder(decimal d1, decimal d2)
            => decimal.Remainder(d1, d2);

        public static decimal Subtract(decimal d1, decimal d2)
            => decimal.Subtract(d1, d2);

        public static byte ToByte(decimal value)
            => decimal.ToByte(value);

        public static double ToDouble(decimal d)
            => decimal.ToDouble(d);

        public static short ToInt16(decimal value)
            => decimal.ToInt16(value);

        public static int ToInt32(decimal d)
            => decimal.ToInt32(d);

        public static long ToInt64(decimal d)
            => decimal.ToInt64(d);

        [CLSCompliant(false)]
        public static sbyte ToSByte(decimal value)
            => decimal.ToSByte(value);

        public static float ToSingle(decimal d)
            => decimal.ToSingle(d);

        [CLSCompliant(false)]
        public static ushort ToUInt16(decimal value)
            => decimal.ToUInt16(value);

        [CLSCompliant(false)]
        public static uint ToUInt32(decimal d)
            => decimal.ToUInt32(d);

        [CLSCompliant(false)]
        public static ulong ToUInt64(decimal d)
            => decimal.ToUInt64(d);

        public static decimal Truncate(decimal d)
            => decimal.Truncate(d);

        public static bool TryParse(string s, out decimal result)
        {
            bool retVal = false;
            try
            {
                result = decimal.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out decimal result)
        {
            bool retVal = false;
            try
            {
                result = decimal.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
