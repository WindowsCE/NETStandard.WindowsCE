#if DEBUG
using System;

#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    /// <summary>
    /// Specifies a <see cref="Type"/> that should be forwarded but its not available.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class TypeNotForwardedAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotForwardedAttribute"/>
        /// class specifying the type that is not available.
        /// </summary>
        /// <param name="reference">The type found into current assembly.</param>
        public TypeNotForwardedAttribute(Type reference) { }
    }
}
#endif
