// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;

namespace SomaSim.Util
{
    public static class ListGenerators
    {
        /// <summary>
        /// Appends a sequence of integers in the [min, max) range into the list.
        /// If the range is invalid (min >= max) nothing is inserted.
        /// </summary>
        public static void Iota (List<int> list, int min, int max) {
            for (int i = min; i < max; i++) { list.Add(i); }
        }

        /// <summary>
        /// Creates a new list with a sequence of integers in the [min, max) range.
        /// If the range is invalid (min >= max) an empty list is returned.
        /// </summary>
        public static List<int> ListOfIntegers (int min, int max) {
            var list = new List<int>(max - min);
            Iota(list, min, max);
            return list;
        }

        /// <summary>
        /// Creates a new list with a sequence of integers in the [0, count) range.
        /// </summary>
        public static List<int> ListOfIntegers (int count) {
            var list = new List<int>(count);
            Iota(list, 0, count);
            return list;
        }

        /// <summary>
        /// Creates a new list with a sequence of default values for the given type.
        /// </summary>
        public static List<T> ListOfDefaultValues<T> (int count) => ListOfValues<T>(count, default);

        /// <summary>
        /// Creates a new list with a sequence of copies of the specified value.
        /// </summary>
        public static List<T> ListOfValues<T> (int count, T value) {
            var list = new List<T>(count);
            for (int i = 0; i < count; i++) { list.Add(value); }
            return list;
        }

        /// <summary>
        /// Creates a new list of _count_ objects created by calling _generator_
        /// </summary>
        public static List<T> ListOfNewInstances<T> (int count, Func<T> generator) {
            var list = new List<T>(count);
            for (int i = 0; i < count; i++) { list.Add(generator()); }
            return list;
        }

        /// <summary>
        /// Creates a new list of _count_ objects
        /// </summary>
        public static List<T> ListOfNewInstances<T> (int count) where T : new() {
            var list = new List<T>(count);
            for (int i = 0; i < count; i++) { list.Add(new T()); }
            return list;
        }

        /// <summary>
        /// Given two lists of types S and T, creates a new list witht tuples of type (S, T).
        /// Throws an exception if the two lists are not of identical size.
        /// </summary>
        public static List<(S, T)> Zip<S, T> (List<S> first, List<T> second) {
            if (first == null || second == null || first.Count != second.Count) {
                throw new ArgumentException("Arguments must be of identical size");
            }

            var list = new List<(S, T)>(first.Count);
            for (int i = 0, count = first.Count; i < count; i++) { list.Add((first[i], second[i])); }
            return list;
        }

    }
}
