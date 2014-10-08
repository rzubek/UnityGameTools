using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Game.Util
{
    /// <summary>
    /// Additional math helper functions beyond what Unity provides.
    /// </summary>
    public class MathUtil
    {
        /// <summary>
        /// Snaps the given value to the nearest lower multiple of the step value.
        /// For example, given step of 5, values in [0, 5) will snap to 0; 
        /// values in [5, 10) will snap to 5, values in [-5, 0) will snap to -5, etc.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static float Quantize (float value, float step) {
            return Mathf.Floor(value / step) * step;
        }

        /// <summary>
        /// Snaps the given value to the nearest lower multiple of the step value.
        /// For example, given step of 5, integer values in [0, 4] will snap to 0; 
        /// values in [5, 9] will snap to 5, values in [-5, -1] will snap to -5, etc.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static int Quantize (int value, int step) {
            return Mathf.FloorToInt((float)value / step) * step;
        }

        /// <summary>
        /// Clamps the value to be within the range [min, max]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp (float value, float min = 0f, float max = 1f) {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        /// <summary>
        /// Clamps the value to be within the range [min, max]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Clamp (int value, int min, int max) {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        /// <summary>
        /// Interpolates between two values. If p = 0, returns a,
        /// if p = 1, returns b, otherwise returns lerp of both.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Interpolate (float p, float a, float b) {
            return (1 - p) * a + p * b;
        }

        /// <summary>
        /// Finds position of q between two values a and b - an inverse of interpolation. 
        /// If q = a returns 0, if q = b, returns 1, otherwise returns a value proportional to 
		/// q's position between them. Throws a division by zero error if a = b.
        /// </summary>
        /// <param name="q"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Uninterpolate (float q, float a, float b) {
            return (q - a) / (b - a);
        }

        /// <summary>
        /// Modulus function, in in C# the % operator is not mod, it's actually a remainder, and can be negative.
        /// For example, Modulus(1, 3) == 1 and 1 % 3 == 1, but Modulus(-1, 3) == 2, while -1 % 3 == -1.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Modulus (float a, float b) {
            float r = a % b;
            return (r < 0) ? r + b : r;
        }

        /// <summary>
        /// Modulus function, in in C# the % operator is not mod, it's actually a remainder, and can be negative.
        /// For example, Modulus(1, 3) == 1 and 1 % 3 == 1, but Modulus(-1, 3) == 2, while -1 % 3 == -1.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Modulus (int a, int b) {
            int r = a % b;
            return (r < 0) ? r + b : r;
        }

        public static bool IsEven (int value) { return (value & 1) == 0; }
        public static bool IsEven (uint value) { return (value & 1) == 0; }
        public static bool IsEven (float value) { return ((int)value & 1) == 0; }

        public static bool IsOdd (int value) { return (value & 1) == 1; }
        public static bool IsOdd (uint value) { return (value & 1) == 1; }
        public static bool IsOdd (float value) { return ((int)value & 1) == 1; }
    }
}
