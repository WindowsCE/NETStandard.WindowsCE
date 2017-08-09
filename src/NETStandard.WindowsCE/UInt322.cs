using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [CLSCompliant(false)]
    public static class UInt322
    {
        public static uint Parse(string s)
            => uint.Parse(s);

        public static uint Parse(string s, IFormatProvider provider)
            => uint.Parse(s, NumberStyles.Integer, provider);

        public static uint Parse(string s, NumberStyles style)
            => uint.Parse(s, style);

        public static uint Parse(string s, NumberStyles style, IFormatProvider provider)
            => uint.Parse(s, style, provider);

        public static bool TryParse(string s, out uint result)
        {
            bool retVal = false;
            try
            {
                result = uint.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out uint result)
        {
            bool retVal = false;
            try
            {
                result = uint.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
