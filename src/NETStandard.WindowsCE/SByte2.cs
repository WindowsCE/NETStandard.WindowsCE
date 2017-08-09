using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [CLSCompliant(false)]
    public static class SByte2
    {
        public const sbyte MaxValue = sbyte.MaxValue;
        public const sbyte MinValue = sbyte.MinValue;

        public static sbyte Parse(string s)
            => sbyte.Parse(s);

        public static sbyte Parse(string s, IFormatProvider provider)
            => sbyte.Parse(s, NumberStyles.Integer, provider);

        public static sbyte Parse(string s, NumberStyles style)
            => sbyte.Parse(s, style);

        public static sbyte Parse(string s, NumberStyles style, IFormatProvider provider)
            => sbyte.Parse(s, style, provider);

        public static bool TryParse(string s, out sbyte result)
        {
            bool retVal = false;
            try
            {
                result = sbyte.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out sbyte result)
        {
            bool retVal = false;
            try
            {
                result = sbyte.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
