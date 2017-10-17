using System;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Boolean2
    {
        //
        // Internal Constants are real consts for performance.
        //

        // The internal string representation of true.
        // 
        internal const String TrueLiteral = "True";

        // The internal string representation of false.
        // 
        internal const String FalseLiteral = "False";

        public static readonly string FalseString = FalseLiteral;
        public static readonly string TrueString = TrueLiteral;

        public static bool Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Boolean result = false;
            if (!TryParse(value, out result))
            {
                throw new FormatException(SR.Format_BadBoolean);
            }
            else
            {
                return result;
            }
        }

        public static bool TryParse(string value, out bool result)
        {
            result = false;
            if (value == null)
            {
                return false;
            }
            // For perf reasons, let's first see if they're equal, then do the
            // trim to get rid of white space, and check again.
            if (TrueLiteral.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }
            if (FalseLiteral.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }

            // Special case: Trim whitespace as well as null characters.
            value = TrimWhiteSpaceAndNull(value);

            if (TrueLiteral.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }

            if (FalseLiteral.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }

            return false;
        }

        private static String TrimWhiteSpaceAndNull(String value)
        {
            int start = 0;
            int end = value.Length - 1;
            char nullChar = (char)0x0000;

            while (start < value.Length)
            {
                if (!Char.IsWhiteSpace(value[start]) && value[start] != nullChar)
                {
                    break;
                }
                start++;
            }

            while (end >= start)
            {
                if (!Char.IsWhiteSpace(value[end]) && value[end] != nullChar)
                {
                    break;
                }
                end--;
            }

            return value.Substring(start, end - start + 1);
        }
    }
}
