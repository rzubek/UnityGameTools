// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;

namespace SomaSim.Util
{
    public static class DictionaryExtensions
    {
        /// <summary> Adds all elements from the IEnumerable into the dictionary </summary>
        public static void AddRange<K, V> (this Dictionary<K, V> set, IEnumerable<(K, V)> entries) {
            foreach (var (k, v) in entries) { set.Add(k, v); }
        }

        /// <summary> Adds all keys from the IEnumerable, and their converted values, into the dictionary </summary>
        public static void AddRange<K, V> (this Dictionary<K, V> set, IEnumerable<K> keys, Func<K, V> fn) {
            foreach (var key in keys) { set.Add(key, fn(key)); }
        }

        /// <summary>
        /// Returns true if the dictionary contains at least one entry that satisfied the provided predicate.
        /// </summary>
        public static bool Contains<K, V> (this Dictionary<K, V> dict, Predicate<KeyValuePair<K, V>> pred) {
            foreach (var entry in dict) { if (pred(entry)) { return true; } }
            return false;
        }

        /// <summary>
        /// Returns the value for the specified key, or null if not found, and logs a warning
        /// </summary>
        public static V FindOrNullAndWarn<K, V> (this Dictionary<K, V> dict, K key, string err) where V : class {
            if (dict.TryGetValue(key, out V result)) { return result; }
            Logger.Warning(key, ":", err);
            return null;
        }

        /// <summary>
        /// Returns the value for the specified key, or null if not found
        /// </summary>
        public static V FindOrNull<K, V> (this Dictionary<K, V> dict, K key) where V : class
            => dict.TryGetValue(key, out V result) ? result : null;

        /// <summary>
        /// Returns the value for the specified key, or null if not found
        /// </summary>
        public static V? FindOrNullable<K, V> (this Dictionary<K, V> dict, K key) where V : struct
            => dict.TryGetValue(key, out V result) ? result : (V?) null;

        /// <summary>
        /// Returns the value for the specified key, or null if not found, and logs a warning
        /// </summary>
        public static V FindOrDefaultAndWarn<K, V> (this Dictionary<K, V> dict, K key, string err) where V : struct {
            if (dict.TryGetValue(key, out V result)) { return result; }
            Logger.Warning(key, ":", err);
            return default;
        }

        /// <summary>
        /// Returns the value for the specified key, or the specified default value if not found
        /// </summary>
        public static V FindOrDefault<K, V> (this Dictionary<K, V> dict, K key, V defaultValue)
            => dict.TryGetValue(key, out V result) ? result : defaultValue;

        /// <summary>
        /// Returns the value for the specified key, or the default type value if not found
        /// </summary>
        public static V FindOrDefault<K, V> (this Dictionary<K, V> dict, K key)
            => dict.TryGetValue(key, out V result) ? result : default;

        /// <summary>
        /// Returns the value for the specified key, or if not found, adds the default value to the dictionary and returns it
        /// </summary>
        public static V FindOrAddDefault<K, V> (this Dictionary<K, V> dict, K key) where V : struct {
            if (!dict.TryGetValue(key, out V result)) {
                result = dict[key] = default;
            }
            return result;
        }

        /// <summary>
        /// Returns the value for the specified key, or if not found, adds the default value to the dictionary and returns it
        /// </summary>
        public static V FindOrAddNew<K, V> (this Dictionary<K, V> dict, K key) where V : class, new() {
            if (!dict.TryGetValue(key, out V result)) {
                result = dict[key] = new V();
            }
            return result;
        }

        /// <summary>
        /// Returns the value for the specified key, or if not found, adds the default value to the dictionary and returns it
        /// </summary>
        public static V FindOrAddNew<K, V> (this Dictionary<K, V> dict, K key, Func<V> fn) where V : class {
            if (!dict.TryGetValue(key, out V result)) {
                result = dict[key] = fn();
            }
            return result;
        }

        /// <summary> Find the returns the given key value, also removing it from the dictionary.
        /// Throws an exception if the item doesn't exits. </summary>
        public static V FindAndRemove<K, V> (this Dictionary<K, V> dict, K key) {
            var val = dict[key];
            dict.Remove(key);
            return val;
        }

