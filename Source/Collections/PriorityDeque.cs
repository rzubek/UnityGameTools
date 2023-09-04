// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SomaSim.Util
{
    /// <summary>
    /// Implements a priority-based list of double-ended queues.
    /// An object can be added or removed to any queue at any priority,
    /// and additionally, the entire collection can be iterated over, which will
    /// visit every element in every queue in priority order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityDeque<T> : IEnumerable<T>
    {
        private struct Entry : IComparable<Entry>
        {
            public int priority;
            public Deque<T> queue;

            public static int Compare (Entry a, Entry b) => a.priority - b.priority;
            public int CompareTo (Entry other) => Compare(this, other);
        }

        private List<Entry> _data;
        private int _size;
        private int _version;

        public PriorityDeque () {
            _data = new List<Entry>();
            _size = 0;
            _version = 0;
        }


        //
        // private accessors


        /// <summary> Private accessor to retrieve the right queue for the priority,
        /// creating a new one if needed. </summary>
        private Deque<T> GetOrAddQueue (int priority) {
            for (int i = 0, count = _data.Count; i < count; i++) {
                if (_data[i].priority == priority) {
                    return _data[i].queue;
                }
            }

            // create a new one and set it up
            var entry = new Entry() { priority = priority, queue = new Deque<T>() };
            _data.Add(entry);
            _data.StableSort(Entry.Compare);
            _version++;
            return entry.queue;
        }

        /// <summary> Private accessor to return the first non-empty queue.
        /// Throws an exception if all queues are empty. </summary>
        private Deque<T> GetFirstNonEmptyQueue () {
            for (int i = 0, count = _data.Count; i < count; i++) {
                if (_data[i].queue.Count > 0) {
                    return _data[i].queue;
                }
            }
            throw new InvalidOperationException();
        }

        /// <summary> Private accessor to return the last non-empty queue.
        /// Throws an exception if all queues are empty. </summary>
        private Deque<T> GetLastNonEmptyQueue () {
            for (int i = _data.Count - 1; i >= 0; i--) {
                if (_data[i].queue.Count > 0) {
                    return _data[i].queue;
                }
            }
            throw new InvalidOperationException();
        }


        //
        // public api  


        /// <summary>Gets the number of elements contained in the deque.</summary>
        /// <returns>The number of elements contained in the deque.</returns>
        public int Count => _size;

        /// <summary>Removes all objects from the priority deque.</summary>
        public void Clear () {
            foreach (var entry in _data) { entry.queue.Clear(); }
            _data.Clear();
            _size = 0;
            _version++;
        }

        /// <summary>Determines whether an element is in the priority deque.</summary>
        /// <returns>true if <paramref name="item" /> is found in the deque; otherwise, false.</returns>
        public bool Contains (T item) {
            for (int i = 0, count = _data.Count; i < count; i++) {
                if (_data[i].queue.Contains(item)) { return true; }
            }
            return false;
        }

        /// <summary>Removes and returns the object at the beginning of the priority deque.</summary>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T RemoveFirst () {
            var queue = GetFirstNonEmptyQueue();
            T result = queue.RemoveFirst();
            _size--;
            _version++;
            return result;
        }

        /// <summary>Removes and returns the object at the end of the priority deque.</summary>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T RemoveLast () {
            var queue = GetLastNonEmptyQueue();
            T result = queue.RemoveLast();
            _size--;
            _version++;
            return result;
        }

        /// <summary>Adds an object to the priority deque, at a given priority level
        /// and at the front of that level.</summary>
        /// <param name="item">The object to add to the deque.</param>
        public void AddFirst (int priority, T item) {
            var queue = GetOrAddQueue(priority);
            queue.AddFirst(item);
            _size++;
            _version++;
        }

        /// <summary>Adds an object to the priority deque, at a given priority level
        /// and at the end of that level.</summary>
        /// <param name="item">The object to add to the deque.</param>
        public void AddLast (int priority, T item) {
            var queue = GetOrAddQueue(priority);
            queue.AddLast(item);
            _size++;
            _version++;
        }

        /// <summary>Sets the capacity to the actual number of elements in the deque,
        /// if that number is less than 90 percent of current capacity, and if
        /// current capacity is greater than the specified threshold.</summary>
        public void TrimExcess (int threshold = 4) {
            for (int i = 0, count = _data.Count; i < count; i++) {
                _data[i].queue.TrimExcess(threshold);
            }
            _version++;
        }

        /// <summary>If the object exists in the queue, it swap-removes the first encountered
        /// instance at that priority level (ie. it moves the last element with the same priority
        /// into the object's position, thus overwriting it) and returns true.
        /// If object was not found, returns false.
        /// </summary>
        public bool SwapRemove (T item) {
            for (int i = 0, count = _data.Count; i < count; i++) {
                if (_data[i].queue.SwapRemove(item)) {
                    _size--;
                    _version++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>Returns an enumerator that iterates through the priority deque.</summary>
        public IEnumerator<T> GetEnumerator () {
            int version = _version;
            foreach (var entry in _data) {
                foreach (T item in entry.queue) {
                    if (version != _version) { throw new InvalidOperationException("Collection changed during iteration"); }
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
    }
}

