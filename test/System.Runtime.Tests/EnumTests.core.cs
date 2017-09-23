using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WindowsCE
#else
using Mock.System;
#endif

namespace Tests
{
    partial class EnumTests
    {
        private static IEnumerable<object[]> Format_TestData()
        {
            // Format: D
            yield return new object[] { typeof(SimpleEnum), 1, "D", "1" };

            // Format: X
            yield return new object[] { typeof(SimpleEnum), SimpleEnum.Red, "X", "00000001" };
            yield return new object[] { typeof(SimpleEnum), 1, "X", "00000001" };

            // Format: F
            yield return new object[] { typeof(SimpleEnum), 1, "F", "Red" };
        }

        [TestMethod]
        public void Enum_Format_Theory()
        {
            foreach (var fact in Format_TestData())
                Format((Type)fact[0], fact[1], (string)fact[2], (string)fact[3]);
        }

        private static void Format(Type enumType, object value, string format, string expected)
        {
            // Format string is case insensitive
            Assert.AreEqual(expected, Enum2.Format(enumType, value, format.ToUpperInvariant()));
            Assert.AreEqual(expected, Enum2.Format(enumType, value, format.ToLowerInvariant()));
        }

        [TestMethod]
        public void Enum_Format_Invalid()
        {
            AssertExtensions.Throws<ArgumentNullException>("enumType", () => Enum2.Format(null, (Int32Enum)1, "F")); // Enum type is null
            AssertExtensions.Throws<ArgumentNullException>("value", () => Enum2.Format(typeof(SimpleEnum), null, "F")); // Value is null
            AssertExtensions.Throws<ArgumentNullException>("format", () => Enum2.Format(typeof(SimpleEnum), SimpleEnum.Red, null)); // Format is null

            AssertExtensions.Throws<ArgumentException>("enumType", () => Enum2.Format(typeof(object), 1, "F")); // Enum type is not an enum type

            AssertExtensions.Throws<ArgumentException>(null, () => Enum2.Format(typeof(SimpleEnum), (Int32Enum)1, "F")); // Value is of the wrong enum type

            AssertExtensions.Throws<ArgumentException>(null, () => Enum2.Format(typeof(SimpleEnum), (short)1, "F")); // Value is of the wrong integral
            AssertExtensions.Throws<ArgumentException>(null, () => Enum2.Format(typeof(SimpleEnum), "Red", "F")); // Value is of the wrong integral

            AssertExtensions.Throws<FormatException>(() => Enum2.Format(typeof(SimpleEnum), SimpleEnum.Red, "")); // Format is empty
            AssertExtensions.Throws<FormatException>(() => Enum2.Format(typeof(SimpleEnum), SimpleEnum.Red, "   \t")); // Format is whitespace
            AssertExtensions.Throws<FormatException>(() => Enum2.Format(typeof(SimpleEnum), SimpleEnum.Red, "t")); // No such format
        }

