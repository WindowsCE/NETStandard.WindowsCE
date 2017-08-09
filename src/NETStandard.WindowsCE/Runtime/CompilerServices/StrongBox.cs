#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    public class StrongBox<T> : IStrongBox
    {
        public T Value;

        public StrongBox() { }

        public StrongBox(T value)
        {
            Value = value;
        }

        object IStrongBox.Value
        {
            get { return Value; }
            set { Value = (T)value; }
        }
    }
}
