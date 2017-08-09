using System;

#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class ReferenceAssemblyAttribute : Attribute
    {
        private readonly string _description;

        public ReferenceAssemblyAttribute() { }

        public ReferenceAssemblyAttribute(string description)
        {
            _description = description;
        }

        public string Description
            => _description;
    }
}
