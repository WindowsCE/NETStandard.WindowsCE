using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace System.Runtime.Tests
{
    [TestClass]
    public class TypeTests
    {
        [TestMethod]
        public void Type_GetInterface()
        {
            Type typeBar = typeof(Bar);
            Type typeFoo = typeof(Foo);

            Type foundType = Mock.System.Type2.GetInterface(typeBar, "Foo");
            Assert.IsNotNull(foundType);
            Assert.AreEqual(typeFoo.FullName, foundType.FullName);
            Assert.IsTrue(foundType.IsInterface);

            foundType = Mock.System.Type2.GetInterface(typeBar, "foo");
            Assert.IsNull(foundType);

            foundType = Mock.System.Type2.GetInterface(typeBar, "Qux");
            Assert.IsNull(foundType);

            foundType = Mock.System.Type2.GetInterface(typeBar, "foo", true);
            Assert.IsNotNull(foundType);
            Assert.AreEqual(typeFoo.FullName, foundType.FullName);
            Assert.IsTrue(foundType.IsInterface);

            foundType = Mock.System.Type2.GetInterface(typeBar, "qux", true);
            Assert.IsNull(foundType);
        }

        [TestMethod]
        public void Type_GetMember()
        {
            Type typeBar = typeof(Bar);
            Type typeFoo = typeof(Foo);

            var expectedFields = new[]
            {
                new {
                    Name = "_field1", MemberType = MemberTypes.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic, Length = 1,
                    Members = new[] { new { Type = typeof(FieldInfo), MemberType = MemberTypes.Field, Parameters = new Type[0] } }
                },
                new {
                    Name = "_field2", MemberType = MemberTypes.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic, Length = 1,
                    Members = new[] { new { Type = typeof(FieldInfo), MemberType = MemberTypes.Field, Parameters = new Type[0] } }
                },
                new {
                    Name = "_field3", MemberType = MemberTypes.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic, Length = 0,
                    Members = new [] { new { Type = (Type)null, MemberType = (MemberTypes)0, Parameters = new Type[0] } }
                },
                new {
                    Name = "_field1", MemberType = MemberTypes.Property, Flags = BindingFlags.Instance | BindingFlags.NonPublic, Length = 0,
                    Members = new [] { new { Type = (Type)null, MemberType = (MemberTypes)0, Parameters = new Type[0] } }
                },
                new {
                    Name = "_field1", MemberType = MemberTypes.All, Flags = BindingFlags.Instance | BindingFlags.NonPublic, Length = 1,
                    Members = new[] { new { Type = typeof(FieldInfo), MemberType = MemberTypes.Field, Parameters = new Type[0] } }
                },
                new {
                    Name = "Field1", MemberType = MemberTypes.Property, Flags = BindingFlags.Instance | BindingFlags.Public, Length = 1,
                    Members = new[] { new { Type = typeof(PropertyInfo), MemberType = MemberTypes.Property, Parameters = new Type[0] } }
                },
                new {
                    Name = "Field2", MemberType = MemberTypes.Property, Flags = BindingFlags.Instance | BindingFlags.Public, Length = 1,
                    Members = new[] { new { Type = typeof(PropertyInfo), MemberType = MemberTypes.Property, Parameters = new Type[0] } }
                },
                new {
                    Name = "Field3", MemberType = MemberTypes.Property, Flags = BindingFlags.Instance | BindingFlags.Public, Length = 0,
                    Members = new[] { new { Type = (Type)null, MemberType = (MemberTypes)0, Parameters = new Type[0] } }
                },
                new {
                    Name = "Field1", MemberType = MemberTypes.Method, Flags = BindingFlags.Instance | BindingFlags.Public, Length = 0,
                    Members = new[] { new { Type = (Type)null, MemberType = (MemberTypes)0, Parameters = new Type[0] } }
                },
                new {
                    Name = "SetField1", MemberType = MemberTypes.Method, Flags = BindingFlags.Instance | BindingFlags.Public, Length = 2,
                    Members = new[] {
                        new { Type = typeof(MethodInfo), MemberType = MemberTypes.Method, Parameters = new Type[] { typeof(int) } },
                        new { Type = typeof(MethodInfo), MemberType = MemberTypes.Method, Parameters = new Type[] { typeof(int), typeof(bool) } } }
                },
                new {
                    Name = "SetField2", MemberType = MemberTypes.Method, Flags = BindingFlags.Instance | BindingFlags.Public, Length = 1,
                    Members = new[] {
                        new { Type = typeof(MethodInfo), MemberType = MemberTypes.Method, Parameters = new Type[] { typeof(string) } } }
                },
                new {
                    Name = "SetField3", MemberType = MemberTypes.Method, Flags = BindingFlags.Instance | BindingFlags.Public, Length = 0,
                    Members = new [] { new { Type = (Type)null, MemberType = (MemberTypes)0, Parameters = new Type[0] } }
                },
                new {
                    Name = "DoNothing", MemberType = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method, Flags = BindingFlags.Instance | BindingFlags.Public, Length = 1,
                    Members = new[] {
                        new { Type = typeof(MethodInfo), MemberType = MemberTypes.Method, Parameters = new Type[0] } }
                },
            };

            foreach (var item in expectedFields)
            {
                var members = Mock.System.Type2.GetMember(
                    typeBar, item.Name, item.MemberType, item.Flags);

                Assert.IsNotNull(members);
                Assert.AreEqual(item.Length, members.Length);

                for (int i = 0; i < item.Length; i++)
                {
                    Assert.IsTrue(item.Members[i].Type.IsAssignableFrom(members[i].GetType()));
                    Assert.AreEqual(item.Members[i].MemberType, members[i].MemberType);
                    Assert.AreEqual(item.Name, members[i].Name);
                    Assert.AreEqual(typeBar, members[i].DeclaringType);

                    if (item.Members[i].Type != typeof(MethodInfo))
                        continue;

                    ParameterInfo[] parameters = ((MethodInfo)members[i]).GetParameters();
                    Assert.IsNotNull(parameters);
                    Assert.AreEqual(item.Members[i].Parameters.Length, parameters.Length);

                    for (int j = 0; j < parameters.Length; j++)
                    {
                        Assert.AreEqual(item.Members[i].Parameters[j], parameters[j].ParameterType);
                    }
                }
            }
        }

        [TestMethod]
        public void Type_IsSerializable()
        {
            Assert.IsTrue(Mock.System.Type2.IsSerializable(typeof(Bar)));
            Assert.IsFalse(Mock.System.Type2.IsSerializable(typeof(NotSerializableClass)));
            Assert.IsFalse(Mock.System.Type2.IsSerializable(typeof(Foo)));
            Assert.IsFalse(Mock.System.Type2.IsSerializable(typeof(Qux)));
        }

        [TestMethod]
        public void Type_MakeArrayType()
        {
            Assert.AreEqual(typeof(int[]), Mock.System.Type2.MakeArrayType(typeof(int)));
            Assert.AreEqual(typeof(long[]), Mock.System.Type2.MakeArrayType(typeof(long)));
            Assert.AreEqual(typeof(string[]), Mock.System.Type2.MakeArrayType(typeof(string)));
            Assert.AreEqual(typeof(int[,]), Mock.System.Type2.MakeArrayType(typeof(int), 2));
            Assert.AreEqual(typeof(int[,,]), Mock.System.Type2.MakeArrayType(typeof(int), 3));
            Assert.AreEqual(typeof(int[,,,]), Mock.System.Type2.MakeArrayType(typeof(int), 4));
            Assert.AreEqual(typeof(int[,,,,]), Mock.System.Type2.MakeArrayType(typeof(int), 5));
            Assert.AreEqual(typeof(long[,]), Mock.System.Type2.MakeArrayType(typeof(long), 2));
            Assert.AreEqual(typeof(long[,,]), Mock.System.Type2.MakeArrayType(typeof(long), 3));
            Assert.AreEqual(typeof(long[,,,]), Mock.System.Type2.MakeArrayType(typeof(long), 4));
            Assert.AreEqual(typeof(long[,,,,]), Mock.System.Type2.MakeArrayType(typeof(long), 5));
            Assert.AreEqual(typeof(string[,]), Mock.System.Type2.MakeArrayType(typeof(string), 2));
            Assert.AreEqual(typeof(string[,,]), Mock.System.Type2.MakeArrayType(typeof(string), 3));
            Assert.AreEqual(typeof(string[,,,]), Mock.System.Type2.MakeArrayType(typeof(string), 4));
            Assert.AreEqual(typeof(string[,,,,]), Mock.System.Type2.MakeArrayType(typeof(string), 5));
        }

        [TestMethod]
        public void Type_MakeByRefType()
        {
            Assert.AreEqual(Type.GetType("System.Int32&", true, false), Mock.System.Type2.MakeByRefType(typeof(int)));
            Assert.AreEqual(Type.GetType("System.Int64&", true, false), Mock.System.Type2.MakeByRefType(typeof(long)));
            Assert.AreEqual(Type.GetType("System.String&", true, false), Mock.System.Type2.MakeByRefType(typeof(string)));
        }

        [TestMethod]
        public void Type_MakePointerType()
        {
            Assert.AreEqual(Type.GetType("System.Int32*", true, false), Mock.System.Type2.MakePointerType(typeof(int)));
            Assert.AreEqual(Type.GetType("System.Int64*", true, false), Mock.System.Type2.MakePointerType(typeof(long)));
            Assert.AreEqual(Type.GetType("System.String*", true, false), Mock.System.Type2.MakePointerType(typeof(string)));
        }

        interface Foo
        {
            int Field1 { get; }
            void SetField1(int value);
        }
        interface Qux { }
        [Serializable]
        class Bar : Foo
        {
            private int _field1;
            private string _field2;

            public int Field1
                => _field1;

            public string Field2
                => _field2;

            public void SetField1(int value)
                => _field1 = value;

            public void SetField1(int value, bool set)
                => _field1 = set ? value : _field1;

            public void SetField2(string value)
                => _field2 = value;

            public void DoNothing() { }
        }

        class NotSerializableClass { }
    }
}
