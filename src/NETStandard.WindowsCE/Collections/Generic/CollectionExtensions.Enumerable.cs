using System.Collections.Specialized;

namespace System.Collections.Generic
{
    public static partial class CollectionExtensions
    {
        public static IList<T> AsList<T>(this ArraySegment<T> source) =>
            new ArraySegmentAsList<T>(source);

        public static IEnumerator<T> GetEnumerator<T>(this ArraySegment<T> source) =>
            new ArraySegmentEnumerator<T>(source);
    }
}