using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Int642
    {
        public const long MaxValue = long.MaxValue;
        public const long MinValue = long.MinValue;

        public static long Parse(string s)
        {
            return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static long Parse(string s, IFormatProvider provider)
        {
            return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        public static long Parse(string s, NumberStyles style)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Number.ParseInt64(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static long Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Number.ParseInt64(s, style, NumberFormatInfo.GetInstance(provider));
        }

        public static bool TryParse(string s, out long result)
        {
            return Number.TryParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out long result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Number.TryParseInt64(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }
    }
}
