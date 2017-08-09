#if NET35_CF
namespace System.Runtime
#else
namespace Mock.System.Runtime
#endif
{
    public enum GCLatencyMode
    {
        Batch = 0,
        Interactive = 1,
        LowLatency = 2,
        SustainedLowLatency = 3,
    }
}
