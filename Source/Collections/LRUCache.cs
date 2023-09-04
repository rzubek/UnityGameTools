// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SomaSim.Util
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

            public static bool KeyEquals (K a, K b) => EqualityComparer<K>.Default.Equals(a, b);
        }

        /// <summary>
        /// Returns the capacity of the LRU cache. Once count meets capacity, 
        /// adding a new item will cause the oldest (least recently accessed) item to be removed.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Returns the total number of items in the LRU cache. This number will never exceed capacity.
        /// </summary>
        public int Count => _lruList.Count;

        private Dictionary<K, LinkedListNode<Item>> _cacheMap;
        private LinkedList<Item> _lruList;

        /// <summary>
        /// Creates a new LRU cache with specified max capacity. 
        /// </summary>
        public LRUCache (int capacity) {
            if (capacity <= 0) {
                throw new ArgumentOutOfRangeException("capacity", "Cache capacity must be greater than zero");
            }

            this.Capacity = capacity;

            this._cacheMap = new Dictionary<K, LinkedListNode<Item>>();
            this._lruList = new LinkedList<Item>();
        }

        /// <summary>
        /// Returns true if this cache contains a value with the specified key.
        /// Accessing this function does not affect the key's last access time.
        /// </summary>
        public bool ContainsKey (K key) => _cacheMap.ContainsKey(key);

        /// <summary>
        /// Retrieves cached item from the LRU cache. If the key exists in the cache,
        /// updates its last access time and returns the value. Otherwise 
        /// returns a default value for type V. 
        /// </summary>
        public V Get (K key) {
            if (_cacheMap.TryGetValue(key, out LinkedListNode<Item> node)) {
                V value = node.Value.value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
                return value;
            }
            return default;
        }

        /// <summary>
        /// Adds a new item to the cache, potentially evicting the least recently used item
        /// if the cache is at capacity. The new item becomes the most recently used.
        /// Attempting to add a key that already exists in the cache will throw an exception.
        /// </summary>
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
        public bool Remove (K key) {
            if (_cacheMap.TryGetValue(key, out LinkedListNode<Item> node)) {
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


        /// <summary>
        /// Returns true if this cache contains a value with the specified key, 
        /// and that value is at the specified position in the LRU cache where
        /// 0 means oldest (least recently used) and Count - 1 means youngest.
        /// Accessing this function does not affect the key's last access time.
        /// For internal use only, as this operation is O(n) in cache size.
        /// </summary>
        internal bool ContainsKeyAt (K key, int i) {
            if (i < 0 || i >= _lruList.Count) {
                return false;
            }

            if (!_cacheMap.ContainsKey(key)) {
                return false;
            }

            var item = _lruList.ElementAt(i);
            return Item.KeyEquals(item.key, key);
        }
    }
}
