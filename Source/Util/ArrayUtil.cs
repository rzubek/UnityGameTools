// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

namespace SomaSim.Util
{
    /// <summary>
    /// Helpers for arrays
    /// </summary>
    public static class ArrayUtil
    {
        public static T[] CreateFilled<T> (int count) where T : new() {
            var array = new T[count];
            for (int i = 0; i < count; i++) {
                array[i] = new T();
            }
            return array;
        }

        public static T[,] CreateFilled<T> (int width, int height) where T : new() {
            var array = new T[width, height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    array[x, y] = new T();
                }
            }
            return array;
        }
    }
}
