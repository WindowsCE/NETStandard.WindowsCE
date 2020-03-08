using System;
using System.Globalization;

namespace System
{
    public static class Int322
    {
        public const int MaxValue = int.MaxValue;
        public const int MinValue = int.MinValue;

        public static int Parse(string s)
        {
            return Number.ParseInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static int Parse(string s, IFormatProvider provider)
        {
            return Number.ParseInt32(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        public static int Parse(string s, NumberStyles style)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Number.ParseInt32(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static int Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Number.ParseInt32(s, style, NumberFormatInfo.GetInstance(provider));
        }

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
