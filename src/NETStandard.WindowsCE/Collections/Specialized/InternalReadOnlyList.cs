using System.Collections.Generic;

namespace System.Collections.Specialized
{
    internal sealed class InternalReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _list;

        public InternalReadOnlyList(IList<T> list) => _list = list ?? throw new ArgumentNullException(nameof(list));

        public int Count => _list.Count;

        public T this[int index] => _list[index];

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}
