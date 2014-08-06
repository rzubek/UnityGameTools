using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Serializer
{
    /// <summary>
    /// Helper class to merge two JSON structures. 
    /// 
    /// When merging a parent with a child, it will produce a new structure such that:
    /// - Each key/value pair from the parent is copied into result 
    /// - Then each key/value pair from the child is copied into result, and in case of a conflict:
    ///   - If child's value is scalar (string, bool, double) or ArrayList, overrides parent's value
    ///   - If child's value is a hashtable, but paren't wasn't, also overrides
    ///   - If both values were hashtables, recursively merge them as well
    /// </summary>
    public class JSONMerger
    {
        public static bool ThrowExceptionOnBadData = false;

        public static object Merge (object parent, object child) {
            if (IsScalar(child)) {
                return child;
            } else if (child is ArrayList) {
                return child;
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
                            result[entry.Key] = JSONMerger.Merge(phash[entry.Key], chash[entry.Key]);
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
            return value is double || value is bool || value is string;
        }
    }
}
