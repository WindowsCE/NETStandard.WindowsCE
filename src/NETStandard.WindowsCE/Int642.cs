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
            => long.Parse(s);

        public static long Parse(string s, IFormatProvider provider)
            => long.Parse(s, NumberStyles.Integer, provider);

        public static long Parse(string s, NumberStyles style)
            => long.Parse(s, style);

        public static long Parse(string s, NumberStyles style, IFormatProvider provider)
            => long.Parse(s, style, provider);

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
