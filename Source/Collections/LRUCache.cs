using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Collections
{
    /// <summary>
    /// Implementation of a least-recently used cache.
    /// Based on http://stackoverflow.com/questions/754233/is-it-there-any-lru-implementation-of-idictionary
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class LRUCache<K, V>
    {
        private class Item
        {
            public K key;
            public V value;
        }

        /// <summary>
        /// Returns the capacity of the LRU cache. Once count meets capacity, 
        /// adding a new item will cause the oldest (least recently accessed) item to be removed.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Returns the total number of items in the LRU cache. This number will never exceed capacity.
        /// </summary>
        public int Count { get { return _lruList.Count; } }

        private Dictionary<K, LinkedListNode<Item>> _cacheMap;
        private LinkedList<Item> _lruList;

        /// <summary>
        /// Creates a new LRU cache with specified max capacity. 
        /// </summary>
        /// <param name="capacity"></param>
        public LRUCache (int capacity) {
            if (capacity <= 0) throw new ArgumentOutOfRangeException("capacity", "Cache capacity must be greater than zero");

            this.Capacity = capacity;

            this._cacheMap = new Dictionary<K, LinkedListNode<Item>>();
            this._lruList = new LinkedList<Item>();
        }

        /// <summary>
        /// Returns true if this cache contains a value with the specified key.
        /// Accessing this function does not affect the key's last access time.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey (K key) {
            return _cacheMap.ContainsKey(key);
        }

        /// <summary>
        /// Retrieves cached item from the LRU cache. If the key exists in the cache,
        /// updates its last access time and returns the value. Otherwise 
        /// returns a default value for type V. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public V Get (K key) {
            LinkedListNode<Item> node;
            if (_cacheMap.TryGetValue(key, out node)) {
                V value = node.Value.value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
                return value;
            }
            return default(V);
        }

        /// <summary>
        /// Adds a new item to the cache, potentially evicting the least recently used item
        /// if the cache is at capacity. The new item becomes the most recently used.
        /// Attempting to add a key that already exists in the cache will throw an exception.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void Add (K key, V val) {
            Item cacheItem = new Item() { key = key, value = val };
            LinkedListNode<Item> newnode = new LinkedListNode<Item>(cacheItem);
            _cacheMap.Add(key, newnode);
            _lruList.AddLast(newnode);

            if (Count > Capacity) {
                K oldest = _lruList.First.Value.key;
                _lruList.RemoveFirst();
                _cacheMap.Remove(oldest);
            }
        }

        /// <summary>
        /// Removes the item with the specified key from the cache and returns true, 
        /// or returns false if this key was not in the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove (K key) {
            LinkedListNode<Item> node;
            if (_cacheMap.TryGetValue(key, out node)) {
                _lruList.Remove(node);
                return _cacheMap.Remove(key);
            }
            return false;
        }

        /// <summary>
        /// Clears all elements from the cache.
        /// </summary>
        public void Clear () {
            _cacheMap.Clear();
            _lruList.Clear();
        }
    }
}
