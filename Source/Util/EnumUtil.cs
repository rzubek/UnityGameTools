// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SomaSim.Util
{
    /// <summary> Helper class for listing out enums </summary>
    public static class EnumUtil<T> where T : struct, IConvertible
    {
        // check at init time that this type is an enum
        static EnumUtil () {
            if (!typeof(T).IsEnum) {
                throw new InvalidCastException($"{typeof(T).FullName} is not an enum type, in call to {nameof(EnumUtil<T>)}");
            }
        }

        /// <summary>
        /// Returns all enum values as a strongly typed array
        /// </summary>
        public static T[] GetValuesAsArray () => Enum.GetValues(typeof(T)) as T[];

        /// <summary>
        /// Returns all enum values as a strongly typed list
        /// </summary>
        public static List<T> GetValuesAsList () => GetValuesAsArray().ToList();

        /// <summary>
        /// Tries to convert a given int to an enum of given type,
        /// returns it if successful, or null if failed
        /// </summary>
        public static T? TryCastToEnum (object value) => Enum.IsDefined(typeof(T), value) ? (T?) value : null;
    }
}
