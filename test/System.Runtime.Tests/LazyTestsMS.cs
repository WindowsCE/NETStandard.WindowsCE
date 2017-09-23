// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

#if !WindowsCE
using Mock.System;
using Mock.System.Threading;
#endif

namespace Tests
{
    [TestClass]
    public class LazyTestsMS
    {
        [TestMethod]
        public void Lazy_Ctor()
        {
            var lazyString = new Lazy<string>();
            VerifyLazy(lazyString, "", false, false);

            var lazyObject = new Lazy<int>();
            VerifyLazy(lazyObject, 0, true, false);
        }

        [TestMethod]
        public void Lazy_Ctor_Bool_True()
        {
            Ctor_Bool(true);
        }

        [TestMethod]
        public void Lazy_Ctor_Bool_False()
        {
            Ctor_Bool(false);
        }

        private static void Ctor_Bool(bool isThreadSafe)
        {
            var lazyString = new Lazy<string>(isThreadSafe);
            VerifyLazy(lazyString, "", false, false);
        }

        [TestMethod]
        public void Lazy_Ctor_ValueFactory()
        {
            var lazyString = new Lazy<string>(() => "foo");
            VerifyLazy(lazyString, "foo", true, false);

            var lazyInt = new Lazy<int>(() => 1);
            VerifyLazy(lazyInt, 1, true, false);
        }

        [TestMethod]
        public void Lazy_Ctor_ValueFactory_NullValueFactory_ThrowsArguentNullException()
        {
            AssertExtensions.Throws<ArgumentNullException>("valueFactory", () => new Lazy<object>(null)); // Value factory is null
        }

        [TestMethod]
        public void Lazy_Ctor_LazyThreadSafetyMode()
        {
            var lazyString = new Lazy<string>(LazyThreadSafetyMode.PublicationOnly);
            VerifyLazy(lazyString, "", false, false);
        }

        [TestMethod]
        public void Lazy_Ctor_LazyThreadSafetyMode_InvalidMode_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("mode", () => new Lazy<string>(LazyThreadSafetyMode.None - 1)); // Invalid thread saftety mode
            AssertExtensions.Throws<ArgumentOutOfRangeException>("mode", () => new Lazy<string>(LazyThreadSafetyMode.ExecutionAndPublication + 1)); // Invalid thread saftety mode
        }

        [TestMethod]
        public void Lazy_Ctor_ValueFactor_Bool_True()
        {
            Ctor_ValueFactor_Bool(true);
        }

        [TestMethod]
        public void Lazy_Ctor_ValueFactor_Bool_False()
        {
            Ctor_ValueFactor_Bool(false);
        }

        private static void Ctor_ValueFactor_Bool(bool isThreadSafe)
        {
            var lazyString = new Lazy<string>(() => "foo", isThreadSafe);
            VerifyLazy(lazyString, "foo", true, false);
        }

        [TestMethod]
        public void Lazy_Ctor_ValueFactory_Bool_NullValueFactory_ThrowsArgumentNullException()
        {
            AssertExtensions.Throws<ArgumentNullException>("valueFactory", () => new Lazy<object>(null, false)); // Value factory is null
        }

        [TestMethod]
        public void Lazy_Ctor_ValueFactor_LazyThreadSafetyMode()
        {
            var lazyString = new Lazy<string>(() => "foo", LazyThreadSafetyMode.PublicationOnly);
            VerifyLazy(lazyString, "foo", true, false);

            var lazyInt = new Lazy<int>(() => 1, LazyThreadSafetyMode.PublicationOnly);
            VerifyLazy(lazyInt, 1, true, false);
        }

        [TestMethod]
        public void Lazy_Ctor_ValueFactor_LazyThreadSafetyMode_Invalid()
        {
            AssertExtensions.Throws<ArgumentNullException>("valueFactory", () => new Lazy<object>(null, LazyThreadSafetyMode.PublicationOnly)); // Value factory is null

            AssertExtensions.Throws<ArgumentOutOfRangeException>("mode", () => new Lazy<string>(() => "foo", LazyThreadSafetyMode.None - 1)); // Invalid thread saftety mode
            AssertExtensions.Throws<ArgumentOutOfRangeException>("mode", () => new Lazy<string>(() => "foof", LazyThreadSafetyMode.ExecutionAndPublication + 1)); // Invalid thread saftety mode
        }

