using System.Collections;
using System.Collections.Generic;

namespace System
{
    internal sealed class ArraySegmentEnumerator<T> : IEnumerator<T>
    {
        private T[] _array;
        private int _start;
        private int _end;
        private int _current;

        internal ArraySegmentEnumerator(ArraySegment<T> arraySegment)
        {
            this._array = arraySegment.Array ?? throw new InvalidOperationException(SR.InvalidOperation_NullArray);
            this._start = arraySegment.Offset;
            this._end = this._start + arraySegment.Count;
            this._current = this._start - 1;
        }

        public bool MoveNext()
        {
            if (this._current >= this._end)
                return false;
            ++this._current;
            return this._current < this._end;
        }

        public T Current
        {
            get
            {
                if (this._current < this._start)
                    throw new InvalidOperationException(SR.InvalidOperation_EnumNotStarted);
                if (this._current >= this._end)
                    throw new InvalidOperationException(SR.InvalidOperation_EnumEnded);
                return this._array[this._current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return (object)this.Current;
            }
        }

        void IEnumerator.Reset()
        {
            this._current = this._start - 1;
        }

        public void Dispose()
        {
        }
    }
}
