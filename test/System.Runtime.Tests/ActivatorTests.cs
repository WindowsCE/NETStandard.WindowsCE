// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Reflection;

#if WindowsCE
using Activator = System.Activator2;
#else
using Activator = Mock.System.Activator2;
#endif

namespace Tests
{
    [TestClass]
    public class ActivatorTests
    {
        [TestMethod]
        public void Activator_CreateInstance()
        {
            // Passing null args is equivalent to an empty array of args.
            Choice1 c = (Choice1)(Activator.CreateInstance(typeof(Choice1), null));
            Assert.AreEqual(1, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { }));
            Assert.AreEqual(1, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { 42 }));
            Assert.AreEqual(2, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { "Hello" }));
            Assert.AreEqual(3, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { 5.1, "Hello" }));
            Assert.AreEqual(4, c.I);

            Activator.CreateInstance(typeof(StructTypeWithoutReflectionMetadata));
        }

        [TestMethod]
        public void Activator_CreateInstance_ConstructorWithPrimitive_PerformsPrimitiveWidening()
        {
            // Primitive widening is allowed by the binder, but not by Dynamic.DelegateInvoke().
            Choice1 c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { (short)-2 }));
            Assert.AreEqual(2, c.I);
        }

        [TestMethod]
        public void Activator_CreateInstance_ConstructorWithParamsParameter()
        {
            // C# params arguments are honored by Activator.CreateInstance()
            Choice1 c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { new VarArgs() }));
            Assert.AreEqual(5, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { new VarArgs(), "P1" }));
            Assert.AreEqual(5, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { new VarArgs(), "P1", "P2" }));
            Assert.AreEqual(5, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { new VarStringArgs() }));
            Assert.AreEqual(6, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { new VarStringArgs(), "P1" }));
            Assert.AreEqual(6, c.I);

            c = (Choice1)(Activator.CreateInstance(typeof(Choice1), new object[] { new VarStringArgs(), "P1", "P2" }));
            Assert.AreEqual(6, c.I);
        }

        [TestMethod]
        public void Activator_CreateInstance_Invalid()
        {
            Type nullType = null;
            AssertExtensions.Throws<ArgumentNullException>("type", () => Activator.CreateInstance(nullType)); // Type is null
            AssertExtensions.Throws<ArgumentNullException>("type", () => Activator.CreateInstance(null, new object[0])); // Type is null

            AssertExtensions.Throws<AmbiguousMatchException>(() => Activator.CreateInstance(typeof(Choice1), new object[] { null }));

            // C# designated optional parameters are not optional as far as Activator.CreateInstance() is concerned.
            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance(typeof(Choice1), new object[] { 5.1 }));
            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance(typeof(Choice1), new object[] { 5.1, Type.Missing }));

            // Invalid params args
            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance(typeof(Choice1), new object[] { new VarStringArgs(), 5, 6 }));

            // Primitive widening not supported for "params" arguments.
            //
            // (This is probably an accidental behavior on the desktop as the default binder specifically checks to see if the params arguments are widenable to the
            // params array element type and gives it the go-ahead if it is. Unfortunately, the binder then bollixes itself by using Array.Copy() to copy
            // the params arguments. Since Array.Copy() doesn't tolerate this sort of type mismatch, it throws an InvalidCastException which bubbles out
            // out of Activator.CreateInstance. Accidental or not, we'll inherit that behavior on .NET Native.)
            AssertExtensions.Throws<InvalidCastException>(() => Activator.CreateInstance(typeof(Choice1), new object[] { new VarIntArgs(), 1, (short)2 }));

            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance(typeof(TypeWithoutDefaultCtor))); // Type has no default constructor
            AssertExtensions.Throws<TargetInvocationException>(() => Activator.CreateInstance(typeof(TypeWithDefaultCtorThatThrows))); // Type has a default constructor throws an exception
            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance(typeof(AbstractTypeWithDefaultCtor))); // Type is abstract
            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance(typeof(IInterfaceType))); // Type is an interface

