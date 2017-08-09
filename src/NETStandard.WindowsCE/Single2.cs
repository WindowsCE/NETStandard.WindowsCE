using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Single2
    {
        public static bool IsInfinity(float f)
            => float.IsInfinity(f);

        public static bool IsNaN(float f)
            => float.IsNaN(f);

        public static bool IsNegativeInfinity(float f)
            => float.IsNegativeInfinity(f);

        public static bool IsPositiveInfinity(float f)
            => float.IsPositiveInfinity(f);

        public static float Parse(string s)
            => float.Parse(s);

        public static float Parse(string s, IFormatProvider provider)
            => float.Parse(s, provider);

        public static float Parse(string s, NumberStyles style)
            => float.Parse(s, style);

        public static float Parse(string s, NumberStyles style, IFormatProvider provider)
            => float.Parse(s, style, provider);

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out float result)
        {
            bool retVal = false;
            try
            {
                result = float.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, out float result)
        {
            bool retVal = false;
            try
            {
                result = float.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
