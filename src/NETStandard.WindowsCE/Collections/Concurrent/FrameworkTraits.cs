using System;
using System.Threading;

namespace FrameworkTraits
{
    internal static class PlatformHelper
    {
        public static int ProcessorCount
        {
            get { return 1; }
        }
    }
}
