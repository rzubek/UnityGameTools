using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SomaSim.Collections
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
        /// either because 1. this element was just pushed on top of the stack, or 
        /// 2. when this element was right beneath another topmost element, that just got popped.
        /// </summary>
        void OnActivated ();

        /// <summary>
        /// Called when the stack element ceases to be the topmost one. This happens either
        /// as a result of 1. another element being pushed on top of this one, or
        /// 2. right before this element is popped off the stack.
        /// </summary>
        void OnDeactivated ();

        /// <summary>
        /// Called just before this topmost stack element is removed from the stack.
        /// </summary>
        void OnPopped ();
    }

    abstract public class AbstractSmartStackElement : ISmartStackElement
    {
        public object Stack { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsOnStack { get { return Stack != null; } }

        /// <inheritDoc />
        virtual public void OnPushed (object stack) {
            if (!IsOnStack) { this.Stack = stack; } else { throw new InvalidOperationException("Failed to double-push a smart stack element"); }
        }

        /// <inheritDoc />
        virtual public void OnActivated () {
            if (IsOnStack) { this.IsActive = true; } else { throw new InvalidOperationException("Only elements on the stack can be activated"); }
        }

        /// <inheritDoc />
        virtual public void OnDeactivated () {
            if (IsOnStack) { this.IsActive = false; } else { throw new InvalidOperationException("Only elements on the stack can be deactivated"); }
        }

        /// <inheritDoc />
        virtual public void OnPopped () {
            if (IsOnStack) { this.Stack = null; } else { throw new InvalidOperationException("Only elements on the stack can be popped"); }
        }
    }

    [DebuggerDisplay("Count = {Count}")]
    public class SmartStack<T> : IEnumerable<T>, ICollection<T>, IEnumerable
        where T : class, ISmartStackElement
    {
        private Stack<T> _stack;

        public SmartStack () {
            this._stack = new Stack<T>();
        }

        /// <summary>
        /// Returns true if the stack is empty
        /// </summary>
        public bool IsEmpty { get { return _stack.Count == 0; } }

        /// <summary>
        /// Pushes a new element onto the stack and activates it as the topmost one.
        /// It will also deactivate the previous topmost element, if it exists.
        /// </summary>
        /// <param name="element"></param>
        public void Push (T element) {
            if (!IsEmpty) {
                T top = _stack.Peek();
                top.OnDeactivated();
            }

            _stack.Push(element);
            element.OnPushed(this);
            element.OnActivated();
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

            T old = _stack.Peek();
            old.OnDeactivated();
            old.OnPopped();
            _stack.Pop();

            if (!IsEmpty) {
                T top = _stack.Peek();
                top.OnActivated();
            }

            return old;
        }

        /// <summary>
        /// Returns the topmost element on the stack without removing it, or null if empty.
        /// </summary>
        /// <returns></returns>
        public T Peek () {
            return IsEmpty ? null : _stack.Peek();
        }

        /// <summary>
        /// Returns the topmost element on the stack without removing it, or null if empty.
        /// </summary>
        /// <returns></returns>
        public T Top { get { return Peek(); } }

        #region IEnumerable Members

        /// <inheritDoc />
        public IEnumerator<T> GetEnumerator () {
            return _stack.GetEnumerator();
        }

        /// <inheritDoc />
        IEnumerator IEnumerable.GetEnumerator () {
            return _stack.GetEnumerator();
        }

        #endregion

        #region ICollection Members

        /// <inheritDoc />
        public void CopyTo (T[] array, int index) {
            _stack.CopyTo(array, index);
        }

        /// <inheritDoc />
        public int Count {
            get { return _stack.Count; }
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
            Push(item);
        }

        /// <inheritDoc />
        public void Clear () {
            while (Count > 0) {
                Pop();
            }
        }

        /// <inheritDoc />
        public bool Contains (T item) {
            return _stack.Contains(item);
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
