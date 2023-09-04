// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;

namespace SomaSim.SION
{
    /// <summary>
    /// Helper class to merge two JSON structures. 
    /// 
    /// When merging a parent with a child, it will produce a new structure such that:
    /// - Each key/value pair from the parent is copied into result 
    /// - Then each key/value pair from the child is copied into result, and in case of a conflict:
    ///   - If child's value is string or primitive (bool, double, etc), overrides parent's value
    ///   - If child's value is a Hashtable or ArrayList, but parent wasn't, overrides
    ///   - If both values are ArrayList, either overrides or appends parent's value, depending on settings
    ///   - If both values are hashtables, recursively merge them
    /// </summary>
    public class HashtableMerger
    {
        public static bool ThrowExceptionOnBadData = false;

        public static object Merge (object parent, object child, bool arrayappend = false) {
            if (IsScalar(child)) {
                return child;

            } else if (child is ArrayList) {
                if (!arrayappend || !(parent is ArrayList)) {
                    return child;
                } else {
                    ArrayList parray = parent as ArrayList;
                    ArrayList carray = child as ArrayList;
                    ArrayList result = new ArrayList(parray.Count + carray.Count);
                    result.AddRange(parray);
                    result.AddRange(carray);
                    return result;
                }

            } else if (child is Hashtable) {
                if (!(parent is Hashtable)) {
                    return child;
                } else {
                    Hashtable chash = child as Hashtable;
                    Hashtable phash = parent as Hashtable;
                    Hashtable result = new Hashtable();

                    foreach (DictionaryEntry entry in phash) {
                        result[entry.Key] = phash[entry.Key];
                    }

                    foreach (DictionaryEntry entry in chash) {
                        if (result.ContainsKey(entry.Key)) {
                            result[entry.Key] = Merge(phash[entry.Key], chash[entry.Key], arrayappend);
                        } else {
                            result[entry.Key] = chash[entry.Key];
                        }
                    }
                    return result;
                }
            } else if (ThrowExceptionOnBadData) {
                throw new Exception("Unknown value type for value: " + child);
            } else {
                return null; // not valid, boo
            }
        }

        private static bool IsScalar (object value) {
            return value.GetType().IsPrimitive || value is string;
        }
    }
}