        private static IEnumerable<object[]> GetName_TestData()
        {
            // SByte
            yield return new object[] { typeof(SByteEnum), SByteEnum.Min, "Min" };
            yield return new object[] { typeof(SByteEnum), SByteEnum.One, "One" };
            yield return new object[] { typeof(SByteEnum), SByteEnum.Two, "Two" };
            yield return new object[] { typeof(SByteEnum), SByteEnum.Max, "Max" };
            yield return new object[] { typeof(SByteEnum), sbyte.MinValue, "Min" };
            yield return new object[] { typeof(SByteEnum), (sbyte)1, "One" };
            yield return new object[] { typeof(SByteEnum), (sbyte)2, "Two" };
            yield return new object[] { typeof(SByteEnum), sbyte.MaxValue, "Max" };
            yield return new object[] { typeof(SByteEnum), (sbyte)3, null };

            // Byte
            yield return new object[] { typeof(ByteEnum), ByteEnum.Min, "Min" };
            yield return new object[] { typeof(ByteEnum), ByteEnum.One, "One" };
            yield return new object[] { typeof(ByteEnum), ByteEnum.Two, "Two" };
            yield return new object[] { typeof(ByteEnum), ByteEnum.Max, "Max" };
            yield return new object[] { typeof(ByteEnum), byte.MinValue, "Min" };
            yield return new object[] { typeof(ByteEnum), (byte)1, "One" };
            yield return new object[] { typeof(ByteEnum), (byte)2, "Two" };
            yield return new object[] { typeof(ByteEnum), byte.MaxValue, "Max" };
            yield return new object[] { typeof(ByteEnum), (byte)3, null };

            // Int16
            yield return new object[] { typeof(Int16Enum), Int16Enum.Min, "Min" };
            yield return new object[] { typeof(Int16Enum), Int16Enum.One, "One" };
            yield return new object[] { typeof(Int16Enum), Int16Enum.Two, "Two" };
            yield return new object[] { typeof(Int16Enum), Int16Enum.Max, "Max" };
            yield return new object[] { typeof(Int16Enum), short.MinValue, "Min" };
            yield return new object[] { typeof(Int16Enum), (short)1, "One" };
            yield return new object[] { typeof(Int16Enum), (short)2, "Two" };
            yield return new object[] { typeof(Int16Enum), short.MaxValue, "Max" };
            yield return new object[] { typeof(Int16Enum), (short)3, null };

            // UInt16
            yield return new object[] { typeof(UInt16Enum), UInt16Enum.Min, "Min" };
            yield return new object[] { typeof(UInt16Enum), UInt16Enum.One, "One" };
            yield return new object[] { typeof(UInt16Enum), UInt16Enum.Two, "Two" };
            yield return new object[] { typeof(UInt16Enum), UInt16Enum.Max, "Max" };
            yield return new object[] { typeof(UInt16Enum), ushort.MinValue, "Min" };
            yield return new object[] { typeof(UInt16Enum), (ushort)1, "One" };
            yield return new object[] { typeof(UInt16Enum), (ushort)2, "Two" };
            yield return new object[] { typeof(UInt16Enum), ushort.MaxValue, "Max" };
            yield return new object[] { typeof(UInt16Enum), (ushort)3, null };

            // Int32
            yield return new object[] { typeof(Int32Enum), Int32Enum.Min, "Min" };
            yield return new object[] { typeof(Int32Enum), Int32Enum.One, "One" };
            yield return new object[] { typeof(Int32Enum), Int32Enum.Two, "Two" };
            yield return new object[] { typeof(Int32Enum), Int32Enum.Max, "Max" };
            yield return new object[] { typeof(Int32Enum), int.MinValue, "Min" };
            yield return new object[] { typeof(Int32Enum), 1, "One" };
            yield return new object[] { typeof(Int32Enum), 2, "Two" };
            yield return new object[] { typeof(Int32Enum), int.MaxValue, "Max" };
            yield return new object[] { typeof(Int32Enum), 3, null };

            yield return new object[] { typeof(SimpleEnum), 99, null };
            yield return new object[] { typeof(SimpleEnum), SimpleEnum.Red, "Red" };
            yield return new object[] { typeof(SimpleEnum), 1, "Red" };

            // UInt32
            yield return new object[] { typeof(UInt32Enum), UInt32Enum.Min, "Min" };
            yield return new object[] { typeof(UInt32Enum), UInt32Enum.One, "One" };
            yield return new object[] { typeof(UInt32Enum), UInt32Enum.Two, "Two" };
            yield return new object[] { typeof(UInt32Enum), UInt32Enum.Max, "Max" };
            yield return new object[] { typeof(UInt32Enum), uint.MinValue, "Min" };
            yield return new object[] { typeof(UInt32Enum), (uint)1, "One" };
            yield return new object[] { typeof(UInt32Enum), (uint)2, "Two" };
            yield return new object[] { typeof(UInt32Enum), uint.MaxValue, "Max" };
            yield return new object[] { typeof(UInt32Enum), (uint)3, null };

            // Int64
            yield return new object[] { typeof(Int64Enum), Int64Enum.Min, "Min" };
            yield return new object[] { typeof(Int64Enum), Int64Enum.One, "One" };
            yield return new object[] { typeof(Int64Enum), Int64Enum.Two, "Two" };
            yield return new object[] { typeof(Int64Enum), Int64Enum.Max, "Max" };
            yield return new object[] { typeof(Int64Enum), long.MinValue, "Min" };
            yield return new object[] { typeof(Int64Enum), (long)1, "One" };
            yield return new object[] { typeof(Int64Enum), (long)2, "Two" };
            yield return new object[] { typeof(Int64Enum), long.MaxValue, "Max" };
            yield return new object[] { typeof(Int64Enum), (long)3, null };

            // UInt64
            yield return new object[] { typeof(UInt64Enum), UInt64Enum.Min, "Min" };
            yield return new object[] { typeof(UInt64Enum), UInt64Enum.One, "One" };
            yield return new object[] { typeof(UInt64Enum), UInt64Enum.Two, "Two" };
            yield return new object[] { typeof(UInt64Enum), UInt64Enum.Max, "Max" };
            yield return new object[] { typeof(UInt64Enum), ulong.MinValue, "Min" };
            yield return new object[] { typeof(UInt64Enum), 1UL, "One" };
            yield return new object[] { typeof(UInt64Enum), 2UL, "Two" };
            yield return new object[] { typeof(UInt64Enum), ulong.MaxValue, "Max" };
            yield return new object[] { typeof(UInt64Enum), 3UL, null };

#if netcoreapp
            // Char
            yield return new object[] { s_charEnumType, Enum.Parse(s_charEnumType, "Value1"), "Value1" };
            yield return new object[] { s_charEnumType, Enum.Parse(s_charEnumType, "Value2"), "Value2" };
            yield return new object[] { s_charEnumType, (char)1, "Value1"  };
            yield return new object[] { s_charEnumType, (char)2, "Value2" };
            yield return new object[] { s_charEnumType, (char)4, null };

            // Bool
            yield return new object[] { s_boolEnumType, Enum.Parse(s_boolEnumType, "Value1"), "Value1" };
            yield return new object[] { s_boolEnumType, Enum.Parse(s_boolEnumType, "Value2"), "Value2" };
            yield return new object[] { s_boolEnumType, true, "Value1" };
            yield return new object[] { s_boolEnumType, false, "Value2" };
#endif // netcoreapp            
        }

