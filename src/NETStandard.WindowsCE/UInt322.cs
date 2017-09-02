using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [CLSCompliant(false)]
    public static class UInt322
    {
        public const uint MaxValue = uint.MaxValue;
        public const uint MinValue = uint.MinValue;

        public static uint Parse(string s)
            => uint.Parse(s);

        public static uint Parse(string s, IFormatProvider provider)
            => uint.Parse(s, NumberStyles.Integer, provider);

        public static uint Parse(string s, NumberStyles style)
            => uint.Parse(s, style);

        public static uint Parse(string s, NumberStyles style, IFormatProvider provider)
            => uint.Parse(s, style, provider);

        public static bool TryParse(string s, out uint result)
        {
            return Number.TryParseUInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out uint result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Number.TryParseUInt32(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }
    }
}
