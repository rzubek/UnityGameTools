// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;

namespace SomaSim.Util
{
    public static class ListExtensions
    {
        /// <summary> Returns true if the list is empty </summary>
        public static bool IsEmpty<T> (this List<T> list) => list.Count == 0;

        /// <summary> Returns true if the list is not empty </summary>
        public static bool IsNotEmpty<T> (this List<T> list) => list.Count > 0;

        /// <summary>
        /// Binary equivalent of built-in AddRange()
        /// </summary>
        public static void AddRange<T> (this List<T> list, T a, T b) {
            list.Add(a);
            list.Add(b);
        }

        /// <summary>
        /// Vararg equivalent of built-in AddRange()
        /// </summary>
        public static void AddRange<T> (this List<T> list, params T[] elements) => list.AddRange(elements);

        /// <summary>
        /// Clears this list and then adds all elements from the specified list.
        /// </summary>
        public static void ClearAndAddRange<T> (this List<T> list, IEnumerable<T> elements) {
            list.Clear();
            list.AddRange(elements);
        }

        /// <summary>
        /// Adds all elements from the IEnumerable, but safely skips if the variable is null
        /// </summary>
        public static void AddRangeIfNotNull<T> (this List<T> list, IEnumerable<T> elements) {
            if (elements != null) { list.AddRange(elements); }
        }

        /// <summary>
        /// Adds the given element multiple times. If T is a reference type, multiple references
        /// to the same item will be added. 
        /// Note: this operation executes immediately (unlike Linq operations).
        /// </summary>
        public static void AddTimes<T> (this List<T> list, T value, int count) {
            for (int i = 0; i < count; i++) {
                list.Add(value);
            }
        }

