using System;

#if NET35_CF
namespace System.Runtime
#else
namespace Mock.System.Runtime
#endif
{
    public static class GCSettings
    {
        public static bool IsServerGC
            => false;

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static GCLatencyMode LatencyMode
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }
    }
}
