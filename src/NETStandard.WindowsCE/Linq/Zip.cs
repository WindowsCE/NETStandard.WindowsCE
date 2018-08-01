using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable2
    {
        /// <summary>
        /// Merges two sequences by using the specified predicate function.
        /// </summary>
        /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
        /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <param name="first">The first sequence to merge.</param>
        /// <param name="second">The second sequence to merge.</param>
        /// <param name="resultSelector">
        /// A function that specifies how to merge the elements from the two sequences.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> that contains merged elements of two input sequences.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/>, <paramref name="second"/> or <paramref name="resultSelector"/> is null.</exception>
        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func2<TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null)
            {
                throw Error.ArgumentNull(nameof(first));
            }

            if (second == null)
            {
                throw Error.ArgumentNull(nameof(second));
            }

            if (resultSelector == null)
            {
                throw Error.ArgumentNull(nameof(resultSelector));
            }

            return ZipIterator(first, second, resultSelector);
        }

        private static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func2<TFirst, TSecond, TResult> resultSelector)
        {
            using (IEnumerator<TFirst> e1 = first.GetEnumerator())
            using (IEnumerator<TSecond> e2 = second.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    yield return resultSelector(e1.Current, e2.Current);
                }
            }
        }
    }
}
