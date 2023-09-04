// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SomaSim.Util
{
    /// <summary>Represents a list collection of objects that can also be accessed as a double-ended queue</summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    [Serializable]
    public class Deque<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        // internally, the deque is stored as a list, where _head points to the first element

        private T[] _array;
        private int _head;
        private int _size;
        private int _version;

        /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> 
        /// is synchronized (thread safe).</summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.
        /// In the default implementation of deque, this property always returns false.</returns>
        bool ICollection.IsSynchronized => false;

        /// <summary>Gets an object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>An object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection" />.  In the default implementation of deque, 
        /// this property always returns the current instance.</returns>
        object ICollection.SyncRoot => this;

        /// <summary>Gets the number of elements contained in the deque.</summary>
        /// <returns>The number of elements contained in the deque.</returns>
        public int Count => _size;

        internal int Capacity => _array.Length;

        /// <summary>Initializes a new instance of the deque class that is empty and has the default initial capacity.</summary>
        public Deque () {
            _array = new T[0];
        }

        /// <summary>Initializes a new instance of the deque class with specified capacity.</summary>
        /// <param name="capacity">The initial capacity of the deque.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="capacity" /> is less than zero.</exception>
        public Deque (int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("count");
            }
            _array = new T[capacity];
        }

        /// <summary>Initializes a new instance of the deque class that contains elements copied from the specified collection.</summary>
        /// <param name="collection">The collection whose elements are copied to the new deque.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="collection" /> is null.</exception>
        public Deque (IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            int num = collection is ICollection<T> icoll ? icoll.Count : 0;
            _array = new T[num];
            foreach (T current in collection) {
                AddLast(current);
            }
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> 
        /// to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="target">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="target" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="target" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="target" /> is multidimensional.-or-<paramref name="target" /> does not have zero-based 
        ///   indexing.-or-The number of elements in the source <see cref="T:System.Collections.ICollection" /> is 
        ///   greater than the available space from <paramref name="index" /> to the end of the destination 
        ///   <paramref name="target" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> 
        ///   cannot be cast automatically to the type of the destination <paramref name="target" />.</exception>
        void ICollection.CopyTo (Array target, int index) {
            if (target == null) { throw new ArgumentNullException("array"); }
            if (index > target.Length) { throw new ArgumentOutOfRangeException("index"); }
            if (target.Length - index < _size) { throw new ArgumentException(); }

            if (_size == 0) { return; }

            try {
                int lengthFromHead = _array.Length - _head;
                Array.Copy(_array, _head, target, index, Math.Min(_size, lengthFromHead));
                if (this._size > lengthFromHead) {
                    Array.Copy(this._array, 0, target, index + lengthFromHead, _size - lengthFromHead);
                }
            } catch (ArrayTypeMismatchException) {
                throw new ArgumentException();
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator () => GetEnumerator();

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();

        /// <summary>Removes all objects from the deque.</summary>
        /// <filterpriority>1</filterpriority>
        public void Clear () {
            Array.Clear(_array, 0, _array.Length);
            _head = _size = 0;
            _version++;
        }

        /// <summary>Determines whether an element is in the deque.</summary>
        /// <returns>true if <paramref name="item" /> is found in the deque; otherwise, false.</returns>
        /// <param name="item">The object to locate in the deque. The value can be null for reference types.</param>
        public bool Contains (T item) {
            int index = IndexOf(item);
            return index >= 0;
        }

        /// <summary>Returns the internal array index of the item in question </summary>
        private int IndexOf (T item) {
            var length = _array.Length;
            EqualityComparer<T> @default = EqualityComparer<T>.Default;

            for (int i = 0; i < _size; i++) {
                int index = (_head + i) % length;
                T current = _array[index];
                if (@default.Equals(current, item)) {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>Copies the deque elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from deque. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="arrayIndex" /> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source deque is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo (T[] array, int idx) {
            if (array == null) { throw new ArgumentNullException(); }
            ((ICollection) this).CopyTo(array, idx);
        }

        /// <summary>Removes and returns the object at the beginning of the deque.</summary>
        /// <returns>The object that is removed from the beginning of the deque.</returns>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T RemoveFirst () {
            T result = PeekFirst();

            _array[_head] = default; // gc helper

            _head = (_head + 1) % _array.Length;
            _size--;
            _version++;
            return result;
        }

        /// <summary> Returns the index of the last item in the array (wrapped around) </summary>
        private int Last => (_head + _size - 1) % _array.Length;

        /// <summary>Removes and returns the object at the end of the deque.</summary>
        /// <returns>The object that is removed from the end of the deque.</returns>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T RemoveLast () {
            T result = PeekLast();
            _array[Last] = default; // gc helper

            _size--;
            _version++;
            return result;
        }

        /// <summary>If the object exists in the queue, it swap-removes the first encountered
        /// instance (ie. it moves the last element into the object's position, thus overwriting it)
        /// and returns true. If object was not found, returns false.
        /// </summary>
        public bool SwapRemove (T item) {
            int index = IndexOf(item);
            if (index < 0) {
                return false;
            }

            // drop the last item into this slot
            _array[index] = _array[Last];
            _array[Last] = default; // gc helper

            _size--;
            _version++;
            return true;
        }

        /// <summary>Returns the object at the beginning of the deque without removing it.</summary>
        /// <returns>The object at the beginning of the deque.</returns>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T PeekFirst () {
            if (_size == 0) { throw new InvalidOperationException(); }
            return _array[_head];
        }

        /// <summary>Returns the object at the end of the deque without removing it.</summary>
        /// <returns>The object at the beginning of the deque.</returns>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T PeekLast () {
            if (_size == 0) { throw new InvalidOperationException(); }
            return _array[Last];
        }

        /// <summary>Adds an object to the end of the deque.</summary>
        /// <param name="item">The object to add to the deque.</param>
        public void AddLast (T item) {
            if (_size == _array.Length) { SetCapacity(Math.Max(_size * 2, 4)); }

            int next = (_head + _size) % _array.Length;
            _array[next] = item;

            _size++;
            _version++;
        }

        /// <summary>Adds an object to the front of the deque.</summary>
        /// <param name="item">The object to add to the deque.</param>
        public void AddFirst (T item) {
            if (_size == _array.Length) { SetCapacity(Math.Max(_size * 2, 4)); }

            if (--_head < 0) {
                _head = _array.Length - 1;
            }

            _array[_head] = item;
            _size++;
            _version++;
        }

        /// <summary>Copies the deque elements to a new array.</summary>
        /// <returns>A new array containing elements copied from the deque.</returns>
        public T[] ToArray () {
            T[] newarray = new T[_size];
            CopyTo(newarray, 0);
            return newarray;
        }

        /// <summary>Sets the capacity to the actual number of elements in the deque,
        /// if that number is less than 90 percent of current capacity, and if
        /// current capacity is greater than the specified threshold.</summary>
        public void TrimExcess (int threshold = 4) {
            if (_size < _array.Length * 0.9 && _array.Length > threshold) {
                SetCapacity(_size);
            }
        }

        private void SetCapacity (int newSize) {
            if (newSize == _array.Length) {
                return;
            }

            if (newSize < _size) {
                throw new InvalidOperationException("Cannot set capacity below current number of elements");
            }

            T[] newarray = new T[newSize];
            if (_size > 0) {
                CopyTo(newarray, 0);
            }

            _array = newarray;
            _head = 0;
            _version++;
        }

        /// <summary>Returns an enumerator that iterates through the deque.</summary>
        /// <returns>An enumerator for the deque.</returns>
        public Enumerator GetEnumerator () => new Enumerator(this);


        /// <summary>Enumerates the elements.</summary>
        [Serializable]
        public struct Enumerator : IEnumerator, IDisposable, IEnumerator<T>
        {
            private const int NOT_STARTED = -2;
            private const int FINISHED = -1;

            private Deque<T> _d;
            private int _index;
            private int _version;

            internal Enumerator (Deque<T> deq) {
                _index = NOT_STARTED;
                _d = deq;
                _version = deq._version;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned 
            /// before the first element of the collection or after the last element. </exception>
            object IEnumerator.Current => Current;

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the deque at the current position of the enumerator.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned 
            /// before the first element of the collection or after the last element. </exception>
            public T Current {
                get {
                    if (_index < 0) { throw new InvalidOperationException(); }

                    int last = _d._head + _d._size - 1;
                    int deqindex = (last - _index) % _d._array.Length;
                    return _d._array[deqindex];
                }
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified 
            /// after the enumerator was created. </exception>
            void IEnumerator.Reset () {
                if (_version != _d._version) { throw new InvalidOperationException(); }
                _index = NOT_STARTED;
            }

            /// <summary>Releases all resources used by the enumerator.</summary>
            public void Dispose () => _index = NOT_STARTED; //? FINISHED?

            /// <summary>Advances the enumerator to the next element of the deque.</summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; 
            /// false if the enumerator has passed the end of the collection.</returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified 
            /// after the enumerator was created. </exception>
            public bool MoveNext () {
                if (_version != _d._version) { throw new InvalidOperationException(); }

                if (_index == NOT_STARTED) {
                    _index = _d._size;
                }

                return _index != FINISHED && --_index != FINISHED;
            }
        }

    }
}

