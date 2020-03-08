using System;

namespace System
{
    public enum GCCollectionMode
    {
        Default = 0,
        Forced = 1,
        Optimized = 2,
    }

    public static class GC2
    {
        public static int MaxGeneration
            => GC.MaxGeneration;

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static void AddMemoryPressure(long bytesAllocated)
        {
            throw new PlatformNotSupportedException();
        }

        public static void Collect()
            => GC.Collect();

        public static void Collect(int generation)
        {
            // We ignore generation here because the GC of Compact Framework
            // does not have generations.
            GC.Collect();
        }

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static void Collect(int generation, GCCollectionMode mode)
        {
            throw new PlatformNotSupportedException();
        }

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static void Collect(int generation, GCCollectionMode mode, bool blocking)
        {
            throw new PlatformNotSupportedException();
        }

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static int CollectionCount(int generation)
        {
            throw new PlatformNotSupportedException();
        }

        public static long GetTotalMemory(bool forceFullCollection)
            => GC.GetTotalMemory(forceFullCollection);

        public static void KeepAlive(object obj)
            => GC.KeepAlive(obj);

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static void RemoveMemoryPressure(long bytesAllocated)
        {
            throw new PlatformNotSupportedException();
        }

        public static void ReRegisterForFinalize(object obj)
            => GC.ReRegisterForFinalize(obj);

        public static void SuppressFinalize(object obj)
            => GC.SuppressFinalize(obj);

        public static void WaitForPendingFinalizers()
            => GC.WaitForPendingFinalizers();
    }
}
