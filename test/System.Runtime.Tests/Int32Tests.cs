// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mock.System;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tests
{
    [TestClass]
    public partial class Int32Tests
    {
        private static IEnumerable<object[]> Parse_Valid_TestData()
        {
            NumberFormatInfo samePositiveNegativeFormat = new NumberFormatInfo()
            {
                PositiveSign = "|",
                NegativeSign = "|"
            };

            NumberFormatInfo emptyPositiveFormat = new NumberFormatInfo() { PositiveSign = "" };
            NumberFormatInfo emptyNegativeFormat = new NumberFormatInfo() { NegativeSign = "" };

            // None
            yield return new object[] { "0", NumberStyles.None, null, 0 };
            yield return new object[] { "123", NumberStyles.None, null, 123 };
            yield return new object[] { "2147483647", NumberStyles.None, null, 2147483647 };
            yield return new object[] { "123\0\0", NumberStyles.None, null, 123 };

            // HexNumber
            yield return new object[] { "123", NumberStyles.HexNumber, null, 0x123 };
            yield return new object[] { "abc", NumberStyles.HexNumber, null, 0xabc };
            yield return new object[] { "ABC", NumberStyles.HexNumber, null, 0xabc };
            yield return new object[] { "12", NumberStyles.HexNumber, null, 0x12 };
            yield return new object[] { "80000000", NumberStyles.HexNumber, null, -2147483648 };
            yield return new object[] { "FFFFFFFF", NumberStyles.HexNumber, null, -1 };

            // Currency
            NumberFormatInfo currencyFormat = new NumberFormatInfo()
            {
                CurrencySymbol = "$",
                CurrencyGroupSeparator = "|",
                NumberGroupSeparator = "/"
            };
            yield return new object[] { "$1|000", NumberStyles.Currency, currencyFormat, 1000 };
            yield return new object[] { "$1000", NumberStyles.Currency, currencyFormat, 1000 };
            yield return new object[] { "$   1000", NumberStyles.Currency, currencyFormat, 1000 };
            yield return new object[] { "1000", NumberStyles.Currency, currencyFormat, 1000 };
            yield return new object[] { "$(1000)", NumberStyles.Currency, currencyFormat, -1000 };
            yield return new object[] { "($1000)", NumberStyles.Currency, currencyFormat, -1000 };
            yield return new object[] { "$-1000", NumberStyles.Currency, currencyFormat, -1000 };
            yield return new object[] { "-$1000", NumberStyles.Currency, currencyFormat, -1000 };

            NumberFormatInfo emptyCurrencyFormat = new NumberFormatInfo() { CurrencySymbol = "" };
            yield return new object[] { "100", NumberStyles.Currency, emptyCurrencyFormat, 100 };

            // If CurrencySymbol and Negative are the same, NegativeSign is preferred
            NumberFormatInfo sameCurrencyNegativeSignFormat = new NumberFormatInfo()
            {
                NegativeSign = "|",
                CurrencySymbol = "|"
            };
            yield return new object[] { "|1000", NumberStyles.AllowCurrencySymbol | NumberStyles.AllowLeadingSign, sameCurrencyNegativeSignFormat, -1000 };

            // Any
            yield return new object[] { "123", NumberStyles.Any, null, 123 };

            // AllowLeadingSign
            yield return new object[] { "-2147483648", NumberStyles.AllowLeadingSign, null, -2147483648 };
            yield return new object[] { "-123", NumberStyles.AllowLeadingSign, null, -123 };
            yield return new object[] { "+0", NumberStyles.AllowLeadingSign, null, 0 };
            yield return new object[] { "-0", NumberStyles.AllowLeadingSign, null, 0 };
            yield return new object[] { "+123", NumberStyles.AllowLeadingSign, null, 123 };

            // If PositiveSign and NegativeSign are the same, PositiveSign is preferred
            yield return new object[] { "|123", NumberStyles.AllowLeadingSign, samePositiveNegativeFormat, 123 };

            // Empty PositiveSign or NegativeSign
            yield return new object[] { "100", NumberStyles.AllowLeadingSign, emptyPositiveFormat, 100 };
            yield return new object[] { "100", NumberStyles.AllowLeadingSign, emptyNegativeFormat, 100 };

            // AllowTrailingSign
            yield return new object[] { "123", NumberStyles.AllowTrailingSign, null, 123 };
            yield return new object[] { "123+", NumberStyles.AllowTrailingSign, null, 123 };
            yield return new object[] { "123-", NumberStyles.AllowTrailingSign, null, -123 };

            // If PositiveSign and NegativeSign are the same, PositiveSign is preferred
            yield return new object[] { "123|", NumberStyles.AllowTrailingSign, samePositiveNegativeFormat, 123 };

            // Empty PositiveSign or NegativeSign
            yield return new object[] { "100", NumberStyles.AllowTrailingSign, emptyPositiveFormat, 100 };
            yield return new object[] { "100", NumberStyles.AllowTrailingSign, emptyNegativeFormat, 100 };

            // AllowLeadingWhite and AllowTrailingWhite
            yield return new object[] { "123  ", NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, null, 123 };
            yield return new object[] { "  123", NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, null, 123 };
            yield return new object[] { "  123  ", NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, null, 123 };

            // AllowThousands
            NumberFormatInfo thousandsFormat = new NumberFormatInfo() { NumberGroupSeparator = "|" };
            yield return new object[] { "1000", NumberStyles.AllowThousands, thousandsFormat, 1000 };
            yield return new object[] { "1|0|0|0", NumberStyles.AllowThousands, thousandsFormat, 1000 };
            yield return new object[] { "1|||", NumberStyles.AllowThousands, thousandsFormat, 1 };

            NumberFormatInfo integerNumberSeparatorFormat = new NumberFormatInfo() { NumberGroupSeparator = "1" };
            yield return new object[] { "1111", NumberStyles.AllowThousands, integerNumberSeparatorFormat, 1111 };

            // AllowExponent
            yield return new object[] { "1E2", NumberStyles.AllowExponent, null, 100 };
            yield return new object[] { "1E+2", NumberStyles.AllowExponent, null, 100 };
            yield return new object[] { "1e2", NumberStyles.AllowExponent, null, 100 };
            yield return new object[] { "1E0", NumberStyles.AllowExponent, null, 1 };
            yield return new object[] { "(1E2)", NumberStyles.AllowExponent | NumberStyles.AllowParentheses, null, -100 };
            yield return new object[] { "-1E2", NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign, null, -100 };

            NumberFormatInfo negativeFormat = new NumberFormatInfo() { PositiveSign = "|" };
            yield return new object[] { "1E|2", NumberStyles.AllowExponent, negativeFormat, 100 };

            // AllowParentheses
            yield return new object[] { "123", NumberStyles.AllowParentheses, null, 123 };
            yield return new object[] { "(123)", NumberStyles.AllowParentheses, null, -123 };

            // AllowDecimalPoint
            NumberFormatInfo decimalFormat = new NumberFormatInfo() { NumberDecimalSeparator = "|" };
            yield return new object[] { "67|", NumberStyles.AllowDecimalPoint, decimalFormat, 67 };

            // NumberFormatInfo has a custom property with length > 1
            NumberFormatInfo integerCurrencyFormat = new NumberFormatInfo() { CurrencySymbol = "123" };
            yield return new object[] { "123123", NumberStyles.AllowCurrencySymbol, integerCurrencyFormat, 123 };

            NumberFormatInfo integerPositiveSignFormat = new NumberFormatInfo() { PositiveSign = "123" };
            yield return new object[] { "123123", NumberStyles.AllowLeadingSign, integerPositiveSignFormat, 123 };
        }

        [TestMethod]
        public void Parse_Theory()
        {
            foreach (var fact in Parse_Valid_TestData())
                Parse((string)fact[0], (NumberStyles)fact[1], (IFormatProvider)fact[2], (int)fact[3]);
        }

        private static void Parse(string value, NumberStyles style, IFormatProvider provider, int expected)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.CurrentInfo;
            int result;
            if ((style & ~NumberStyles.Integer) == 0 && style != NumberStyles.None)
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.IsTrue(Int322.TryParse(value, out result));
                    Assert.AreEqual(expected, result);

                    Assert.AreEqual(expected, Int322.Parse(value));
                }

                Assert.AreEqual(expected, Int322.Parse(value, provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.IsTrue(Int322.TryParse(value, style, provider, out result));
            Assert.AreEqual(expected, result);

            Assert.AreEqual(expected, Int322.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.IsTrue(Int322.TryParse(value, style, NumberFormatInfo.CurrentInfo, out result));
                Assert.AreEqual(expected, result);

                Assert.AreEqual(expected, Int322.Parse(value, style));
                Assert.AreEqual(expected, Int322.Parse(value, style, NumberFormatInfo.CurrentInfo));
            }
        }

        private static IEnumerable<object[]> Parse_Invalid_TestData()
        {
            // String is null, empty or entirely whitespace
            yield return new object[] { null, NumberStyles.Integer, null, typeof(ArgumentNullException) };
            yield return new object[] { null, NumberStyles.Any, null, typeof(ArgumentNullException) };
            yield return new object[] { "", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "", NumberStyles.Any, null, typeof(FormatException) };
            yield return new object[] { " \t \n \r ", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { " \t \n \r ", NumberStyles.Any, null, typeof(FormatException) };

            // String is garbage
            yield return new object[] { "Garbage", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "Garbage", NumberStyles.Any, null, typeof(FormatException) };

            // String has leading zeros
            yield return new object[] { "\0\0123", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "\0\0123", NumberStyles.Any, null, typeof(FormatException) };

            // String has internal zeros
            yield return new object[] { "1\023", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "1\023", NumberStyles.Any, null, typeof(FormatException) };

            // Integer doesn't allow hex, exponents, paretheses, currency, thousands, decimal
            yield return new object[] { "abc", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "1E23", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "(123)", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { 1000.ToString("C0"), NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { 1000.ToString("N0"), NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { 678.90.ToString("F2"), NumberStyles.Integer, null, typeof(FormatException) };

            // HexNumber
            yield return new object[] { "0xabc", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "&habc", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "G1", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "g1", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "+abc", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "-abc", NumberStyles.HexNumber, null, typeof(FormatException) };

            // None doesn't allow hex or leading or trailing whitespace
            yield return new object[] { "abc", NumberStyles.None, null, typeof(FormatException) };
            yield return new object[] { "123   ", NumberStyles.None, null, typeof(FormatException) };
            yield return new object[] { "   123", NumberStyles.None, null, typeof(FormatException) };
            yield return new object[] { "  123  ", NumberStyles.None, null, typeof(FormatException) };

            // AllowLeadingSign
            yield return new object[] { "+", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "-", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "+-123", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "-+123", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "- 123", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "+ 123", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };

            // AllowTrailingSign
            yield return new object[] { "123-+", NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "123+-", NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "123 -", NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "123 +", NumberStyles.AllowTrailingSign, null, typeof(FormatException) };

            // Parentheses has priority over CurrencySymbol and PositiveSign
            NumberFormatInfo currencyNegativeParenthesesFormat = new NumberFormatInfo()
            {
                CurrencySymbol = "(",
                PositiveSign = "))"
            };
            yield return new object[] { "(100))", NumberStyles.AllowParentheses | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowTrailingSign, currencyNegativeParenthesesFormat, typeof(FormatException) };

            // AllowTrailingSign and AllowLeadingSign
            yield return new object[] { "+123+", NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "+123-", NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "-123+", NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "-123-", NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, null, typeof(FormatException) };

            // AllowLeadingSign and AllowParentheses
            yield return new object[] { "-(1000)", NumberStyles.AllowLeadingSign | NumberStyles.AllowParentheses, null, typeof(FormatException) };
            yield return new object[] { "(-1000)", NumberStyles.AllowLeadingSign | NumberStyles.AllowParentheses, null, typeof(FormatException) };

            // AllowLeadingWhite
            yield return new object[] { "1   ", NumberStyles.AllowLeadingWhite, null, typeof(FormatException) };
            yield return new object[] { "   1   ", NumberStyles.AllowLeadingWhite, null, typeof(FormatException) };

            // AllowTrailingWhite
            yield return new object[] { "   1       ", NumberStyles.AllowTrailingWhite, null, typeof(FormatException) };
            yield return new object[] { "   1", NumberStyles.AllowTrailingWhite, null, typeof(FormatException) };

            // AllowThousands
            NumberFormatInfo thousandsFormat = new NumberFormatInfo() { NumberGroupSeparator = "|" };
            yield return new object[] { "|||1", NumberStyles.AllowThousands, null, typeof(FormatException) };

            // AllowExponent
            yield return new object[] { "65E", NumberStyles.AllowExponent, null, typeof(FormatException) };
            yield return new object[] { "65E10", NumberStyles.AllowExponent, null, typeof(OverflowException) };
            yield return new object[] { "65E+10", NumberStyles.AllowExponent, null, typeof(OverflowException) };
            yield return new object[] { "65E-1", NumberStyles.AllowExponent, null, typeof(OverflowException) };

            // AllowDecimalPoint
            NumberFormatInfo decimalFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };
            yield return new object[] { (67.9).ToString(), NumberStyles.AllowDecimalPoint, null, typeof(OverflowException) };

            // Parsing integers doesn't allow NaN, PositiveInfinity or NegativeInfinity
            NumberFormatInfo doubleFormat = new NumberFormatInfo()
            {
                NaNSymbol = "NaN",
                PositiveInfinitySymbol = "Infinity",
                NegativeInfinitySymbol = "-Infinity"
            };
            yield return new object[] { "NaN", NumberStyles.Any, doubleFormat, typeof(FormatException) };
            yield return new object[] { "Infinity", NumberStyles.Any, doubleFormat, typeof(FormatException) };
            yield return new object[] { "-Infinity", NumberStyles.Any, doubleFormat, typeof(FormatException) };

            // NumberFormatInfo has a custom property with length > 1
            NumberFormatInfo integerCurrencyFormat = new NumberFormatInfo() { CurrencySymbol = "123" };
            yield return new object[] { "123", NumberStyles.AllowCurrencySymbol, integerCurrencyFormat, typeof(FormatException) };

            NumberFormatInfo integerPositiveSignFormat = new NumberFormatInfo() { PositiveSign = "123" };
            yield return new object[] { "123", NumberStyles.AllowLeadingSign, integerPositiveSignFormat, typeof(FormatException) };

            // Not in range of Int32
            yield return new object[] { "2147483648", NumberStyles.Any, null, typeof(OverflowException) };
            yield return new object[] { "2147483648", NumberStyles.Integer, null, typeof(OverflowException) };
            yield return new object[] { "-2147483649", NumberStyles.Any, null, typeof(OverflowException) };
            yield return new object[] { "-2147483649", NumberStyles.Integer, null, typeof(OverflowException) };
            yield return new object[] { "2147483649-", NumberStyles.AllowTrailingSign, null, typeof(OverflowException) };
            yield return new object[] { "(2147483649)", NumberStyles.AllowParentheses, null, typeof(OverflowException) };
            yield return new object[] { "2E10", NumberStyles.AllowExponent, null, typeof(OverflowException) };
            yield return new object[] { "800000000", NumberStyles.AllowHexSpecifier, null, typeof(OverflowException) };

            yield return new object[] { "9223372036854775808", NumberStyles.Integer, null, typeof(OverflowException) };
            yield return new object[] { "-9223372036854775809", NumberStyles.Integer, null, typeof(OverflowException) };
            yield return new object[] { "8000000000000000", NumberStyles.AllowHexSpecifier, null, typeof(OverflowException) };
        }

        [TestMethod]
        public void Parse_Invalid_Theory()
        {
            foreach (var fact in Parse_Invalid_TestData())
                Parse_Invalid((string)fact[0], (NumberStyles)fact[1], (IFormatProvider)fact[2], (Type)fact[3]);
        }

        private static void Parse_Invalid(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.CurrentInfo;
            int result;
            if ((style & ~NumberStyles.Integer) == 0 && style != NumberStyles.None && (style & NumberStyles.AllowLeadingWhite) == (style & NumberStyles.AllowTrailingWhite))
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.IsFalse(Int322.TryParse(value, out result));
                    Assert.AreEqual(default(int), result);

                    AssertExtensions.Throws(exceptionType, () => Int322.Parse(value));
                }

                AssertExtensions.Throws(exceptionType, () => Int322.Parse(value, provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.IsFalse(Int322.TryParse(value, style, provider, out result));
            Assert.AreEqual(default(int), result);

            AssertExtensions.Throws(exceptionType, () => Int322.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.IsFalse(Int322.TryParse(value, style, NumberFormatInfo.CurrentInfo, out result));
                Assert.AreEqual(default(int), result);

                AssertExtensions.Throws(exceptionType, () => Int322.Parse(value, style));
                AssertExtensions.Throws(exceptionType, () => Int322.Parse(value, style, NumberFormatInfo.CurrentInfo));
            }
        }

        [TestMethod]
        public void TryParse_InvalidNumberStyle_ThrowsArgumentException_Theory()
        {
            TryParse_InvalidNumberStyle_ThrowsArgumentException(NumberStyles.HexNumber | NumberStyles.AllowParentheses, null);
            TryParse_InvalidNumberStyle_ThrowsArgumentException(unchecked((NumberStyles)0xFFFFFC00), "style");
        }

        public static void TryParse_InvalidNumberStyle_ThrowsArgumentException(NumberStyles style, string paramName)
        {
            int result = 0;
            AssertExtensions.Throws<ArgumentException>(paramName, () => Int322.TryParse("1", style, null, out result));
            Assert.AreEqual(default(int), result);

            AssertExtensions.Throws<ArgumentException>(paramName, () => Int322.Parse("1", style));
            AssertExtensions.Throws<ArgumentException>(paramName, () => Int322.Parse("1", style, null));
        }
    }
}