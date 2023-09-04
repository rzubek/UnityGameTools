// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SomaSim.Util
{
    /// <summary>
    /// Implements a priority-based list of hash sets.
    /// An object can be added or removed to any hash set at any priority,
    /// and additionally, the entire collection can be iterated over, which will
    /// visit every element in every set in priority order (but no particular
    /// order within a given set).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityHashSet<T> : IEnumerable<T>
    {
        private struct Entry : IComparable<Entry>, IEquatable<Entry>
        {
            public int priority;
            public HashSet<T> set;

            public static int Compare (Entry a, Entry b) => a.priority - b.priority;
            public int CompareTo (Entry other) => Compare(this, other);

            public bool Equals (Entry other) => this.priority == other.priority && this.set == other.set;
        }

        private List<Entry> _data;
        private int _size;
        private int _version;

        public PriorityHashSet () {
            _data = new List<Entry>();
            _size = 0;
            _version = 0;
        }


        //
        // private accessors


        /// <summary> Private accessor to retrieve the right set for the priority,
        /// creating a new one if needed. </summary>
        private HashSet<T> GetOrAdd (int priority) {
            var existing = GetOrNull(priority);
            if (existing != null) { return existing; }

            // create a new one and set it up
            var entry = new Entry() { priority = priority, set = new HashSet<T>() };
            _data.Add(entry);
            _data.StableSort(Entry.Compare);
            _version++;
            return entry.set;
        }

        /// <summary> Private accessor to retrieve the right set for the priority,
        /// or null if one does not exist. </summary>
        private HashSet<T> GetOrNull (int priority) {
            for (int i = 0, count = _data.Count; i < count; i++) {
                if (_data[i].priority == priority) { return _data[i].set; }
            }
            return null;
        }


        //
        // public api  


        /// <summary>Gets the number of elements contained in the deque.</summary>
        /// <returns>The number of elements contained in the deque.</returns>
        public int Count => _size;

        /// <summary>Removes all objects from the priority deque.</summary>
        public void Clear () {
            foreach (var entry in _data) { entry.set.Clear(); }
            _data.Clear();
            _size = 0;
            _version++;
        }

        /// <summary>Removes the specified object and returns true,
        /// or false if not found.</summary>
        public bool Remove (int priority, T datum) {
            var set = GetOrNull(priority);
            if (set == null) { return false; }

            bool result = set.Remove(datum);
            if (result) {
                _size--;
                _version++;
            }
            return result;
        }

        /// <summary>Adds the specified object and returns true if new,
        /// or false if one was already in the set.</summary>
        public bool Add (int priority, T datum) {
            var set = GetOrAdd(priority);
            bool result = set.Add(datum);
            if (result) {
                _size++;
                _version++;
            }
            return result;
        }

        /// <summary>Removes objects from the data structure, up to but not exceeding the specified count,
        /// and populates the results list with them (by priority, but in unpredictable order within a given priority).
        /// Returns the number of objects actually removed from this structure and added to results. </summary>
        public int MoveToList (int maxCount, List<T> results) {
            if (_size == 0) { return 0; } // sanity check

            int count = 0, todo = maxCount;
            for (int i = 0; i < _data.Count; i++) {
                var set = _data[i].set;
                var moved = MoveFromSetToList(set, todo, results);
                count += moved;
                todo -= moved;
                if (todo <= 0) { break; }
            }

            return count;
        }

        /// <summary>Removes objects from the hash set, up to but not exceeding the specified max count </summary>
        private int MoveFromSetToList (HashSet<T> set, int maxCount, List<T> results) {
            if (set.Count == 0) { return 0; } // sanity check

            int count = 0, start = results.Count;
            IEnumerator<T> en = set.GetEnumerator();

            // copy over
            while (count < maxCount && en.MoveNext()) {
                results.Add(en.Current);
                count++;
            }

            // remove from set
            for (int i = 0; i < count; i++) { set.Remove(results[start + i]); }

            _size -= count;
            _version++;

            return count;
        }

        /// <summary>Sets the capacity to the actual number of elements in the deque</summary>
        public void TrimExcess () {
            for (int i = 0, count = _data.Count; i < count; i++) {
                _data[i].set.TrimExcess();
            }
            _version++;
        }

        /// <summary>Returns an enumerator that iterates through the priority deque.</summary>
        public IEnumerator<T> GetEnumerator () {
            int version = _version;
            foreach (var entry in _data) {
                foreach (T item in entry.set) {
                    if (version != _version) { throw new InvalidOperationException("Collection changed during iteration"); }
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
    }
}

