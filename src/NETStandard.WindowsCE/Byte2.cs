using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Byte2
    {
        public const byte MaxValue = byte.MaxValue;
        public const byte MinValue = byte.MinValue;

        public static byte Parse(string s)
            => byte.Parse(s);

        public static byte Parse(string s, IFormatProvider provider)
            => byte.Parse(s, NumberStyles.Integer, provider);

        public static byte Parse(string s, NumberStyles style)
            => byte.Parse(s, style);

        public static byte Parse(string s, NumberStyles style, IFormatProvider provider)
            => byte.Parse(s, style, provider);

        public static bool TryParse(string s, out byte result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out byte result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out byte result)
        {
            result = 0;
            int i;
            if (!Number.TryParseInt32(s, style, info, out i))
            {
                return false;
            }
            if (i < MinValue || i > MaxValue)
            {
                return false;
            }
            result = (byte)i;
            return true;
        }
    }
}
