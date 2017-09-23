using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if !WindowsCE
using Mock.System;
#endif

namespace System.Runtime.Tests
{
    [TestClass]
    public class StringTests
    {
        private static IEnumerable<AttributeTargets> EnumerableEnum()
        {
            yield return AttributeTargets.Assembly;
            yield return AttributeTargets.Module;
            yield return AttributeTargets.Class;
            yield return AttributeTargets.Struct;
            yield return AttributeTargets.Enum;
            yield return AttributeTargets.Constructor;
            yield return AttributeTargets.Method;
            yield return AttributeTargets.Property;
            yield return AttributeTargets.Field;
            yield return AttributeTargets.Event;
            yield return AttributeTargets.Interface;
            yield return AttributeTargets.Parameter;
            yield return AttributeTargets.Delegate;
            yield return AttributeTargets.ReturnValue;
            yield return AttributeTargets.GenericParameter;
            yield return AttributeTargets.All;
        }

        private static IEnumerable<string> EnumerableString()
        {
            yield return AttributeTargets.Assembly.ToString();
            yield return AttributeTargets.Module.ToString();
            yield return AttributeTargets.Class.ToString();
            yield return AttributeTargets.Struct.ToString();
            yield return AttributeTargets.Enum.ToString();
            yield return AttributeTargets.Constructor.ToString();
            yield return AttributeTargets.Method.ToString();
            yield return AttributeTargets.Property.ToString();
            yield return AttributeTargets.Field.ToString();
            yield return AttributeTargets.Event.ToString();
            yield return AttributeTargets.Interface.ToString();
            yield return AttributeTargets.Parameter.ToString();
            yield return AttributeTargets.Delegate.ToString();
            yield return AttributeTargets.ReturnValue.ToString();
            yield return AttributeTargets.GenericParameter.ToString();
            yield return AttributeTargets.All.ToString();
        }

        [TestMethod]
        public void StringConcatEnumerable()
        {
            StringBuilder sbExpected = new StringBuilder();
            foreach (var item in EnumerableEnum())
                sbExpected.Append(item.ToString());

            string result = String2.Concat(EnumerableEnum());
            string expected = sbExpected.ToString();
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Length, result.Length);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void StringConcatEnumerableString()
        {
            StringBuilder sbExpected = new StringBuilder();
            foreach (var item in EnumerableString())
                sbExpected.Append(item);

            string result = String2.Concat(EnumerableString());
            string expected = sbExpected.ToString();
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Length, result.Length);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
#if WindowsCE
        [ExpectedException(typeof(ArgumentNullException))]
#else
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
#endif
        public void StringConcatEnumerableNull()
        {
            String2.Concat<AttributeTargets>(null);
        }

        [TestMethod]
#if WindowsCE
        [ExpectedException(typeof(ArgumentNullException))]
#else
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
#endif
        public void StringConcatEnumerableStringNull()
        {
            String2.Concat((IEnumerable<string>)null);
        }

        [TestMethod]
        public void StringIsNullOrWhiteSpace()
        {
            Assert.IsTrue(String2.IsNullOrWhiteSpace(null));
            Assert.IsTrue(String2.IsNullOrWhiteSpace(""));
            Assert.IsTrue(String2.IsNullOrWhiteSpace(string.Empty));
            Assert.IsTrue(String2.IsNullOrWhiteSpace(" "));
            Assert.IsTrue(String2.IsNullOrWhiteSpace("  "));
            Assert.IsTrue(String2.IsNullOrWhiteSpace("   "));
            Assert.IsTrue(String2.IsNullOrWhiteSpace("    "));
            Assert.IsTrue(String2.IsNullOrWhiteSpace("     "));
            Assert.IsFalse(String2.IsNullOrWhiteSpace("a"));
            Assert.IsFalse(String2.IsNullOrWhiteSpace("     a"));
            Assert.IsFalse(String2.IsNullOrWhiteSpace("a     "));
            Assert.IsFalse(String2.IsNullOrWhiteSpace("     a     "));
        }

        [TestMethod]
        public void StringJoinEnumerable()
        {
            const string separator = ", ";
            string expected = string.Join(separator,
                EnumerableEnum()
                .Select(a => a.ToString())
                .ToArray());

            string result = String2.Join(separator, EnumerableEnum());
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Length, result.Length);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void StringJoinEnumerableString()
        {
            const string separator = ", ";
            string expected = string.Join(separator,
                EnumerableString()
                .ToArray());

            string result = String2.Join(separator, EnumerableString());
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Length, result.Length);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void StringRemove()
        {
            //                   00000000000111111111222222
            //                   01234567890123456789012345
            const string text = "Lorem ipsum dolor sit amet";
            const string expected = "Lorem ipsum";

            Assert.AreEqual(expected, String2.Remove(text, 11));
            Assert.AreEqual("", String2.Remove(text, 0));
            Assert.AreEqual(text.Substring(0, text.Length - 1), String2.Remove(text, text.Length - 1));
        }

