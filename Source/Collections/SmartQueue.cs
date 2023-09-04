// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SomaSim.Util
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
        /// because 1. another head element in front of it just got dequeued, 
        /// 2. this element was enqueued as the only element, or 3. this element
        /// got pushed back onto the head of the queue (in this case the "pushedbach" parameter will be true)
        /// </summary>
        void OnActivated (bool pushedback);

        /// <summary>
        /// Called when the queue element ceases to be the head one. This happens either
        /// as a result of 1. getting dequeued (in which case this function gets called 
        /// before the dequeue one), or 2. another object being pushed back in front of it
        /// (in this case the "pushedbach" parameter will be true).
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
        public bool IsActive { get; private set; }
        public bool IsEnqueued => Queue != null;

        /// <inheritDoc />
        virtual public void OnEnqueued (object queue) {
            if (!IsEnqueued) { this.Queue = queue; } else { throw new InvalidOperationException("Failed to double-enqueue a smart queue element"); }
        }

        /// <inheritDoc />
        virtual public void OnActivated (bool pushedback) {
            if (IsEnqueued) { this.IsActive = true; } else { throw new InvalidOperationException("Only enqueued elements can be activated"); }
        }

        /// <inheritDoc />
        virtual public void OnDeactivated (bool pushedback) {
            if (IsEnqueued) { this.IsActive = false; } else { throw new InvalidOperationException("Only enqueued elements can be deactivated"); }
        }

        /// <inheritDoc />
        virtual public void OnDequeued (bool poppedtail) {
            if (IsEnqueued) { this.Queue = null; } else { throw new InvalidOperationException("Failed to dequeue a smart queue element that wasn't in the queue"); }
        }
    }

    /// <summary>
    /// Implementation of a queue which informs its elements when they're being
    /// added or removed, and when they're at the head of the queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class SmartQueue<T> : IEnumerable<T>, ICollection<T>, IEnumerable
        where T : class, ISmartQueueElement
    {
        private Deque<T> _queue;

        public SmartQueue () {
            this._queue = new Deque<T>();
        }

        public SmartQueue (int capacity) {
            this._queue = new Deque<T>(capacity);
        }

        /// <summary>
        /// Returns true if the queue is empty
        /// </summary>
        public bool IsEmpty => _queue.Count == 0;

        /// <summary>
        /// Adds a new element to the end of the queue. If it's the only element,
        /// it will also activate it.
        /// </summary>
        /// <param name="element"></param>
        public void Enqueue (T element) {
            _queue.AddLast(element);
            element.OnEnqueued(this);

            if (_queue.Count == 1) {
                element.OnActivated(false);
            }
        }

        /// <summary>
        /// Adds a collection of new elements to the end of the queue.
        /// If the queue was previously empty, the first element will be activated 
        /// when it's pushed.
        /// </summary>
        public void Enqueue (List<T> elements) {
            for (int i = 0, count = elements.Count; i < count; i++) {
                Enqueue(elements[i]);
            }
        }

        /// <summary>
        /// Adds a collection of new elements to the end of the queue.
        /// If the queue was previously empty, the first element will be activated 
        /// when it's pushed.
        /// </summary>
        public void Enqueue (T[] elements) {
            for (int i = 0, count = elements.Length; i < count; i++) {
                Enqueue(elements[i]);
            }
        }

        /// <summary>
        /// Adds a new element to the front of the queue, automatically making it
        /// the head of the queue, and deactivating the previous one if it existed.
        /// </summary>
        /// <param name="element"></param>
        public void PushHead (T element) {
            if (!IsEmpty) {
                T head = _queue.PeekFirst();
                head.OnDeactivated(true);
            }

            _queue.AddFirst(element);
            element.OnEnqueued(this);
            element.OnActivated(true);
        }

        /// <summary>
        /// Removes and returns the head element of the queue, or null if the queue is empty. 
        /// Also activates the next element in the queue, if one exists.
        /// </summary>
        /// <returns></returns>
        public T Dequeue () {
            if (IsEmpty) {
                return null;
            }

            T head = _queue.PeekFirst();
            head.OnDeactivated(false);
            head.OnDequeued(false);
            _queue.RemoveFirst();

            if (!IsEmpty) {
                _queue.PeekFirst().OnActivated(false);
            }

            return head;
        }

        /// <summary>
        /// Removes the last element of the queue. If it was not active (ie. not the head element),
        /// it will not be deactivated, just dequeued. Returns null if the queue was empty.
        /// </summary>
        /// <returns></returns>
        public T PopTail () {
            if (IsEmpty) {
                return null;
            }

            T tail = _queue.PeekLast();
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
        public T Head => IsEmpty ? null : _queue.PeekFirst();

        /// <summary>
        /// Returns the tail element of the queue without removing it, or null if empty.
        /// </summary>
        /// <returns></returns>
        public T Tail => IsEmpty ? null : _queue.PeekLast();

        #region IEnumerable Members

        /// <inheritDoc />
        public IEnumerator<T> GetEnumerator () => _queue.GetEnumerator();

        /// <inheritDoc />
        IEnumerator IEnumerable.GetEnumerator () => _queue.GetEnumerator();

        #endregion

        #region ICollection Members

        /// <inheritDoc />
        public void CopyTo (T[] array, int index) => _queue.CopyTo(array, index);

        /// <inheritDoc />
        public int Count => _queue.Count;

        /// <inheritDoc />
        public bool IsSynchronized => false;

        /// <inheritDoc />
        public object SyncRoot => this;

        /// <inheritDoc />
        public void Add (T item) => Enqueue(item);

        /// <inheritDoc />
        public void Clear () {
            while (Count > 0) {
                PopTail();
            }
        }

        /// <inheritDoc />
        public bool Contains (T item) => _queue.Contains(item);

        /// <inheritDoc />
        public bool IsReadOnly => false;

        /// <inheritDoc />
        public bool Remove (T item) => throw new NotImplementedException();

        #endregion
    }
}
