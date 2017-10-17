using System;

#if NET35_CF
using System.Globalization;
#else
using Mock.System.Globalization;
#endif

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Char2
    {
        public const char MaxValue = char.MaxValue;
        public const char MinValue = char.MinValue;

        internal const int UNICODE_PLANE00_END = 0x00ffff;
        // The starting codepoint for Unicode plane 1.  Plane 1 contains 0x010000 ~ 0x01ffff.
        internal const int UNICODE_PLANE01_START = 0x10000;
        // The end codepoint for Unicode plane 16.  This is the maximum code point value allowed for Unicode.
        // Plane 16 contains 0x100000 ~ 0x10ffff.
        internal const int UNICODE_PLANE16_END = 0x10ffff;

        internal const int HIGH_SURROGATE_START = 0x00d800;
        internal const int LOW_SURROGATE_END = 0x00dfff;

        /*================================= ConvertFromUtf32 ============================
        ** Convert an UTF32 value into a surrogate pair.
        ==============================================================================*/

        public static string ConvertFromUtf32(int utf32)
        {
            // For UTF32 values from U+00D800 ~ U+00DFFF, we should throw.  They
            // are considered as irregular code unit sequence, but they are not illegal.
            if ((utf32 < 0 || utf32 > UNICODE_PLANE16_END) || (utf32 >= HIGH_SURROGATE_START && utf32 <= LOW_SURROGATE_END))
            {
                throw new ArgumentOutOfRangeException(nameof(utf32), SR.ArgumentOutOfRange_InvalidUTF32);
            }

            if (utf32 < UNICODE_PLANE01_START)
            {
                // This is a BMP character.
                return (Char.ToString((char)utf32));
            }

            unsafe
            {
                // This is a supplementary character.  Convert it to a surrogate pair in UTF-16.
                utf32 -= UNICODE_PLANE01_START;
                uint surrogate = 0; // allocate 2 chars worth of stack space
                char* address = (char*)&surrogate;
                address[0] = (char)((utf32 / 0x400) + (int)CharUnicodeInfo2.HIGH_SURROGATE_START);
                address[1] = (char)((utf32 % 0x400) + (int)CharUnicodeInfo2.LOW_SURROGATE_START);
                return new string(address, 0, 2);
            }
        }

        /*=============================ConvertToUtf32===================================
        ** Convert a surrogate pair to UTF32 value                                    
        ==============================================================================*/

        public static int ConvertToUtf32(char highSurrogate, char lowSurrogate)
        {
            if (!IsHighSurrogate(highSurrogate))
            {
                throw new ArgumentOutOfRangeException(nameof(highSurrogate), SR.ArgumentOutOfRange_InvalidHighSurrogate);
            }
            if (!IsLowSurrogate(lowSurrogate))
            {
                throw new ArgumentOutOfRangeException(nameof(lowSurrogate), SR.ArgumentOutOfRange_InvalidLowSurrogate);
            }
            return (((highSurrogate - CharUnicodeInfo2.HIGH_SURROGATE_START) * 0x400) + (lowSurrogate - CharUnicodeInfo2.LOW_SURROGATE_START) + UNICODE_PLANE01_START);
        }

        /*=============================ConvertToUtf32===================================
        ** Convert a character or a surrogate pair starting at index of the specified string 
        ** to UTF32 value.
        ** The char pointed by index should be a surrogate pair or a BMP character.
        ** This method throws if a high-surrogate is not followed by a low surrogate.
        ** This method throws if a low surrogate is seen without preceding a high-surrogate.
        ==============================================================================*/

        public static int ConvertToUtf32(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (index < 0 || index >= s.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), SR.ArgumentOutOfRange_Index);
            }

            // Check if the character at index is a high surrogate.
            int temp1 = (int)s[index] - CharUnicodeInfo2.HIGH_SURROGATE_START;
            if (temp1 >= 0 && temp1 <= 0x7ff)
            {
                // Found a surrogate char.
                if (temp1 <= 0x3ff)
                {
                    // Found a high surrogate.
                    if (index < s.Length - 1)
                    {
                        int temp2 = (int)s[index + 1] - CharUnicodeInfo2.LOW_SURROGATE_START;
                        if (temp2 >= 0 && temp2 <= 0x3ff)
                        {
                            // Found a low surrogate.
                            return ((temp1 * 0x400) + temp2 + UNICODE_PLANE01_START);
                        }
                        else
                        {
                            throw new ArgumentException(string.Format(SR.Argument_InvalidHighSurrogate, index), nameof(s));
                        }
                    }
                    else
                    {
                        // Found a high surrogate at the end of the string.
                        throw new ArgumentException(string.Format(SR.Argument_InvalidHighSurrogate, index), nameof(s));
                    }
                }
                else
                {
                    // Find a low surrogate at the character pointed by index.
                    throw new ArgumentException(string.Format(SR.Argument_InvalidLowSurrogate, index), nameof(s));
                }
            }
            // Not a high-surrogate or low-surrogate. Genereate the UTF32 value for the BMP characters.
            return ((int)s[index]);
        }

        public static double GetNumericValue(char c)
            => char.GetNumericValue(c);

        public static double GetNumericValue(string s, int index)
            => char.GetNumericValue(s, index);

        public static bool IsControl(char c)
            => char.IsControl(c);

        public static bool IsControl(string s, int index)
            => char.IsControl(s, index);

        public static bool IsDigit(char c)
            => char.IsDigit(c);

        public static bool IsDigit(string s, int index)
            => char.IsDigit(s, index);

        public static bool IsHighSurrogate(char c)
        {
            return ((c >= CharUnicodeInfo2.HIGH_SURROGATE_START) && (c <= CharUnicodeInfo2.HIGH_SURROGATE_END));
        }

        public static bool IsHighSurrogate(string s, int index)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (index < 0 || index >= s.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return (IsHighSurrogate(s[index]));
        }

        public static bool IsLetter(char c)
            => char.IsLetter(c);

        public static bool IsLetter(string s, int index)
            => char.IsLetter(s, index);

        public static bool IsLetterOrDigit(char c)
            => char.IsLetterOrDigit(c);

        public static bool IsLetterOrDigit(string s, int index)
            => char.IsLetterOrDigit(s, index);

        public static bool IsLower(char c)
            => char.IsLower(c);

        public static bool IsLower(string s, int index)
            => char.IsLower(s, index);

        public static bool IsLowSurrogate(char c)
        {
            return ((c >= CharUnicodeInfo2.LOW_SURROGATE_START) && (c <= CharUnicodeInfo2.LOW_SURROGATE_END));
        }

        public static bool IsLowSurrogate(string s, int index)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (index < 0 || index >= s.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return (IsLowSurrogate(s[index]));
        }

        public static bool IsNumber(char c)
            => char.IsNumber(c);

        public static bool IsNumber(string s, int index)
            => char.IsNumber(s, index);

        public static bool IsPunctuation(char c)
            => char.IsPunctuation(c);

        public static bool IsPunctuation(string s, int index)
            => char.IsPunctuation(s, index);

        public static bool IsSeparator(char c)
            => char.IsSeparator(c);

        public static bool IsSeparator(string s, int index)
            => char.IsSeparator(s, index);

        public static bool IsSurrogate(char c)
        {
            return (c >= HIGH_SURROGATE_START && c <= LOW_SURROGATE_END);
        }

        public static bool IsSurrogate(string s, int index)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (((uint)index) >= ((uint)s.Length))
                throw new ArgumentOutOfRangeException(nameof(index));

            return (IsSurrogate(s[index]));
        }

        public static bool IsSurrogatePair(char highSurrogate, char lowSurrogate)
        {
            return ((highSurrogate >= CharUnicodeInfo2.HIGH_SURROGATE_START && highSurrogate <= CharUnicodeInfo2.HIGH_SURROGATE_END) &&
                    (lowSurrogate >= CharUnicodeInfo2.LOW_SURROGATE_START && lowSurrogate <= CharUnicodeInfo2.LOW_SURROGATE_END));
        }

        public static bool IsSurrogatePair(string s, int index)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (index < 0 || index >= s.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index + 1 < s.Length)
                return (IsSurrogatePair(s[index], s[index + 1]));

            return false;
        }

        public static bool IsSymbol(char c)
            => char.IsSymbol(c);

        public static bool IsSymbol(string s, int index)
            => char.IsSymbol(s, index);

        public static bool IsUpper(char c)
            => char.IsUpper(c);

        public static bool IsUpper(string s, int index)
            => char.IsUpper(s, index);

        public static bool IsWhiteSpace(char c)
            => char.IsWhiteSpace(c);

        public static bool IsWhiteSpace(string s, int index)
            => char.IsWhiteSpace(s, index);

        public static char ToLower(char c)
            => char.ToLower(c);

        public static char ToLowerInvariant(char c)
            => char.ToLowerInvariant(c);

        public static char ToUpper(char c)
            => char.ToUpper(c);

        public static char ToUpperInvariant(char c)
            => new string(c, 1).ToUpperInvariant()[0];

        public static char Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (s.Length != 1)
                throw new FormatException(SR.Format_NeedSingleChar);

            return s[0];
        }

        public static bool TryParse(string s, out char result)
        {
            result = '\0';
            if (s == null)
                return false;

            if (s.Length != 1)
                return false;

            result = s[0];
            return true;
        }
    }
}
