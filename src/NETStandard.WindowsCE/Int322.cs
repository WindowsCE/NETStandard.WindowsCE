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
            bool retVal = false;
            try
            {
                result = int.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out int result)
        {
            bool retVal = false;
            try
            {
                result = int.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
