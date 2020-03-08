using System;
using System.Globalization;

namespace System
{
    [CLSCompliant(false)]
    public static class UInt162
    {
        public const ushort MaxValue = ushort.MaxValue;
        public const ushort MinValue = ushort.MinValue;

        public static ushort Parse(string s)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static ushort Parse(string s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        public static ushort Parse(string s, NumberStyles style)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static ushort Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }

        private static ushort Parse(String s, NumberStyles style, NumberFormatInfo info)
        {
            uint i = 0;
            try
            {
                i = Number.ParseUInt32(s, style, info);
            }
            catch (OverflowException e)
            {
                throw new OverflowException(SR.Overflow_UInt16, e);
            }

            if (i > MaxValue) throw new OverflowException(SR.Overflow_UInt16);
            return (ushort)i;
        }

        public static bool TryParse(string s, out ushort result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ushort result)
        {
            NumberFormatInfo2.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out ushort result)
        {
            result = 0;
            uint i;
            if (!Number.TryParseUInt32(s, style, info, out i))
            {
                return false;
            }
            if (i > ushort.MaxValue)
            {
                return false;
            }
            result = (ushort)i;
            return true;
        }
    }
}
