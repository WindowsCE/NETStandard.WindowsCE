using System;

#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerFilePathAttribute : Attribute
    {
        public CallerFilePathAttribute() { }
    }
}
