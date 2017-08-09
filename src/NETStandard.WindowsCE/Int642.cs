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
            bool retVal = false;
            try
            {
                result = long.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out long result)
        {
            bool retVal = false;
            try
            {
                result = long.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
