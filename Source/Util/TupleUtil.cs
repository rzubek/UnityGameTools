// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

namespace SomaSim.Util
{
    /// <summary>
    /// Simple binary tuple of type float, with meaningfully named fields for readability
    /// </summary>
    public struct FloatRange
    {
        public float from;
        public float to;

        /// <summary>
        /// Returns true if the specified value is in the half-open interval [from, to).
        /// </summary>
        public bool InInterval (float value) => from <= value && value < to;

        /// <summary>
        /// Returns true if the specified value is in the closed interval [from, to].
        /// </summary>
        public bool InIntervalClosed (float value) => from <= value && value <= to;
    }

    /// <summary>
    /// Simple binary tuple of type int, with meaningfully named fields for readability
    /// </summary>
    public struct IntRange
    {
        public int from;
        public int to;

        /// <summary>
        /// Returns true if the specified value is in the half-open interval [from, to).
        /// </summary>
        public bool InInterval (int value) => from <= value && value < to;

        /// <summary>
        /// Returns true if the specified value is in the closed interval [from, to].
        /// </summary>
        public bool InIntervalClosed (int value) => from <= value && value <= to;

        public bool InIntervalWrapped (int value, int wrapped) {
            if (to < from) {
                if (from <= value) {
                    return value < (to + wrapped);
                } else {
                    return value < to;
                }
            } else {
                return InInterval(value);
            }
        }
    }
}
