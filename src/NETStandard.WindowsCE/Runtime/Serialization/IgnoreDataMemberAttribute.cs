using System;

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreDataMemberAttribute : Attribute
    { }
}