        /// <summary>
        /// Ensures the list has a given number of objects, by either removing
        /// elements from the end if the current list is too large, or by adding
        /// new default(T) elements to the end if it is too small.
        /// </summary>
        public static void EnsureCount<T> (this List<T> list, int count) {
            if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count)); }

            while (list.Count > count) { list.RemoveAt(list.Count - 1); }
            while (list.Count < count) { list.Add(default); }
        }

        /// <summary>
        /// Calls the specified factory given number of times, and adds result to this list.
        /// Note: this operation executes immediately (unlike Linq operations).
        /// </summary>
        public static void AddTimes<T> (this List<T> list, Func<T> factory, int count) {
            for (int i = 0; i < count; i++) {
                list.Add(factory());
            }
        }

        /// <summary>
        /// Creates a specified number of new instances of T and adds result to this list.
        /// Note: this operation executes immediately (unlike Linq operations).
        /// </summary>
        public static void AddTimes<T> (this List<T> list, int count) where T : new() {
            for (int i = 0; i < count; i++) {
                list.Add(new T());
            }
        }


        /// <summary>
        /// Removes and returns element at the specified index. 
        /// Unlike RemoveAt this function has O(1) performance, but it changes list element ordering.
        /// </summary>
        /// <returns>Returns the element that was removed from the indicated index</returns>
        public static T SwapRemoveAt<T> (this List<T> list, int index) {
            var lastIndex = list.Count - 1;
            T result;

            if (index == lastIndex) {   // removing last item
                result = list[lastIndex];
                list.RemoveAt(lastIndex);

            } else if (index >= 0 && index < lastIndex) { // swap remove in the middle
                result = list[index];
                list[index] = list[lastIndex];
                list.RemoveAt(lastIndex);

            } else {    // error condition
                result = default;
            }

            return result;
        }

        /// <summary>
        /// Swaps elements at the given positions, or throws an exception if the positions are invalid.
        /// </summary>
        public static void Swap<T> (this List<T> list, int a, int b) =>
            (list[b], list[a]) = (list[a], list[b]);

        /// <summary>
        /// If the element is found in the list, it removes it and returns true; returns false otherwise.
        /// Unlike Remove this function has O(n) perf in search but O(1) perf in removal, and it changes list element ordering.
        /// </summary>
        /// <returns>Returns the element that was removed from the indicated index</returns>
        public static bool SwapRemove<T> (this List<T> list, T item) {
            int index = list.IndexOf(item);
            if (index >= 0) {
                list.SwapRemoveAt(index);
                return true;
            } else {
                return false;
            }
        }


        /// <summary>
        /// Returns the last element of the list, or throws an exception if the list is empty.
        /// </summary>
        public static T RemoveLast<T> (this List<T> list) {
            if (list.Count > 0) {
                int lastIndex = list.Count - 1;
                T last = list[lastIndex];
                list.RemoveAt(lastIndex);
                return last;
            } else {
                throw new InvalidOperationException("Failed to remove last element from an empty list");
            }
        }

        /// <summary>
        /// Removes and returns the item at the specified index, or throws an exception if index is invalid.
        /// </summary>
        public static T RemoveAndReturn<T> (this List<T> list, int index) {
            T result = list[index];
            list.RemoveAt(index);
            return result;
        }

        /// <summary>
        /// Removes and returns the item at the specified index, or default value if index is invalid.
        /// </summary>
        public static T RemoveAndReturnOrDefault<T> (this List<T> list, int index)
            => (list != null && index >= 0 && index < list.Count) ? RemoveAndReturn(list, index) : default;

        /// <summary>
        /// Removes and returns the last element of the list, or the default value for the type if the list was empty.
        /// </summary>
        public static T RemoveLastOrDefault<T> (this List<T> list) =>
            RemoveAndReturnOrDefault(list, list.Count - 1);

        /// <summary>
        /// Removes and returns the first element of the list, or the default value for the type if the list was empty.
        /// </summary>
        public static T RemoveFirstOrDefault<T> (this List<T> list) =>
            RemoveAndReturnOrDefault(list, 0);

        /// <summary>
        /// Returns the last element in the list, or the default value for T (eg. null or zero) if the list is empty.
        /// </summary>
        public static T LastOrDefaultFast<T> (this List<T> list)
            => list.Count > 0 ? list[^1] : default;

        /// <summary>
        /// Returns the first element in the list, or the default value for T (eg. null or zero) if the list is empty.
        /// </summary>
        public static T FirstOrDefaultFast<T> (this List<T> list)
            => list.Count > 0 ? list[0] : default;

        /// <summary>
        /// If the index is valid, returns the element at that index, or default value for T (eg. null or zero) otherwise.
        /// </summary>
        public static T GetOrDefaultFast<T> (this List<T> list, int index)
            => (list != null && index >= 0 && index < list.Count) ? list[index] : default;

        /// <summary>
        /// If the index is valid, returns the element at that index, or specified default value otherwise.
        /// </summary>
        public static T GetOrDefaultFast<T> (this List<T> list, int index, T defaultValue)
            => (list != null && index >= 0 && index < list.Count) ? list[index] : defaultValue;

        /// <summary>
        /// Returns the index of the given item in the list, up to but excluding the end position,
        /// otherwise -1 if not found before the end, or the list is empty or null.
        /// </summary>
        public static int FindIndexBefore<T> (this List<T> list, T value, int end) where T : IEquatable<T> {
            if (list != null) {
                var max = (list.Count < end) ? list.Count : end;
                for (int i = 0; i < max; i++) {
                    if (value.Equals(list[i])) { return i; }
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the given item in the list, up to but excluding the end position,
        /// otherwise -1 if not found before the end, or the list is empty or null.
        /// </summary>
        public static int FindIndexBeforeByRef<T> (this List<T> list, T value, int end) where T : class {
            if (list != null) {
                var max = (list.Count < end) ? list.Count : end;
                for (int i = 0; i < max; i++) {
                    if (value == list[i]) { return i; }
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the given item in the list, otherwise -1 if not found, or the list is empty or null.
        /// </summary>
        public static int FindIndex<T> (this List<T> list, T value) where T : IEquatable<T> =>
            FindIndexBefore(list, value, int.MaxValue);


        // pseudo-functional

        /// <summary>
        /// Adds all elements of other to the end of this list, and return this list (now modified) for further processing.
        /// </summary>
        public static List<T> AddRangeAndReturn<T> (this List<T> list, IEnumerable<T> other) {
            list.AddRange(other);
            return list;
        }

        /// <summary>
        /// Adds all elements of other to the end of this list, and return this list (now modified) for further processing.
        /// </summary>
        public static List<T> AddRangeAndReturn<T> (this List<T> list, params T[] other) {
            list.AddRange(other);
            return list;
        }

        // autofill

        /// <summary>
        /// Creates and returns a list with a new instance of T being created for each element.
        /// </summary>
        public static List<T> MakeFilled<T> (int count) where T : new() {
            List<T> list = new List<T>(count);
            list.AddTimes(count);
            return list;
        }

        /// <summary>
        /// Creates and returns a list with the value repeated the specified number of times.
        /// </summary>
        public static List<T> MakeFilled<T> (int count, T value) {
            List<T> list = new List<T>(count);
            list.AddTimes(value, count);
            return list;
        }

        /// <summary>
        /// Given a list and an index, if this index is larger than the list size, it adds enough
        /// default values to make the index valid, and finally returns the value stored at that index.
        /// </summary>
        public static T GetWithAutofill<T> (this List<T> list, int index, T defaultvalue) {
            while (index >= list.Count) { list.Add(defaultvalue); }
            return list[index];
        }

        /// <summary>
        /// Given a list and an index, if this index is larger than the list size, it adds enough
        /// default values to make the index valid, and finally returns the value stored at that index.
        /// </summary>
        public static T GetWithAutofill<T> (this List<T> list, int index) where T : new() {
            while (index >= list.Count) { list.Add(new T()); }
            return list[index];
        }

        /// <summary>
        /// Given a list and an index, if this index is larger than the list size, it adds enough
        /// default values to make the index valid, and finally sets the value stored at that index.
        /// </summary>
        public static void SetWithAutofill<T> (this List<T> list, int index, T value, T defaultvalue) {
            while (index >= list.Count) { list.Add(defaultvalue); }
            list[index] = value;
        }

        /// <summary>
        /// Given a list and an index, if this index is larger than the list size, it adds enough
        /// default values to make the index valid, and finally sets the value stored at that index.
        /// </summary>
        public static void SetWithAutofill<T> (this List<T> list, int index, T value) where T : new() {
            while (index >= list.Count) { list.Add(new T()); }
            list[index] = value;
        }

        /// <summary>
        /// Given a list and an index, if this index is larger than the list size, it adds enough
        /// default values to make the index valid, and finally increments the value stored at that index by delta.
        /// </summary>
        public static void IncrementWithAutofill (this List<int> list, int index, int delta, int defaultvalue = 0) {
            int value = list.GetWithAutofill(index, defaultvalue);
            list[index] = value + delta;
        }

        /// <summary>
        /// Given a list and an index, if this index is larger than the list size, it adds enough
        /// default values to make the index valid, and finally increments the value stored at that index by delta.
        /// </summary>
        public static void IncrementWithAutofill (this List<float> list, int index, float delta, float defaultvalue = 0f) {
            float value = list.GetWithAutofill(index, defaultvalue);
            list[index] = value + delta;
        }


        // select into

        /// <summary>
        /// Takes an existing destination list and selects (ie. maps) items from the current list into it.
        /// </summary>
        public static void SelectInto<S, T> (this List<S> source, Func<S, T> func, List<T> destination) {
            for (int i = 0, count = source.Count; i < count; ++i) {
                destination.Add(func(source[i]));
            }
        }

        /// <summary>
        /// Takes an existing destination list and selects (ie. maps) items from the current list into it.
        /// </summary>
        public static void SelectInto<S, T> (this List<S> source, Func<int, S, T> func, List<T> destination) {
            for (int i = 0, count = source.Count; i < count; ++i) {
                destination.Add(func(i, source[i]));
            }
        }

        /// <summary>
        /// Takes an existing destination list and adds those items from the source list that pass the predicate.
        /// </summary>
        public static void WhereInto<T> (this List<T> source, Predicate<T> predicate, List<T> destination) {
            for (int i = 0, count = source.Count; i < count; ++i) {
                if (predicate(source[i])) {
                    destination.Add(source[i]);
                }
            }
        }

        /// <summary>
        /// Selects (ie. maps) items from the current list into a new list.
        /// Similar to calling List[T].Select(func).ToList() except the resulting list
        /// is pre-allocated as appropriately sized, and does not cause multiple re-sizings.
        /// </summary>
        public static List<T> SelectIntoNewList<S, T> (this List<S> source, Func<S, T> func) {
            var list = new List<T>(source.Count);
            source.SelectInto(func, list);
            return list;
        }

        /// <summary>
        /// Selects (ie. maps) items from the current list into a new list.
        /// Similar to calling List[T].Select(func).ToList() except the resulting list
        /// is pre-allocated as appropriately sized, and does not cause multiple re-sizings.
        /// </summary>
        public static List<T> SelectIntoNewList<S, T> (this List<S> source, Func<int, S, T> func) {
            var list = new List<T>(source.Count);
            source.SelectInto(func, list);
            return list;
        }

        /// <summary>
        /// Given a source list and a selector function, evaluates the selector on all elements and returns the sum.
        /// </summary>
        public static int SelectAndSum<T> (this List<T> source, Func<T, int> selector) {
            int sum = 0;
            for (int i = 0, count = source.Count; i < count; ++i) {
                sum += selector(source[i]);
            }
            return sum;
        }

        /// <summary>
        /// Given a source list and a selector function, evaluates the selector on all elements and returns the sum.
        /// </summary>
        public static Fixnum SelectAndSum<T> (this List<T> source, Func<T, Fixnum> selector) {
            Fixnum sum = 0;
            for (int i = 0, count = source.Count; i < count; ++i) {
                sum += selector(source[i]);
            }
            return sum;
        }

        /// <summary>
        /// Reverses the list in place, without allocating a temporary list like Mono does.
        /// </summary>
        public static List<T> ReverseInPlace<T> (this List<T> list) {
            int first = 0, last = list.Count - 1;
            while (first < last) {
                (list[last], list[first]) = (list[first], list[last]);
                first++;
                last--;
            }
            return list;
        }

        /// <summary>
        /// Checks that both lists have the same elements in the same order (elements are compared shallowly).
        /// </summary>
        public static bool ElementsEqual<T> (this List<T> lhs, List<T> rhs) where T : IEquatable<T> {
            if (lhs.Count != rhs.Count) {
                return false;
            }

            for (int i = 0, count = lhs.Count; i < count; i++) {
                if (!lhs[i].Equals(rhs[i])) {
                    return false;
                }
            }

            return true;
        }

        /// <summary> Counts how many instances of this element there are in the list,
        /// without using LINQ and creating a new closure. </summary>
        public static int CountFast<T> (this List<T> list, T element) where T : IEquatable<T> {
            int sum = 0;
            for (int i = 0, count = list.Count; i < count; i++) {
                if (list[i].Equals(element)) { sum++; }
            }
            return sum;
        }

        /// <summary> Returns true if this list contains at least one instance of the element,
        /// using the typed equality test and without using LINQ. </summary>
        public static bool ContainsFast<T> (this List<T> list, T element) where T : IEquatable<T> {
            for (int i = 0, count = list.Count; i < count; i++) {
                if (list[i].Equals(element)) { return true; }
            }
            return false;
        }

        /// <summary> Adds specified value if not already contained in the list and returns true,
        /// or returns false and leaves the list unchanged if it's already there. 
        /// Uses the typed equality test rather than using LINQ. </summary>
        public static bool AddIfNotContains<T> (this List<T> list, T element) where T : IEquatable<T> {
            bool add = !ContainsFast(list, element);
            if (add) { list.Add(element); }
            return add;
        }

        /// <summary> Adds specified value if it's not null </summary>
        public static void AddIfNotNull<T> (this List<T> list, T element) where T : class {
            if (element != null) { list.Add(element); }
        }

        /// <summary> Adds specified value if it's not already contained </summary>
        public static void AddIfNotContained<T> (this List<T> list, T element, bool skipnull = true) where T : class {
            bool elementOk = element != null || !skipnull;
            if (elementOk && !list.Contains(element)) { list.Add(element); }
        }

        /// <summary> Returns true if the list contains an instance of any of the items in the passed in another list
        /// using the typed equality test and without using LINQ </summary>
        public static bool ContainsAnyOf<T> (this List<T> list, List<T> other) {
            for (int i = 0, count = other.Count; i < count; i++) {
                if (list.Contains(other[i])) { return true; }
            }
            return false;
        }

        /// <summary> Returns true if the list contains all of the items in the passed in other list
        /// using the typed equality test and without using LINQ </summary>
        public static bool ContainsAllOf<T> (this List<T> list, List<T> other) {
            for (int i = 0, count = other.Count; i < count; i++) {
                if (!list.Contains(other[i])) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Destructively removes duplicates from a list of IEquatables
        /// </summary>
        public static void RemoveDuplicates<T> (this List<T> list) where T : struct, IEquatable<T> {
            // go backwards because we're removing
            for (int i = list.Count - 1; i >= 0; i--) {
                bool dupe = ContainsBefore(list, i);
                if (dupe) { list.RemoveAt(i); }
            }

            static bool ContainsBefore (List<T> list, int index) => list.FindIndexBefore(list[index], index) >= 0;
        }

        /// <summary>
        /// Destructively removes duplicates from a list of references
        /// </summary>
        public static void RemoveDuplicateInstances<T> (this List<T> list) where T : class {
            // go backwards because we're removing
            for (int i = list.Count - 1; i >= 0; i--) {
                bool dupe = ContainsBefore(list, i);
                if (dupe) { list.RemoveAt(i); }
            }

            static bool ContainsBefore (List<T> list, int index) => list.FindIndexBeforeByRef(list[index], index) >= 0;
        }


        //
        // numeric operations on ints

        /// <summary>
        /// Performs a simple sum on a list of numbers. 
        /// Faster than Mono Linq implementation because it does not involve delegates.
        /// </summary>
        public static int SumFast (this List<int> list) {
            int sum = 0;
            for (int i = 0, count = list.Count; i < count; i++) {
                sum += list[i];
            }
            return sum;
        }

        /// <summary>
        /// Returns the largest value of a list of numbers, or no value if the list is empty.
        /// Faster than Mono Linq implementation because it does not involve delegates.
        /// </summary>
        public static int? MaxFast (this List<int> list) {
            var index = list.ArgMaxFast();
            return index >= 0 ? list[index] : (int?) null;
        }

        /// <summary>
        /// Returns the index of the largest value of a list of numbers, or -1 if the list is empty.
        /// </summary>
        public static int ArgMaxFast (this List<int> list) {
            int index = -1;
            int max = int.MinValue;

            for (int i = 0, count = list.Count; i < count; i++) {
                int val = list[i];
                if (index == -1 || val > max) {
                    index = i;
                    max = val;
                }
            }

            return index;
        }

        /// <summary>
        /// Calculates the average of a list of numbers. 
        /// </summary>
        public static float? AverageFast (this List<int> list) {
            if (list.Count == 0) { return null; }
            return ((float) list.SumFast()) / list.Count;
        }


        //
        // numeric operations on floats

        /// <summary>
        /// Performs a simple sum on a list of numbers. 
        /// Faster than Mono Linq implementation because it does not involve delegates.
        /// </summary>
        public static float SumFast (this List<float> list) {
            float sum = 0f;
            for (int i = 0, count = list.Count; i < count; i++) {
                sum += list[i];
            }
            return sum;
        }

        /// <summary>
        /// Returns the largest value of a list of numbers, or no value if the list is empty.
        /// Faster than Mono Linq implementation because it does not involve delegates.
        /// </summary>
        public static float? MaxFast (this List<float> list) {
            var index = list.ArgMaxFast();
            return index >= 0 ? list[index] : (float?) null;
        }

        /// <summary>
        /// Returns the index of the largest value of a list of numbers, or -1 if the list is empty.
        /// </summary>
        public static int ArgMaxFast (this List<float> list) {
            int index = -1;
            float max = float.NegativeInfinity;

            for (int i = 0, count = list.Count; i < count; i++) {
                float val = list[i];
                if (index == -1 || val > max) {
                    index = i;
                    max = val;
                }
            }

            return index;
        }

        /// <summary>
        /// Calculates the average of a list of numbers. 
        /// </summary>
        public static float? AverageFast (this List<float> list) {
            if (list.Count == 0) { return null; }
            return list.SumFast() / list.Count;
        }

        /// <summary>
        /// Normalizes a list of weights to add up to 1.0, as an optimization before calling PickElement
        /// many times, and returns true. When the list is empty or adds up to zero, 
        /// the list is unmodified and returns false. 
        /// </summary>
        public static bool Normalize (this List<float> weights) {
            float sum = 0;
            for (int i = 0, count = weights.Count; i < count; i++) { sum += weights[i]; }

            if (sum == 0) { return false; }
            if (sum == 1) { return true; }

            for (int i = 0, count = weights.Count; i < count; i++) { weights[i] /= sum; }
            return true;
        }



        //
        // numeric operations on fixnums

        /// <summary>
        /// Performs a simple sum on a list of numbers. 
        /// Faster than Mono Linq implementation because it does not involve delegates.
        /// </summary>
        public static Fixnum SumFast (this List<Fixnum> list) {
            Fixnum sum = 0;
            for (int i = 0, count = list.Count; i < count; i++) {
                sum += list[i];
            }
            return sum;
        }

        /// <summary>
        /// Returns the largest value of a list of numbers, or no value if the list is empty.
        /// Faster than Mono Linq implementation because it does not involve delegates.
        /// </summary>
        public static Fixnum? MaxFast (this List<Fixnum> list) {
            var index = list.ArgMaxFast();
            return index >= 0 ? list[index] : (Fixnum?) null;
        }

        /// <summary>
        /// Returns the index of the largest value of a list of numbers, or -1 if the list is empty.
        /// </summary>
        public static int ArgMaxFast (this List<Fixnum> list) {
            int index = -1;
            Fixnum max = Fixnum.MIN_VALUE;

            for (int i = 0, count = list.Count; i < count; i++) {
                Fixnum val = list[i];
                if (index == -1 || val > max) {
                    index = i;
                    max = val;
                }
            }

            return index;
        }

        /// <summary>
        /// Calculates the average of a list of numbers. 
        /// </summary>
        public static Fixnum? AverageFast (this List<Fixnum> list) {
            if (list.Count == 0) { return null; }
            return list.SumFast() / list.Count;
        }



        // operations on lists of collections

        /// <summary>
        /// Given a list of collections, clears out each of those collections before clearing the list itself.
        /// </summary>
        public static void ClearDeep<T> (this List<List<T>> list) {
            for (int i = 0, count = list.Count; i < count; i++) {
                list[i].Clear();
            }
            list.Clear();
        }


        /// <summary>
        /// Performs a stable insertion sort (unlike built-in Sort() which is unstable).
        /// </summary>
        public static void StableSort<T> (this List<T> list, Comparison<T> comparison) {
            // i have to write my own goddamn insertion sort because List.Sort() is unstable. WTF?
            for (int j = 1, count = list.Count; j < count; j++) {
                T key = list[j];

                int i = j - 1;
                while (i >= 0 && comparison(list[i], key) > 0) {
                    list[i + 1] = list[i];
                    i--;
                }

                list[i + 1] = key;
            }
        }


        /// <summary>
        /// Returns the first element of the given type, or default value if none were found.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TSubtype FirstOfType<TType, TSubtype> (this List<TType> source)
            where TSubtype : TType {
            foreach (var element in source) {
                if (element is TSubtype subtype) { return subtype; }
            }
            return default;
        }

    }
}
