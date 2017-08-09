using System;

#if NET35_CF
namespace System.Reflection
#else
namespace Mock.System.Reflection
#endif
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class AssemblyMetadataAttribute : Attribute
    {
        private readonly string _key;
        private readonly string _value;

        public AssemblyMetadataAttribute(string key, string value)
        {
            _key = key;
            _value = value;
        }

        public string Key
            => _key;

        public string Value
            => _value;
    }
}
