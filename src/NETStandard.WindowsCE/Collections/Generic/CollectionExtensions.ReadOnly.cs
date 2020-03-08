using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Collections.Generic
{
    public static partial class CollectionExtensions
    {
        public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> source) =>
            new InternalReadOnlyCollection<T>(source);

        public static IReadOnlyList<T> AsReadOnly<T>(this ArraySegment<T> source) =>
            new ArraySegmentAsList<T>(source);

        public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> source) =>
            new InternalReadOnlyList<T>(source);

        public static IReadOnlyList<T> AsReadOnlyList<T>(this List<T> source) =>
            new InternalReadOnlyList<T>(source);

        public static IReadOnlyDictionary<TKey, TValue>
            AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> source) =>
            new ReadOnlyDictionary<TKey, TValue>(source);
    }
}