using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ***** Regexes *****
// - No parameters
// public static ([A-z0-9<,>]+\??) (\w+)(<[A-z, ]+>)?\(this IEnumerable<([A-z0-9?]+)> source\);
// public static $1 $2$3(this IEnumerable<$4> source)\r\n            => Enumerable.$2(source);\r\n
//
// - Single parameter
// public static ([A-z0-9<,>]+\??) (\w+)(<[A-z, ]+>)\(this IEnumerable<([A-z0-9?]+)> source, ([A-z0-9<,>? ]+) (\w+)\);
// public static $1 $2$3(this IEnumerable<$4> source, $5 $6)\r\n            => Enumerable.$2(source, $6);\r\n

#if NET35_CF
namespace System.Linq
#else
namespace Mock.System.Linq
#endif
{
    public static partial class Enumerable2
    {
#if DISABLED
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
            => Enumerable.Aggregate(source, func);

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => Enumerable.Aggregate(source, seed, func);

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => Enumerable.Aggregate(source, seed, func, resultSelector);

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.All(source, predicate);

        public static bool Any<TSource>(this IEnumerable<TSource> source)
            => Enumerable.Any(source);

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.Any(source, predicate);

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
            => Enumerable.AsEnumerable(source);

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
            => Enumerable.Average(source, selector);

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
            => Enumerable.Average(source, selector);

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
            => Enumerable.Average(source, selector);

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
            => Enumerable.Average(source, selector);

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
            => Enumerable.Average(source, selector);

        public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
            => Enumerable.Average(source, selector);

        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
            => Enumerable.Average(source, selector);

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
            => Enumerable.Average(source, selector);

        public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
            => Enumerable.Average(source, selector);

        public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
            => Enumerable.Average(source, selector);

        public static double Average(this IEnumerable<int> source)
            => Enumerable.Average(source);

        public static double Average(this IEnumerable<long> source)
            => Enumerable.Average(source);

        public static double Average(this IEnumerable<double> source)
            => Enumerable.Average(source);

        public static double? Average(this IEnumerable<int?> source)
            => Enumerable.Average(source);

        public static double? Average(this IEnumerable<long?> source)
            => Enumerable.Average(source);

        public static float Average(this IEnumerable<float> source)
            => Enumerable.Average(source);

        public static decimal Average(this IEnumerable<decimal> source)
            => Enumerable.Average(source);

        public static double? Average(this IEnumerable<double?> source)
            => Enumerable.Average(source);

        public static float? Average(this IEnumerable<float?> source)
            => Enumerable.Average(source);

        public static decimal? Average(this IEnumerable<decimal?> source)
            => Enumerable.Average(source);

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
            => Enumerable.Cast<TResult>(source);

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
            => Enumerable.Concat(first, second);

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
            => Enumerable.Contains(source, value);

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
            => Enumerable.Contains(source, value, comparer);

        public static int Count<TSource>(this IEnumerable<TSource> source)
            => Enumerable.Count(source);

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.Count(source, predicate);

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source)
            => Enumerable.DefaultIfEmpty(source);

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
            => Enumerable.DefaultIfEmpty(source, defaultValue);

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
            => Enumerable.Distinct(source);

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
            => Enumerable.Distinct(source, comparer);

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
            => Enumerable.ElementAt(source, index);

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
            => Enumerable.ElementAtOrDefault(source, index);

        public static IEnumerable<TResult> Empty<TResult>()
            => Enumerable.Empty<TResult>();

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
            => Enumerable.Except(first, second);

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
            => Enumerable.Except(first, second, comparer);

        public static TSource First<TSource>(this IEnumerable<TSource> source)
            => Enumerable.First(source);

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.First(source, predicate);

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
            => Enumerable.FirstOrDefault(source);

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.FirstOrDefault(source, predicate);

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => Enumerable.GroupBy(source, keySelector);

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
            => Enumerable.GroupBy(source, keySelector, comparer);

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
            => Enumerable.GroupBy(source, keySelector, elementSelector);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
            => Enumerable.GroupBy(source, keySelector, resultSelector);

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
            => Enumerable.GroupBy(source, keySelector, elementSelector, comparer);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => Enumerable.GroupBy(source, keySelector, resultSelector, comparer);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
            => Enumerable.GroupBy(source, keySelector, elementSelector, resultSelector);

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => Enumerable.GroupBy(source, keySelector, elementSelector, resultSelector, comparer);

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
            => Enumerable.GroupJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector);

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => Enumerable.GroupJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
            => Enumerable.Intersect(first, second);

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
            => Enumerable.Intersect(first, second, comparer);

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
            => Enumerable.Join(outer, inner, outerKeySelector, innerKeySelector, resultSelector);

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => Enumerable.Join(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

        public static TSource Last<TSource>(this IEnumerable<TSource> source)
            => Enumerable.Last(source);

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.Last(source, predicate);

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
            => Enumerable.LastOrDefault(source);

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.LastOrDefault(source, predicate);

        public static long LongCount<TSource>(this IEnumerable<TSource> source)
            => Enumerable.LongCount(source);

        public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.LongCount(source, predicate);

        public static TSource Max<TSource>(this IEnumerable<TSource> source)
            => Enumerable.Max(source);

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
            => Enumerable.Max(source, selector);

        public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
            => Enumerable.Max(source, selector);

        public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
            => Enumerable.Max(source, selector);

        public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
            => Enumerable.Max(source, selector);

        public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
            => Enumerable.Max(source, selector);

        public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
            => Enumerable.Max(source, selector);

        public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
            => Enumerable.Max(source, selector);

        public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
            => Enumerable.Max(source, selector);

        public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
            => Enumerable.Max(source, selector);

        public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
            => Enumerable.Max(source, selector);

        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            => Enumerable.Max(source, selector);

        public static int Max(this IEnumerable<int> source)
            => Enumerable.Max(source);

        public static long Max(this IEnumerable<long> source)
            => Enumerable.Max(source);

        public static double Max(this IEnumerable<double> source)
            => Enumerable.Max(source);

        public static int? Max(this IEnumerable<int?> source)
            => Enumerable.Max(source);

        public static long? Max(this IEnumerable<long?> source)
            => Enumerable.Max(source);

        public static float Max(this IEnumerable<float> source)
            => Enumerable.Max(source);

        public static decimal Max(this IEnumerable<decimal> source)
            => Enumerable.Max(source);

        public static double? Max(this IEnumerable<double?> source)
            => Enumerable.Max(source);

        public static float? Max(this IEnumerable<float?> source)
            => Enumerable.Max(source);

        public static decimal? Max(this IEnumerable<decimal?> source)
            => Enumerable.Max(source);

        public static TSource Min<TSource>(this IEnumerable<TSource> source)
            => Enumerable.Min(source);

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
            => Enumerable.Min(source, selector);

        public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
            => Enumerable.Min(source, selector);

        public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
            => Enumerable.Min(source, selector);

        public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
            => Enumerable.Min(source, selector);

        public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
            => Enumerable.Min(source, selector);

        public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
            => Enumerable.Min(source, selector);

        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
            => Enumerable.Min(source, selector);

        public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
            => Enumerable.Min(source, selector);

        public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
            => Enumerable.Min(source, selector);

        public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
            => Enumerable.Min(source, selector);

        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            => Enumerable.Min(source, selector);

        public static int Min(this IEnumerable<int> source)
            => Enumerable.Min(source);

        public static long Min(this IEnumerable<long> source)
            => Enumerable.Min(source);

        public static double Min(this IEnumerable<double> source)
            => Enumerable.Min(source);

        public static int? Min(this IEnumerable<int?> source)
            => Enumerable.Min(source);

        public static long? Min(this IEnumerable<long?> source)
            => Enumerable.Min(source);

        public static float Min(this IEnumerable<float> source)
            => Enumerable.Min(source);

        public static decimal Min(this IEnumerable<decimal> source)
            => Enumerable.Min(source);

        public static double? Min(this IEnumerable<double?> source)
            => Enumerable.Min(source);

        public static float? Min(this IEnumerable<float?> source)
            => Enumerable.Min(source);

        public static decimal? Min(this IEnumerable<decimal?> source)
            => Enumerable.Min(source);

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
            => Enumerable.OfType<TResult>(source);

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => Enumerable.OrderBy(source, keySelector);

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
            => Enumerable.OrderBy(source, keySelector, comparer);

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => Enumerable.OrderByDescending(source, keySelector);

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
            => Enumerable.OrderByDescending(source, keySelector, comparer);

        public static IEnumerable<int> Range(int start, int count)
            => Enumerable.Range(start, count);

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
            => Enumerable.Repeat(element, count);

        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
            => Enumerable.Reverse(source);

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            => Enumerable.Select(source, selector);

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
            => Enumerable.Select(source, selector);

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
            => Enumerable.SelectMany(source, selector);

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
            => Enumerable.SelectMany(source, selector);

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            => Enumerable.SelectMany(source, collectionSelector, resultSelector);

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            => Enumerable.SelectMany(source, collectionSelector, resultSelector);

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
            => Enumerable.SequenceEqual(first, second);

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
            => Enumerable.SequenceEqual(first, second, comparer);

        public static TSource Single<TSource>(this IEnumerable<TSource> source)
            => Enumerable.Single(source);

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.Single(source, predicate);

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
            => Enumerable.SingleOrDefault(source);

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.SingleOrDefault(source, predicate);

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
            => Enumerable.Skip(source, count);

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.SkipWhile(source, predicate);

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
            => Enumerable.SkipWhile(source, predicate);

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
            => Enumerable.Sum(source, selector);

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
            => Enumerable.Sum(source, selector);

        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
            => Enumerable.Sum(source, selector);

        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
            => Enumerable.Sum(source, selector);

        public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
            => Enumerable.Sum(source, selector);

        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
            => Enumerable.Sum(source, selector);

        public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
            => Enumerable.Sum(source, selector);

        public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
            => Enumerable.Sum(source, selector);

        public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
            => Enumerable.Sum(source, selector);

        public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
            => Enumerable.Sum(source, selector);

        public static int Sum(this IEnumerable<int> source)
            => Enumerable.Sum(source);

        public static long Sum(this IEnumerable<long> source)
            => Enumerable.Sum(source);

        public static double Sum(this IEnumerable<double> source)
            => Enumerable.Sum(source);

        public static int? Sum(this IEnumerable<int?> source)
            => Enumerable.Sum(source);

        public static long? Sum(this IEnumerable<long?> source)
            => Enumerable.Sum(source);

        public static float Sum(this IEnumerable<float> source)
            => Enumerable.Sum(source);

        public static decimal Sum(this IEnumerable<decimal> source)
            => Enumerable.Sum(source);

        public static double? Sum(this IEnumerable<double?> source)
            => Enumerable.Sum(source);

        public static float? Sum(this IEnumerable<float?> source)
            => Enumerable.Sum(source);

        public static decimal? Sum(this IEnumerable<decimal?> source)
            => Enumerable.Sum(source);

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
            => Enumerable.Take(source, count);

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.TakeWhile(source, predicate);

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
            => Enumerable.TakeWhile(source, predicate);

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => Enumerable.ThenBy(source, keySelector);

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
            => Enumerable.ThenBy(source, keySelector, comparer);

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => Enumerable.ThenByDescending(source, keySelector);

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
            => Enumerable.ThenByDescending(source, keySelector, comparer);

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
            => Enumerable.ToArray(source);

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => Enumerable.ToDictionary(source, keySelector);

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
            => Enumerable.ToDictionary(source, keySelector, comparer);

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
            => Enumerable.ToDictionary(source, keySelector, elementSelector);

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
            => Enumerable.ToDictionary(source, keySelector, elementSelector, comparer);

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
            => Enumerable.ToList(source);

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => Enumerable.ToLookup(source, keySelector);

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
            => Enumerable.ToLookup(source, keySelector, comparer);

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
            => Enumerable.ToLookup(source, keySelector, elementSelector);

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
            => Enumerable.ToLookup(source, keySelector, elementSelector, comparer);

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
            => Enumerable.Union(first, second);

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
            => Enumerable.Union(first, second, comparer);

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => Enumerable.Where(source, predicate);

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
            => Enumerable.Where(source, predicate);
#endif
    }
}
