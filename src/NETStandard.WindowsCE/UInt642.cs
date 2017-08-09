using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [CLSCompliant(false)]
    public static class UInt642
    {
        public static ulong Parse(string s)
            => ulong.Parse(s);

        public static ulong Parse(string s, IFormatProvider provider)
            => ulong.Parse(s, NumberStyles.Integer, provider);

        public static ulong Parse(string s, NumberStyles style)
            => ulong.Parse(s, style);

        public static ulong Parse(string s, NumberStyles style, IFormatProvider provider)
            => ulong.Parse(s, style, provider);

        public static bool TryParse(string s, out ulong result)
        {
            bool retVal = false;
            try
            {
                result = ulong.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ulong result)
        {
            bool retVal = false;
            try
            {
                result = ulong.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
