using System;

namespace System
{
    [Obsolete(Consts.PlatformNotSupportedDescription)]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class STAThreadAttribute : Attribute
    {
        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public STAThreadAttribute() { }
    }
}
