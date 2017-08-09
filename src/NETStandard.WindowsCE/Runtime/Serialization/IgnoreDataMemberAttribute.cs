using System;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreDataMemberAttribute : Attribute
    { }
}
