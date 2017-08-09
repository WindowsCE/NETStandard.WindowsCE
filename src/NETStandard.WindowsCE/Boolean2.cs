using System;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Boolean2
    {
        public static readonly string FalseString = bool.FalseString;
        public static readonly string TrueString = bool.TrueString;

        public static bool Parse(string value)
            => bool.Parse(value);

        public static bool TryParse(string value, out bool result)
        {
            bool retVal = false;
            try
            {
                result = bool.Parse(value);
                retVal = true;
            }
            catch (FormatException) { result = false; }
            catch (InvalidCastException) { result = false; }

            return retVal;
        }
    }
}
