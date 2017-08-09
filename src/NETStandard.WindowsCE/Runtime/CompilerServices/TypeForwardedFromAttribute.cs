using System;

#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    [Obsolete(Consts.PlatformNotSupportedDescription)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
    public sealed class TypeForwardedFromAttribute : Attribute
    {
        private readonly string _assemblyFullName;

        public TypeForwardedFromAttribute(string assemblyFullName)
        {
            _assemblyFullName = assemblyFullName;
        }

        public string AssemblyFullName
            => _assemblyFullName;
    }
}