        [TestMethod]
        public void StringSplitByChars()
        {
            const string text =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Cras convallis, nulla eget faucibus sagittis, dolor.";
            string[] expected1 = {
                "Lorem ipsum dolor sit amet",
                " consectetur adipiscing elit",
                " Cras convallis",
                " nulla eget faucibus sagittis",
                " dolor",
                ""
            };

            string[] result = String2.Split(text, new char[] { ',', '.' }, StringSplitOptions.None);
            Assert.AreEqual(expected1.Length, result.Length);
            if (!result.SequenceEqual(expected1))
                Assert.Fail("String split by chars test #1");

            string[] expected2 = new string[expected1.Length - 1];
            Array.Copy(expected1, expected2, expected2.Length);
            result = String2.Split(text, new char[] { ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expected2.Length, result.Length);
            if (!result.SequenceEqual(expected2))
                Assert.Fail("String split by chars test #2");
        }

        [TestMethod]
        public void StringSplitByCharsCount()
        {
            const string text =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Cras convallis, nulla eget faucibus sagittis, dolor.";
            string[] expected1 = {
                "Lorem ipsum dolor sit amet",
                " consectetur adipiscing elit",
                " Cras convallis, nulla eget faucibus sagittis, dolor.",
            };

            string[] result = String2.Split(text, new char[] { ',', '.' }, 3, StringSplitOptions.None);
            Assert.AreEqual(expected1.Length, result.Length);
            if (!result.SequenceEqual(expected1))
                Assert.Fail("String split count by chars test #1");

            result = String2.Split(text, new char[] { ',', '.' }, 3, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expected1.Length, result.Length);
            if (!result.SequenceEqual(expected1))
                Assert.Fail("String split count by chars test #2");

            result = String2.Split(text, new char[] { ',', '.' }, 1, StringSplitOptions.None);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(text, result[0], "String split count by chars test #3");

            result = String2.Split(text, new char[] { ',', '.' }, 1, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(text, result[0], "String split count by chars test #4");
        }

        [TestMethod]
        public void StringSplitByStrings()
        {
            const string text =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Cras convallis, nulla eget faucibus sagittis, dolor.";
            string[] expected1 = {
                "Lorem ipsum dolor sit amet",
                "consectetur adipiscing elit",
                "Cras convallis",
                "nulla eget faucibus sagittis",
                "dolor",
                ""
            };

            string[] result = String2.Split(text, new string[] { ", ", ". ", ",", "." }, StringSplitOptions.None);
            Assert.AreEqual(expected1.Length, result.Length);
            if (!result.SequenceEqual(expected1))
                Assert.Fail("String split by string test #1");

            string[] expected2 = new string[expected1.Length - 1];
            Array.Copy(expected1, expected2, expected2.Length);
            result = String2.Split(text, new string[] { ", ", ". ", ",", "." }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expected2.Length, result.Length);
            if (!result.SequenceEqual(expected2))
                Assert.Fail("String split by string test #2");
        }

        [TestMethod]
        public void StringSplitByStringsCount()
        {
            const string text =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Cras convallis, nulla eget faucibus sagittis, dolor.";
            string[] expected1 = {
                "Lorem ipsum dolor sit amet",
                "consectetur adipiscing elit",
                "Cras convallis, nulla eget faucibus sagittis, dolor.",
            };

            string[] result = String2.Split(text, new string[] { ", ", ". ", ",", "." }, 3, StringSplitOptions.None);
            Assert.AreEqual(expected1.Length, result.Length);
            if (!result.SequenceEqual(expected1))
                Assert.Fail("String split count by string test #1");

            result = String2.Split(text, new string[] { ", ", ". ", ",", "." }, 3, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expected1.Length, result.Length);
            if (!result.SequenceEqual(expected1))
                Assert.Fail("String split count by string test #2");

            result = String2.Split(text, new string[] { ", ", ". ", ",", "." }, 1, StringSplitOptions.None);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(text, result[0], "String split count by string test #3");

            result = String2.Split(text, new string[] { ", ", ". ", ",", "." }, 1, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(text, result[0], "String split count by string test #4");
        }

        [TestMethod]
        public void StringToCharArrayRange()
        {
            //                   00000000000111111111222222
            //                   01234567890123456789012345
            const string text = "Lorem ipsum dolor sit amet";
            char[] expected = "ipsum dolor".ToCharArray();

            char[] result = String2.ToCharArray(text, 6, 11);
            Assert.AreEqual(expected.Length, result.Length);
            if (!result.SequenceEqual(expected))
                Assert.Fail("String range char array test #1");

            expected = "Lorem ipsum".ToCharArray();
            result = String2.ToCharArray(text, 0, 11);
            Assert.AreEqual(expected.Length, result.Length);
            if (!result.SequenceEqual(expected))
                Assert.Fail("String range char array test #2");

            expected = "sit amet".ToCharArray();
            result = String2.ToCharArray(text, 18, 8);
            Assert.AreEqual(expected.Length, result.Length);
            if (!result.SequenceEqual(expected))
                Assert.Fail("String range char array test #3");
        }

        [TestMethod]
        public void StringToUpperAndLowerInvariant()
        {
            //                   00000000000111111111222222
            //                   01234567890123456789012345
            const string text = "Lorem ipsum dolor sit amet";
            const string expectedUpper = "LOREM IPSUM DOLOR SIT AMET";
            const string expectedLower = "lorem ipsum dolor sit amet";

            Assert.AreEqual(expectedUpper, String2.ToUpperInvariant(text));
            Assert.AreEqual(expectedLower, String2.ToLowerInvariant(text));
        }
    }
}
