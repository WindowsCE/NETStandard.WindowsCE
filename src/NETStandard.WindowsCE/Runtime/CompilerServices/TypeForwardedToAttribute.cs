using System;

#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
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
