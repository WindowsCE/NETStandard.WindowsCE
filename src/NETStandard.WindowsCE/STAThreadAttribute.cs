using System;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [Obsolete(Consts.PlatformNotSupportedDescription)]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class STAThreadAttribute : Attribute
    {
        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public STAThreadAttribute() { }
    }
}
