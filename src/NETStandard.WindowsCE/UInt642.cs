using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [CLSCompliant(false)]
    public static class UInt642
    {
        public const ulong MaxValue = ulong.MaxValue;
        public const ulong MinValue = ulong.MinValue;

        public static ulong Parse(string s)
            => ulong.Parse(s);

        public static ulong Parse(string s, IFormatProvider provider)
            => ulong.Parse(s, NumberStyles.Integer, provider);

        public static ulong Parse(string s, NumberStyles style)
            => ulong.Parse(s, style);

        public static ulong Parse(string s, NumberStyles style, IFormatProvider provider)
            => ulong.Parse(s, style, provider);

        public static bool TryParse(string s, out ulong result)
        {
            return Number.TryParseUInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ulong result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Number.TryParseUInt64(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }
    }
}
