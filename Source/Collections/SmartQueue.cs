using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SomaSim.Collections
{
    /// <summary>
    /// Interface for queue elements that get informed when they're added or removed,
    /// as well as start or end being the head element.
    /// </summary>
    public interface ISmartQueueElement
    {
        /// <summary>
        /// Called after this element was enqueued onto the queue. If this is the 
        /// only element, it will be activated as the front element after this call.
        /// </summary>
        void OnEnqueued (object queue);

        /// <summary>
        /// Called when this queue element becomes the head element. This could happen
        /// because 1. another head element in front of it just got dequeued, or
        /// 2. this element was enqueued as the only element.
        /// </summary>
        void OnActivated ();

        /// <summary>
        /// Called when the queue element ceases to be the head one. This happens either
        /// as a result of 1. getting dequeued (in which case this function gets called 
        /// before the dequeue one), or 2. another object being pushed back in front of it.
        /// </summary>
        void OnDeactivated (bool pushedback);

        /// <summary>
        /// Called just before this element of the queue gets dequeued. This happens
        /// either because it's a head element that got dequeued, it because it's the last
        /// that got popped from the tail. 
        /// </summary>
        void OnDequeued (bool poppedtail);
    }

    abstract public class AbstractSmartQueueElement : ISmartQueueElement
    {
        public object Queue { get; private set; }
        public bool IsHead { get; private set; }
        public bool IsEnqueued { get { return Queue != null; } }

        /// <inheritDoc />
        virtual public void OnEnqueued (object queue) {
            this.Queue = queue;
        }

        /// <inheritDoc />
        virtual public void OnActivated () {
            this.IsHead = true;
        }

        /// <inheritDoc />
        virtual public void OnDeactivated (bool pushedback) {
            this.IsHead = false;
        }

        /// <inheritDoc />
        virtual public void OnDequeued (bool poppedtail) {
            this.Queue = null;
        }
    }

    [DebuggerDisplay("Count = {Count}")]
    public class SmartQueue<T> : IEnumerable<T>, ICollection<T>, IEnumerable
        where T : class, ISmartQueueElement
    {
        private LinkedList<T> _queue;

        public SmartQueue () {
            this._queue = new LinkedList<T>();
        }

        /// <summary>
        /// Returns true if the queue is empty
        /// </summary>
        public bool Empty { get { return _queue.Count == 0; } }

        /// <summary>
        /// Adds a new element to the end of the queue. If it's the only element,
        /// it will also activate it.
        /// </summary>
        /// <param name="element"></param>
        public void Enqueue (T element) {
            _queue.AddLast(element);
            element.OnEnqueued(this);

            if (_queue.Count == 1) {
                element.OnActivated();
            }
        }

        /// <summary>
        /// Adds a new element to the front of the queue, automatically making it
        /// the head of the queue, and deactivating the previous one if it existed.
        /// </summary>
        /// <param name="element"></param>
        public void PushHead (T element) {
            if (!Empty) {
                T head = _queue.First.Value;
                head.OnDeactivated(true);
            }

            _queue.AddFirst(element);
            element.OnEnqueued(this);
            element.OnActivated();
        }

        /// <summary>
        /// Removes and returns the head element of the queue, or null if the queue is empty. 
        /// Also activates the next element in the queue, if one exists.
        /// </summary>
        /// <returns></returns>
        public T Dequeue () {
            if (Empty) {
                return null;
            }

            T head = _queue.First.Value;
            head.OnDeactivated(false);
            head.OnDequeued(false);
            _queue.RemoveFirst();

            if (!Empty) {
                _queue.First.Value.OnActivated();
            }

            return head;
        }

        /// <summary>
        /// Removes the last element of the queue. If it was not active (ie. not the head element),
        /// it will not be deactivated, just dequeued. Returns null if the queue was empty.
        /// </summary>
        /// <returns></returns>
        public T PopTail () {
            if (Empty) {
                return null;
            }

            T tail = _queue.Last.Value;
            if (_queue.Count == 1) {
                tail.OnDeactivated(false);
            }

            tail.OnDequeued(true);
            _queue.RemoveLast();

            return tail;
        }

        /// <summary>
        /// Returns the head element on the queue without removing it, or null if empty.
        /// </summary>
        /// <returns></returns>
        public T Head { get { return Empty ? null : _queue.First.Value; } }

        /// <summary>
        /// Returns the tail element of the queue without removing it, or null if empty.
        /// </summary>
        /// <returns></returns>
        public T Tail { get { return Empty ? null : _queue.Last.Value; } }

        #region IEnumerable Members

        /// <inheritDoc />
        public IEnumerator<T> GetEnumerator () {
            return _queue.GetEnumerator();
        }

        /// <inheritDoc />
        IEnumerator IEnumerable.GetEnumerator () {
            return _queue.GetEnumerator();
        }

        #endregion

        #region ICollection Members

        /// <inheritDoc />
        public void CopyTo (T[] array, int index) {
            _queue.CopyTo(array, index);
        }

        /// <inheritDoc />
        public int Count {
            get { return _queue.Count; }
        }

        /// <inheritDoc />
        public bool IsSynchronized {
            get { return false; }
        }

        /// <inheritDoc />
        public object SyncRoot {
            get { return this; }
        }

        /// <inheritDoc />
        public void Add (T item) {
            Enqueue(item);
        }

        /// <inheritDoc />
        public void Clear () {
            while (Count > 0) {
                PopTail();
            }
        }

        /// <inheritDoc />
        public bool Contains (T item) {
            return _queue.Contains(item);
        }

        /// <inheritDoc />
        public bool IsReadOnly {
            get { return false; }
        }

        /// <inheritDoc />
        public bool Remove (T item) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
