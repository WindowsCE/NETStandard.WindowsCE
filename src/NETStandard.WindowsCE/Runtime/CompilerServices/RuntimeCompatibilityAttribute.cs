using System;

#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class RuntimeCompatibilityAttribute : Attribute
    {
        public RuntimeCompatibilityAttribute() { }
        public bool WrapNonExceptionThrows { get; set; }
    }
}
