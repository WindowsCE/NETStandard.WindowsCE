// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

#if WindowsCE
using System.Runtime.Serialization;
#else
using Mock.System.Runtime.Serialization;
#endif

namespace Tests.Runtime.Serialization
{
    [TestClass]
    public class FormatterConverterTests
    {
        [TestMethod]
        public void FormatterConverter_InvalidArguments_ThrowExceptions()
        {
            var f = new FormatterConverter();
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.Convert(null, typeof(int)));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.Convert(null, TypeCode.Char));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToBoolean(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToByte(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToChar(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToDateTime(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToDecimal(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToDouble(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToInt16(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToInt32(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToInt64(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToSByte(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToSingle(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToString(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToUInt16(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToUInt32(null));
            AssertExtensions.Throws<ArgumentNullException>("value", () => f.ToUInt64(null));
        }

        [TestMethod]
        public void FormatterConverter_ToMethods_ExpectedValue()
        {
            Assert.IsTrue(new FormatterConverter().ToBoolean("true"));
            Assert.AreEqual((byte)42, new FormatterConverter().ToByte("42"));
            Assert.AreEqual('c', new FormatterConverter().ToChar("c"));
            Assert.AreEqual(new DateTime(2000, 1, 1), new FormatterConverter().ToDateTime(new DateTime(2000, 1, 1).ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(1.2m, new FormatterConverter().ToDecimal("1.2"));
            Assert.AreEqual(1.2, new FormatterConverter().ToDouble("1.2"));
            Assert.AreEqual((short)42, new FormatterConverter().ToInt16("42"));
            Assert.AreEqual(42, new FormatterConverter().ToInt32("42"));
            Assert.AreEqual(42, new FormatterConverter().ToInt64("42"));
            Assert.AreEqual((sbyte)42, new FormatterConverter().ToSByte("42"));
            Assert.AreEqual(1.2f, new FormatterConverter().ToSingle("1.2"));
            Assert.AreEqual("1.2", new FormatterConverter().ToString("1.2"));
            Assert.AreEqual((ushort)42, new FormatterConverter().ToUInt16("42"));
            Assert.AreEqual((uint)42, new FormatterConverter().ToUInt32("42"));
            Assert.AreEqual((ulong)42, new FormatterConverter().ToUInt64("42"));
            Assert.AreEqual(42, new FormatterConverter().Convert("42", TypeCode.Int32));
        }

        [TestMethod]
        public void FormatterConverter_Convert_ExpectedValue()
        {
            var theory = new[]
            {
                new { Input = "true", Expected = (object)true },
                new { Input = "false", Expected = (object)false },
                new { Input = "42", Expected = (object)(byte)42 },
                new { Input = "c", Expected = (object)'c' },
                new { Input = "1.2", Expected = (object)1.2 },
                new { Input = "42", Expected = (object)(short)42 },
                new { Input = "42", Expected = (object)42 },
                new { Input = "42", Expected = (object)(long)42 },
                new { Input = "42", Expected = (object)(sbyte)42 },
                new { Input = "1.2", Expected = (object)(float)1.2 },
                new { Input = "1.2", Expected = (object)"1.2" },
                new { Input = "42", Expected = (object)(ushort)42 },
                new { Input = "42", Expected = (object)(uint)42 },
                new { Input = "42", Expected = (object)(ulong)42 },
            };

            foreach (var fact in theory)
            {
                Assert.AreEqual(
                    fact.Expected,
                    new FormatterConverter().Convert(fact.Input, fact.Expected.GetType()));
            }
        }
    }
}