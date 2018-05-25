using System.Collections;
using System.Collections.Generic;

namespace System
{
    public static class Array2
    {
        // We impose limits on maximum array lenght in each dimension to allow efficient 
        // implementation of advanced range check elimination in future.
        // Keep in sync with vm\gcscan.cpp and HashHelpers.MaxPrimeArrayLength.
        // The constants are defined in this method: inline SIZE_T MaxArrayLength(SIZE_T componentSize) from gcscan
        // We have different max sizes for arrays with elements of size 1 for backwards compatibility
        internal const int MaxArrayLength = 0X7FEFFFFF;
        internal const int MaxByteArrayLength = 0x7FFFFFC7;

        public static int BinarySearch<T>(T[] array, T value) =>
            Array.BinarySearch(array, value);

        public static int BinarySearch<T>(T[] array, T value, IComparer<T> comparer) =>
            Array.BinarySearch(array, value, comparer);

        public static int BinarySearch<T>(T[] array, int index, int length, T value) =>
            Array.BinarySearch(array, index, length, value);

        public static int BinarySearch<T>(T[] array, int index, int length, T value, IComparer<T> comparer) =>
            Array.BinarySearch(array, index, length, value, comparer);

        public static int BinarySearch(Array array, int index, int length, object value) =>
            Array.BinarySearch(array, index, length, value, null);

        public static int BinarySearch(Array array, int index, int length, object value, IComparer comparer) =>
            Array.BinarySearch(array, index, length, value, comparer);

        public static int BinarySearch(Array array, object value) =>
            Array.BinarySearch(array, value);

        public static int BinarySearch(Array array, object value, IComparer comparer) =>
            Array.BinarySearch(array, 0, array?.Length ?? -1, value, comparer);

        public static void Clear(Array array, int index, int length) =>
            Array.Clear(array, index, length);

        [Obsolete(Consts.PlatformNotSupportedDescription, true)]
        public static void ConstrainedCopy(
            Array sourceArray,
            int sourceIndex,
            Array destinationArray,
            int destinationIndex,
            int length) =>
            Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

        public static void Copy(Array sourceArray, Array destinationArray, int length) =>
            Array.Copy(sourceArray, destinationArray, length);

        public static void Copy(
            Array sourceArray,
            int sourceIndex,
            Array destinationArray,
            int destinationIndex,
            int length) =>
            Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

        public static Array CreateInstance(Type elementType, int length) =>
            Array.CreateInstance(elementType, length);

        public static Array CreateInstance(Type elementType, params int[] lengths) =>
            Array.CreateInstance(elementType, lengths);

        [Obsolete(Consts.PlatformNotSupportedDescription, true)]
        public static Array CreateInstance(Type elementType, int[] lengths, int[] lowerBounds) =>
            throw new NotSupportedException();
        
        public static T[] Empty<T>() =>
            EmptyArray<T>.Value;

        public static bool Exists<T>(T[] array, Predicate<T> match) =>
            FindIndex(array, match) != -1;

        public static T Find<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < array.Length; i++)
            {
                T value = array[i];
                if (match(value))
                    return value;
            }