        [TestMethod]
        public void Lazy_ToString_DoesntForceAllocation()
        {
            var lazy = new Lazy<object>(() => 1);
            Assert.AreNotEqual("1", lazy.ToString());
            Assert.IsFalse(lazy.IsValueCreated);

            object tmp = lazy.Value;
            Assert.AreEqual("1", lazy.ToString());
        }

        private static void Value_Invalid_Impl<T>(ref Lazy<T> x, Lazy<T> lazy)
        {
            x = lazy;
            AssertExtensions.Throws<InvalidOperationException>(() => lazy.Value);
        }

        [TestMethod]
        public void Lazy_Value_Invalid()
        {
            Lazy<int> x = null;
            Func<int> f = () => x.Value;

            Value_Invalid_Impl(ref x, new Lazy<int>(f));
            Value_Invalid_Impl(ref x, new Lazy<int>(f, true));
            Value_Invalid_Impl(ref x, new Lazy<int>(f, false));
            Value_Invalid_Impl(ref x, new Lazy<int>(f, LazyThreadSafetyMode.ExecutionAndPublication));
            Value_Invalid_Impl(ref x, new Lazy<int>(f, LazyThreadSafetyMode.None));

            // When used with LazyThreadSafetyMode.PublicationOnly this causes a stack overflow
            // Value_Invalid_Impl(ref x, new Lazy<int>(f, LazyThreadSafetyMode.PublicationOnly));
        }

        public class InitiallyExceptionThrowingCtor
        {
            public static int counter = 0;
            public static int getValue()
            {
                if (++counter < 5)
                    throw new Exception();
                else
                    return counter;
            }

            public int Value { get; set; }

            public InitiallyExceptionThrowingCtor()
            {
                Value = getValue();
            }
        }

        private static IEnumerable<Lazy<InitiallyExceptionThrowingCtor>> Ctor_ExceptionRecovery_MemberData()
        {
            yield return new Lazy<InitiallyExceptionThrowingCtor>();
            yield return new Lazy<InitiallyExceptionThrowingCtor>(true);
            yield return new Lazy<InitiallyExceptionThrowingCtor>(false);
            yield return new Lazy<InitiallyExceptionThrowingCtor>(LazyThreadSafetyMode.ExecutionAndPublication);
            yield return new Lazy<InitiallyExceptionThrowingCtor>(LazyThreadSafetyMode.None);
            yield return new Lazy<InitiallyExceptionThrowingCtor>(LazyThreadSafetyMode.PublicationOnly);
        }

        //
        // Do not use [Theory]. XUnit argument formatter can invoke the lazy.Value property underneath you and ruin the assumptions
        // made by the test.
        //
        [TestMethod]
        public void Lazy_Ctor_ExceptionRecovery()
        {
            foreach (Lazy<InitiallyExceptionThrowingCtor> lazy in Ctor_ExceptionRecovery_MemberData())
            {
                InitiallyExceptionThrowingCtor.counter = 0;
                InitiallyExceptionThrowingCtor result = null;
                for (int i = 0; i < 10; ++i)
                {
                    try
                    { result = lazy.Value; }
                    catch (Exception) { }
                }
                Assert.AreEqual(5, result.Value);
            }
        }

        private static void Value_ExceptionRecovery_IntImpl(Lazy<int> lazy, ref int counter, int expected)
        {
            counter = 0;
            int result = 0;
            for (var i = 0; i < 10; ++i)
            {
                try { result = lazy.Value; }
                catch (Exception) { }
            }
            Assert.AreEqual(result, expected);
        }

