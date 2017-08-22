using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Int162
    {
        public static short Parse(string s)
            => short.Parse(s);

        public static short Parse(string s, IFormatProvider provider)
            => short.Parse(s, NumberStyles.Integer, provider);

        public static short Parse(string s, NumberStyles style)
            => short.Parse(s, style);

        public static short Parse(string s, NumberStyles style, IFormatProvider provider)
            => short.Parse(s, style, provider);

        public static bool TryParse(string s, out short result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out short result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out short result)
        {
            result = 0;
            int i;
            if (!Number.TryParseInt32(s, style, info, out i))
            {
                return false;
            }

            // We need this check here since we don't allow signs to specified in hex numbers. So we fixup the result
            // for negative numbers
            if ((style & NumberStyles.AllowHexSpecifier) != 0)
            { // We are parsing a hexadecimal number
                if ((i < 0) || i > ushort.MaxValue)
                {
                    return false;
                }
                result = (short)i;
                return true;
            }

            if (i < short.MinValue || i > short.MaxValue)
            {
                return false;
            }
            result = (short)i;
            return true;
        }
    }
}
