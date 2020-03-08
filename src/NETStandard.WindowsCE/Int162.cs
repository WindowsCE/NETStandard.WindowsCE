using System;
using System.Globalization;

namespace System
{
    public static class Int162
    {
        public const short MaxValue = short.MaxValue;
        public const short MinValue = short.MinValue;

        public static short Parse(string s)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static short Parse(string s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        public static short Parse(string s, NumberStyles style)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static short Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }

        private static short Parse(String s, NumberStyles style, NumberFormatInfo info)
        {
            int i = 0;
            try
            {
                i = Number.ParseInt32(s, style, info);
            }
            catch (OverflowException e)
            {
                throw new OverflowException(SR.Overflow_Int16, e);
            }

            // We need this check here since we don't allow signs to specified in hex numbers. So we fixup the result
            // for negative numbers
            if ((style & NumberStyles.AllowHexSpecifier) != 0)
            { // We are parsing a hexadecimal number
                if ((i < 0) || (i > UInt16.MaxValue))
                {
                    throw new OverflowException(SR.Overflow_Int16);
                }
                return (short)i;
            }

            if (i < MinValue || i > MaxValue) throw new OverflowException(SR.Overflow_Int16);
            return (short)i;
        }

        public static bool TryParse(string s, out short result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out short result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out short result)
        {
            result = 0;
            int i;
            if (!Number.TryParseInt32(s, style, info, out i))
            {
                return false;
            }

            // We need this check here since we don't allow signs to specified in hex numbers. So we fixup the result
            // for negative numbers
            if ((style & NumberStyles.AllowHexSpecifier) != 0)
            { // We are parsing a hexadecimal number
                if ((i < 0) || i > ushort.MaxValue)
                {
                    return false;
                }
                result = (short)i;
                return true;
            }

            if (i < MinValue || i > MaxValue)
            {
                return false;
            }
            result = (short)i;
            return true;
        }
    }
}
