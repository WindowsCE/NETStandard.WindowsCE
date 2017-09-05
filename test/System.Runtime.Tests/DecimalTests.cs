// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mock.System;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tests
{
    [TestClass]
    public partial class DecimalTests
    {
        private static IEnumerable<object[]> GetBits_TestData()
        {
            yield return new object[] { 1M, new int[] { 0x00000001, 0x00000000, 0x00000000, 0x00000000 } };
            yield return new object[] { 100000000000000M, new int[] { 0x107A4000, 0x00005AF3, 0x00000000, 0x00000000 } };
            yield return new object[] { 100000000000000.00000000000000M, new int[] { 0x10000000, 0x3E250261, 0x204FCE5E, 0x000E0000 } };
            yield return new object[] { 1.0000000000000000000000000000M, new int[] { 0x10000000, 0x3E250261, 0x204FCE5E, 0x001C0000 } };
            yield return new object[] { 123456789M, new int[] { 0x075BCD15, 0x00000000, 0x00000000, 0x00000000 } };
            yield return new object[] { 0.123456789M, new int[] { 0x075BCD15, 0x00000000, 0x00000000, 0x00090000 } };
            yield return new object[] { 0.000000000123456789M, new int[] { 0x075BCD15, 0x00000000, 0x00000000, 0x00120000 } };
            yield return new object[] { 0.000000000000000000123456789M, new int[] { 0x075BCD15, 0x00000000, 0x00000000, 0x001B0000 } };
            yield return new object[] { 4294967295M, new int[] { unchecked((int)0xFFFFFFFF), 0x00000000, 0x00000000, 0x00000000 } };
            yield return new object[] { 18446744073709551615M, new int[] { unchecked((int)0xFFFFFFFF), unchecked((int)0xFFFFFFFF), 0x00000000, 0x00000000 } };
            yield return new object[] { decimal.MaxValue, new int[] { unchecked((int)0xFFFFFFFF), unchecked((int)0xFFFFFFFF), unchecked((int)0xFFFFFFFF), 0x00000000 } };
            yield return new object[] { decimal.MinValue, new int[] { unchecked((int)0xFFFFFFFF), unchecked((int)0xFFFFFFFF), unchecked((int)0xFFFFFFFF), unchecked((int)0x80000000) } };
            yield return new object[] { -7.9228162514264337593543950335M, new int[] { unchecked((int)0xFFFFFFFF), unchecked((int)0xFFFFFFFF), unchecked((int)0xFFFFFFFF), unchecked((int)0x801C0000) } };
        }

        [TestMethod]
        public void GetBits_Theory()
        {
            foreach (var fact in GetBits_TestData())
                GetBits((decimal)fact[0], (int[])fact[1]);
        }

        private static void GetBits(decimal input, int[] expected)
        {
            int[] bits = decimal.GetBits(input);

            Assert.IsTrue(expected.SequenceEqual(bits));
            //Assert.AreEqual(expected, bits);

            bool sign = (bits[3] & 0x80000000) != 0;
            byte scale = (byte)((bits[3] >> 16) & 0x7F);
            decimal newValue = new decimal(bits[0], bits[1], bits[2], sign, scale);

            Assert.AreEqual(input, newValue);
        }

        private static IEnumerable<object[]> Parse_Valid_TestData()
        {
            NumberStyles defaultStyle = NumberStyles.Float;

            NumberFormatInfo emptyFormat = NumberFormatInfo.InvariantInfo;

            var customFormat1 = new NumberFormatInfo();
            customFormat1.CurrencySymbol = "$";
            customFormat1.CurrencyGroupSeparator = ",";

            var customFormat2 = new NumberFormatInfo();
            customFormat2.NumberDecimalSeparator = ".";

            var customFormat3 = new NumberFormatInfo();
            customFormat3.NumberGroupSeparator = ",";

            var customFormat4 = new NumberFormatInfo();
            customFormat4.NumberDecimalSeparator = ".";

            yield return new object[] { "-123", defaultStyle, null, -123m };
            yield return new object[] { "0", defaultStyle, null, 0m };
            yield return new object[] { "123", defaultStyle, null, 123m };
            yield return new object[] { "  123  ", defaultStyle, null, 123m };
            yield return new object[] { (567.89m).ToString(NumberFormatInfo.InvariantInfo), defaultStyle, null, 567.89m };
            yield return new object[] { (-567.89m).ToString(NumberFormatInfo.InvariantInfo), defaultStyle, null, -567.89m };

            yield return new object[] { "79228162514264337593543950335", defaultStyle, null, 79228162514264337593543950335m };
            yield return new object[] { "-79228162514264337593543950335", defaultStyle, null, -79228162514264337593543950335m };
            yield return new object[] { "79,228,162,514,264,337,593,543,950,335", NumberStyles.AllowThousands, customFormat3, 79228162514264337593543950335m };

            yield return new object[] { (123.1m).ToString(NumberFormatInfo.InvariantInfo), NumberStyles.AllowDecimalPoint, null, 123.1m };
            yield return new object[] { 1000.ToString("N0", NumberFormatInfo.InvariantInfo), NumberStyles.AllowThousands, null, 1000m };

            yield return new object[] { "123", NumberStyles.Any, emptyFormat, 123m };
            yield return new object[] { (123.567m).ToString(NumberFormatInfo.InvariantInfo), NumberStyles.Any, emptyFormat, 123.567m };
            yield return new object[] { "123", NumberStyles.Float, emptyFormat, 123m };
            yield return new object[] { "$1000", NumberStyles.Currency, customFormat1, 1000m };
            yield return new object[] { "123.123", NumberStyles.Float, customFormat2, 123.123m };
            yield return new object[] { "(123)", NumberStyles.AllowParentheses, customFormat2, -123m };

            // Number buffer limit ran out (string too long)
            yield return new object[] { "1234567890123456789012345.678456", defaultStyle, customFormat4, 1234567890123456789012345.6785m };
        }

        [TestMethod]
        public void Parse_Theory()
        {
            foreach (var fact in Parse_Valid_TestData())
                Parse((string)fact[0], (NumberStyles)fact[1], (IFormatProvider)fact[2], (decimal)fact[3]);
        }

        public static void Parse(string value, NumberStyles style, IFormatProvider provider, decimal expected)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.InvariantInfo;
            if (provider == null)
                provider = NumberFormatInfo.InvariantInfo;

            decimal result;
            if ((style & ~NumberStyles.Integer) == 0 && style != NumberStyles.None)
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.IsTrue(Decimal2.TryParse(value, out result));
                    Assert.AreEqual(expected, result);

                    Assert.AreEqual(expected, Decimal2.Parse(value));
                }

                Assert.AreEqual(expected, Decimal2.Parse(value, provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.IsTrue(Decimal2.TryParse(value, style, provider, out result), "Error parsing: {0}", value);
            Assert.AreEqual(expected, result);

            Assert.AreEqual(expected, Decimal2.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.IsTrue(Decimal2.TryParse(value, style, NumberFormatInfo.InvariantInfo, out result));
                Assert.AreEqual(expected, result);

                //Assert.AreEqual(expected, Decimal2.Parse(value, style));
                Assert.AreEqual(expected, Decimal2.Parse(value, style, NumberFormatInfo.InvariantInfo));
            }
        }

        private static IEnumerable<object[]> Parse_Invalid_TestData()
        {
            NumberStyles defaultStyle = NumberStyles.Float;

            var customFormat = new NumberFormatInfo();
            customFormat.CurrencySymbol = "$";
            customFormat.NumberDecimalSeparator = ".";

            yield return new object[] { null, defaultStyle, null, typeof(ArgumentNullException) };
            yield return new object[] { "79228162514264337593543950336", defaultStyle, null, typeof(OverflowException) };
            yield return new object[] { "", defaultStyle, null, typeof(FormatException) };
            yield return new object[] { " ", defaultStyle, null, typeof(FormatException) };
            yield return new object[] { "Garbage", defaultStyle, null, typeof(FormatException) };

            yield return new object[] { "ab", defaultStyle, null, typeof(FormatException) }; // Hex value
            yield return new object[] { "(123)", defaultStyle, null, typeof(FormatException) }; // Parentheses
            yield return new object[] { 100.ToString("C0"), defaultStyle, null, typeof(FormatException) }; // Currency

            yield return new object[] { (123.456m).ToString(), NumberStyles.Integer, null, typeof(FormatException) }; // Decimal
            yield return new object[] { "  " + (123.456m).ToString(), NumberStyles.None, null, typeof(FormatException) }; // Leading space
            yield return new object[] { (123.456m).ToString() + "   ", NumberStyles.None, null, typeof(FormatException) }; // Leading space
            yield return new object[] { "1E23", NumberStyles.None, null, typeof(FormatException) }; // Exponent

            yield return new object[] { "ab", NumberStyles.None, null, typeof(FormatException) }; // Hex value
            yield return new object[] { "  123  ", NumberStyles.None, null, typeof(FormatException) }; // Trailing and leading whitespace
        }

        [TestMethod]
        public void Parse_Invalid_Theory()
        {
            foreach (var fact in Parse_Invalid_TestData())
                Parse_Invalid((string)fact[0], (NumberStyles)fact[1], (IFormatProvider)fact[2], (Type)fact[3]);
        }

        public static void Parse_Invalid(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.InvariantInfo;
            if (provider == null)
                provider = NumberFormatInfo.InvariantInfo;

            decimal result;
            if ((style & ~NumberStyles.Integer) == 0 && style != NumberStyles.None && (style & NumberStyles.AllowLeadingWhite) == (style & NumberStyles.AllowTrailingWhite))
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    //Assert.IsFalse(Decimal2.TryParse(value, out result));
                    //Assert.AreEqual(default(decimal), result);

                    //AssertExtensions.Throws(exceptionType, () => Decimal2.Parse(value));
                }

                AssertExtensions.Throws(exceptionType, () => Decimal2.Parse(value, provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.IsFalse(Decimal2.TryParse(value, style, provider, out result));
            Assert.AreEqual(default(decimal), result);

            AssertExtensions.Throws(exceptionType, () => Decimal2.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.IsFalse(Decimal2.TryParse(value, style, NumberFormatInfo.InvariantInfo, out result));
                Assert.AreEqual(default(decimal), result);

                //AssertExtensions.Throws(exceptionType, () => Decimal2.Parse(value, style));
                AssertExtensions.Throws(exceptionType, () => Decimal2.Parse(value, style, NumberFormatInfo.InvariantInfo));
            }
        }
    }
}