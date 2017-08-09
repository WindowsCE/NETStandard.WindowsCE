namespace System.Collections.Generic
{
    public interface IEnumerable2<out T> : IEnumerable
    {
        new IEnumerator2<T> GetEnumerator();
    }

    public interface IEnumerator2<out T> : IDisposable, IEnumerator
    {
        new T Current { get; }
    }
}
