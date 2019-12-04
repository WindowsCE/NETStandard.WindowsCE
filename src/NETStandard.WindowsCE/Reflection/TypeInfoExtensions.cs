using System.Collections.Generic;
using System.ComponentModel;

#if !NET35_CF
using Mock.System;
#endif

namespace System.Reflection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TypeInfoExtensions
    {
        private const BindingFlags DeclaredBindingFlags =
            BindingFlags.DeclaredOnly |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        public static EventInfo GetDeclaredEvent(this Type type, string name)
        {
            return type.GetEvent(name, DeclaredBindingFlags);
        }

        public static FieldInfo GetDeclaredField(this Type type, string name)
        {
            return type.GetField(name, DeclaredBindingFlags);
        }

        public static MethodInfo GetDeclaredMethod(this Type type, string name)
        {
            return type.GetMethod(name, DeclaredBindingFlags);
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name)
        {
            var methods = type.GetMethods(DeclaredBindingFlags);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo methodInfo = methods[i];
                if (methodInfo.Name == name)
                    yield return methodInfo;
            }
        }

        public static Type GetDeclaredNestedType(this Type type, string name)
        {
            return type.GetNestedType(name, DeclaredBindingFlags);
        }

        public static PropertyInfo GetDeclaredProperty(this Type type, string name)
        {
            return type.GetProperty(name, DeclaredBindingFlags);
        }

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type)
        {
            return type.GetConstructors(DeclaredBindingFlags);
        }

        public static IEnumerable<EventInfo> GetDeclaredEvents(this Type type)
        {
            return type.GetEvents(DeclaredBindingFlags);
        }

        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type)
        {
            return type.GetFields(DeclaredBindingFlags);
        }

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type type)
        {
            return type.GetMembers(DeclaredBindingFlags);
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
        {
            return type.GetMethods(DeclaredBindingFlags);
        }

        public static IEnumerable<Type> GetDeclaredNestedTypes(this Type type)
        {
            return type.GetNestedTypes(DeclaredBindingFlags);
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            return type.GetProperties(DeclaredBindingFlags);
        }

        public static Type[] GetGenericTypeParameters(this Type type)
        {
            if (type.IsGenericTypeDefinition)
                return type.GetGenericArguments();

            return Type2.EmptyTypes;
        }

        public static IEnumerable<Type> GetImplementedInterfaces(this Type type)
        {
            return type.GetInterfaces();
        }
    }
}