            return default;
        }

        public static T[] FindAll<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int pos = 0;
            T[] result = Empty<T>();
            for (int i = 0; i < array.Length; i++)
            {
                T value = array[i];
                if (!match(value))
                    continue;

                if (pos == result.Length)
                    Resize(ref result, Math.Min(pos == 0 ? 4 : pos * 2, array.Length));

                result[pos++] = value;
            }

            if (pos != result.Length)
                Resize(ref result, pos);

            return result;
        }

        public static int FindIndex<T>(T[] array, Predicate<T> match) =>
            FindIndex(array, 0, array?.Length ?? -1, match);

        public static int FindIndex<T>(T[] array, int startIndex, Predicate<T> match) =>
            FindIndex(array, startIndex, array?.Length - startIndex ?? -1, match);

        public static int FindIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0 || startIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            if (count < 0 || startIndex > array.Length - count)
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(array[i]))
                    return i;
            }

            return -1;
        }

        public static T FindLast<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = array.Length - 1; i >= 0; i--)
            {
                T value = array[i];
                if (match(value))
                    return value;
            }

            return default;
        }

        public static int FindLastIndex<T>(T[] array, Predicate<T> match) =>
            FindLastIndex(array, array?.Length - 1 ?? -1, array?.Length ?? -1, match);

        public static int FindLastIndex<T>(T[] array, int startIndex, Predicate<T> match) =>
            FindLastIndex(array, startIndex, startIndex + 1, match);

        public static int FindLastIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            if (array.Length == 0)
            {
                // Special case for 0 length List
                if (startIndex != -1)
                    throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }
            else
            {
                // Make sure we're not out of range            
                if (startIndex < 0 || startIndex >= array.Length)
                    throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if (count < 0 || startIndex - count + 1 < 0)
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);

            int endIndex = startIndex - count;
            for (int i = startIndex; i > endIndex; i--)
            {
                if (match(array[i]))
                    return i;
            }

            return -1;
        }

        public static int IndexOf<T>(T[] array, T value) =>
            Array.IndexOf(array, value);

        public static int IndexOf<T>(T[] array, T value, int startIndex) =>
            Array.IndexOf(array, value, startIndex);

        public static int IndexOf<T>(T[] array, T value, int startIndex, int count) =>
            Array.IndexOf(array, value, startIndex, count);

        public static int IndexOf(Array array, object value) =>
            Array.IndexOf(array, value, array?.GetLowerBound(0) ?? -1, array?.Length ?? -1);

        public static int IndexOf(Array array, object value, int startIndex) =>
            Array.IndexOf(array, value, startIndex, array?.Length - startIndex + array?.GetLowerBound(0) ?? -1);

        public static int IndexOf(Array array, object value, int startIndex, int count) =>
            Array.IndexOf(array, value, startIndex, count);

        public static int LastIndexOf<T>(T[] array, T value) =>
            Array.LastIndexOf(array, value);

        public static int LastIndexOf<T>(T[] array, T value, int startIndex) =>
            Array.LastIndexOf(array, value, startIndex);

        public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count) =>
            Array.LastIndexOf(array, value, startIndex, count);

        public static int LastIndexOf(Array array, object value) =>
            Array.LastIndexOf(array, value, array?.Length - 1 ?? -1, array?.Length ?? -1);

        public static int LastIndexOf(Array array, object value, int startIndex) =>
            Array.LastIndexOf(array, value, startIndex, startIndex + 1);

        public static int LastIndexOf(Array array, object value, int startIndex, int count) =>
            Array.LastIndexOf(array, value, startIndex, count);

        public static void Resize<T>(ref T[] array, int newSize) =>
            Array.Resize(ref array, newSize);

        public static void Reverse(Array array) =>
            Array.Reverse(array);

        public static void Reverse(Array array, int index, int length) =>
            Array.Reverse(array, index, length);

        public static void Sort<T>(T[] array) =>
            Array.Sort(array);

        public static void Sort<T>(T[] array, IComparer<T> comparer) =>
            Array.Sort(array, comparer);

        public static void Sort<T>(T[] array, int index, int length) =>
            Array.Sort(array, index, length);

        public static void Sort<T>(T[] array, Comparison<T> comparison) =>
            Array.Sort(array, comparison);

        public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer) =>
            Array.Sort(array, index, length, comparer);

        public static void Sort(Array array) =>
            Array.Sort(array);

        public static void Sort(Array array, IComparer comparer) =>
            Array.Sort(array, comparer);

        public static void Sort(Array array, int index, int length) =>
            Array.Sort(array, index, length, null);

        public static void Sort(Array array, int index, int length, IComparer comparer) =>
            Array.Sort(array, index, length, comparer);

        public static bool TrueForAll<T>(T[] array, Predicate<T> match) =>
            Array.TrueForAll(array, match);

        private static class EmptyArray<T>
        {
            internal static readonly T[] Value = new T[0];
        }
    }
}