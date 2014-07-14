using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Utils
{
    /// <summary>
    /// Interface for objects managed by an object pool. 
    /// </summary>
    public interface IObjectPoolElement
    {
        /// <summary>
        /// This function should be implemented by any object pool entries, and it should
        /// release any resources used by this entry, and null out any references
        /// that this instance is holding.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Object pool manages a number of entires, which can be reused instead of 
    /// getting garbage collected.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> where T : class, IObjectPoolElement, new()
    {
        /// <summary>
        /// Function that generates instances of IObjectPoolElement
        /// </summary>
        private Func<T> _factory;

        /// <summary>
        /// List of all entires already in circulation
        /// </summary>
        private List<T> _used;

        /// <summary>
        /// List of all entries returned from circulation, and ready for reuse.
        /// </summary>
        private Stack<T> _free;

        public int UsedListSize { get { return _used.Count; } }
        public int FreeListSize { get { return _free.Count; } }        

        /// <summary>
        /// Initializes the object pool.
        /// </summary>
        public void Initialize ()
        {
            Initialize(() => new T());
        }

        /// <summary>
        /// Initializes the object pool with a specific factory for new elements
        /// (the factory can initialize the elements in a specific way, for example)
        /// </summary>
        /// <param name="factory"></param>
        public void Initialize (Func<T> factory)
        {
            _factory = factory;
            _used = new List<T>();
            _free = new Stack<T>();
        }

        /// <summary>
        /// Releases all elements and resources held by this pool.
        /// </summary>
        public void Release ()
        {
            while (_used.Count > 0)
            {
                Free(_used[0]);
            }

            _used.Clear();
            _used = null;
            _free.Clear();
            _free = null;
            _factory = null;
        }

        /// <summary>
        /// Allocates a new object from the pool, either by pulling from the free list, or 
        /// by calling the factory function to generate a brand new instance.
        /// </summary>
        /// <returns></returns>
        public T Allocate()
        {
            T element = (_free.Count > 0) ? _free.Pop() : _factory.Invoke();
            _used.Add(element);
            return element;
        }

        /// <summary>
        /// Returns the element back to the pool, and resets its data.
        /// </summary>
        /// <param name="element"></param>
        public void Free(T element)
        {
            element.Reset();
            _used.Remove(element);
            _free.Push(element);
        }
    }
}
