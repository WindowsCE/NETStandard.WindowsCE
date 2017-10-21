// Ref: http://stackoverflow.com/a/7264835/1028452

using System.Reflection;

namespace System.Runtime.CompilerServices
{
    public static class RuntimeHelpers2
    {
        private static readonly Func<object, int> _IdentityHashCode;

        static RuntimeHelpers2()
        {
            Assembly mscorlib = typeof(object).Assembly;
            Type t;
            MethodInfo mi;

            if ((t = mscorlib.GetType("System.PInvoke.EE")) != null)
            {
                if ((mi = t.GetMethod("Object_GetHashCode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) != null)
                    _IdentityHashCode = (Func<object, int>)Delegate.CreateDelegate(typeof(Func<object, int>), null, mi);
            }
        }

        public static int OffsetToStringData
            => RuntimeHelpers.OffsetToStringData;

        public static int GetHashCode(object o)
        {
            if (_IdentityHashCode == null)
                throw new PlatformNotSupportedException();

            return _IdentityHashCode(o);
        }

        public static object GetObjectValue(object obj)
            => RuntimeHelpers.GetObjectValue(obj);

        public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle)
            => RuntimeHelpers.InitializeArray(array, fldHandle);

        // TODO: Improve for real usage
        public static void RunClassConstructor(RuntimeTypeHandle typeHandle)
        {
            const BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            var type = Type.GetTypeFromHandle(typeHandle);
            var ctor = type.GetConstructor(flags, null, new Type[0], null);
            type = null;

            ctor.Invoke(new object[0]);
            ctor = null;
        }
    }
}
