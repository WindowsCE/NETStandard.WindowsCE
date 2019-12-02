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
        {
            return RuntimeHelpers.GetObjectValue(obj);
        }

#nullable enable
        /// <summary>Slices the specified array using the specified range.</summary>
        // Ref: https://gist.github.com/bgrainger/fb2c18659c2cdfce494c82a8c4803360
        public static T[] GetSubArray<T>(T[] array, Range range)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            (int offset, int length) = range.GetOffsetAndLength(array.Length);

            if (default(T)! != null || typeof(T[]) == array.GetType()) // TODO-NULLABLE: default(T) == null warning (https://github.com/dotnet/roslyn/issues/34757)
            {
                // We know the type of the array to be exactly T[].

                if (length == 0)
                {
                    return Array2.Empty<T>();
                }

                var dest = new T[length];
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
            else
            {
                // The array is actually a U[] where U:T.
                T[] dest = (T[])Array.CreateInstance(array.GetType().GetElementType()!, length);
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
        }
#nullable disable

        public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle)
        {
            RuntimeHelpers.InitializeArray(array, fldHandle);
        }

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