#if netcoreapp || uapaot
            foreach (Type nonRuntimeType in Helpers.NonRuntimeTypes)
            {
                // Type is not a valid RuntimeType
                AssertExtensions.Throws<ArgumentException>("type", () => Activator.CreateInstance(nonRuntimeType));
            }
#endif // netcoreapp || uapaot
        }

        [TestMethod]
        public void Activator_CreateInstance_Generic()
        {
            Choice1 c = Activator.CreateInstance<Choice1>();
            Assert.AreEqual(1, c.I);

            Activator.CreateInstance<DateTime>();
            Activator.CreateInstance<StructTypeWithoutReflectionMetadata>();
        }

        [TestMethod]
        public void Activator_CreateInstance_Generic_Invalid()
        {
            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance<int[]>()); // Cannot create array type

            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance<TypeWithoutDefaultCtor>()); // Type has no default constructor
            AssertExtensions.Throws<TargetInvocationException>(() => Activator.CreateInstance<TypeWithDefaultCtorThatThrows>()); // Type has a default constructor that throws
            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance<AbstractTypeWithDefaultCtor>()); // Type is abstract
            AssertExtensions.ThrowsAny<MissingMemberException>(() => Activator.CreateInstance<IInterfaceType>()); // Type is an interface
        }

        [TestMethod]
        public void Activator_TestActivatorOnNonActivatableFinalizableTypes()
        {
            // On runtimes where the generic Activator is implemented with special codegen intrinsics, we might allocate
            // an uninitialized instance of the object before we realize there's no default constructor to run.
            // Make sure this has no observable side effects.
            AssertExtensions.ThrowsAny<MissingMemberException>(() => { Activator.CreateInstance<TypeWithPrivateDefaultCtorAndFinalizer>(); });

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(TypeWithPrivateDefaultCtorAndFinalizer.WasCreated);
        }

        class PrivateType
        {
            public PrivateType() { }
        }

        class PrivateTypeWithDefaultCtor
        {
            private PrivateTypeWithDefaultCtor() { }
        }

        class PrivateTypeWithoutDefaultCtor
        {
            private PrivateTypeWithoutDefaultCtor(int x) { }
        }

        class PrivateTypeWithDefaultCtorThatThrows
        {
            public PrivateTypeWithDefaultCtorThatThrows() { throw new Exception(); }
        }

        //[TestMethod]
        //public void Activator_CreateInstance_Type_Bool()
        //{
        //    Assert.AreEqual(typeof(PrivateType), Activator.CreateInstance(typeof(PrivateType), true).GetType());
        //    Assert.AreEqual(typeof(PrivateType), Activator.CreateInstance(typeof(PrivateType), false).GetType());

        //    Assert.AreEqual(typeof(PrivateTypeWithDefaultCtor), Activator.CreateInstance(typeof(PrivateTypeWithDefaultCtor), true).GetType());
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(PrivateTypeWithDefaultCtor), false).GetType());

        //    AssertExtensions.Throws<TargetInvocationException>(() => Activator.CreateInstance(typeof(PrivateTypeWithDefaultCtorThatThrows), true).GetType());
        //    AssertExtensions.Throws<TargetInvocationException>(() => Activator.CreateInstance(typeof(PrivateTypeWithDefaultCtorThatThrows), false).GetType());

        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(PrivateTypeWithoutDefaultCtor), true).GetType());
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(PrivateTypeWithoutDefaultCtor), false).GetType());
        //}

        public class Choice1 : Attribute
        {
            public Choice1()
            {
                I = 1;
            }

            public Choice1(int i)
            {
                I = 2;
            }

            public Choice1(string s)
            {
                I = 3;
            }
#if !WindowsCE
            public Choice1(double d, string optionalS = "Hey")
            {
                I = 4;
            }
#else
            public Choice1(double d, string optionalS)
            {
                I = 4;
            }
#endif

            public Choice1(VarArgs varArgs, params object[] parameters)
            {
                I = 5;
            }

            public Choice1(VarStringArgs varArgs, params string[] parameters)
            {
                I = 6;
            }

            public Choice1(VarIntArgs varArgs, params int[] parameters)
            {
                I = 7;
            }

            public int I;
        }

        public class VarArgs { }

        public class VarStringArgs { }

        public class VarIntArgs { }

        public class TypeWithPrivateDefaultCtorAndFinalizer
        {
            public static bool WasCreated { get; private set; }

            private TypeWithPrivateDefaultCtorAndFinalizer() { }

            ~TypeWithPrivateDefaultCtorAndFinalizer()
            {
                WasCreated = true;
            }
        }

        private interface IInterfaceType
        {
        }

        public abstract class AbstractTypeWithDefaultCtor
        {
            public AbstractTypeWithDefaultCtor() { }
        }

        public struct StructTypeWithoutReflectionMetadata { }

        public class TypeWithoutDefaultCtor
        {
            private TypeWithoutDefaultCtor(int x) { }
        }

        public class TypeWithDefaultCtorThatThrows
        {
            public TypeWithDefaultCtorThatThrows() { throw new Exception(); }
        }

        class ClassWithPrivateCtor
        {
            static ClassWithPrivateCtor() { Flag.Reset(100); }
            private ClassWithPrivateCtor() { Flag.Increase(200); }
            public ClassWithPrivateCtor(int i) { Flag.Increase(300); }
        }
        class ClassWithPrivateCtor2
        {
            static ClassWithPrivateCtor2() { Flag.Reset(100); }
            private ClassWithPrivateCtor2() { Flag.Increase(200); }
            public ClassWithPrivateCtor2(int i) { Flag.Increase(300); }
        }
        class ClassWithPrivateCtor3
        {
            static ClassWithPrivateCtor3() { Flag.Reset(100); }
            private ClassWithPrivateCtor3() { Flag.Increase(200); }
            public ClassWithPrivateCtor3(int i) { Flag.Increase(300); }
        }

        class HasPublicCtor
        {
            public int Value = 0;

            public HasPublicCtor(int value) { Value = value; }
        }

        class HasPrivateCtor
        {
            public int Value = 0;

            private HasPrivateCtor(int value) { Value = value; }
        }

        public class IsTestedAttribute : Attribute
        {
            private bool flag;
            public IsTestedAttribute(bool flag)
            {
                this.flag = flag;

            }
        }
        [IsTestedAttribute(false)]
        class ClassWithIsTestedAttribute { }
        [Serializable]
        class ClassWithSerializableAttribute { }
        [IsTestedAttribute(false)]
        class MBRWithIsTestedAttribute : MarshalByRefObject { }
        class Flag
        {
            public static int cnt = 0;
            public static void Reset(int i) { cnt = i; }
            public static void Increase(int i) { cnt += i; }
            public static bool Equal(int i) { return cnt == i; }
        }

        //[ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsInvokingStaticConstructorsSupported))]
        static void TestingBindingFlags()
        {
            AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.CurrentCulture));
            AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Public | BindingFlags.Instance, null, new object[] { 1, 2, 3 }, CultureInfo.CurrentCulture));


            Flag.Reset(0); Assert.AreEqual(0, Flag.cnt);
            Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Static | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(300, Flag.cnt);
            Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(500, Flag.cnt);

            Flag.Reset(0); Assert.AreEqual(0, Flag.cnt);
            Activator.CreateInstance(typeof(ClassWithPrivateCtor2), BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(300, Flag.cnt);
            Activator.CreateInstance(typeof(ClassWithPrivateCtor2), BindingFlags.Static | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(500, Flag.cnt);

            Flag.Reset(0); Assert.AreEqual(0, Flag.cnt);
            Activator.CreateInstance(typeof(ClassWithPrivateCtor3), BindingFlags.Instance | BindingFlags.Public, null, new object[] { 122 }, CultureInfo.CurrentCulture);
            Assert.AreEqual(400, Flag.cnt);
            Activator.CreateInstance(typeof(ClassWithPrivateCtor3), BindingFlags.Static | BindingFlags.NonPublic, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(600, Flag.cnt);
            Activator.CreateInstance(typeof(ClassWithPrivateCtor3), BindingFlags.Instance | BindingFlags.Public, null, new object[] { 122 }, CultureInfo.CurrentCulture);
            Assert.AreEqual(900, Flag.cnt);

            AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Public | BindingFlags.Static, null, null, CultureInfo.CurrentCulture));
            AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Public | BindingFlags.Static, null, new object[] { 122 }, CultureInfo.CurrentCulture));
        }

        //[TestMethod]
        //public void Activator_TestingBindingFlagsInstanceOnly()
        //{
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(HasPublicCtor), default(BindingFlags), null, null, null));
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(HasPublicCtor), BindingFlags.NonPublic | BindingFlags.Instance, null, null, null));
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(HasPublicCtor), BindingFlags.Public | BindingFlags.Static, null, null, null));

        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(HasPrivateCtor), default(BindingFlags), null, null, null));
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(HasPrivateCtor), BindingFlags.Public | BindingFlags.Instance, null, null, null));
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(HasPrivateCtor), BindingFlags.NonPublic | BindingFlags.Static, null, null, null));

        //    {
        //        HasPublicCtor a = (HasPublicCtor)Activator.CreateInstance(typeof(HasPublicCtor), BindingFlags.Public | BindingFlags.Instance, null, new object[] { 100 }, null);
        //        Assert.AreEqual(100, a.Value);
        //    }

        //    {
        //        HasPrivateCtor a = (HasPrivateCtor)Activator.CreateInstance(typeof(HasPrivateCtor), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { 100 }, null);
        //        Assert.AreEqual(100, a.Value);
        //    }
        //}

        //[TestMethod]
        //public void Activator_TestingBindingFlags1()
        //{
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.CurrentCulture, null));
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Public | BindingFlags.Instance, null, new object[] { 1, 2, 3 }, CultureInfo.CurrentCulture, null));

        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Public | BindingFlags.Static, null, null, CultureInfo.CurrentCulture, null));
        //    AssertExtensions.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof(ClassWithPrivateCtor), BindingFlags.Public | BindingFlags.Static, null, new object[] { 122 }, CultureInfo.CurrentCulture, null));
        //}

        //[TestMethod]
        //[SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework)]
        //public void Activator_TestingActivationAttributes()
        //{
        //    AssertExtensions.Throws<PlatformNotSupportedException>(() => Activator.CreateInstance(typeof(ClassWithIsTestedAttribute), null, new object[] { new Object() }));
        //    AssertExtensions.Throws<PlatformNotSupportedException>(() => Activator.CreateInstance(typeof(ClassWithIsTestedAttribute), null, new object[] { new IsTestedAttribute(true) }));
        //    AssertExtensions.Throws<PlatformNotSupportedException>(() => Activator.CreateInstance(typeof(ClassWithSerializableAttribute), null, new object[] { new ClassWithIsTestedAttribute() }));
        //    AssertExtensions.Throws<PlatformNotSupportedException>(() => Activator.CreateInstance(typeof(MBRWithIsTestedAttribute), null, new object[] { new IsTestedAttribute(true) }));

        //    AssertExtensions.Throws<PlatformNotSupportedException>(() => Activator.CreateInstance(typeof(ClassWithIsTestedAttribute), 0, null, null, CultureInfo.CurrentCulture, new object[] { new IsTestedAttribute(true) }));
        //    AssertExtensions.Throws<PlatformNotSupportedException>(() => Activator.CreateInstance(typeof(ClassWithSerializableAttribute), 0, null, null, CultureInfo.CurrentCulture, new object[] { new ClassWithIsTestedAttribute() }));
        //    AssertExtensions.Throws<PlatformNotSupportedException>(() => Activator.CreateInstance(typeof(MBRWithIsTestedAttribute), 0, null, null, CultureInfo.CurrentCulture, new object[] { new IsTestedAttribute(true) }));
        //}

        //[TestMethod]
        //public void Activator_TestingActivationAttributes1()
        //{
        //    Activator.CreateInstance(typeof(ClassWithIsTestedAttribute), null, null);
        //    Activator.CreateInstance(typeof(ClassWithIsTestedAttribute), null, new object[] { });

        //}
    }
}