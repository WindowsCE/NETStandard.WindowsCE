using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [CLSCompliant(false)]
    public static class UInt162
    {
        public static ushort Parse(string s)
            => ushort.Parse(s);

        public static ushort Parse(string s, IFormatProvider provider)
            => ushort.Parse(s, NumberStyles.Integer, provider);

        public static ushort Parse(string s, NumberStyles style)
            => ushort.Parse(s, style);

        public static ushort Parse(string s, NumberStyles style, IFormatProvider provider)
            => ushort.Parse(s, style, provider);

        public static bool TryParse(string s, out ushort result)
        {
            bool retVal = false;
            try
            {
                result = ushort.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ushort result)
        {
            bool retVal = false;
            try
            {
                result = ushort.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
