using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Collections
{
    /// <summary>Represents a list collection of objects that can also be accessed as a double-ended queue</summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    [Serializable]
    public class Deque<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        // internally, the deque is stored as a list, where _head points to the first element
        
        private T[] array;
        private int head;
        private int size;
        private int version;

        /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> 
        /// is synchronized (thread safe).</summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of deque, this property always returns false.</returns>
        bool ICollection.IsSynchronized { get { return false; } }

        /// <summary>Gets an object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>An object that can be used to synchronize access to the 
        /// <see cref="T:System.Collections.ICollection" />.  In the default implementation of deque, 
        /// this property always returns the current instance.</returns>
        object ICollection.SyncRoot { get { return this; } }

        /// <summary>Gets the number of elements contained in the deque.</summary>
        /// <returns>The number of elements contained in the deque.</returns>
        public int Count { get { return size; } }

        internal int Capacity { get { return array.Length; } }

        /// <summary>Initializes a new instance of the deque class that is empty and has the default initial capacity.</summary>
        public Deque () {
            array = new T[0];
        }

        /// <summary>Initializes a new instance of the deque class with specified capacity.</summary>
        /// <param name="capacity">The initial capacity of the deque.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="capacity" /> is less than zero.</exception>
        public Deque (int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("count");
            }
            array = new T[capacity];
        }

        /// <summary>Initializes a new instance of the deque class that contains elements copied from the specified collection.</summary>
        /// <param name="collection">The collection whose elements are copied to the new deque.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="collection" /> is null.</exception>
        public Deque (IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            ICollection<T> icoll = collection as ICollection<T>;
            int num = (icoll == null) ? 0 : icoll.Count;
            array = new T[num];
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
            if (target.Length - index < size) { throw new ArgumentException(); }

            if (size == 0) {
                return;
            }

            try {
                int lengthFromHead = array.Length - head;
                Array.Copy(array, head, target, index, System.Math.Min(size, lengthFromHead));
                if (this.size > lengthFromHead) {
                    Array.Copy(this.array, 0, target, index + lengthFromHead, size - lengthFromHead);
                }
            } catch (ArrayTypeMismatchException) {
                throw new ArgumentException();
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator () { return GetEnumerator(); }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator () { return GetEnumerator(); }

        /// <summary>Removes all objects from the deque.</summary>
        /// <filterpriority>1</filterpriority>
        public void Clear () {
            Array.Clear(array, 0, array.Length);
            head = size = 0;
            version++;
        }

        /// <summary>Determines whether an element is in the deque.</summary>
        /// <returns>true if <paramref name="item" /> is found in the deque; otherwise, false.</returns>
        /// <param name="item">The object to locate in the deque. The value can be null for reference types.</param>
        public bool Contains (T item) {
            if (item == null) {
                foreach (T current in this) {
                    if (current == null) { return true; }
                }
            } else {
                foreach (T current in this) {
                    if (item.Equals(current)) { return true; }
                }
            }
            return false;
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
            ((ICollection)this).CopyTo(array, idx);
        }

        /// <summary>Removes and returns the object at the beginning of the deque.</summary>
        /// <returns>The object that is removed from the beginning of the deque.</returns>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T RemoveFirst() { 
            T result = PeekFirst();

            array[head] = default(T); // gc helper

            head = (head + 1) % array.Length;
            size--;
            version++;
            return result;
        }

        /// <summary>Removes and returns the object at the end of the deque.</summary>
        /// <returns>The object that is removed from the end of the deque.</returns>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T RemoveLast () {
            T result = PeekLast();

            int last = (head + size - 1) % array.Length;
            array[last] = default(T); // gc helper

            size--;
            version++;
            return result;
        }

        /// <summary>Returns the object at the beginning of the deque without removing it.</summary>
        /// <returns>The object at the beginning of the deque.</returns>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T PeekFirst () {
            if (size == 0) { throw new InvalidOperationException(); }
            return array[head];
        }

        /// <summary>Returns the object at the end of the deque without removing it.</summary>
        /// <returns>The object at the beginning of the deque.</returns>
        /// <exception cref="T:System.InvalidOperationException">The deque is empty.</exception>
        public T PeekLast () {
            if (size == 0) { throw new InvalidOperationException(); }
            int last = (head + size - 1) % array.Length;
            return array[last];
        }

        /// <summary>Adds an object to the end of the deque.</summary>
        /// <param name="item">The object to add to the deque. The value can be null for reference types.</param>
        public void AddLast (T item) {
            if (size == array.Length) { SetCapacity(System.Math.Max(size * 2, 4)); }

            int next = (head + size) % array.Length;
            array[next] = item;

            size++;
            version++;
        }

        /// <summary>Adds an object to the front of the deque.</summary>
        /// <param name="item">The object to add to the deque. The value can be null for reference types.</param>
        public void AddFirst (T item) {
            if (size == array.Length) { SetCapacity(System.Math.Max(size * 2, 4)); }

            if (--head < 0) {
                head = array.Length - 1;
            }

            array[head] = item;
            size++;
            version++;
        }

        /// <summary>Copies the deque elements to a new array.</summary>
        /// <returns>A new array containing elements copied from the deque.</returns>
        public T[] ToArray () {
            T[] newarray = new T[size];
            CopyTo(newarray, 0);
            return newarray;
        }

        /// <summary>Sets the capacity to the actual number of elements in the deque, if that number is less than 90 percent of current capacity.</summary>
        public void TrimExcess () {
            if (size < array.Length * 0.9) {
                SetCapacity(size);
            }
        }

        private void SetCapacity (int newSize) {
            if (newSize == array.Length) {
                return;
            }

            if (newSize < size) {
                throw new InvalidOperationException("Cannot set capacity below current number of elements");
            }

            T[] newarray = new T[newSize];
            if (size > 0) {
                CopyTo(newarray, 0);
            }

            array = newarray;
            head = 0;
            version++;
        }

        /// <summary>Returns an enumerator that iterates through the deque.</summary>
        /// <returns>An enumerator for the deque.</returns>
        public Deque<T>.Enumerator GetEnumerator () {
            return new Deque<T>.Enumerator(this);
        }


        /// <summary>Enumerates the elements.</summary>
        [Serializable]
        public struct Enumerator : IEnumerator, IDisposable, IEnumerator<T>
        {
            private const int NOT_STARTED = -2;
            private const int FINISHED = -1;

            private Deque<T> d;
            private int index;
            private int version;

            internal Enumerator (Deque<T> deq) {
                index = NOT_STARTED;
                d = deq;
                version = deq.version;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned 
            /// before the first element of the collection or after the last element. </exception>
            object IEnumerator.Current { get { return this.Current; } }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the deque at the current position of the enumerator.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned 
            /// before the first element of the collection or after the last element. </exception>
            public T Current {
                get {
                    if (index < 0) { throw new InvalidOperationException(); }

                    int last = d.head + d.size - 1;
                    int deqindex = (last - index) % d.array.Length;
                    return d.array[deqindex];
                }
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified 
            /// after the enumerator was created. </exception>
            void IEnumerator.Reset () {
                if (version != d.version) { throw new InvalidOperationException(); }
                index = NOT_STARTED;
            }

            /// <summary>Releases all resources used by the enumerator.</summary>
            public void Dispose () {
                index = NOT_STARTED; //? FINISHED?
            }

            /// <summary>Advances the enumerator to the next element of the deque.</summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; 
            /// false if the enumerator has passed the end of the collection.</returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified 
            /// after the enumerator was created. </exception>
            public bool MoveNext () {
                if (version != d.version) { throw new InvalidOperationException(); }

                if (index == NOT_STARTED) {
                    index = d.size;
                }

                return index != FINISHED && --index != FINISHED;
            }
        }

    }
}

