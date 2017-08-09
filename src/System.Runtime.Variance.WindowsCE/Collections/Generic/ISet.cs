namespace System.Collections.Generic
{
    public interface ISet2<T> : ICollection<T>, IEnumerable, IEnumerable2<T>
    {
        new bool Add(T item);
        void ExceptWith(IEnumerable2<T> other);
        void IntersectWith(IEnumerable2<T> other);
        bool IsProperSubsetOf(IEnumerable2<T> other);
        bool IsProperSupersetOf(IEnumerable2<T> other);
        bool IsSubsetOf(IEnumerable2<T> other);
        bool IsSupersetOf(IEnumerable2<T> other);
        bool Overlaps(IEnumerable2<T> other);
        bool SetEquals(IEnumerable2<T> other);
        void SymmetricExceptWith(IEnumerable2<T> other);
        void UnionWith(IEnumerable2<T> other);
    }
}
