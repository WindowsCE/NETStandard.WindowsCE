using System;

namespace System
{
    [Obsolete(Consts.PlatformNotSupportedDescription)]
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class ThreadStaticAttribute : Attribute
    {
        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public ThreadStaticAttribute() { }
    }
}
