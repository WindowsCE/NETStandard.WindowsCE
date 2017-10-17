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
        public const float MaxValue = float.MaxValue;
        public const float MinValue = float.MinValue;

        public const float Epsilon = float.Epsilon;
        public const float PositiveInfinity = float.PositiveInfinity;
        public const float NegativeInfinity = float.NegativeInfinity;
        public const float NaN = float.NaN;

        internal static float NegativeZero = BitConverter2.Int32BitsToSingle(unchecked((int)0x80000000));

        public static bool IsInfinity(float f)
            => float.IsInfinity(f);

        public static bool IsNaN(float f)
            => float.IsNaN(f);

        public static bool IsNegativeInfinity(float f)
            => float.IsNegativeInfinity(f);

        public static bool IsPositiveInfinity(float f)
            => float.IsPositiveInfinity(f);

        public static float Parse(string s)
        {
            return Number.ParseSingle(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo);
        }

        public static float Parse(string s, IFormatProvider provider)
        {
            return Number.ParseSingle(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance(provider));
        }

        public static float Parse(string s, NumberStyles style)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);
            return Number.ParseSingle(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static float Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);
            return Number.ParseSingle(s, style, NumberFormatInfo.GetInstance(provider));
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out float result)
        {
            NumberFormatInfo2.ValidateParseStyleFloatingPoint(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static bool TryParse(string s, out float result)
        {
            return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out float result)
        {
            if (s == null)
            {
                result = 0;
                return false;
            }
            bool success = Number.TryParseSingle(s, style, info, out result);
            if (!success)
            {
                string sTrim = s.Trim();
                if (sTrim.Equals(info.PositiveInfinitySymbol))
                {
                    result = float.PositiveInfinity;
                }
                else if (sTrim.Equals(info.NegativeInfinitySymbol))
                {
                    result = float.NegativeInfinity;
                }
                else if (sTrim.Equals(info.NaNSymbol))
                {
                    result = float.NaN;
                }
                else
                    return false; // We really failed
            }
            return true;
        }
    }
}