        [TestMethod]
        public void Enum_GetName_Theory()
        {
            foreach (var fact in GetName_TestData())
                Enum_GetName((Type)fact[0], fact[1], (string)fact[2]);
        }

        private static void Enum_GetName(Type enumType, object value, string expected)
        {
            Assert.AreEqual(expected, Enum2.GetName(enumType, value),
                "Invalid name for {0} of type {1}", value, enumType.Name);

            // The format "G" should return the name of the enum case
            if (value.GetType() == enumType)
            {
                ToString_Format((Enum)value, "G", expected);
            }
            else
            {
                Format(enumType, value, "G", expected ?? value.ToString());
            }
        }

        [TestMethod]
        public void Enum_GetName_MultipleMatches()
        {
            // In the case of multiple matches, GetName returns one of them (which one is an implementation detail.)
            string s = Enum2.GetName(typeof(SimpleEnum), 3);
            Assert.IsTrue(s == "Green" || s == "Green_a" || s == "Green_b", "got '{0}'", s);
        }

        [TestMethod]
        public void Enum_GetName_Invalid()
        {
            Type t = typeof(SimpleEnum);
            AssertExtensions.Throws<ArgumentNullException>("enumType", () => Enum2.GetName(null, 1)); // Enum type is null
            AssertExtensions.Throws<ArgumentNullException>("value", () => Enum2.GetName(t, null)); // Value is null

            //AssertExtensions.Throws<ArgumentException>(null, () => Enum2.GetName(typeof(object), 1)); // Enum type is not an enum
            AssertExtensions.Throws<ArgumentException>("enumType", () => Enum2.GetName(typeof(object), 1)); // Enum type is not an enum
            AssertExtensions.Throws<ArgumentException>("value", () => Enum2.GetName(t, "Red")); // Value is not the type of the enum's raw data
            AssertExtensions.Throws<ArgumentException>("value", () => Enum2.GetName(t, (IntPtr)0)); // Value is out of range
        }

