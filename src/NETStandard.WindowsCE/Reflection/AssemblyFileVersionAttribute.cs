using System;

#if NET35_CF
namespace System.Reflection
#else
namespace Mock.System.Reflection
#endif
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class AssemblyFileVersionAttribute : Attribute
    {
        private readonly string _version;

        public AssemblyFileVersionAttribute(string version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            _version = version;
        }

        public string Version
            => _version;
    }
}
