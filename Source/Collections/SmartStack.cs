// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SomaSim.Util
{
    /// <summary>
    /// Interface for stack elements that get informed when they're pushed or popped,
    /// as well as start or end being the topmost element.
    /// </summary>
    public interface ISmartStackElement
    {
        /// <summary>
        /// Called after this element was pushed onto the stack. It has not yet been
        /// activated as the topmost element, that will happen after this call.
        /// </summary>
        void OnPushed (object stack);

        /// <summary>
        /// Called when the stack element becomes the topmost one. This could happen
        /// either because 1. this element was just pushed on top of the stack 
        /// (in which case the "pushed" parameter will be true), or 2. when this element 
        /// was right beneath another topmost element, that just got popped.
        /// </summary>
        void OnActivated (bool pushed);

        /// <summary>
        /// Called when the stack element ceases to be the topmost one. This happens either
        /// as a result of 1. another element being pushed on top of this one, or
        /// 2. right before this element is popped off the stack (in which case the 
        /// "popped" parameter will be true).
        /// </summary>
        void OnDeactivated (bool popped);

        /// <summary>
        /// Called just before this topmost stack element is removed from the stack.
        /// </summary>
        void OnPopped ();
    }

    abstract public class AbstractSmartStackElement : ISmartStackElement
    {
        public object Stack { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsOnStack => Stack != null;

        /// <inheritDoc />
        virtual public void OnPushed (object stack) {
            if (!IsOnStack) { this.Stack = stack; } else { throw new InvalidOperationException("Failed to double-push a smart stack element"); }
        }

        /// <inheritDoc />
        virtual public void OnActivated (bool pushed) {
            if (IsOnStack) { this.IsActive = true; } else { throw new InvalidOperationException("Only elements on the stack can be activated"); }
        }

        /// <inheritDoc />
        virtual public void OnDeactivated (bool popped) {
            if (IsOnStack) { this.IsActive = false; } else { throw new InvalidOperationException("Only elements on the stack can be deactivated"); }
        }

        /// <inheritDoc />
        virtual public void OnPopped () {
            if (IsOnStack) { this.Stack = null; } else { throw new InvalidOperationException("Only elements on the stack can be popped"); }
        }
    }

    /// <summary>
    /// Implementation of a stack which informs its elements when they've been
    /// added or removed, or reached the top of the stack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("SmartStack, Count = {Count}")]
    public class SmartStack<T> : IEnumerable<T>, ICollection<T>, IEnumerable
        where T : class, ISmartStackElement
    {
        private List<T> _stack;

        public SmartStack () {
            this._stack = new List<T>();
        }

        /// <summary>
        /// Returns true if the stack is empty
        /// </summary>
        public bool IsEmpty => _stack.Count == 0;

        /// <summary>
        /// Pushes a new element onto the stack and activates it as the topmost one.
        /// It will also deactivate the previous topmost element, if it exists.
        /// </summary>
        /// <param name="element"></param>
        public void Push (T element) {
            if (!IsEmpty) {
                T top = Peek();
                top.OnDeactivated(false);
            }

            _stack.Add(element);
            element.OnPushed(this);
            element.OnActivated(true);
        }

        /// <summary>
        /// Removes and return the topmost element on the stack, or null if empty.
        /// Popping an element off the stack will activate the next element under it, if it exists.
        /// </summary>
        /// <returns></returns>
        public T Pop () {
            if (IsEmpty) {
                return null;
            }

            T old = Peek();
            old.OnDeactivated(true);
            old.OnPopped();
            _stack.RemoveAt(_stack.Count - 1);

            if (!IsEmpty) {
                T top = Peek();
                top.OnActivated(false);
            }

            return old;
        }

        /// <summary>
        /// Returns the topmost element on the stack without removing it, or null if empty.
        /// </summary>
        /// <returns></returns>
        public T Peek () => _stack.Count <= 0 ? null : _stack[^1];

        /// <summary>
        /// Returns the topmost element on the stack without removing it, or null if empty.
        /// </summary>
        /// <returns></returns>
        public T Top => Peek();

        #region IEnumerable Members

        /// <inheritDoc />
        public IEnumerator<T> GetEnumerator () => _stack.GetEnumerator();

        /// <inheritDoc />
        IEnumerator IEnumerable.GetEnumerator () => _stack.GetEnumerator();

        #endregion

        #region ICollection Members

        /// <inheritDoc />
        public bool Remove (T item) {
            if (_stack.Count == 0) {
                throw new InvalidOperationException();
            }

            if (item == Peek()) {
                Pop();
                return true;
            }

            // remove from the middle of the stack, which means the item is already deactivated
            int index = _stack.IndexOf(item);
            if (index < 0) {
                return false; // not found
            }

            item.OnPopped();
            _stack.RemoveAt(index);
            return true;
        }

        /// <inheritDoc />
        public void CopyTo (T[] array, int index) => _stack.CopyTo(array, index);

        /// <inheritDoc />
        public int Count => _stack.Count;

        /// <inheritDoc />
        public bool IsSynchronized => false;

        /// <inheritDoc />
        public object SyncRoot => this;

        /// <inheritDoc />
        public void Add (T item) => Push(item);

        /// <inheritDoc />
        public void Clear () {
            while (Count > 0) {
                Pop();
            }
        }

        /// <inheritDoc />
        public bool Contains (T item) => _stack.Contains(item);

        /// <inheritDoc />
        public bool IsReadOnly => false;

        #endregion
    }
}
