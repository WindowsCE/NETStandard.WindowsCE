using System;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class OnDeserializingAttribute : Attribute
    { }
}
