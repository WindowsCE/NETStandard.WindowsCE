using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [CLSCompliant(false)]
    public static class UInt162
    {
        public const ushort MaxValue = ushort.MaxValue;
        public const ushort MinValue = ushort.MinValue;

        public static ushort Parse(string s)
            => ushort.Parse(s);

        public static ushort Parse(string s, IFormatProvider provider)
            => ushort.Parse(s, NumberStyles.Integer, provider);

        public static ushort Parse(string s, NumberStyles style)
            => ushort.Parse(s, style);

        public static ushort Parse(string s, NumberStyles style, IFormatProvider provider)
            => ushort.Parse(s, style, provider);

        public static bool TryParse(string s, out ushort result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ushort result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out ushort result)
        {
            result = 0;
            uint i;
            if (!Number.TryParseUInt32(s, style, info, out i))
            {
                return false;
            }
            if (i > ushort.MaxValue)
            {
                return false;
            }
            result = (ushort)i;
            return true;
        }
    }
}
