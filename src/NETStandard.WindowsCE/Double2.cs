using System;
using System.Globalization;

namespace System
{
    public static class Double2
    {
        public const double MaxValue = double.MaxValue;
        public const double MinValue = double.MinValue;

        // Note Epsilon should be a double whose hex representation is 0x1
        // on little endian machines.
        public const double Epsilon = double.Epsilon;
        public const double NegativeInfinity = double.NegativeInfinity;
        public const double PositiveInfinity = double.PositiveInfinity;
        public const double NaN = double.NaN;

        internal static double NegativeZero = BitConverter2.Int64BitsToDouble(unchecked((long)0x8000000000000000));

        public static bool IsInfinity(double d)
            => double.IsInfinity(d);

        public static bool IsNaN(double d)
            => double.IsNaN(d);

        public static bool IsNegativeInfinity(double d)
            => double.IsNegativeInfinity(d);

        public static bool IsPositiveInfinity(double d)
            => double.IsPositiveInfinity(d);

        public static double Parse(string s)
        {
            return Number.ParseDouble(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo);
        }

        public static double Parse(string s, IFormatProvider provider)
        {
            return Number.ParseDouble(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance(provider));
        }

        public static double Parse(string s, NumberStyles style)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);
            return Number.ParseDouble(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static double Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);
            return Number.ParseDouble(s, style, NumberFormatInfo.GetInstance(provider));
        }

        public static bool TryParse(string s, out double result)
        {
            return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out double result)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out double result)
        {
            if (s == null)
            {
                result = 0;
                return false;
            }
            bool success = Number.TryParseDouble(s, style, info, out result);
            if (!success)
            {
                string sTrim = s.Trim();
                if (sTrim.Equals(info.PositiveInfinitySymbol))
                {
                    result = double.PositiveInfinity;
                }
                else if (sTrim.Equals(info.NegativeInfinitySymbol))
                {
                    result = double.NegativeInfinity;
                }
                else if (sTrim.Equals(info.NaNSymbol))
                {
                    result = double.NaN;
                }
                else
                    return false; // We really failed
            }
            return true;
        }
    }
}
