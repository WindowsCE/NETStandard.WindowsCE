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
            bool retVal = false;
            try
            {
                result = short.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out short result)
        {
            bool retVal = false;
            try
            {
                result = short.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
