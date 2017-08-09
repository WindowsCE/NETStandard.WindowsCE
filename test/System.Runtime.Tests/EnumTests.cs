using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Runtime.Tests
{
    [TestClass]
    public class EnumTests
    {
        private IEnumerable<int> GetTypeCodeValues(bool extended)
        {
            yield return (int)TypeCode.Empty;
            yield return (int)TypeCode.Object;
            yield return (int)TypeCode.DBNull;
            yield return (int)TypeCode.Boolean;
            yield return (int)TypeCode.Char;
            yield return (int)TypeCode.SByte;
            yield return (int)TypeCode.Byte;
            yield return (int)TypeCode.Int16;
            yield return (int)TypeCode.UInt16;
            yield return (int)TypeCode.Int32;
            yield return (int)TypeCode.UInt32;
            yield return (int)TypeCode.Int64;
            yield return (int)TypeCode.UInt64;
            yield return (int)TypeCode.Single;
            yield return (int)TypeCode.Double;
            yield return (int)TypeCode.Decimal;
            yield return (int)TypeCode.DateTime;
            yield return (int)TypeCode.String;

            if (extended)
                yield return 200;
        }

        private IEnumerable<string> GetTypeCodeNames(bool extended)
        {
            yield return nameof(TypeCode.Empty);
            yield return nameof(TypeCode.Object);
            yield return nameof(TypeCode.DBNull);
            yield return nameof(TypeCode.Boolean);
            yield return nameof(TypeCode.Char);
            yield return nameof(TypeCode.SByte);
            yield return nameof(TypeCode.Byte);
            yield return nameof(TypeCode.Int16);
            yield return nameof(TypeCode.UInt16);
            yield return nameof(TypeCode.Int32);
            yield return nameof(TypeCode.UInt32);
            yield return nameof(TypeCode.Int64);
            yield return nameof(TypeCode.UInt64);
            yield return nameof(TypeCode.Single);
            yield return nameof(TypeCode.Double);
            yield return nameof(TypeCode.Decimal);
            yield return nameof(TypeCode.DateTime);
            yield return nameof(TypeCode.String);

            if (extended)
                yield return "200";
        }

        private IEnumerable<int> GetAttributeTargetsValues(bool extended)
        {
            yield return (int)AttributeTargets.Assembly;
            yield return (int)AttributeTargets.Module;
            yield return (int)AttributeTargets.Class;
            yield return (int)AttributeTargets.Struct;
            yield return (int)AttributeTargets.Enum;
            yield return (int)AttributeTargets.Constructor;
            yield return (int)AttributeTargets.Method;
            yield return (int)AttributeTargets.Property;
            yield return (int)AttributeTargets.Field;
            yield return (int)AttributeTargets.Event;
            yield return (int)AttributeTargets.Interface;
            yield return (int)AttributeTargets.Parameter;
            yield return (int)AttributeTargets.Delegate;
            yield return (int)AttributeTargets.ReturnValue;
            yield return (int)AttributeTargets.GenericParameter;
            yield return (int)AttributeTargets.All;

            if (extended)
            {
                yield return (int)(AttributeTargets.Assembly | AttributeTargets.Module);
                yield return (int)(AttributeTargets.Assembly | AttributeTargets.Class);
                yield return (int)(AttributeTargets.Module | AttributeTargets.Class);
                yield return (int)(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class);
                yield return (int)(AttributeTargets.Assembly | AttributeTargets.Constructor);
                yield return (int)(AttributeTargets.Struct | AttributeTargets.Interface);
                yield return ((int)AttributeTargets.All) * 2;
            }
        }

        private IEnumerable<string> GetAttributeTargetsNames(bool extended)
        {
            const string separator = ", ";

            yield return nameof(AttributeTargets.Assembly);
            yield return nameof(AttributeTargets.Module);
            yield return nameof(AttributeTargets.Class);
            yield return nameof(AttributeTargets.Struct);
            yield return nameof(AttributeTargets.Enum);
            yield return nameof(AttributeTargets.Constructor);
            yield return nameof(AttributeTargets.Method);
            yield return nameof(AttributeTargets.Property);
            yield return nameof(AttributeTargets.Field);
            yield return nameof(AttributeTargets.Event);
            yield return nameof(AttributeTargets.Interface);
            yield return nameof(AttributeTargets.Parameter);
            yield return nameof(AttributeTargets.Delegate);
            yield return nameof(AttributeTargets.ReturnValue);
            yield return nameof(AttributeTargets.GenericParameter);
            yield return nameof(AttributeTargets.All);

            if (extended)
            {
                yield return nameof(AttributeTargets.Assembly) + separator + nameof(AttributeTargets.Module);
                yield return nameof(AttributeTargets.Assembly) + separator + nameof(AttributeTargets.Class);
                yield return nameof(AttributeTargets.Module) + separator + nameof(AttributeTargets.Class);
                yield return nameof(AttributeTargets.Assembly) + separator + nameof(AttributeTargets.Module) + separator + nameof(AttributeTargets.Class);
                yield return nameof(AttributeTargets.Assembly) + separator + nameof(AttributeTargets.Constructor);
                yield return nameof(AttributeTargets.Struct) + separator + nameof(AttributeTargets.Interface);
                yield return (((int)AttributeTargets.All) * 2).ToString();
            }
        }

        private void EnumFormat(Type enumType, object[] values, string[] names)
        {
            Assert.IsNotNull(values);
            Assert.IsNotNull(names);
            Assert.AreEqual(values.Length, names.Length);

            string hexFmt = null;
            TypeCode typeCode = Convert.GetTypeCode(values[0]);
            switch (typeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                    hexFmt = "X2";
                    break;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    hexFmt = "X4";
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    hexFmt = "X8";
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    hexFmt = "X16";
                    break;
                default:
                    Assert.Fail("Invalid type code: {0}", typeCode);
                    break;
            }

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(typeCode, Convert.GetTypeCode(values[i]));

                string fmtG = Mock.System.Enum2.Format(enumType, values[i], "g");
                string fmtG2 = Mock.System.Enum2.Format(enumType, values[i], "G");
                string fmtX = Mock.System.Enum2.Format(enumType, values[i], "x");
                string fmtX2 = Mock.System.Enum2.Format(enumType, values[i], "X");
                string fmtD = Mock.System.Enum2.Format(enumType, values[i], "d");
                string fmtD2 = Mock.System.Enum2.Format(enumType, values[i], "D");
                string fmtF = Mock.System.Enum2.Format(enumType, values[i], "f");
                string fmtF2 = Mock.System.Enum2.Format(enumType, values[i], "F");

                Assert.IsNotNull(fmtG);
                Assert.IsNotNull(fmtG2);
                Assert.IsNotNull(fmtX);
                Assert.IsNotNull(fmtX2);
                Assert.IsNotNull(fmtD);
                Assert.IsNotNull(fmtD2);
                Assert.IsNotNull(fmtF);
                Assert.IsNotNull(fmtF2);

                Assert.AreEqual(fmtG, fmtG2);
                Assert.AreEqual(fmtX, fmtX2);
                Assert.AreEqual(fmtD, fmtD2);
                Assert.AreEqual(fmtF, fmtF2);

                Assert.AreEqual(names[i], fmtG);
                Assert.AreEqual(Convert.ToUInt64(values[i]).ToString(hexFmt), fmtX);
                Assert.AreEqual(values[i].ToString(), fmtD);
                Assert.AreEqual(names[i], fmtF);
            }
        }

        [TestMethod]
        public void EnumFormat_TypeCode()
        {
            object[] values = GetTypeCodeValues(true).Cast<object>().ToArray();
            string[] names = GetTypeCodeNames(true).ToArray();

            EnumFormat(typeof(TypeCode), values, names);
        }

        [TestMethod]
        public void EnumFormat_AttributeTargets()
        {
            object[] values = GetAttributeTargetsValues(true).Cast<object>().ToArray();
            string[] names = GetAttributeTargetsNames(true).ToArray();

            EnumFormat(typeof(AttributeTargets), values, names);
        }

        private void EnumGetNames(Type enumType, string[] expected)
        {
            string[] result = Mock.System.Enum2.GetNames(enumType);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Length, result.Length);

            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void EnumGetNames_TypeCode()
        {
            string[] expected = GetTypeCodeNames(false).ToArray();

            EnumGetNames(typeof(TypeCode), expected);
        }

        [TestMethod]
        public void EnumGetNames_AttributeTargets()
        {
            string[] expected = GetAttributeTargetsNames(false).ToArray();

            EnumGetNames(typeof(AttributeTargets), expected);
        }

        private void EnumGetValues(Type enumType, ulong[] expected)
        {
            Array result = Mock.System.Enum2.GetValues(enumType);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Length, result.Length);

            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(expected[i], Convert.ToUInt64(result.GetValue(i)));
            }
        }

        [TestMethod]
        public void EnumGetValues_TypeCode()
        {
            ulong[] expected = GetTypeCodeValues(false)
                .Select(a => (ulong)a)
                .ToArray();

            EnumGetValues(typeof(TypeCode), expected);
        }

        [TestMethod]
        public void EnumGetValues_AttributeTargets()
        {
            ulong[] expected = GetAttributeTargetsValues(false)
                .Select(a => (ulong)a)
                .ToArray();

            EnumGetValues(typeof(AttributeTargets), expected);
        }

        private void EnumGetName(Type enumType, object[] values, string[] names)
        {
            Assert.IsNotNull(values);
            Assert.IsNotNull(names);
            Assert.AreEqual(values.Length, names.Length);

            if (values.Length < 1)
                Assert.Fail("Values cannot be empty");

            for (int i = 0; i < values.Length; i++)
            {
                string name = Mock.System.Enum2.GetName(enumType, values[i]);
                Assert.AreEqual(names[i], name);
            }
        }

        [TestMethod]
        public void EnumGetName_TypeCode()
        {
            object[] values = GetTypeCodeValues(true)
                .Cast<object>()
                .ToArray();
            string[] names = GetTypeCodeNames(false).ToArray();
            Assert.IsFalse(names.Length == values.Length, "Should be specified non-named elements for testing");
            names = names
                .Concat(new string[values.Length - names.Length])
                .ToArray();

            EnumGetName(typeof(TypeCode), values, names);
        }

        [TestMethod]
        public void EnumGetName_AttributeTargets()
        {
            object[] values = GetAttributeTargetsValues(true)
                .Cast<object>()
                .ToArray();
            string[] names = GetAttributeTargetsNames(false).ToArray();
            Assert.IsFalse(names.Length == values.Length, "Should be specified non-named elements for testing");
            names = names
                .Concat(new string[values.Length - names.Length])
                .ToArray();

            EnumGetName(typeof(AttributeTargets), values, names);
        }

        [TestMethod]
        public void EnumHasFlag()
        {
            Assert.IsTrue(Mock.System.Enum2.HasFlag(
                AttributeTargets.All,
                AttributeTargets.All));
            Assert.IsTrue(Mock.System.Enum2.HasFlag(
                AttributeTargets.All,
                AttributeTargets.Assembly));
            Assert.IsTrue(Mock.System.Enum2.HasFlag(
                AttributeTargets.All,
                AttributeTargets.Constructor));
            Assert.IsTrue(Mock.System.Enum2.HasFlag(
                AttributeTargets.Assembly | AttributeTargets.Module,
                AttributeTargets.Assembly));
            Assert.IsTrue(Mock.System.Enum2.HasFlag(
                AttributeTargets.Assembly | AttributeTargets.Module,
                AttributeTargets.Module));
            Assert.IsFalse(Mock.System.Enum2.HasFlag(
                AttributeTargets.Assembly,
                AttributeTargets.Module));
            Assert.IsFalse(Mock.System.Enum2.HasFlag(
                AttributeTargets.Assembly | AttributeTargets.Module,
                AttributeTargets.Class));

            Assert.IsFalse(Mock.System.Enum2.HasFlag(
                Reflection.MemberTypes.Method,
                Reflection.MemberTypes.All));
            Assert.IsTrue(Mock.System.Enum2.HasFlag(
                Reflection.MemberTypes.All,
                Reflection.MemberTypes.Method));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = false)]
        public void EnumHasFlag_TypeMismatch()
        {
            Mock.System.Enum2.HasFlag(AttributeTargets.All, TypeCode.Boolean);
        }
    }
}
