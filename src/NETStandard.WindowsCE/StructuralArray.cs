using System.Collections;

namespace System
{
    internal sealed class StructuralArray : IStructuralComparable, IStructuralEquatable
    {
        private readonly Array _array;

        public StructuralArray(Array array) => _array = array;

        public override string ToString() => _array.ToString();

        public override bool Equals(object obj) => _array.Equals(obj);

        public override int GetHashCode() => _array.GetHashCode();

        public int CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }

            Array o = other as Array;

            if (o == null || _array.Length != o.Length)
            {
                //ThrowHelper.ThrowArgumentException(ExceptionResource.ArgumentException_OtherNotArrayOfCorrectLength, ExceptionArgument.other);
                throw new ArgumentException(SR.ArgumentException_OtherNotArrayOfCorrectLength, nameof(other));
            }

            int i = 0;
            int c = 0;

            while (i < o.Length && c == 0)
            {
                object left = _array.GetValue(i);
                object right = o.GetValue(i);

                c = comparer.Compare(left, right);
                i++;
            }

            return c;
        }

        public bool Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }

            if (object.ReferenceEquals(_array, other))
            {
                return true;
            }

            Array o = other as Array;

            if (o == null || o.Length != _array.Length)
            {
                return false;
            }

            int i = 0;
            while (i < o.Length)
            {
                object left = _array.GetValue(i);
                object right = o.GetValue(i);

                if (!comparer.Equals(left, right))
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        public int GetHashCode(IEqualityComparer comparer)
        {
            if (comparer == null)
            {
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparer);
                throw new ArgumentNullException(nameof(comparer));
            }

            int ret = 0;

            for (int i = (_array.Length >= 8 ? _array.Length - 8 : 0); i < _array.Length; i++)
            {
                ret = CombineHashCodes(ret, comparer.GetHashCode(_array.GetValue(i)));
            }

            return ret;
        }

        // From System.Web.Util.HashCodeCombiner
        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }
    }
}
