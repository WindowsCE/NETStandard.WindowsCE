// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.CompilerServices;

namespace Tests.Runtime.CompilerServices
{
    [TestClass]
    public partial class RuntimeHelpersTests
    {
        [TestMethod]
#if !WindowsCE
        [ExpectedException(typeof(PlatformNotSupportedException))]
#endif
        public void RuntimeHelpers_GetHashCodeTest()
        {
            // Int32 RuntimeHelpers2.GetHashCode(Object)
            object obj1 = new object();
            int h1 = RuntimeHelpers2.GetHashCode(obj1);
            int h2 = RuntimeHelpers2.GetHashCode(obj1);

            Assert.AreEqual(h1, h2);

            object obj2 = new object();
            int h3 = RuntimeHelpers2.GetHashCode(obj2);
            Assert.AreNotEqual(h1, h3); // Could potentially clash but very unlikely

            int i123 = 123;
            int h4 = RuntimeHelpers2.GetHashCode(i123);
            Assert.AreNotEqual(i123.GetHashCode(), h4);

            int h5 = RuntimeHelpers2.GetHashCode(null);
            Assert.AreEqual(h5, 0);
        }

        public struct TestStruct
        {
            public int i1;
            public int i2;
            public override bool Equals(object obj)
            {
                if (!(obj is TestStruct))
                    return false;

                TestStruct that = (TestStruct)obj;

                return i1 == that.i1 && i2 == that.i2;
            }

            public override int GetHashCode()
            {
                return i1 ^ i2;
            }
        }

        [TestMethod]
        public unsafe void RuntimeHelpers_GetObjectValue()
        {
            // Object RuntimeHelpers2.GetObjectValue(Object)
            TestStruct t = new TestStruct() { i1 = 2, i2 = 4 };
            object tOV = RuntimeHelpers2.GetObjectValue(t);
            Assert.AreEqual(t, (TestStruct)tOV);

            object o = new object();
            object oOV = RuntimeHelpers2.GetObjectValue(o);
            Assert.AreEqual(o, oOV);

            int i = 3;
            object iOV = RuntimeHelpers2.GetObjectValue(i);
            Assert.AreEqual(i, (int)iOV);
        }

        [TestMethod]
        public unsafe void RuntimeHelpers_OffsetToStringData()
        {
            // RuntimeHelpers2.OffsetToStringData
            char[] expectedValues = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };
            string s = "abcdef";

            fixed (char* values = s) // Compiler will use OffsetToStringData with fixed statements
            {
                for (int i = 0; i < expectedValues.Length; i++)
                {
                    Assert.AreEqual(expectedValues[i], values[i]);
                }
            }
        }

        [TestMethod]
        public void RuntimeHelpers_InitializeArray()
        {
            // Void RuntimeHelpers2.InitializeArray(Array, RuntimeFieldHandle)
            char[] expected = new char[] { 'a', 'b', 'c' }; // Compiler will use RuntimeHelpers2.InitializeArrary these
        }

        [TestMethod]
#if WindowsCE
        [ExpectedException(typeof(NotSupportedException))]
#endif
        public void RuntimeHelpers_RunClassConstructor()
        {
            RuntimeTypeHandle t = typeof(HasCctor).TypeHandle;
            RuntimeHelpers2.RunClassConstructor(t);
            Assert.AreEqual(HasCctorReceiver.S, "Hello");
            return;
        }

        internal class HasCctor
        {
            static HasCctor()
            {
                HasCctorReceiver.S = "Hello" + (Guid.NewGuid().ToString().Substring(string.Empty.Length, 0));  // Make sure the preinitialization optimization doesn't eat this.
            }
        }

        internal class HasCctorReceiver
        {
            public static string S;
        }
    }
}