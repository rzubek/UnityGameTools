// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;

namespace SomaSim.Util
{
    /// <summary>
    /// Object factory pool manages a number of factories, which can be reset and reused.
    /// The canonical example is a StringBuilder, which can be pooled efficiently
    /// instead of generating a new instance every time.
    /// </summary>
    /// <typeparam name="S">Type of objects created by the factories</typeparam>
    /// <typeparam name="T">Type of the factory being pooled</typeparam>
    public class FactoryPool<S, T> where T : class, new()
    {
        /// <summary>
        /// Function that generates instances of IFactoryPoolElement
        /// </summary>
        private Func<T> _creator;

        /// <summary>
        /// Function that calls the factory to produce an item of type S,
        /// and then resets the factory to a reusable state.
        /// </summary>
        private Func<T, S> _finalizer;

        /// <summary>
        /// List of all entries returned from circulation, and ready for reuse.
        /// </summary>
        private Stack<T> _free;

        /// <summary>
        /// Number of elements currently in circulation, that haven't been returned.
        /// </summary>
        private int _usedcount;

        public int UsedListSize { get { return _usedcount; } }
        public int FreeListSize { get { return _free != null ? _free.Count : -1; } }

        /// <summary>
        /// Initializes the pool with a specific creator for new factories,
        /// and a finalizer function that will create an item from the factory,
        /// and then reset the factory.
        /// </summary>
        /// <param name="creator"></param>
        public void Initialize (Func<T> creator, Func<T, S> finalizer) {
            _creator = creator;
            _finalizer = finalizer;
            _free = new Stack<T>();
        }

        /// <summary>
        /// Releases all elements and resources held by this pool.
        /// </summary>
        public void Release () {
            _free.Clear();
            _free = null;
            _finalizer = null;
            _creator = null;
        }

        /// <summary>
        /// Allocates a new factory from the pool, either by pulling from the free list, or 
        /// by calling the generator function to generate a brand new instance.
        /// </summary>
        /// <returns></returns>
        public T Allocate () {
            T element = (_free.Count > 0) ? _free.Pop() : _creator.Invoke();
            _usedcount++;
            return element;
        }

        /// <summary>
        /// Generates a new element, resets the factory and returns it to the pool.
        /// </summary>
        /// <param name="element"></param>
        public S ProduceValueAndFree (T element) {
            S result = _finalizer(element);
            _usedcount--;
            _free.Push(element);
            return result;
        }
    }
}
