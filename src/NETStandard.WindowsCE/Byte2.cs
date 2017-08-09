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
            bool retVal = false;
            try
            {
                result = byte.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out byte result)
        {
            bool retVal = false;
            try
            {
                result = byte.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