        /// <summary> Find the returns the given key value, also removing it from the dictionary.
        /// Returns null if it doesn't exist. </summary>
        public static V? FindAndRemoveOrNullable<K, V> (this Dictionary<K, V> dict, K key) where V : struct {
            if (dict.TryGetValue(key, out V result)) {
                dict.Remove(key);
                return result;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Given a dictionary of collections, clears out the collections first, and then the dictionary.
        /// </summary>
        public static void ClearDeep<K, V> (this Dictionary<K, ICollection<V>> dict) {
            foreach (var entry in dict) { entry.Value.Clear(); }
            dict.Clear();
        }

        /// <summary>
        /// Given a dictionary of collections, clears out the collections first, and then the dictionary.
        /// </summary>
        public static void ClearDeep<K1, K2, V> (this Dictionary<K1, Dictionary<K2, V>> dict) {
            foreach (var entry in dict) { entry.Value.Clear(); }
            dict.Clear();
        }

        /// <summary>
        /// Given a dictionary of collections, clears out the collections first, and then the dictionary.
        /// </summary>
        public static void ClearDeep<K, V> (this Dictionary<K, List<V>> dict) {
            foreach (var entry in dict) { entry.Value.Clear(); }
            dict.Clear();
        }

        /// <summary>
        /// Given a dictionary of queues, clears out the queues first, and then the dictionary.
        /// </summary>
        public static void ClearDeep<K, V> (this Dictionary<K, Queue<V>> dict) {
            foreach (var entry in dict) { entry.Value.Clear(); }
            dict.Clear();
        }

        /// <summary>
        /// Given a dictionary of hash sets, clears out the queues first, and then the dictionary.
        /// </summary>
        public static void ClearDeep<K, V> (this Dictionary<K, HashSet<V>> dict) {
            foreach (var entry in dict) { entry.Value.Clear(); }
            dict.Clear();
        }



        //
        // helpers for dictionaries of dictionaries

        /// <summary>
        /// Given a dictionary of dictionaries, returns an inner dictionary for a given key,
        /// and if it doesn't exist, creates a new one, adds it, and returns
        /// </summary>
        public static Dictionary<K2, V> FindOrMakeSubDictionary<K, K2, V> (this Dictionary<K, Dictionary<K2, V>> dict, K key) {
            if (!dict.TryGetValue(key, out Dictionary<K2, V> sub)) {
                sub = dict[key] = new Dictionary<K2, V>();
            }
            return sub;
        }

        /// <summary>
        /// Given a dictionary of dictionaries, returns the value specified by the two keys, or null if it doesn't exist
        /// </summary>
        public static V FindOrNull<K, K2, V> (this Dictionary<K, Dictionary<K2, V>> dict, K key, K2 subkey) where V : class {
            if (!dict.TryGetValue(key, out Dictionary<K2, V> sub)) { return null; }
            if (!sub.TryGetValue(subkey, out V result)) { return null; }
            return result;
        }

        /// <summary>
        /// Given a dictionary of dictionaries, adds the specified value, creating the subdictionary if needed
        /// </summary>
        public static void Add<K, K2, V> (this Dictionary<K, Dictionary<K2, V>> dict, K key, K2 subkey, V value) where V : class {
            if (!dict.TryGetValue(key, out Dictionary<K2, V> sub)) {
                sub = dict[key] = new Dictionary<K2, V>();
            }
            sub.Add(subkey, value);
        }

        /// <summary>
        /// Given a dictionary of dictionaries, returns true if the specified double-keyed entry exists
        /// </summary>
        public static bool ContainsKeys<K, K2, V> (this Dictionary<K, Dictionary<K2, V>> dict, K key, K2 subkey) where V : class {
            if (!dict.TryGetValue(key, out Dictionary<K2, V> sub)) { return false; }
            return sub.ContainsKey(subkey);
        }

        /// <summary>
        /// Given a dictionary of dictionaries of lists, adds the specified value, creating the subdictionary if needed
        /// </summary>
        public static void Add<K, K2, V> (this Dictionary<K, Dictionary<K2, List<V>>> dict, K key, K2 subkey, V value) {
            if (!dict.TryGetValue(key, out Dictionary<K2, List<V>> subdict)) {
                subdict = dict[key] = new Dictionary<K2, List<V>>();
            }
            if (!subdict.TryGetValue(subkey, out List<V> list)) {
                list = subdict[subkey] = new List<V>();
            }
            list.Add(value);
        }

        //
        // helpers when dealing with dictionaries whose values are lists

        /// <summary>
        /// Given a dictionary of lists, finds the right list for the key and adds the value to it,
        /// and if the list doesn't exist, creates a new empty one first.
        /// </summary>
        public static void AddToList<K, V> (this Dictionary<K, List<V>> dict, K key, V value) {
            bool exists = dict.TryGetValue(key, out List<V> list);
            if (!exists) {
                list = new List<V>();
                dict.Add(key, list);
            }

            list.Add(value);
        }

        /// <summary>
        /// Given a dictionary of lists, finds the keyed list and removes the value from it.
        /// If the resulting list is empty, it may then be removed from the dictionary depending on the remove parameter.
        /// Returns true when the list was found in the dictionary, and the value was found and removed from the list; false otherwise.
        /// </summary>
        public static bool RemoveFromList<K, V> (this Dictionary<K, List<V>> dict, K key, V value, bool removeEmptyList) {
            bool result = false;
            bool exists = dict.TryGetValue(key, out List<V> list);
            if (exists) {
                result = list.Remove(value);
                if (list.Count == 0 && removeEmptyList) {
                    dict.Remove(key);
                }
            }
            return result;
        }

        /// <summary>
        /// Given a dictionary of lists, finds the list specified by key, and if it's missing, it adds an empty one first.
        /// </summary>
        public static List<V> FindOrMakeList<K, V> (this Dictionary<K, List<V>> dict, K key) {
            bool exists = dict.TryGetValue(key, out List<V> list);
            if (!exists) {
                list = new List<V>();
                dict.Add(key, list);
            }
            return list;
        }


        //
        // helpers when dealing with dictionaries whose values are hash sets

        /// <summary>
        /// Given a dictionary of hash sets, finds the right set for the key and adds the value to it,
        /// and if the set doesn't exist, creates a new empty one first.
        /// </summary>
        public static void AddToSet<K, V> (this Dictionary<K, HashSet<V>> dict, K key, V value) {
            bool exists = dict.TryGetValue(key, out HashSet<V> set);
            if (!exists) {
                set = new HashSet<V>();
                dict.Add(key, set);
            }

            set.Add(value);
        }

        /// <summary>
        /// Given a dictionary of hash sets, finds the keyed set and removes the value from it.
        /// If the resulting set is empty, it may then be removed from the dictionary depending on the remove parameter.
        /// Returns true when the set was found in the dictionary, and the value was found and removed from the set; false otherwise.
        /// </summary>
        public static bool RemoveFromSet<K, V> (this Dictionary<K, HashSet<V>> dict, K key, V value, bool removeEmptySet) {
            bool result = false;
            bool exists = dict.TryGetValue(key, out HashSet<V> set);
            if (exists) {
                result = set.Remove(value);
                if (set.Count == 0 && removeEmptySet) {
                    dict.Remove(key);
                }
            }
            return result;
        }


        //
        // helpers when dealing with dictionaries whose values are queues

        /// <summary>
        /// Given a dictionary of queues, finds the keyed queue and returns the peeked head value, 
        /// or null if the queue is empty or doesn't exist.
        /// </summary>
        public static V PeekOrNull<K, V> (this Dictionary<K, Queue<V>> dict, K key) where V : class {
            bool exists = dict.TryGetValue(key, out Queue<V> q);
            return exists && q.Count > 0 ? q.Peek() : null;
        }

        /// <summary>
        /// Given a dictionary of queues, finds the keyed queue and returns the peeked head value, 
        /// or default type value if the queue is empty or doesn't exist.
        /// </summary>
        public static V PeekOrDefault<K, V> (this Dictionary<K, Queue<V>> dict, K key) where V : struct {
            bool exists = dict.TryGetValue(key, out Queue<V> q);
            return exists && q.Count > 0 ? q.Peek() : default;
        }

        /// <summary>
        /// Given a dictionary of queues, finds the keyed queue and enqueues the value on it,
        /// and if the queue doesn't exist it creates an empty one first.
        /// </summary>
        public static void Enqueue<K, V> (this Dictionary<K, Queue<V>> dict, K key, V value) {
            bool exists = dict.TryGetValue(key, out Queue<V> q);
            if (!exists) {
                q = new Queue<V>();
                dict.Add(key, q);
            }

            q.Enqueue(value);
        }

        /// <summary>
        /// Given a dictionary of queues, finds the keyed queue and dequeues and returns the head value if one exists.
        /// If the queue is empty, it may be then removed from the dictionary depending on the remove parameter.
        /// Returns the value dequeued, or the default value for this type (or null for reference types) otherwise.
        /// </summary>
        public static V Dequeue<K, V> (this Dictionary<K, Queue<V>> dict, K key, bool removeEmptyQueue) {
            V result = default;
            if (dict.TryGetValue(key, out Queue<V> q)) {
                if (q.Count > 0) {
                    result = q.Dequeue();
                }
                if (q.Count == 0 && removeEmptyQueue) {
                    dict.Remove(key);
                }
            }
            return result;
        }



        //
        // helpers when dealing with dictionaries mapping to ints

        /// <summary>
        /// Given a dictionary whose value are ints, this operation finds the keyed value and increments it by delta.
        /// If the key was not found, it may be first added with the value of zero, if the skip parameter is false.
        /// </summary>
        public static void Increment<K> (this Dictionary<K, int> dict, K key, int delta, bool skipMissingKey = false) {
            bool found = dict.TryGetValue(key, out int current);
            bool replace = found || !skipMissingKey;
            if (replace) {
                dict[key] = current + delta;
            }
        }

        /// <summary>
        /// Given a dictionary whose value are ints, this operation finds the keyed value and decrements it by delta.
        /// If the key was not found, it may be first added with the value of zero, if the skip parameter is false.
        /// </summary>
        public static void Decrement<K> (this Dictionary<K, int> dict, K key, int delta, bool skipMissingKey)
            => dict.Increment(key, -delta, skipMissingKey);


        //
        // helpers when dealing with dictionaries mapping to floats

        /// <summary>
        /// Given a dictionary whose value are floats, this operation finds the keyed value and increments it by delta.
        /// If the key was not found, it may be first added with the value of zero, if the skip parameter is false.
        /// </summary>
        public static void Increment<K> (this Dictionary<K, float> dict, K key, float delta, bool skipMissingKey = false) {
            bool found = dict.TryGetValue(key, out float current);
            bool replace = found || !skipMissingKey;
            if (replace) {
                dict[key] = current + delta;
            }
        }

        /// <summary>
        /// Given a dictionary whose value are floats, this operation finds the keyed value and decrements it by delta.
        /// If the key was not found, it may be first added with the value of zero, if the skip parameter is false.
        /// </summary>
        public static void Decrement<K> (this Dictionary<K, float> dict, K key, float delta, bool skipMissingKey)
            => dict.Increment(key, -delta, skipMissingKey);

        //
        // helpers when dealing with dictionaries mapping to Fixnums

        /// <summary>
        /// Given a dictionary whose value are Fixnum, this operation finds the keyed value and increments it by delta.
        /// If the key was not found, it may be first added with the value of zero, if the skip parameter is false.
        /// </summary>
        public static void Increment<K> (this Dictionary<K, Fixnum> dict, K key, Fixnum delta, bool skipMissingKey = false) {
            bool found = dict.TryGetValue(key, out Fixnum current);
            bool replace = found || !skipMissingKey;
            if (replace) {
                dict[key] = current + delta;
            }
        }


        /// <summary>
        /// Given a dictionary whose value are Fixnum, this operation finds the keyed value and decrements it by delta.
        /// If the key was not found, it may be first added with the value of zero, if the skip parameter is false.
        /// </summary>
        public static void Decrement<K> (this Dictionary<K, Fixnum> dict, K key, Fixnum delta, bool skipMissingKey)
            => dict.Increment(key, -delta, skipMissingKey);

        public static void AddToIntRange<K> (this Dictionary<K, IntRange> dict, K key, IntRange range, bool skipMissingKey = false) {
            bool found = dict.TryGetValue(key, out IntRange current);
            bool replace = found || !skipMissingKey;
            if (replace) {
                dict[key] = new IntRange() { from = current.from + range.from, to = current.to + range.to };
            }
        }

        /// <summary>
        /// Performs a shallow copy of all keys and values from this source dictionary to the target.
        /// If any given key already exists in the target, its value will be overwritten.
        /// </summary>
        public static void ConcatInto<K, V> (this Dictionary<K, V> source, Dictionary<K, V> target) {
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext()) { // no foreach so that we don't generate extra garbage 
                var entry = enumerator.Current;
                target[entry.Key] = entry.Value;
            }
        }

    }
}
