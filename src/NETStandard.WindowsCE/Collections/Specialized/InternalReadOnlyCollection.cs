using System.Collections.Generic;

#if !NET35_CF
using Mock.System.Collections.Generic;
#endif

namespace System.Collections.Specialized
{
    internal sealed class InternalReadOnlyCollection<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private readonly ICollection<T> _collection;

        public InternalReadOnlyCollection(ICollection<T> collection) => 
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _collection).GetEnumerator();

        public void Add(T item) => _collection.Add(item);

        public void Clear() => _collection.Clear();

        public bool Contains(T item) => _collection.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        public bool Remove(T item) => _collection.Remove(item);
    }
}