        private static IEnumerable<object[]> GetNames_GetValues_TestData()
        {
            // SimpleEnum
            yield return new object[]
            {
                typeof(SimpleEnum),
                new string[] { "Red", "Blue", "Green", "Green_a", "Green_b", "B" },
                new object[] { SimpleEnum.Red, SimpleEnum.Blue, SimpleEnum.Green, SimpleEnum.Green_a, SimpleEnum.Green_b, SimpleEnum.B }
            };

            // SByte
            yield return new object[]
            {
                typeof(SByteEnum),
                //new string[] { "One", "Two", "Max", "Min" },
                //new object[] { SByteEnum.One, SByteEnum.Two, SByteEnum.Max, SByteEnum.Min }
                new string[] { "Min", "One", "Two", "Max" },
                new object[] { SByteEnum.Min, SByteEnum.One, SByteEnum.Two, SByteEnum.Max }
            };

            // Byte
            yield return new object[]
            {
                typeof(ByteEnum),
                new string[] { "Min", "One", "Two", "Max" },
                new object[] { ByteEnum.Min, ByteEnum.One, ByteEnum.Two, ByteEnum.Max }
            };

            // Int16
            yield return new object[]
            {
                typeof(Int16Enum),
                new string[] { "Min", "One", "Two", "Max" },
                new object[] { Int16Enum.Min, Int16Enum.One, Int16Enum.Two, Int16Enum.Max }
            };

            // UInt16
            yield return new object[]
            {
                typeof(UInt16Enum),
                new string[] { "Min", "One", "Two", "Max" },
                new object[] { UInt16Enum.Min, UInt16Enum.One, UInt16Enum.Two, UInt16Enum.Max }
            };

            // Int32
            yield return new object[]
            {
                typeof(Int32Enum),
                new string[] { "Min", "One", "Two", "Max" },
                new object[] { Int32Enum.Min, Int32Enum.One, Int32Enum.Two, Int32Enum.Max }
            };

            // UInt32
            yield return new object[]
            {
                typeof(UInt32Enum),
                new string[] { "Min", "One", "Two", "Max" },
                new object[] { UInt32Enum.Min, UInt32Enum.One, UInt32Enum.Two, UInt32Enum.Max }
            };

            // Int64
            yield return new object[]
            {
                typeof(Int64Enum),
                new string[] { "Min", "One", "Two", "Max" },
                new object[] { Int64Enum.Min, Int64Enum.One, Int64Enum.Two, Int64Enum.Max }
            };

            // UInt64
            yield return new object[]
            {
                typeof(UInt64Enum),
                new string[] { "Min", "One", "Two", "Max" },
                new object[] { UInt64Enum.Min, UInt64Enum.One, UInt64Enum.Two, UInt64Enum.Max }
            };

#if netcoreapp
            // Char
            yield return new object[]
            {
                s_charEnumType,
                new string[] { "Value0x0000", "Value1", "Value2", "Value0x0010", "Value0x0f06", "Value0x1000", "Value0x3000", "Value0x3f06", "Value0x3f16" },
                new object[] { Enum.Parse(s_charEnumType, "Value0x0000"), Enum.Parse(s_charEnumType, "Value1"), Enum.Parse(s_charEnumType, "Value2"), Enum.Parse(s_charEnumType, "Value0x0010"), Enum.Parse(s_charEnumType, "Value0x0f06"), Enum.Parse(s_charEnumType, "Value0x1000"), Enum.Parse(s_charEnumType, "Value0x3000"), Enum.Parse(s_charEnumType, "Value0x3f06"), Enum.Parse(s_charEnumType, "Value0x3f16") }
            };

            // Bool
            yield return new object[]
            {
                s_boolEnumType,
                new string[] { "Value2", "Value1" },
                new object[] { Enum.Parse(s_boolEnumType, "Value2"), Enum.Parse(s_boolEnumType, "Value1") }
            };

            // Single
            yield return new object[]
            {
                s_floatEnumType,
                new string[] { "Value1", "Value2", "Value0x3f06", "Value0x3000", "Value0x0f06", "Value0x1000", "Value0x0000", "Value0x0010", "Value0x3f16" },
                new object[] { Enum.Parse(s_floatEnumType, "Value1"), Enum.Parse(s_floatEnumType, "Value2"), Enum.Parse(s_floatEnumType, "Value0x3f06"), Enum.Parse(s_floatEnumType, "Value0x3000"), Enum.Parse(s_floatEnumType, "Value0x0f06"), Enum.Parse(s_floatEnumType, "Value0x1000"), Enum.Parse(s_floatEnumType, "Value0x0000"), Enum.Parse(s_floatEnumType, "Value0x0010"), Enum.Parse(s_floatEnumType, "Value0x3f16") }
            };

            // Double
            yield return new object[]
            {
                s_doubleEnumType,
                new string[] { "Value1", "Value2", "Value0x3f06", "Value0x3000", "Value0x0f06", "Value0x1000", "Value0x0000", "Value0x0010", "Value0x3f16" },
                new object[] { Enum.Parse(s_doubleEnumType, "Value1"), Enum.Parse(s_doubleEnumType, "Value2"), Enum.Parse(s_doubleEnumType, "Value0x3f06"), Enum.Parse(s_doubleEnumType, "Value0x3000"), Enum.Parse(s_doubleEnumType, "Value0x0f06"), Enum.Parse(s_doubleEnumType, "Value0x1000"), Enum.Parse(s_doubleEnumType, "Value0x0000"), Enum.Parse(s_doubleEnumType, "Value0x0010"), Enum.Parse(s_doubleEnumType, "Value0x3f16") }
            };

            // IntPtr
            yield return new object[]
            {
                s_intPtrEnumType,
                new string[0],
                new object[0]
            };

            // UIntPtr
            yield return new object[]
            {
                s_uintPtrEnumType,
                new string[0],
                new object[0]
            };
#endif // netcoreapp
        }

