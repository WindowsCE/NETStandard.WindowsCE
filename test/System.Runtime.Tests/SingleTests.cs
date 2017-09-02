// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Tests
{
    [TestClass]
    public partial class SingleTests
    {
        private static IEnumerable<object[]> Parse_Valid_TestData()
        {
            // Defaults: AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign | AllowDecimalPoint | AllowExponent | AllowThousands
            NumberStyles defaultStyle = NumberStyles.Float;

            NumberFormatInfo emptyFormat = NumberFormatInfo.InvariantInfo;

            var dollarSignCommaSeparatorFormat = new NumberFormatInfo()
            {
                CurrencySymbol = "$",
                CurrencyGroupSeparator = ","
            };

            var decimalSeparatorFormat = new NumberFormatInfo()
            {
                NumberDecimalSeparator = "."
            };

            NumberFormatInfo invariantFormat = NumberFormatInfo.InvariantInfo;

            yield return new object[] { "-123", defaultStyle, null, (float)-123 };
            yield return new object[] { "0", defaultStyle, null, (float)0 };
            yield return new object[] { "123", defaultStyle, null, (float)123 };
            yield return new object[] { "  123  ", defaultStyle, null, (float)123 };
            yield return new object[] { (567.89f).ToString(NumberFormatInfo.InvariantInfo), defaultStyle, null, (float)567.89 };
            yield return new object[] { (-567.89f).ToString(NumberFormatInfo.InvariantInfo), defaultStyle, null, (float)-567.89 };
            yield return new object[] { "1E23", defaultStyle, null, (float)1E23 };

            yield return new object[] { (123.1f).ToString(NumberFormatInfo.InvariantInfo), NumberStyles.AllowDecimalPoint, null, (float)123.1 };
            yield return new object[] { 1000.ToString("N0", NumberFormatInfo.InvariantInfo), NumberStyles.AllowThousands, null, (float)1000 };

            yield return new object[] { "123", NumberStyles.Any, emptyFormat, (float)123 };
            yield return new object[] { (123.567f).ToString(NumberFormatInfo.InvariantInfo), NumberStyles.Any, emptyFormat, 123.567 };
            yield return new object[] { "123", NumberStyles.Float, emptyFormat, (float)123 };
            yield return new object[] { "$1,000", NumberStyles.Currency, dollarSignCommaSeparatorFormat, (float)1000 };
            yield return new object[] { "$1000", NumberStyles.Currency, dollarSignCommaSeparatorFormat, (float)1000 };
            yield return new object[] { "123.123", NumberStyles.Float, decimalSeparatorFormat, (float)123.123 };
            yield return new object[] { "(123)", NumberStyles.AllowParentheses, decimalSeparatorFormat, (float)-123 };

            yield return new object[] { "NaN", NumberStyles.Any, invariantFormat, float.NaN };
            yield return new object[] { "Infinity", NumberStyles.Any, invariantFormat, float.PositiveInfinity };
            yield return new object[] { "-Infinity", NumberStyles.Any, invariantFormat, float.NegativeInfinity };
        }

        [TestMethod]
        public void Single_Parse_Theory()
        {
            foreach (var fact in Parse_Valid_TestData())
                Parse((string)fact[0], (NumberStyles)fact[1], (IFormatProvider)fact[2], Convert.ToSingle(fact[3]));
        }

        private static void Parse(string value, NumberStyles style, IFormatProvider provider, float expected)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.InvariantInfo;
            if (provider == null)
                provider = NumberFormatInfo.InvariantInfo;

            float result;
            if ((style & ~NumberStyles.Integer) == 0 && style != NumberStyles.None)
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.IsTrue(float.TryParse(value, out result));
                    Assert.AreEqual(expected, result);

                    Assert.AreEqual(expected, float.Parse(value));
                }

                Assert.AreEqual(expected, float.Parse(value, provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.IsTrue(float.TryParse(value, style, provider, out result));
            Assert.AreEqual(expected, result);

            Assert.AreEqual(expected, float.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.IsTrue(float.TryParse(value, style, NumberFormatInfo.InvariantInfo, out result));
                Assert.AreEqual(expected, result);

                //Assert.AreEqual(expected, float.Parse(value, style));
                Assert.AreEqual(expected, float.Parse(value, style, NumberFormatInfo.InvariantInfo));
            }
        }

        private static IEnumerable<object[]> Parse_Invalid_TestData()
        {
            NumberStyles defaultStyle = NumberStyles.Float;

            var dollarSignDecimalSeparatorFormat = new NumberFormatInfo();
            dollarSignDecimalSeparatorFormat.CurrencySymbol = "$";
            dollarSignDecimalSeparatorFormat.NumberDecimalSeparator = ".";

            yield return new object[] { null, defaultStyle, null, typeof(ArgumentNullException) };
            yield return new object[] { "", defaultStyle, null, typeof(FormatException) };
            yield return new object[] { " ", defaultStyle, null, typeof(FormatException) };
            yield return new object[] { "Garbage", defaultStyle, null, typeof(FormatException) };

            yield return new object[] { "ab", defaultStyle, null, typeof(FormatException) }; // Hex value
            yield return new object[] { "(123)", defaultStyle, null, typeof(FormatException) }; // Parentheses
            yield return new object[] { 100.ToString("C0", NumberFormatInfo.InvariantInfo), defaultStyle, null, typeof(FormatException) }; // Currency

            yield return new object[] { (123.456f).ToString(NumberFormatInfo.InvariantInfo), NumberStyles.Integer, null, typeof(FormatException) }; // Decimal
            yield return new object[] { "  " + (123.456f).ToString(NumberFormatInfo.InvariantInfo), NumberStyles.None, null, typeof(FormatException) }; // Leading space
            yield return new object[] { (123.456f).ToString(NumberFormatInfo.InvariantInfo) + "   ", NumberStyles.None, null, typeof(FormatException) }; // Leading space
            yield return new object[] { "1E23", NumberStyles.None, null, typeof(FormatException) }; // Exponent

            yield return new object[] { "ab", NumberStyles.None, null, typeof(FormatException) }; // Negative hex value
            yield return new object[] { "  123  ", NumberStyles.None, null, typeof(FormatException) }; // Trailing and leading whitespace
        }

        [TestMethod]
        public void Single_Parse_Invalid_Theory()
        {
            foreach (var fact in Parse_Invalid_TestData())
                Parse_Invalid((string)fact[0], (NumberStyles)fact[1], (IFormatProvider)fact[2], (Type)fact[3]);
        }

        private static void Parse_Invalid(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.InvariantInfo;
            if (provider == null)
                provider = NumberFormatInfo.InvariantInfo;

            float result;
            if ((style & ~NumberStyles.Integer) == 0 && style != NumberStyles.None && (style & NumberStyles.AllowLeadingWhite) == (style & NumberStyles.AllowTrailingWhite))
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    //Assert.IsFalse(float.TryParse(value, out result));
                    //Assert.AreEqual(default(float), result);

                    //AssertExtensions.Throws(exceptionType, () => float.Parse(value));
                }

                AssertExtensions.Throws(exceptionType, () => float.Parse(value, provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.IsFalse(float.TryParse(value, style, provider, out result));
            Assert.AreEqual(default(float), result);

            AssertExtensions.Throws(exceptionType, () => float.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.IsFalse(float.TryParse(value, style, NumberFormatInfo.InvariantInfo, out result));
                Assert.AreEqual(default(float), result);

                //AssertExtensions.Throws(exceptionType, () => float.Parse(value, style));
                AssertExtensions.Throws(exceptionType, () => float.Parse(value, style, NumberFormatInfo.InvariantInfo));
            }
        }
    }
}