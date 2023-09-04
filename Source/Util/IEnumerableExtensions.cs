// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SomaSim.Util
{
    /// <summary>
    /// Extensions for enumerables such as those produced by LINQ
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary> Just a bit of syntactic sugar over string.Join </summary>
        public static string JoinToString<T> (this IEnumerable<T> enumerable, string separator = ", ")
            => string.Join(separator, enumerable);

        /// <summary> Performs a Linq Select() and feeds the results to string.Join </summary>
        public static string SelectToString<T> (this IEnumerable<T> enumerable, Func<T, string> selector, string separator = ", ")
            => string.Join(separator, enumerable.Select(selector));

        /// <summary>
        /// Given a source IEnumerable, returns a type-safe IEnumerable[T] that contains only those elements
        /// of the source that are of type TResult, as determined by "is" returning true.
        /// </summary>
        public static IEnumerable<TResult> WhereTypeIs<TResult> (this IEnumerable source) {
            foreach (var element in source) {
                if (element is TResult result) { yield return result; }
            }
        }

        /// <summary> Given a source IEnumerable of reference types, performs Where(x => x != null) </summary>
        public static IEnumerable<T> WhereNotNull<T> (this IEnumerable<T> enumerable) where T : class
            => enumerable.Where(x => x != null);

        /// <summary>
        /// Given a source IEnumerable[T], calls the action on each element produced by this enumerable.
        /// Note: unlike most Linq queries, this one is eager rather than lazy, and will consume the enumerable immediately.
        /// </summary>
        public static void ForEach<T> (this IEnumerable<T> source, Action<T> action) {
            foreach (var element in source) { action(element); }
        }

        /// <summary>
        /// Given a source IEnumerable[T], adds all the source elements to the destination ICollection[T].
        /// If the clear parameter is true, the destination list will be cleared first before adding elements.
        /// Note: unlike most Linq queries, this one is eager rather than lazy, and will consume the enumerable immediately.
        /// </summary>
        public static void SendTo<T> (this IEnumerable<T> source, ICollection<T> destination, bool clearFirst = false) {
            if (clearFirst) { destination.Clear(); }
            foreach (var element in source) { destination.Add(element); }
        }

        /// <summary> Like Sum() but for Fixnums </summary>
        public static Fixnum Sum (this IEnumerable<Fixnum> source) {
            Fixnum result = 0;
            foreach (var item in source) { result += item; }
            return result;
        }

        /// <summary> Given a source enumerable and a predicate, returns two new lists, one of all elements
        /// for which the predicate passes, the other one for which the predicate fails. </summary>
        public static (List<T> passing, List<T> failing) SplitOn<T> (this IEnumerable<T> items, Predicate<T> predicate) {
            List<T> passing = new List<T>(), failing = new List<T>();
            foreach (var item in items) {
                var list = predicate(item) ? passing : failing;
                list.Add(item);
            }
            return (passing, failing);
        }
    }
}