        [TestMethod]
        public void GetNames_GetValues_Theory()
        {
            foreach (var fact in GetNames_GetValues_TestData())
                GetNames_GetValues((Type)fact[0], (string[])fact[1], (object[])fact[2]);
        }

        private static void GetNames_GetValues(Type enumType, string[] expectedNames, object[] expectedValues)
        {
            //Assert.AreEqual(expectedNames, Enum.GetNames(enumType));
            //Assert.AreEqual(expectedValues, Enum.GetValues(enumType).Cast<object>().ToArray());
            string[] names = Enum2.GetNames(enumType);
            Assert.IsNotNull(names, enumType.ToString());
            Assert.AreEqual(expectedNames.Length, names.Length, enumType.ToString());
            for (int i = 0; i < names.Length; i++)
                Assert.AreEqual(expectedNames[i], names[i], enumType.ToString());

            object[] values = Enum2.GetValues(enumType).Cast<object>().ToArray();
            Assert.IsNotNull(values);
            Assert.AreEqual(expectedValues.Length, values.Length);
            for (int i = 0; i < values.Length; i++)
                Assert.AreEqual(expectedValues[i], values[i], enumType.ToString());
        }

        private static IEnumerable<object[]> HasFlag_TestData()
        {
            // SByte
            yield return new object[] { (SByteEnum)0x36, (SByteEnum)0x30, true };
            yield return new object[] { (SByteEnum)0x36, (SByteEnum)0x06, true };
            yield return new object[] { (SByteEnum)0x36, (SByteEnum)0x10, true };
            yield return new object[] { (SByteEnum)0x36, (SByteEnum)0x00, true };
            yield return new object[] { (SByteEnum)0x36, (SByteEnum)0x36, true };
            yield return new object[] { (SByteEnum)0x36, (SByteEnum)0x05, false };
            yield return new object[] { (SByteEnum)0x36, (SByteEnum)0x46, false };

            // Byte
            yield return new object[] { (ByteEnum)0x36, (ByteEnum)0x30, true };
            yield return new object[] { (ByteEnum)0x36, (ByteEnum)0x06, true };
            yield return new object[] { (ByteEnum)0x36, (ByteEnum)0x10, true };
            yield return new object[] { (ByteEnum)0x36, (ByteEnum)0x00, true };
            yield return new object[] { (ByteEnum)0x36, (ByteEnum)0x36, true };
            yield return new object[] { (ByteEnum)0x36, (ByteEnum)0x05, false };
            yield return new object[] { (ByteEnum)0x36, (ByteEnum)0x46, false };

            // Int16
            yield return new object[] { (Int16Enum)0x3f06, (Int16Enum)0x3000, true };
            yield return new object[] { (Int16Enum)0x3f06, (Int16Enum)0x0f06, true };
            yield return new object[] { (Int16Enum)0x3f06, (Int16Enum)0x1000, true };
            yield return new object[] { (Int16Enum)0x3f06, (Int16Enum)0x0000, true };
            yield return new object[] { (Int16Enum)0x3f06, (Int16Enum)0x3f06, true };
            yield return new object[] { (Int16Enum)0x3f06, (Int16Enum)0x0010, false };
            yield return new object[] { (Int16Enum)0x3f06, (Int16Enum)0x3f16, false };

            // UInt16
            yield return new object[] { (UInt16Enum)0x3f06, (UInt16Enum)0x3000, true };
            yield return new object[] { (UInt16Enum)0x3f06, (UInt16Enum)0x0f06, true };
            yield return new object[] { (UInt16Enum)0x3f06, (UInt16Enum)0x1000, true };
            yield return new object[] { (UInt16Enum)0x3f06, (UInt16Enum)0x0000, true };
            yield return new object[] { (UInt16Enum)0x3f06, (UInt16Enum)0x3f06, true };
            yield return new object[] { (UInt16Enum)0x3f06, (UInt16Enum)0x0010, false };
            yield return new object[] { (UInt16Enum)0x3f06, (UInt16Enum)0x3f16, false };

            // Int32
            yield return new object[] { (Int32Enum)0x3f06, (Int32Enum)0x3000, true };
            yield return new object[] { (Int32Enum)0x3f06, (Int32Enum)0x0f06, true };
            yield return new object[] { (Int32Enum)0x3f06, (Int32Enum)0x1000, true };
            yield return new object[] { (Int32Enum)0x3f06, (Int32Enum)0x0000, true };
            yield return new object[] { (Int32Enum)0x3f06, (Int32Enum)0x3f06, true };
            yield return new object[] { (Int32Enum)0x3f06, (Int32Enum)0x0010, false };
            yield return new object[] { (Int32Enum)0x3f06, (Int32Enum)0x3f16, false };

            // UInt32
            yield return new object[] { (UInt32Enum)0x3f06, (UInt32Enum)0x3000, true };
            yield return new object[] { (UInt32Enum)0x3f06, (UInt32Enum)0x0f06, true };
            yield return new object[] { (UInt32Enum)0x3f06, (UInt32Enum)0x1000, true };
            yield return new object[] { (UInt32Enum)0x3f06, (UInt32Enum)0x0000, true };
            yield return new object[] { (UInt32Enum)0x3f06, (UInt32Enum)0x3f06, true };
            yield return new object[] { (UInt32Enum)0x3f06, (UInt32Enum)0x0010, false };
            yield return new object[] { (UInt32Enum)0x3f06, (UInt32Enum)0x3f16, false };

            // Int64
            yield return new object[] { (Int64Enum)0x3f06, (Int64Enum)0x3000, true };
            yield return new object[] { (Int64Enum)0x3f06, (Int64Enum)0x0f06, true };
            yield return new object[] { (Int64Enum)0x3f06, (Int64Enum)0x1000, true };
            yield return new object[] { (Int64Enum)0x3f06, (Int64Enum)0x0000, true };
            yield return new object[] { (Int64Enum)0x3f06, (Int64Enum)0x3f06, true };
            yield return new object[] { (Int64Enum)0x3f06, (Int64Enum)0x0010, false };
            yield return new object[] { (Int64Enum)0x3f06, (Int64Enum)0x3f16, false };

            // UInt64
            yield return new object[] { (UInt64Enum)0x3f06, (UInt64Enum)0x3000, true };
            yield return new object[] { (UInt64Enum)0x3f06, (UInt64Enum)0x0f06, true };
            yield return new object[] { (UInt64Enum)0x3f06, (UInt64Enum)0x1000, true };
            yield return new object[] { (UInt64Enum)0x3f06, (UInt64Enum)0x0000, true };
            yield return new object[] { (UInt64Enum)0x3f06, (UInt64Enum)0x3f06, true };
            yield return new object[] { (UInt64Enum)0x3f06, (UInt64Enum)0x0010, false };
            yield return new object[] { (UInt64Enum)0x3f06, (UInt64Enum)0x3f16, false };

            //// Char
            //yield return new object[] { Enum.Parse(s_charEnumType, "Value0x3f06"), Enum.Parse(s_charEnumType, "Value0x3000"), true };
            //yield return new object[] { Enum.Parse(s_charEnumType, "Value0x3f06"), Enum.Parse(s_charEnumType, "Value0x0f06"), true };
            //yield return new object[] { Enum.Parse(s_charEnumType, "Value0x3f06"), Enum.Parse(s_charEnumType, "Value0x1000"), true };
            //yield return new object[] { Enum.Parse(s_charEnumType, "Value0x3f06"), Enum.Parse(s_charEnumType, "Value0x0000"), true };
            //yield return new object[] { Enum.Parse(s_charEnumType, "Value0x3f06"), Enum.Parse(s_charEnumType, "Value0x3f06"), true };
            //yield return new object[] { Enum.Parse(s_charEnumType, "Value0x3f06"), Enum.Parse(s_charEnumType, "Value0x0010"), false };
            //yield return new object[] { Enum.Parse(s_charEnumType, "Value0x3f06"), Enum.Parse(s_charEnumType, "Value0x3f16"), false };

            //// Bool
            //yield return new object[] { Enum.Parse(s_boolEnumType, "Value1"), Enum.Parse(s_boolEnumType, "Value1"), true };
            //yield return new object[] { Enum.Parse(s_boolEnumType, "Value1"), Enum.Parse(s_boolEnumType, "Value2"), true };
            //yield return new object[] { Enum.Parse(s_boolEnumType, "Value2"), Enum.Parse(s_boolEnumType, "Value2"), true };
            //yield return new object[] { Enum.Parse(s_boolEnumType, "Value2"), Enum.Parse(s_boolEnumType, "Value1"), false };

            //// Single
            //yield return new object[] { Enum.ToObject(s_floatEnumType, 0x3f06), Enum.ToObject(s_floatEnumType, 0x0000), true };
            //yield return new object[] { Enum.ToObject(s_floatEnumType, 0x3f06), Enum.ToObject(s_floatEnumType, 0x0f06), true };
            //yield return new object[] { Enum.ToObject(s_floatEnumType, 0x3f06), Enum.ToObject(s_floatEnumType, 0x1000), true };
            //yield return new object[] { Enum.ToObject(s_floatEnumType, 0x3f06), Enum.ToObject(s_floatEnumType, 0x0000), true };
            //yield return new object[] { Enum.ToObject(s_floatEnumType, 0x3f06), Enum.ToObject(s_floatEnumType, 0x3f06), true };
            //yield return new object[] { Enum.ToObject(s_floatEnumType, 0x3f06), Enum.ToObject(s_floatEnumType, 0x0010), false };
            //yield return new object[] { Enum.ToObject(s_floatEnumType, 0x3f06), Enum.ToObject(s_floatEnumType, 0x3f16), false };

            //// Double
            //yield return new object[] { Enum.ToObject(s_doubleEnumType, 0x3f06), Enum.ToObject(s_doubleEnumType, 0x0000), true };
            //yield return new object[] { Enum.ToObject(s_doubleEnumType, 0x3f06), Enum.ToObject(s_doubleEnumType, 0x0f06), true };
            //yield return new object[] { Enum.ToObject(s_doubleEnumType, 0x3f06), Enum.ToObject(s_doubleEnumType, 0x1000), true };
            //yield return new object[] { Enum.ToObject(s_doubleEnumType, 0x3f06), Enum.ToObject(s_doubleEnumType, 0x0000), true };
            //yield return new object[] { Enum.ToObject(s_doubleEnumType, 0x3f06), Enum.ToObject(s_doubleEnumType, 0x3f06), true };
            //yield return new object[] { Enum.ToObject(s_doubleEnumType, 0x3f06), Enum.ToObject(s_doubleEnumType, 0x0010), false };
            //yield return new object[] { Enum.ToObject(s_doubleEnumType, 0x3f06), Enum.ToObject(s_doubleEnumType, 0x3f16), false };

            //// IntPtr
            //yield return new object[] { Enum.ToObject(s_intPtrEnumType, 0x3f06), Enum.ToObject(s_intPtrEnumType, 0x0000), true };
            //yield return new object[] { Enum.ToObject(s_intPtrEnumType, 0x3f06), Enum.ToObject(s_intPtrEnumType, 0x0f06), true };
            //yield return new object[] { Enum.ToObject(s_intPtrEnumType, 0x3f06), Enum.ToObject(s_intPtrEnumType, 0x1000), true };
            //yield return new object[] { Enum.ToObject(s_intPtrEnumType, 0x3f06), Enum.ToObject(s_intPtrEnumType, 0x0000), true };
            //yield return new object[] { Enum.ToObject(s_intPtrEnumType, 0x3f06), Enum.ToObject(s_intPtrEnumType, 0x3f06), true };
            //yield return new object[] { Enum.ToObject(s_intPtrEnumType, 0x3f06), Enum.ToObject(s_intPtrEnumType, 0x0010), false };
            //yield return new object[] { Enum.ToObject(s_intPtrEnumType, 0x3f06), Enum.ToObject(s_intPtrEnumType, 0x3f16), false };

            //// UIntPtr
            //yield return new object[] { Enum.ToObject(s_uintPtrEnumType, 0x3f06), Enum.ToObject(s_uintPtrEnumType, 0x0000), true };
            //yield return new object[] { Enum.ToObject(s_uintPtrEnumType, 0x3f06), Enum.ToObject(s_uintPtrEnumType, 0x0f06), true };
            //yield return new object[] { Enum.ToObject(s_uintPtrEnumType, 0x3f06), Enum.ToObject(s_uintPtrEnumType, 0x1000), true };
            //yield return new object[] { Enum.ToObject(s_uintPtrEnumType, 0x3f06), Enum.ToObject(s_uintPtrEnumType, 0x0000), true };
            //yield return new object[] { Enum.ToObject(s_uintPtrEnumType, 0x3f06), Enum.ToObject(s_uintPtrEnumType, 0x3f06), true };
            //yield return new object[] { Enum.ToObject(s_uintPtrEnumType, 0x3f06), Enum.ToObject(s_uintPtrEnumType, 0x0010), false };
            //yield return new object[] { Enum.ToObject(s_uintPtrEnumType, 0x3f06), Enum.ToObject(s_uintPtrEnumType, 0x3f16), false };
        }

