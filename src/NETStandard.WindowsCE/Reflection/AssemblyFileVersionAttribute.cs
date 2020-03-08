using System;

namespace System.Reflection
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
