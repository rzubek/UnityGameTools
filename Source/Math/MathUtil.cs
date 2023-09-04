// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using UnityEngine;

namespace SomaSim.Util
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
        public static float SnapDown (float value, float step) =>
            Mathf.Floor(value / step) * step;

        /// <summary>
        /// Snaps the given value to the nearest lower multiple of the step value.
        /// For example, given step of 5, integer values in [0, 4] will snap to 0; 
        /// values in [5, 9] will snap to 5, values in [-5, -1] will snap to -5, etc.
        /// </summary>
        public static int SnapDown (int value, int step) =>
            Mathf.FloorToInt((float) value / step) * step;

        /// <summary>
        /// Snaps the given value to the nearest higher multiple of the step value.
        /// For example, given step of 5, values in (0, 5] will snap to 5; 
        /// values in (5, 10] will snap to 10, values in (-5, 0] will snap to 0, etc.
        /// </summary>
        public static float SnapUp (float value, float step) =>
            Mathf.Ceil(value / step) * step;

        /// <summary>
        /// Snaps the given value to the nearest lower multiple of the step value.
        /// For example, given step of 5, integer values in [1, 5] will snap to 5; 
        /// values in [6, 10] will snap to 10, values in [-4, 0] will snap to 0, etc.
        /// </summary>
        public static int SnapUp (int value, int step) =>
            Mathf.CeilToInt((float) value / step) * step;

        /// <summary>
        /// Snaps the given value to the nearest higher multiple of the step value.
        /// For example, given step of 5, values in [2.5, 7.5) will snap to 5; 
        /// values in [7.5, 12.5) will snap to 10, etc.
        /// </summary>
        public static float SnapTo (float value, float step) =>
            Mathf.Round(value / step) * step;

        /// <summary>
        /// Snaps the given value to the nearest multiple of the step value.
        /// For example, given step of 5, integer values in [1, 2] will snap to 0; 
        /// values in [3, 7] will snap to 5, etc.
        /// </summary>
        public static int SnapTo (int value, int step) =>
            Mathf.RoundToInt((float) value / step) * step;

        /// <summary>
        /// Snaps the given value to the nearest multiple of the step value unevenly.
        /// The argument pivot determines the max epsilon over a step that will be rounded down.
        /// Anything more than that will be rounded up.
        /// 
        /// For example, given the step of 5 and pivot of 2 integer values in [0, 2) will snap to 0;
        /// values in [2, 5] will snap to 5, etc.
        /// </summary>
        public static int SnapUneven (int value, int step, int pivot) {
            var intermediate = (float) value / step;
            var downFrac = (float) pivot / value;
            if (Modulus(intermediate, 1) < downFrac) {
                return Mathf.FloorToInt(intermediate) * step;
            } else {
                return Mathf.CeilToInt(intermediate) * step;
            }
        }
        /// <summary>
        /// Snaps the given value to the nearest multiple of the step value unevenly.
        /// The argument pivot determines the max epsilon over a step that will be rounded down.
        /// Anything more than that will be rounded up.
        /// 
        /// For example, given the step of 2.5 and pivot of 0.5 values in [0, 0.5) will snap to 0;
        /// values in [0.5, 2.5] will snap to 2.5, etc.
        /// </summary>
        public static float SnapUneven (float value, float step, float pivot) {
            var intermediate = value / step;
            var downFrac = pivot / value;
            if (Modulus(intermediate, 1) < downFrac) {
                return Mathf.Floor(intermediate) * step;
            } else {
                return Mathf.Ceil(intermediate) * step;
            }
        }

        /// <summary>
        /// Clamps the value to be within the range [min, max]
        /// </summary>
        public static float Clamp (float value, float min, float max) {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        /// <summary>
        /// Clamps the value to be within the range [min, max]
        /// </summary>
        public static int Clamp (int value, int min, int max) {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        /// <summary>
        /// Clamps the value to be within the range [min, max]
        /// </summary>
        public static Fixnum Clamp (Fixnum value, Fixnum min, Fixnum max) {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        /// <summary>
        /// Clamps the value to be within the range [min, ∞)
        /// </summary>
        public static int ClampMin (int value, int min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value to be within the range [min, ∞]
        /// </summary>
        public static float ClampMin (float value, float min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value to be within the range [min, ∞]
        /// </summary>
        public static Fixnum ClampMin (Fixnum value, Fixnum min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value to be within the range (-∞, max]
        /// </summary>
        public static int ClampMax (int value, int max) => value > max ? max : value;

        /// <summary>
        /// Clamps the value to be within the range [-∞, max]
        /// </summary>
        public static float ClampMax (float value, float max) => value > max ? max : value;
        /// <summary>
        /// Clamps the value to be within the range [-∞, max]
        /// </summary>
        public static Fixnum ClampMax (Fixnum value, Fixnum max) => value > max ? max : value;


        /// <summary>
        /// Interpolates between two values. If p = 0, returns min,
        /// if p = 1, returns max, otherwise returns lerp of both.
        /// </summary>
        public static float Interpolate (float p, float min, float max) =>
            (1 - p) * min + p * max;

        /// <summary>
        /// Finds position of q between two values a and b - an inverse of interpolation. 
        /// If q = min returns 0, if q = max, returns 1, otherwise returns a value proportional to 
		/// q's position between them. Throws a division by zero error if min = max.
        /// </summary>
        public static float Uninterpolate (float q, float min, float max) =>
            (q - min) / (max - min);

        /// <summary>
        /// Finds position of q between two values a and b - an inverse of interpolation.
        /// This version supports ranges that straddle a period boundary.
        /// If from is less than to, this is the same as regular uninterpolation.
        /// If from is more than to, we move q and to the next cycle to fix the straddle.
        /// </summary>
        public static float Uninterpolate (float q, float from, float to, float period) {
            if (from > to) {
                // convert this from a straddling interval to a continuous interval
                if (q < to) { q += period; } // q might be straddling as well
                to += period; // and "to" was for sure
            }
            return Uninterpolate(q, from, to);
        }

        /// <summary>
        /// Converts a value from the range [amin, amax] to the range [bmin, bmax].
        /// Equivalent to uninterpolating in [amin, amax] and then re-interpoloating between [bmin, bmax].
        /// Throws a division by zero error if amin == amax.
        /// </summary>
        public static float ConvertRange (float value, float amin, float amax, float bmin, float bmax) {
            float p = Uninterpolate(value, amin, amax);
            return Interpolate(p, bmin, bmax);
        }

        /// <summary>
        /// Modulus function, because in C# the % operator is not mod, it's actually a remainder, and can be negative.
        /// For example, Modulus(1, 3) == 1 and 1 % 3 == 1, but Modulus(-1, 3) == 2, while -1 % 3 == -1.
        /// </summary>
        public static float Modulus (float a, float b) {
            float r = a % b;
            return (r < 0) ? r + b : r;
        }

        /// <summary>
        /// Modulus function, because in C# the % operator is not mod, it's actually a remainder, and can be negative.
        /// For example, Modulus(1, 3) == 1 and 1 % 3 == 1, but Modulus(-1, 3) == 2, while -1 % 3 == -1.
        /// </summary>
        public static int Modulus (int a, int b) {
            int r = a % b;
            return (r < 0) ? r + b : r;
        }

        /// <summary>
        /// Given ranges of given size starting at 0, counts how many times the range boundary was crossed in the 
        /// interval between from and to. For example, given period of 5, the ranges would be [0, 5), [5, 10), [10, 15), etc.
        /// So if we take 1 as our from value, from 1 to 4 there are zero crossings, from 1 to 7 there is one (at 5), 
        /// from 1 to 13 there are two (at 5 and 10), and so on.
        /// </summary>
        public static int CountIntervals (int from, int to, int period) {
            int first = (int) Math.Floor((double) from / period);
            int last = (int) Math.Floor((double) to / period);
            return last - first;
        }

        /// <summary>
        /// Given ranges of given size starting at 0, counts how many times the range boundary was crossed in the 
        /// interval between from and to. For example, given period of 5, the ranges would be [0, 5), [5, 10), [10, 15), etc.
        /// So if we take 1 as our from value, from 1 to 4.999 there are zero crossings, from 1 to 7 there is one (at 5), 
        /// from 1 to 13 there are two (at 5 and 10), and so on.
        /// </summary>
        public static int CountIntervals (float from, float to, float period) {
            int first = (int) Math.Floor((double) from / period);
            int last = (int) Math.Floor((double) to / period);
            return last - first;
        }

        /// <summary>
        /// Returns true if the given time is inside the half-open interval [from, to), 
        /// accounting for 24-hour wrapping. For example, if from = 17 and to = 2 
        /// (so between 5pm and 2am), time = 1 is inside the interval, but 2 or 3 is not; 
        /// 16 is not inside the interval, but 17 and 18 are.
        /// </summary>
        public static bool InsideDailyInterval (float time, float from, float to) {

            bool sameday = (from < to);
            time = (time < 24) ? time : (time % 24);

            return sameday ?
                (from <= time && time < to) :
                (from <= time || time < to);
        }


        public static bool IsEven (int value) => (value & 1) == 0;
        public static bool IsEven (uint value) => (value & 1) == 0;
        public static bool IsEven (float value) => ((int) value & 1) == 0;

        public static bool IsOdd (int value) => (value & 1) == 1;
        public static bool IsOdd (uint value) => (value & 1) == 1;
        public static bool IsOdd (float value) => ((int) value & 1) == 1;

        /// <summary>
        /// Simple swap implementation
        /// </summary>
        public static void Swap (ref float a, ref float b) {
            float temp = a; a = b; b = temp;
        }

        /// <summary>
        /// Simple swap implementation
        /// </summary>
        public static void Swap (ref int a, ref int b) {
            int temp = a; a = b; b = temp;
        }

        /// <summary>
        /// Converts a float to an int by rounding up or down stochastically, such that over a large number
        /// of samples the average value will converge back towards the input float.
        /// 
        /// For example, given the value 2.3f, the rounding will produce 2 for 70% of the time, 
        /// and the value 3 for 30% of the time. 
        /// </summary>
        public static int RoundProbabilistic (float input, IRandom rng) =>
            (int) Math.Floor(input + rng.GenerateFloat());
    }
}
