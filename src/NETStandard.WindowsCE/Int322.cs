using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Int322
    {
        public static int Parse(string s)
            => int.Parse(s);

        public static int Parse(string s, IFormatProvider provider)
            => int.Parse(s, provider);

        public static int Parse(string s, NumberStyles style)
            => int.Parse(s, style);

        public static int Parse(string s, NumberStyles style, IFormatProvider provider)
            => int.Parse(s, style, provider);

        public static bool TryParse(string s, out int result)
        {
            if (s == null)
            {
                result = 0;
                return false;
            }

            return Number.TryParseInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out int result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);

            if (s == null)
            {
                result = 0;
                return false;
            }

            return Number.TryParseInt32(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }
    }
}
