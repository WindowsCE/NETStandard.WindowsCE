using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Double2
    {
        public static bool IsInfinity(double d)
            => double.IsInfinity(d);

        public static bool IsNaN(double d)
            => double.IsNaN(d);

        public static bool IsNegativeInfinity(double d)
            => double.IsNegativeInfinity(d);

        public static bool IsPositiveInfinity(double d)
            => double.IsPositiveInfinity(d);

        public static double Parse(string s)
            => double.Parse(s);

        public static double Parse(string s, IFormatProvider provider)
            => double.Parse(s, provider);

        public static double Parse(string s, NumberStyles style)
            => double.Parse(s, style);

        public static double Parse(string s, NumberStyles style, IFormatProvider provider)
            => double.Parse(s, style, provider);

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
