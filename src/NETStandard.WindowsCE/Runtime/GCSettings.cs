using System;

namespace System.Runtime
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
