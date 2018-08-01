using System.Collections.Generic;
using System.Diagnostics;

namespace System.Linq
{
    public static partial class Enumerable2
    {
        public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (count <= 0)
            {
                return source.Skip(0);
            }

            return SkipLastIterator(source, count);
        }

        private static IEnumerable<TSource> SkipLastIterator<TSource>(IEnumerable<TSource> source, int count)
        {
            Debug.Assert(source != null);
            Debug.Assert(count > 0);

            var queue = new Queue<TSource>();

            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (queue.Count == count)
                    {
                        do
                        {
                            yield return queue.Dequeue();
                            queue.Enqueue(e.Current);
                        }
                        while (e.MoveNext());
                        break;
                    }
                    else
                    {
                        queue.Enqueue(e.Current);
                    }
                }
            }
        }
    }
}
