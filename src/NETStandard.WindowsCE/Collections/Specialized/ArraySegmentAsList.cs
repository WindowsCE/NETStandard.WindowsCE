using System.Collections.Generic;
using System.Diagnostics;

#if !NET35_CF
using Mock.System.Collections.Generic;
#endif

namespace System.Collections.Specialized
{
    internal sealed class ArraySegmentAsList<T> : IList<T>, IReadOnlyList<T>
    {
        private readonly ArraySegment<T> _segment;

        public ArraySegmentAsList(ArraySegment<T> segment) => _segment = segment;

        public IEnumerator<T> GetEnumerator()
        {
            return _segment.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _segment.GetEnumerator();
        }

        public void Add(T item)
        {
            ThrowInvalidOperationIfDefault();
        }

        public void Clear()
        {
            ThrowInvalidOperationIfDefault();
        }

        public bool Contains(T item)
        {
            ThrowInvalidOperationIfDefault();

            int index = Array.IndexOf<T>(_segment.Array, item, _segment.Offset, _segment.Count);

            Debug.Assert(index == -1 ||
                         (index >= _segment.Offset && index < _segment.Offset + _segment.Count));

            return index >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ThrowInvalidOperationIfDefault();

            Array.Copy(_segment.Array, _segment.Offset, array, arrayIndex, _segment.Count);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count => _segment.Count;

        public bool IsReadOnly => true;

        public int IndexOf(T item)
        {
            ThrowInvalidOperationIfDefault();

            int index = Array.IndexOf<T>(_segment.Array, item, _segment.Offset, _segment.Count);

            Debug.Assert(index == -1 ||
                         (index >= _segment.Offset && index < _segment.Offset + _segment.Count));

            return index >= 0 ? index - _segment.Offset : -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public T this[int index]
        {
            get
            {
                ThrowInvalidOperationIfDefault();
                if (index < 0 || index >= _segment.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _segment.Array[_segment.Offset + index];
            }
            set
            {
                ThrowInvalidOperationIfDefault();
                if (index < 0 || index >= _segment.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                _segment.Array[_segment.Offset + index] = value;
            }
        }

        private void ThrowInvalidOperationIfDefault()
        {
            if (_segment.Array == null)
                throw new InvalidOperationException(SR.InvalidOperation_NullArray);
        }
    }
}