        private static void Value_ExceptionRecovery_StringImpl(Lazy<string> lazy, ref int counter, string expected)
        {
            counter = 0;
            var result = default(string);
            for (var i = 0; i < 10; ++i)
            {
                try { result = lazy.Value; }
                catch (Exception) { }
            }
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Lazy_Value_ExceptionRecovery()
        {
            int counter = 0; // set in test function

            var fint = new Func<int>(() => { if (++counter < 5) throw new Exception(); else return counter; });
            var fobj = new Func<string>(() => { if (++counter < 5) throw new Exception(); else return counter.ToString(); });

            Value_ExceptionRecovery_IntImpl(new Lazy<int>(fint), ref counter, 0);
            Value_ExceptionRecovery_IntImpl(new Lazy<int>(fint, true), ref counter, 0);
            Value_ExceptionRecovery_IntImpl(new Lazy<int>(fint, false), ref counter, 0);
            Value_ExceptionRecovery_IntImpl(new Lazy<int>(fint, LazyThreadSafetyMode.ExecutionAndPublication), ref counter, 0);
            Value_ExceptionRecovery_IntImpl(new Lazy<int>(fint, LazyThreadSafetyMode.None), ref counter, 0);
            Value_ExceptionRecovery_IntImpl(new Lazy<int>(fint, LazyThreadSafetyMode.PublicationOnly), ref counter, 5);

            Value_ExceptionRecovery_StringImpl(new Lazy<string>(fobj), ref counter, null);
            Value_ExceptionRecovery_StringImpl(new Lazy<string>(fobj, true), ref counter, null);
            Value_ExceptionRecovery_StringImpl(new Lazy<string>(fobj, false), ref counter, null);
            Value_ExceptionRecovery_StringImpl(new Lazy<string>(fobj, LazyThreadSafetyMode.ExecutionAndPublication), ref counter, null);
            Value_ExceptionRecovery_StringImpl(new Lazy<string>(fobj, LazyThreadSafetyMode.None), ref counter, null);
            Value_ExceptionRecovery_StringImpl(new Lazy<string>(fobj, LazyThreadSafetyMode.PublicationOnly), ref counter, 5.ToString());
        }

        class MyException
            : Exception
        {
            public int Value { get; set; }

            public MyException(int value)
            {
                Value = value;
            }
        }

        public class ExceptionInCtor
        {
            public ExceptionInCtor() : this(99) { }

            public ExceptionInCtor(int value)
            {
                throw new MyException(value);
            }
        }

        public static IEnumerable<object[]> Value_Func_Exception_MemberData()
        {
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, true) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, false) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, LazyThreadSafetyMode.ExecutionAndPublication) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, LazyThreadSafetyMode.None) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, LazyThreadSafetyMode.PublicationOnly) };
        }

        [TestMethod]
        public void Lazy_Value_Func_Exception_MemberDataTests()
        {
            foreach (var md in Value_Func_Exception_MemberData())
                Value_Func_Exception((Lazy<int>)(md[0]));
        }

        private static void Value_Func_Exception(Lazy<int> lazy)
        {
            AssertExtensions.Throws<MyException>(() => lazy.Value);
        }

        public static IEnumerable<object[]> Value_FuncCtor_Exception_MemberData()
        {
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99)) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), true) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), false) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), LazyThreadSafetyMode.ExecutionAndPublication) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), LazyThreadSafetyMode.None) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), LazyThreadSafetyMode.PublicationOnly) };
        }

        [TestMethod]
        public void Lazy_Value_FuncCtor_Exception_MemberDataTests()
        {
            foreach (var md in Value_FuncCtor_Exception_MemberData())
                Value_FuncCtor_Exception((Lazy<ExceptionInCtor>)(md[0]));
        }

        private static void Value_FuncCtor_Exception(Lazy<ExceptionInCtor> lazy)
        {
            AssertExtensions.Throws<MyException>(() => lazy.Value);
        }

        private static IEnumerable<object[]> Value_TargetInvocationException_MemberData()
        {
            yield return new object[] { new Lazy<ExceptionInCtor>() };
            yield return new object[] { new Lazy<ExceptionInCtor>(true) };
            yield return new object[] { new Lazy<ExceptionInCtor>(false) };
            yield return new object[] { new Lazy<ExceptionInCtor>(LazyThreadSafetyMode.ExecutionAndPublication) };
            yield return new object[] { new Lazy<ExceptionInCtor>(LazyThreadSafetyMode.None) };
            yield return new object[] { new Lazy<ExceptionInCtor>(LazyThreadSafetyMode.PublicationOnly) };
        }

        [TestMethod]
        public void Lazy_Value_TargetInvocationException_MemberDataTests()
        {
            foreach (var md in Value_TargetInvocationException_MemberData())
                Value_TargetInvocationException((Lazy<ExceptionInCtor>)(md[0]));
        }

        private static void Value_TargetInvocationException(Lazy<ExceptionInCtor> lazy)
        {
            AssertExtensions.Throws<System.Reflection.TargetInvocationException>(() => lazy.Value);
        }

        private static IEnumerable<object[]> Exceptions_Func_Idempotent_MemberData()
        {
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, true) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, false) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, LazyThreadSafetyMode.ExecutionAndPublication) };
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, LazyThreadSafetyMode.None) };
        }

        [TestMethod]
        public void Lazy_Exceptions_Func_Idempotent_MemberDataTests()
        {
            foreach (var md in Exceptions_Func_Idempotent_MemberData())
                Exceptions_Func_Idempotent((Lazy<int>)(md[0]));
        }

        private static void Exceptions_Func_Idempotent(Lazy<int> x)
        {
            var e = AssertExtensions.ThrowsAny<Exception>(() => x.Value);
            Assert.AreSame(e, AssertExtensions.ThrowsAny<Exception>(() => x.Value));
        }

        private static IEnumerable<object[]> Exceptions_Ctor_Idempotent_MemberData()
        {
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99)) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), true) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), false) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), LazyThreadSafetyMode.ExecutionAndPublication) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), LazyThreadSafetyMode.None) };
        }

        [TestMethod]
        public void Lazy_Exceptions_Ctor_Idempotent_MemberDataTests()
        {
            foreach (var md in Exceptions_Ctor_Idempotent_MemberData())
                Exceptions_Ctor_Idempotent((Lazy<ExceptionInCtor>)(md[0]));
        }

        private static void Exceptions_Ctor_Idempotent(Lazy<ExceptionInCtor> x)
        {
            var e = AssertExtensions.ThrowsAny<Exception>(() => x.Value);
            Assert.AreSame(e, AssertExtensions.ThrowsAny<Exception>(() => x.Value));
        }

        private static IEnumerable<object[]> Exceptions_Func_NotIdempotent_MemberData()
        {
            yield return new object[] { new Lazy<int>(() => { throw new MyException(99); }, LazyThreadSafetyMode.PublicationOnly) };
        }

        private static IEnumerable<object[]> Exceptions_Ctor_NotIdempotent_MemberData()
        {
            yield return new object[] { new Lazy<ExceptionInCtor>() };
            yield return new object[] { new Lazy<ExceptionInCtor>(true) };
            yield return new object[] { new Lazy<ExceptionInCtor>(false) };
            yield return new object[] { new Lazy<ExceptionInCtor>(LazyThreadSafetyMode.ExecutionAndPublication) };
            yield return new object[] { new Lazy<ExceptionInCtor>(LazyThreadSafetyMode.None) };
            yield return new object[] { new Lazy<ExceptionInCtor>(LazyThreadSafetyMode.PublicationOnly) };
            yield return new object[] { new Lazy<ExceptionInCtor>(() => new ExceptionInCtor(99), LazyThreadSafetyMode.PublicationOnly) };
        }

        [TestMethod]
        public void Lazy_Exceptions_Func_NotIdempotent_MemberDataTests()
        {
            foreach (var md in Exceptions_Func_NotIdempotent_MemberData())
                Exceptions_Func_NotIdempotent((Lazy<int>)(md[0]));
        }

        private static void Exceptions_Func_NotIdempotent(Lazy<int> x)
        {
            var e = AssertExtensions.ThrowsAny<Exception>(() => x.Value);
            Assert.AreNotSame(e, AssertExtensions.ThrowsAny<Exception>(() => x.Value));
        }

        [TestMethod]
        public void Lazy_Exceptions_Ctor_NotIdempotent_MemberDataTests()
        {
            foreach (var md in Exceptions_Ctor_NotIdempotent_MemberData())
                Exceptions_Ctor_NotIdempotent((Lazy<ExceptionInCtor>)(md[0]));
        }

        private static void Exceptions_Ctor_NotIdempotent(Lazy<ExceptionInCtor> x)
        {
            var e = AssertExtensions.ThrowsAny<Exception>(() => x.Value);
            Assert.AreNotSame(e, AssertExtensions.ThrowsAny<Exception>(() => x.Value));
        }

        [TestMethod]
        public void Lazy_Value_ThrownException_DoesntCreateValue_ExecutionAndPublication()
        {
            Value_ThrownException_DoesntCreateValue(LazyThreadSafetyMode.ExecutionAndPublication);
        }

        [TestMethod]
        public void Lazy_Value_ThrownException_DoesntCreateValue_None()
        {
            Value_ThrownException_DoesntCreateValue(LazyThreadSafetyMode.None);
        }

        private static void Value_ThrownException_DoesntCreateValue(LazyThreadSafetyMode mode)
        {
            var lazy = new Lazy<string>(() => { throw new DivideByZeroException(); }, mode);

            Exception exception1 = AssertExtensions.Throws<DivideByZeroException>(() => lazy.Value);
            Exception exception2 = AssertExtensions.Throws<DivideByZeroException>(() => lazy.Value);
            Assert.AreSame(exception1, exception2);

            Assert.IsFalse(lazy.IsValueCreated);
        }

        [TestMethod]
        public void Lazy_Value_ThrownException_DoesntCreateValue_PublicationOnly()
        {
            var lazy = new Lazy<string>(() => { throw new DivideByZeroException(); }, LazyThreadSafetyMode.PublicationOnly);

            Exception exception1 = AssertExtensions.Throws<DivideByZeroException>(() => lazy.Value);
            Exception exception2 = AssertExtensions.Throws<DivideByZeroException>(() => lazy.Value);
            Assert.AreNotSame(exception1, exception2);

            Assert.IsFalse(lazy.IsValueCreated);
        }

        [TestMethod]
        public void Lazy_EnsureInitalized_SimpleRefTypes()
        {
            var hdcTemplate = new HasDefaultCtor();
            string strTemplate = "foo";

            // Activator.CreateInstance (uninitialized).
            HasDefaultCtor a = null;
            Assert.IsNotNull(LazyInitializer.EnsureInitialized(ref a));
            Assert.AreSame(a, LazyInitializer.EnsureInitialized(ref a));
            Assert.IsNotNull(a);

            // Activator.CreateInstance (already initialized).
            HasDefaultCtor b = hdcTemplate;
            Assert.AreEqual(hdcTemplate, LazyInitializer.EnsureInitialized(ref b));
            Assert.AreSame(b, LazyInitializer.EnsureInitialized(ref b));
            Assert.AreEqual(hdcTemplate, b);

            // Func based initialization (uninitialized).
            string c = null;
            Assert.AreEqual(strTemplate, LazyInitializer.EnsureInitialized(ref c, () => strTemplate));
            Assert.AreSame(c, LazyInitializer.EnsureInitialized(ref c));
            Assert.AreEqual(strTemplate, c);

            // Func based initialization (already initialized).
            string d = strTemplate;
            Assert.AreEqual(strTemplate, LazyInitializer.EnsureInitialized(ref d, () => strTemplate + "bar"));
            Assert.AreSame(d, LazyInitializer.EnsureInitialized(ref d));
            Assert.AreEqual(strTemplate, d);
        }

        [TestMethod]
        public void Lazy_EnsureInitalized_SimpleRefTypes_Invalid()
        {
            // Func based initialization (nulls not permitted).
            string e = null;
            AssertExtensions.Throws<InvalidOperationException>(() => LazyInitializer.EnsureInitialized(ref e, () => null));

            // Activator.CreateInstance (for a type without a default ctor).
            NoDefaultCtor ndc = null;
            AssertExtensions.Throws<MissingMemberException>(() => LazyInitializer.EnsureInitialized(ref ndc));
        }

        [TestMethod]
        public void Lazy_EnsureInitialized_ComplexRefTypes()
        {
            string strTemplate = "foo";
            var hdcTemplate = new HasDefaultCtor();

            // Activator.CreateInstance (uninitialized).
            HasDefaultCtor a = null;
            bool aInit = false;
            object aLock = null;
            Assert.IsNotNull(LazyInitializer.EnsureInitialized(ref a, ref aInit, ref aLock));
            Assert.IsNotNull(a);
            Assert.IsTrue(aInit);
            Assert.IsNotNull(aLock);

            // Activator.CreateInstance (already initialized).
            HasDefaultCtor b = hdcTemplate;
            bool bInit = true;
            object bLock = null;
            Assert.AreEqual(hdcTemplate, LazyInitializer.EnsureInitialized(ref b, ref bInit, ref bLock));
            Assert.AreEqual(hdcTemplate, b);
            Assert.IsTrue(bInit);
            Assert.IsNull(bLock);

            // Func based initialization (uninitialized).
            string c = null;
            bool cInit = false;
            object cLock = null;
            Assert.AreEqual(strTemplate, LazyInitializer.EnsureInitialized(ref c, ref cInit, ref cLock, () => strTemplate));
            Assert.AreEqual(strTemplate, c);
            Assert.IsTrue(cInit);
            Assert.IsNotNull(cLock);

            // Func based initialization (already initialized).
            string d = strTemplate;
            bool dInit = true;
            object dLock = null;
            Assert.AreEqual(strTemplate, LazyInitializer.EnsureInitialized(ref d, ref dInit, ref dLock, () => strTemplate + "bar"));
            Assert.AreEqual(strTemplate, d);
            Assert.IsTrue(dInit);
            Assert.IsNull(dLock);

            // Func based initialization (nulls *ARE* permitted).
            string e = null;
            bool einit = false;
            object elock = null;
            int initCount = 0;

            Assert.IsNull(LazyInitializer.EnsureInitialized(ref e, ref einit, ref elock, () => { initCount++; return null; }));
            Assert.IsNull(e);
            Assert.AreEqual(1, initCount);
            Assert.IsTrue(einit);
            Assert.IsNotNull(elock);
            Assert.IsNull(LazyInitializer.EnsureInitialized(ref e, ref einit, ref elock, () => { initCount++; return null; }));
        }

        [TestMethod]
        public void Lazy_EnsureInitalized_ComplexRefTypes_Invalid()
        {
            // Activator.CreateInstance (for a type without a default ctor).
            NoDefaultCtor ndc = null;
            bool ndcInit = false;
            object ndcLock = null;
            AssertExtensions.Throws<MissingMemberException>(() => LazyInitializer.EnsureInitialized(ref ndc, ref ndcInit, ref ndcLock));
        }

        [TestMethod]
        public void Lazy_LazyInitializerComplexValueTypes()
        {
            var empty = new LIX();
            var template = new LIX(33);

            // Activator.CreateInstance (uninitialized).
            LIX a = default(LIX);
            bool aInit = false;
            object aLock = null;
            LIX ensuredValA = LazyInitializer.EnsureInitialized(ref a, ref aInit, ref aLock);
            Assert.AreEqual(empty, ensuredValA);
            Assert.AreEqual(empty, a);

            // Activator.CreateInstance (already initialized).
            LIX b = template;
            bool bInit = true;
            object bLock = null;
            LIX ensuredValB = LazyInitializer.EnsureInitialized(ref b, ref bInit, ref bLock);
            Assert.AreEqual(template, ensuredValB);
            Assert.AreEqual(template, b);

            // Func based initialization (uninitialized).
            LIX c = default(LIX);
            bool cInit = false;
            object cLock = null;
            LIX ensuredValC = LazyInitializer.EnsureInitialized(ref c, ref cInit, ref cLock, () => template);
            Assert.AreEqual(template, c);
            Assert.AreEqual(template, ensuredValC);

            // Func based initialization (already initialized).
            LIX d = template;
            bool dInit = true;
            object dLock = null;
            LIX template2 = new LIX(template.f * 2);
            LIX ensuredValD = LazyInitializer.EnsureInitialized(ref d, ref dInit, ref dLock, () => template2);
            Assert.AreEqual(template, ensuredValD);
            Assert.AreEqual(template, d);
        }

        private static void VerifyLazy<T>(Lazy<T> lazy, T expectedValue, bool hasValue, bool isValueCreated)
        {
            Assert.AreEqual(isValueCreated, lazy.IsValueCreated);
            if (hasValue)
            {
                Assert.AreEqual(expectedValue, lazy.Value);
                Assert.IsTrue(lazy.IsValueCreated);
            }
            else
            {
                AssertExtensions.Throws<MissingMemberException>(() => lazy.Value); // Value could not be created
                Assert.IsFalse(lazy.IsValueCreated);
            }
        }

        private class HasDefaultCtor { }

        private class NoDefaultCtor
        {
            public NoDefaultCtor(int x) { }
        }

        private struct LIX
        {
            public int f;
            public LIX(int f) { this.f = f; }

            public override bool Equals(object other) { return other is LIX && ((LIX)other).f == f; }
            public override int GetHashCode() { return f.GetHashCode(); }
            public override string ToString() { return "LIX<" + f + ">"; }
        }
    }
}
