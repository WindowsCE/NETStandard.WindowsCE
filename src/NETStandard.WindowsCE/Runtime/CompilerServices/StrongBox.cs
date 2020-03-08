namespace System.Runtime.CompilerServices
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
