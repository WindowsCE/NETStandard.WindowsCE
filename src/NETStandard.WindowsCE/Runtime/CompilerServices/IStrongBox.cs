#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    public interface IStrongBox
    {
        object Value { get; set; }
    }
}
