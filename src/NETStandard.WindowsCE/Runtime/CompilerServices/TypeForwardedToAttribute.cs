using System;

namespace System.Runtime.CompilerServices
{
    [Obsolete(Consts.PlatformNotSupportedDescription)]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class TypeForwardedToAttribute : Attribute
    {
        private readonly Type _destination;

        public Type Destination
        {
            get { return _destination; }
        }

        public TypeForwardedToAttribute(Type destination)
        {
            _destination = destination;
        }
    }
}
