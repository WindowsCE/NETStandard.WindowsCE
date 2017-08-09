using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Double2
    {
        public static bool IsInfinity(double d)
            => double.IsInfinity(d);

        public static bool IsNaN(double d)
            => double.IsNaN(d);

        public static bool IsNegativeInfinity(double d)
            => double.IsNegativeInfinity(d);

        public static bool IsPositiveInfinity(double d)
            => double.IsPositiveInfinity(d);

        public static double Parse(string s)
            => double.Parse(s);

        public static double Parse(string s, IFormatProvider provider)
            => double.Parse(s, provider);

        public static double Parse(string s, NumberStyles style)
            => double.Parse(s, style);

        public static double Parse(string s, NumberStyles style, IFormatProvider provider)
            => double.Parse(s, style, provider);

        public static bool TryParse(string s, out double result)
        {
            bool retVal = false;
            try
            {
                result = double.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out double result)
        {
            bool retVal = false;
            try
            {
                result = double.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }

            return retVal;
        }
    }
}
