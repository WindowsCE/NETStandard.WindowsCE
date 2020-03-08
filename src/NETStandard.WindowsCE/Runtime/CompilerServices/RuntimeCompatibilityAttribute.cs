using System;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class RuntimeCompatibilityAttribute : Attribute
    {
        public RuntimeCompatibilityAttribute() { }
        public bool WrapNonExceptionThrows { get; set; }
    }
}
