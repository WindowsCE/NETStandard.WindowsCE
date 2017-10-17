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
        public const Decimal Zero = decimal.Zero;
        public const Decimal One = decimal.One;
        public const Decimal MinusOne = decimal.MinusOne;
        public const Decimal MaxValue = decimal.MaxValue;
        public const Decimal MinValue = decimal.MinValue;

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
        {
            return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo);
        }

        public static decimal Parse(string s, IFormatProvider provider)
        {
            return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.GetInstance(provider));
        }

        public static decimal Parse(string s, NumberStyles style)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);
            return Number.ParseDecimal(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static decimal Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);
            return Number.ParseDecimal(s, style, NumberFormatInfo.GetInstance(provider));
        }

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
            if (s == null)
            {
                result = 0;
                return false;
            }

            return Number.TryParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out decimal result)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);

            if (s == null)
            {
                result = 0;
                return false;
            }

            return Number.TryParseDecimal(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }
    }
}
