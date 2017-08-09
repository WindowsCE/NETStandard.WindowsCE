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

        internal const int HIGH_SURROGATE_START = 0x00d800;
        internal const int LOW_SURROGATE_END = 0x00dfff;

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static string ConvertFromUtf32(int utf32)
        {
            throw new PlatformNotSupportedException();
        }

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static int ConvertToUtf32(char highSurrogate, char lowSurrogate)
        {
            throw new PlatformNotSupportedException();
        }

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static int ConvertToUtf32(string s, int index)
        {
            throw new PlatformNotSupportedException();
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
                throw new FormatException("Cannot parse multiple char string to single char");

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
