using System;
using System.Reflection;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Activator2
    {
        private const string ParameterElementNull = "None of {0} elements should be null.";

        public static object CreateInstance(Type type)
            => Activator.CreateInstance(type);

        public static object CreateInstance(Type type, params object[] args)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type[] argsType;
            if (args == null || args.Length == 0)
                return Activator.CreateInstance(type);

            argsType = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    throw new ArgumentException(
                        string.Format(ParameterElementNull, nameof(args)),
                        nameof(args));

                argsType[i] = args[i].GetType();
            }

            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
            var ctor = type.GetConstructor(bindingAttr, null, argsType, null);
            return ctor.Invoke(args);
        }

        public static T CreateInstance<T>()
            => Activator.CreateInstance<T>();
    }
}
