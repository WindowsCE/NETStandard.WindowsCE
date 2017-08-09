using System;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [Obsolete(Consts.PlatformNotSupportedDescription)]
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class ThreadStaticAttribute : Attribute
    {
        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public ThreadStaticAttribute() { }
    }
}
