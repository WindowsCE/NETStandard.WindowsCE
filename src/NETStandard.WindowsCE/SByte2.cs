using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [CLSCompliant(false)]
    public static class SByte2
    {
        public const sbyte MaxValue = sbyte.MaxValue;
        public const sbyte MinValue = sbyte.MinValue;

        public static sbyte Parse(string s)
            => sbyte.Parse(s);

        public static sbyte Parse(string s, IFormatProvider provider)
            => sbyte.Parse(s, NumberStyles.Integer, provider);

        public static sbyte Parse(string s, NumberStyles style)
            => sbyte.Parse(s, style);

        public static sbyte Parse(string s, NumberStyles style, IFormatProvider provider)
            => sbyte.Parse(s, style, provider);

        public static bool TryParse(string s, out sbyte result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out sbyte result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out sbyte result)
        {
            result = 0;
            int i;
            if (!Number.TryParseInt32(s, style, info, out i))
            {
                return false;
            }

            if ((style & NumberStyles.AllowHexSpecifier) != 0)
            { // We are parsing a hexadecimal number
                if ((i < 0) || i > byte.MaxValue)
                {
                    return false;
                }
                result = (sbyte)i;
                return true;
            }

            if (i < MinValue || i > MaxValue)
            {
                return false;
            }
            result = (sbyte)i;
            return true;
        }
    }
}
