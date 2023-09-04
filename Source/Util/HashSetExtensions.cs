// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System.Collections.Generic;

namespace SomaSim.Util
{
    public static class HashSetExtensions
    {
        /// <summary> Adds all elements from the IEnumerable into the hash set </summary>
        public static void AddRange<T> (this HashSet<T> set, IEnumerable<T> values) {
            foreach (var v in values) { set.Add(v); }
        }
    }
}
