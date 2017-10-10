// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Refs:
// - https://github.com/dotnet/coreclr/blob/54891e0650e69f08832f75a40dc102efc6115d38/src/mscorlib/src/System/Number.cs

using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System
{
    internal static partial class Number
    {
        private static bool IsWhite(char ch)
        {
            return (((ch) == 0x20) || ((ch) >= 0x09 && (ch) <= 0x0D));
        }

        private unsafe static char* MatchChars(char* p, string str)
        {
            fixed (char* stringPointer = str)
            {
                return MatchChars(p, stringPointer);
            }
        }

        private unsafe static char* MatchChars(char* p, char* str)
        {
            Debug.Assert(p != null && str != null, "");

            if (*str == '\0')
            {
                return null;
            }
            // We only hurt the failure case
            // This fix is for French or Kazakh cultures. Since a user cannot type 0xA0 as a
            // space character we use 0x20 space character instead to mean the same.
            while (*p == *str || (*str == '\u00a0' && *p == '\u0020'))
            {
                p++;
                str++;
                if (*str == '\0') return p;
            }
            return null;
        }

        private unsafe static bool NumberToInt32(ref NumberBuffer number, ref int value)
        {
            int i = number.scale;
            if (i > Int32Precision || i < number.precision)
            {
                return false;
            }
            char* p = number.digits;
            Debug.Assert(p != null, "");
            int n = 0;
            while (--i >= 0)
            {
                if ((uint)n > (0x7FFFFFFF / 10))
                {
                    return false;
                }
                n *= 10;
                if (*p != '\0')
                {
                    n += (int)(*p++ - '0');
                }
            }
            if (number.sign)
            {
                n = -n;
                if (n > 0)
                {
                    return false;
                }
            }
            else
            {
                if (n < 0)
                {
                    return false;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool NumberToInt64(ref NumberBuffer number, ref long value)
        {
            Int32 i = number.scale;
            if (i > Int64Precision || i < number.precision)
            {
                return false;
            }
            char* p = number.digits;
            Debug.Assert(p != null, "");
            Int64 n = 0;
            while (--i >= 0)
            {
                if ((UInt64)n > (0x7FFFFFFFFFFFFFFF / 10))
                {
                    return false;
                }
                n *= 10;
                if (*p != '\0')
                {
                    n += (Int32)(*p++ - '0');
                }
            }
            if (number.sign)
            {
                n = -n;
                if (n > 0)
                {
                    return false;
                }
            }
            else
            {
                if (n < 0)
                {
                    return false;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool NumberToUInt32(ref NumberBuffer number, ref uint value)
        {
            Int32 i = number.scale;
            if (i > UInt32Precision || i < number.precision || number.sign)
            {
                return false;
            }
            char* p = number.digits;
            Debug.Assert(p != null, "");
            UInt32 n = 0;
            while (--i >= 0)
            {
                if (n > (0xFFFFFFFF / 10))
                {
                    return false;
                }
                n *= 10;
                if (*p != '\0')
                {
                    UInt32 newN = n + (UInt32)(*p++ - '0');
                    // Detect an overflow here...
                    if (newN < n)
                    {
                        return false;
                    }
                    n = newN;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool NumberToUInt64(ref NumberBuffer number, ref ulong value)
        {
            Int32 i = number.scale;
            if (i > UInt64Precision || i < number.precision || number.sign)
            {
                return false;
            }
            char* p = number.digits;
            Debug.Assert(p != null, "");
            UInt64 n = 0;
            while (--i >= 0)
            {
                if (n > (0xFFFFFFFFFFFFFFFF / 10))
                {
                    return false;
                }
                n *= 10;
                if (*p != '\0')
                {
                    UInt64 newN = n + (UInt64)(*p++ - '0');
                    // Detect an overflow here...
                    if (newN < n)
                    {
                        return false;
                    }
                    n = newN;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool ParseNumber(ref char* str, NumberStyles options, ref NumberBuffer number, StringBuilder sb, NumberFormatInfo numfmt, bool parseDecimal)
        {
            const Int32 StateSign = 0x0001;
            const Int32 StateParens = 0x0002;
            const Int32 StateDigits = 0x0004;
            const Int32 StateNonZero = 0x0008;
            const Int32 StateDecimal = 0x0010;
            const Int32 StateCurrency = 0x0020;

            number.scale = 0;
            number.sign = false;
            string decSep;                  // decimal separator from NumberFormatInfo.
            string groupSep;                // group separator from NumberFormatInfo.
            string currSymbol = null;       // currency symbol from NumberFormatInfo.

            Boolean parsingCurrency = false;
            if ((options & NumberStyles.AllowCurrencySymbol) != 0)
            {
                currSymbol = numfmt.CurrencySymbol;

                // The idea here is to match the currency separators and on failure match the number separators to keep the perf of VB's IsNumeric fast.
                // The values of decSep are setup to use the correct relevant separator (currency in the if part and decimal in the else part).
                decSep = numfmt.CurrencyDecimalSeparator;
                groupSep = numfmt.CurrencyGroupSeparator;
                parsingCurrency = true;
            }
            else
            {
                decSep = numfmt.NumberDecimalSeparator;
                groupSep = numfmt.NumberGroupSeparator;
            }

            int state = 0;
            bool bigNumber = (sb != null); // When a StringBuilder is provided then we use it in place of the number.digits char[50]
            int maxParseDigits = bigNumber ? int.MaxValue : NumberMaxDigits;

            char* p = str;
            char ch = *p;
            char* next;

            while (true)
            {
                // Eat whitespace unless we've found a sign which isn't followed by a currency symbol.
                // "-Kr 1231.47" is legal but "- 1231.47" is not.
                if (!IsWhite(ch) || (options & NumberStyles.AllowLeadingWhite) == 0 || ((state & StateSign) != 0 && ((state & StateCurrency) == 0 && numfmt.NumberNegativePattern != 2)))
                {
                    if ((((options & NumberStyles.AllowLeadingSign) != 0) && (state & StateSign) == 0) && ((next = MatchChars(p, numfmt.PositiveSign)) != null || ((next = MatchChars(p, numfmt.NegativeSign)) != null && (number.sign = true))))
                    {
                        state |= StateSign;
                        p = next - 1;
                    }
                    else if (ch == '(' && ((options & NumberStyles.AllowParentheses) != 0) && ((state & StateSign) == 0))
                    {
                        state |= StateSign | StateParens;
                        number.sign = true;
                    }
                    else if (currSymbol != null && (next = MatchChars(p, currSymbol)) != null)
                    {
                        state |= StateCurrency;
                        currSymbol = null;
                        // We already found the currency symbol. There should not be more currency symbols. Set
                        // currSymbol to NULL so that we won't search it again in the later code path.
                        p = next - 1;
                    }
                    else
                    {
                        break;
                    }
                }
                ch = *++p;
            }
            Int32 digCount = 0;
            Int32 digEnd = 0;
            while (true)
            {
                if ((ch >= '0' && ch <= '9') || (((options & NumberStyles.AllowHexSpecifier) != 0) && ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F'))))
                {
                    state |= StateDigits;

                    if (ch != '0' || (state & StateNonZero) != 0 || (bigNumber && ((options & NumberStyles.AllowHexSpecifier) != 0)))
                    {
                        if (digCount < maxParseDigits)
                        {
                            if (bigNumber)
                                sb.Append(ch);
                            else
                                number.digits[digCount++] = ch;
                            if (ch != '0' || parseDecimal)
                            {
                                digEnd = digCount;
                            }
                        }
                        if ((state & StateDecimal) == 0)
                        {
                            number.scale++;
                        }
                        state |= StateNonZero;
                    }
                    else if ((state & StateDecimal) != 0)
                    {
                        number.scale--;
                    }
                }
                else if (((options & NumberStyles.AllowDecimalPoint) != 0) && ((state & StateDecimal) == 0) && ((next = MatchChars(p, decSep)) != null || ((parsingCurrency) && (state & StateCurrency) == 0) && (next = MatchChars(p, numfmt.NumberDecimalSeparator)) != null))
                {
                    state |= StateDecimal;
                    p = next - 1;
                }
                else if (((options & NumberStyles.AllowThousands) != 0) && ((state & StateDigits) != 0) && ((state & StateDecimal) == 0) && ((next = MatchChars(p, groupSep)) != null || ((parsingCurrency) && (state & StateCurrency) == 0) && (next = MatchChars(p, numfmt.NumberGroupSeparator)) != null))
                {
                    p = next - 1;
                }
                else
                {
                    break;
                }
                ch = *++p;
            }

            Boolean negExp = false;
            number.precision = digEnd;
            if (bigNumber)
                sb.Append('\0');
            else
                number.digits[digEnd] = '\0';
            if ((state & StateDigits) != 0)
            {
                if ((ch == 'E' || ch == 'e') && ((options & NumberStyles.AllowExponent) != 0))
                {
                    char* temp = p;
                    ch = *++p;
                    if ((next = MatchChars(p, numfmt.PositiveSign)) != null)
                    {
                        ch = *(p = next);
                    }
                    else if ((next = MatchChars(p, numfmt.NegativeSign)) != null)
                    {
                        ch = *(p = next);
                        negExp = true;
                    }
                    if (ch >= '0' && ch <= '9')
                    {
                        Int32 exp = 0;
                        do
                        {
                            exp = exp * 10 + (ch - '0');
                            ch = *++p;
                            if (exp > 1000)
                            {
                                exp = 9999;
                                while (ch >= '0' && ch <= '9')
                                {
                                    ch = *++p;
                                }
                            }
                        } while (ch >= '0' && ch <= '9');
                        if (negExp)
                        {
                            exp = -exp;
                        }
                        number.scale += exp;
                    }
                    else
                    {
                        p = temp;
                        ch = *p;
                    }
                }
                while (true)
                {
                    if (!IsWhite(ch) || (options & NumberStyles.AllowTrailingWhite) == 0)
                    {
                        if (((options & NumberStyles.AllowTrailingSign) != 0 && ((state & StateSign) == 0)) && ((next = MatchChars(p, numfmt.PositiveSign)) != null || (((next = MatchChars(p, numfmt.NegativeSign)) != null) && (number.sign = true))))
                        {
                            state |= StateSign;
                            p = next - 1;
                        }
                        else if (ch == ')' && ((state & StateParens) != 0))
                        {
                            state &= ~StateParens;
                        }
                        else if (currSymbol != null && (next = MatchChars(p, currSymbol)) != null)
                        {
                            currSymbol = null;
                            p = next - 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    ch = *++p;
                }
                if ((state & StateParens) == 0)
                {
                    if ((state & StateNonZero) == 0)
                    {
                        if (!parseDecimal)
                        {
                            number.scale = 0;
                        }
                        if ((state & StateDecimal) == 0)
                        {
                            number.sign = false;
                        }
                    }
                    str = p;
                    return true;
                }
            }
            str = p;
            return false;
        }

        private unsafe static void StringToNumber(String str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo info, Boolean parseDecimal)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(String));
            Debug.Assert(info != null, "");

            fixed (char* stringPointer = str)
            {
                char* p = stringPointer;
                if (!ParseNumber(ref p, options, ref number, null, info, parseDecimal)
                    || (p - stringPointer < str.Length && !TrailingZeros(str, (int)(p - stringPointer))))
                {
                    throw new FormatException("Input string was not in a correct format.");
                }
            }
        }

        private static bool TrailingZeros(string s, int index)
        {
            // For compatibility, we need to allow trailing zeros at the end of a number string
            for (int i = index; i < s.Length; i++)
            {
                if (s[i] != '\0')
                    return false;
            }
            return true;
        }

        internal unsafe static Int32 ParseInt32(String s, NumberStyles style, NumberFormatInfo info)
        {
            fixed (byte* numberBufferBytes = new byte[NumberBuffer.NumberBufferBytes])
            {
                NumberBuffer number = new NumberBuffer(numberBufferBytes);
                Int32 i = 0;

                StringToNumber(s, style, ref number, info, false);

                if ((style & NumberStyles.AllowHexSpecifier) != 0)
                {
                    if (!HexNumberToInt32(ref number, ref i))
                        throw new OverflowException("Value was either too large or too small for an Int32.");
                }
                else
                {
                    if (!NumberToInt32(ref number, ref i))
                        throw new OverflowException("Value was either too large or too small for an Int32.");
                }
                return i;
            }
        }

        internal unsafe static bool TryParseDecimal(string value, NumberStyles options, NumberFormatInfo numfmt, out decimal result)
        {
            fixed (byte* numberBufferBytes = new byte[NumberBuffer.NumberBufferBytes])
            {
                NumberBuffer number = new NumberBuffer(numberBufferBytes);
                result = 0;

                if (!TryStringToNumber(value, options, ref number, numfmt, true))
                {
                    return false;
                }

                if (!NumberBufferToDecimal(number, ref result))
                {
                    return false;
                }
                return true;
            }
        }

        internal unsafe static bool TryParseDouble(string value, NumberStyles options, NumberFormatInfo numfmt, out double result)
        {
            fixed (byte* numberBufferBytes = new byte[NumberBuffer.NumberBufferBytes])
            {
                NumberBuffer number = new NumberBuffer(numberBufferBytes);
                result = 0;


                if (!TryStringToNumber(value, options, ref number, numfmt, false))
                {
                    return false;
                }
                if (!NumberBufferToDouble(number, ref result))
                {
                    return false;
                }
                return true;
            }
        }

        internal unsafe static bool TryParseInt32(string s, NumberStyles style, NumberFormatInfo info, out int result)
        {
            fixed (byte* numberBufferBytes = new byte[NumberBuffer.NumberBufferBytes])
            {
                NumberBuffer number = new NumberBuffer(numberBufferBytes);
                result = 0;

                if (!TryStringToNumber(s, style, ref number, info, false))
                {
                    return false;
                }

                if ((style & NumberStyles.AllowHexSpecifier) != 0)
                {
                    if (!HexNumberToInt32(ref number, ref result))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!NumberToInt32(ref number, ref result))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal unsafe static Boolean TryParseInt64(String s, NumberStyles style, NumberFormatInfo info, out Int64 result)
        {
            fixed (byte* numberBufferBytes = new byte[NumberBuffer.NumberBufferBytes])
            {
                NumberBuffer number = new NumberBuffer(numberBufferBytes);
                result = 0;

                if (!TryStringToNumber(s, style, ref number, info, false))
                {
                    return false;
                }

                if ((style & NumberStyles.AllowHexSpecifier) != 0)
                {
                    if (!HexNumberToInt64(ref number, ref result))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!NumberToInt64(ref number, ref result))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal unsafe static bool TryParseSingle(string value, NumberStyles options, NumberFormatInfo numfmt, out float result)
        {
            fixed (byte* numberBufferBytes = new byte[NumberBuffer.NumberBufferBytes])
            {
                NumberBuffer number = new NumberBuffer(numberBufferBytes);
                result = 0;
                double d = 0;

                if (!TryStringToNumber(value, options, ref number, numfmt, false))
                {
                    return false;
                }
                if (!NumberBufferToDouble(number, ref d))
                {
                    return false;
                }
                float castSingle = (float)d;
                if (float.IsInfinity(castSingle))
                {
                    return false;
                }

                result = castSingle;
                return true;
            }
        }

        internal unsafe static bool TryParseUInt32(string s, NumberStyles style, NumberFormatInfo info, out uint result)
        {
            fixed (byte* numberBufferBytes = new byte[NumberBuffer.NumberBufferBytes])
            {
                NumberBuffer number = new NumberBuffer(numberBufferBytes);
                result = 0;

                if (!TryStringToNumber(s, style, ref number, info, false))
                {
                    return false;
                }

                if ((style & NumberStyles.AllowHexSpecifier) != 0)
                {
                    if (!HexNumberToUInt32(ref number, ref result))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!NumberToUInt32(ref number, ref result))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal unsafe static bool TryParseUInt64(string s, NumberStyles style, NumberFormatInfo info, out ulong result)
        {
            fixed (byte* numberBufferBytes = new byte[NumberBuffer.NumberBufferBytes])
            {
                NumberBuffer number = new NumberBuffer(numberBufferBytes);
                result = 0;

                if (!TryStringToNumber(s, style, ref number, info, false))
                {
                    return false;
                }

                if ((style & NumberStyles.AllowHexSpecifier) != 0)
                {
                    if (!HexNumberToUInt64(ref number, ref result))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!NumberToUInt64(ref number, ref result))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        // Constants used by number parsing
        private const int NumberMaxDigits = 50;

        private const int Int32Precision = 10;
        private const int UInt32Precision = Int32Precision;
        private const int Int64Precision = 19;
        private const int UInt64Precision = 20;

        // NumberBuffer is a partial wrapper around a stack pointer that maps on to
        // the native NUMBER struct so that it can be passed to native directly. It 
        // must be initialized with a stack Byte * of size NumberBufferBytes.
        // For performance, this structure should attempt to be completely inlined.
        // 
        // It should always be initialized like so:
        //
        // Byte * numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
        // NumberBuffer number = new NumberBuffer(numberBufferBytes);
        //
        // For performance, when working on the buffer in managed we use the values in this
        // structure, except for the digits, and pack those values into the byte buffer
        // if called out to managed.
        internal unsafe struct NumberBuffer
        {
            // Enough space for NumberMaxDigit characters plus null and 3 32 bit integers and a pointer
            public static readonly int NumberBufferBytes = 12 + ((NumberMaxDigits + 1) * 2) + IntPtr.Size;

            private byte* baseAddress;
            public char* digits;
            public int precision;
            public int scale;
            public bool sign;

            public NumberBuffer(byte* stackBuffer)
            {
                baseAddress = stackBuffer;
                digits = (((char*)stackBuffer) + 6);
                precision = 0;
                scale = 0;
                sign = false;
            }

            public byte* PackForNative()
            {
                int* baseInteger = (int*)baseAddress;
                baseInteger[0] = precision;
                baseInteger[1] = scale;
                baseInteger[2] = sign ? 1 : 0;
                return baseAddress;
            }
        }

        private static bool HexNumberToInt32(ref NumberBuffer number, ref Int32 value)
        {
            uint passedValue = 0;
            bool returnValue = HexNumberToUInt32(ref number, ref passedValue);
            value = (int)passedValue;
            return returnValue;
        }

        private static bool HexNumberToInt64(ref NumberBuffer number, ref long value)
        {
            ulong passedValue = 0;
            bool returnValue = HexNumberToUInt64(ref number, ref passedValue);
            value = (long)passedValue;
            return returnValue;
        }

        private unsafe static bool HexNumberToUInt32(ref NumberBuffer number, ref uint value)
        {
            int i = number.scale;
            if (i > UInt32Precision || i < number.precision)
            {
                return false;
            }
            char* p = number.digits;
            Debug.Assert(p != null, "");

            uint n = 0;
            while (--i >= 0)
            {
                if (n > ((uint)0xFFFFFFFF / 16))
                {
                    return false;
                }
                n *= 16;
                if (*p != '\0')
                {
                    uint newN = n;
                    if (*p != '\0')
                    {
                        if (*p >= '0' && *p <= '9')
                        {
                            newN += (uint)(*p - '0');
                        }
                        else
                        {
                            if (*p >= 'A' && *p <= 'F')
                            {
                                newN += (uint)((*p - 'A') + 10);
                            }
                            else
                            {
                                Debug.Assert(*p >= 'a' && *p <= 'f', "");
                                newN += (uint)((*p - 'a') + 10);
                            }
                        }
                        p++;
                    }

                    // Detect an overflow here...
                    if (newN < n)
                    {
                        return false;
                    }
                    n = newN;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool HexNumberToUInt64(ref NumberBuffer number, ref ulong value)
        {
            Int32 i = number.scale;
            if (i > UInt64Precision || i < number.precision)
            {
                return false;
            }
            Char* p = number.digits;
            Debug.Assert(p != null, "");

            UInt64 n = 0;
            while (--i >= 0)
            {
                if (n > (0xFFFFFFFFFFFFFFFF / 16))
                {
                    return false;
                }
                n *= 16;
                if (*p != '\0')
                {
                    UInt64 newN = n;
                    if (*p != '\0')
                    {
                        if (*p >= '0' && *p <= '9')
                        {
                            newN += (UInt64)(*p - '0');
                        }
                        else
                        {
                            if (*p >= 'A' && *p <= 'F')
                            {
                                newN += (UInt64)((*p - 'A') + 10);
                            }
                            else
                            {
                                Debug.Assert(*p >= 'a' && *p <= 'f', "");
                                newN += (UInt64)((*p - 'a') + 10);
                            }
                        }
                        p++;
                    }

                    // Detect an overflow here...
                    if (newN < n)
                    {
                        return false;
                    }
                    n = newN;
                }
            }
            value = n;
            return true;
        }

        internal static bool TryStringToNumber(string str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo numfmt, bool parseDecimal)
        {
            return TryStringToNumber(str, options, ref number, null, numfmt, parseDecimal);
        }

        internal unsafe static bool TryStringToNumber(string str, NumberStyles options, ref NumberBuffer number, StringBuilder sb, NumberFormatInfo numfmt, bool parseDecimal)
        {
            if (str == null)
            {
                return false;
            }
            Debug.Assert(numfmt != null, "");

            fixed (char* stringPointer = str)
            {
                char* p = stringPointer;
                if (!ParseNumber(ref p, options, ref number, sb, numfmt, parseDecimal)
                    || (p - stringPointer < str.Length && !TrailingZeros(str, (int)(p - stringPointer))))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