        [TestMethod]
        public void Enum_HasFlag_Theory()
        {
            foreach (var fact in HasFlag_TestData())
                HasFlag((Enum)fact[0], (Enum)fact[1], (bool)fact[2]);
        }

        private static void HasFlag(Enum e, Enum flag, bool expected)
        {
            Assert.AreEqual(expected, e.HasFlag(flag));
        }

        [TestMethod]
        public void Enum_HasFlag_Invalid()
        {
            AssertExtensions.Throws<ArgumentNullException>("flag", () => Int32Enum.One.HasFlag(null)); // Flag is null
            AssertExtensions.Throws<ArgumentException>(null, () => Int32Enum.One.HasFlag((SimpleEnum)0x3000)); // Enum is not the same type as the instance
        }

#pragma warning disable 618 // ToString with IFormatProvider is marked as Obsolete.
        private static void ToString_Format(Enum e, string format, string expected)
        {
            if (format.ToUpperInvariant() == "G")
            {
                Assert.AreEqual(expected, e.ToString());
                Assert.AreEqual(expected, e.ToString(string.Empty));
                Assert.AreEqual(expected, e.ToString((string)null));

                Assert.AreEqual(expected, e.ToString((IFormatProvider)null));
            }

            // Format string is case-insensitive.
            Assert.AreEqual(expected, e.ToString(format));
            Assert.AreEqual(expected, e.ToString(format.ToUpperInvariant()));
            Assert.AreEqual(expected, e.ToString(format.ToLowerInvariant()));

            Assert.AreEqual(expected, e.ToString(format, (IFormatProvider)null));

            Format(e.GetType(), e, format, expected);
        }
#pragma warning restore 618
    }
}
