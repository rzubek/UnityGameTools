// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;

namespace SomaSim.Util
{
    #region Interfaces

    /// <summary>
    /// Interface for random number generators that produce 32-bit values
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Initializes the random number generator with an unsigned int seed.
        /// </summary>
        void Init (uint seed);

        /// <summary>
        /// Generates the next random value as an unsigned int
        /// </summary>
        uint Generate ();
    }

    /// <summary>
    /// Interface for random number generators that produce 64-bit and 32-bit values
    /// </summary>
    public interface IRandom64 : IRandom
    {
        /// <summary>
        /// Generates the next random value as an unsigned long int
        /// </summary>
        ulong Generate64 ();
    }

    #endregion

    #region SplitMix64 RNG

    /*  Written in 2015 by Sebastiano Vigna (vigna@acm.org)

        To the extent possible under law, the author has dedicated all copyright
        and related and neighboring rights to this software to the public domain
        worldwide. This software is distributed without any warranty.

        See <http://creativecommons.org/publicdomain/zero/1.0/>. */

    /* This is a fixed-increment version of Java 8's SplittableRandom generator
       See http://dx.doi.org/10.1145/2714064.2660195 and 
       http://docs.oracle.com/javase/8/docs/api/java/util/SplittableRandom.html

       It is a very fast generator passing BigCrush, and it can be useful if
       for some reason you absolutely want 64 bits of state; otherwise, we
       rather suggest to use a xoroshiro128+ (for moderately parallel
       computations) or xorshift1024* (for massively parallel computations)
       generator. */

    /// <summary>
    /// Implementation of the SplitMix64 RNG with 64-bit state. Particularly useful
    /// for fast seeding of other RNGs since it is not sensitive to initial seed of zero.
    /// See http://xoroshiro.di.unimi.it/splitmix64.c
    /// </summary>
    public sealed class SplitMix64 : IRandom64
    {
        public ulong x; /* The state can be seeded with any value. */

        public SplitMix64 () { }

        public SplitMix64 (uint seed) {
            Init(seed);
        }

        public void Init (uint seed) => x = seed;

        public uint Generate () => (uint) Generate64();

        public ulong Generate64 () {
            ulong z = (x += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    #endregion

    #region Xoroshiro128+ RNG

    /*  Written in 2016 by David Blackman and Sebastiano Vigna (vigna@acm.org)

        To the extent possible under law, the author has dedicated all copyright
        and related and neighboring rights to this software to the public domain
        worldwide. This software is distributed without any warranty.

        See <http://creativecommons.org/publicdomain/zero/1.0/>. */

    /* This is the successor to xorshift128+. It is the fastest full-period
       generator passing BigCrush without systematic failures, but due to the
       relatively short period it is acceptable only for applications with a
       mild amount of parallelism; otherwise, use a xorshift1024* generator.

       Beside passing BigCrush, this generator passes the PractRand test suite
       up to (and included) 16TB, with the exception of binary rank tests,
       which fail due to the lowest bit being an LFSR; all other bits pass all
       tests. We suggest to use a sign test to extract a random Boolean value.

       Note that the generator uses a simulated rotate operation, which most C
       compilers will turn into a single instruction. In Java, you can use
       Long.rotateLeft(). In languages that do not make low-level rotation
       instructions accessible xorshift128+ could be faster.

       The state must be seeded so that it is not everywhere zero. If you have
       a 64-bit seed, we suggest to seed a splitmix64 generator and use its
       output to fill s. */

    /// <summary>
    /// Implementation of the Xoroshiro128+ RNG, a fast RNG with 2^128-1 period,
    /// 128-bit state, excellent random properties, and a far jump function.
    /// See http://xoroshiro.di.unimi.it/xoroshiro128plus.c
    /// </summary>
    public sealed class Xoro128 : IRandom64
    {

        private static readonly ulong[] JUMP = { 0xBEAC0467EBA5FACBUL, 0xD86B048B86AA9922UL };
        private static readonly SplitMix64 _seedgen = new SplitMix64();

        public ulong state0, state1;

        public Xoro128 () {
            // make sure we don't start with a zero state
            state0 = JUMP[0];
            state1 = JUMP[1];
        }

        /// <summary>
        /// Initializes the RNG from specified 128-bit seed.
        /// </summary>
        public void Init (ulong seed0, ulong seed1) {
            if (seed0 == 0UL && seed1 == 0UL) {
                throw new ArgumentException("Seed values cannot be both zero");
            }

            state0 = seed0;
            state1 = seed1;
        }

        /// <summary>
        /// Initializes the RNG from a single uint seed, by feeding it into
        /// a splitmix64 generator which expands it into a 128-bit seed.
        /// </summary>
        public void Init (uint seed) {
            _seedgen.Init(seed);
            state0 = _seedgen.Generate64();
            state1 = _seedgen.Generate64();
        }

        public uint Generate () => (uint) Generate64();

        public ulong Generate64 () {
            ulong s0 = state0;
            ulong s1 = state1;
            ulong result = s0 + s1;

            s1 ^= s0;
            state0 = ((s0 << 55) | (s0 >> (64 - 55))) ^ s1 ^ (s1 << 14); // a, b
            state1 = ((s1 << 36) | (s1 >> (64 - 36))); // c 
            //       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ poor man's left rotate

            return result;
        }


        /* This is the jump function for the generator. It is equivalent
           to 2^64 calls to next(); it can be used to generate 2^64
           non-overlapping subsequences for parallel computations. */

        /// <summary>
        /// Advances the RNG by 2^64 calls to Generate64()
        /// </summary>
        public void Jump () {

            ulong s0 = 0;
            ulong s1 = 0;
            for (int i = 0; i < JUMP.Length; i++) {
                for (int b = 0; b < 64; b++) {
                    if ((JUMP[i] & (1UL << b)) != 0L) {
                        s0 ^= state0;
                        s1 ^= state1;
                    }
                    Generate64();
                }
            }

            state0 = s0;
            state1 = s1;
        }
    }

    #endregion

    #region Xorshift32 RNG

    /// <summary>
    /// Implementation of xorshift-32, a very fast random number generator
    /// with a 2^32-1 period and a 32-bit state.
    /// 
    /// See "Xorshift RNGs" by George Marsaglia, Journal of Statistical Software 8 (14), July 2003. 
    /// http://www.jstatsoft.org/v08/i14/paper 
    /// </summary>
    public class Xorshift : IRandom
    {
        // Default value from the Marsaglia paper above, retained for historical purposes
        private const uint DEFAULT_SEED = 2463534242;

        /// <summary>
        /// Current state of the RNG
        /// </summary>
        public uint y = DEFAULT_SEED;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Xorshift () { }

        /// <summary>
        /// Constructor that also initializes the Xorshift with the given seed
        /// </summary>
        public Xorshift (uint seed) : this() {
            Init(seed);
        }

        public void Init (uint seed) {
            if (seed == 0) {
                throw new ArgumentException("RNG seed may not be all zero", "seed");
            }
            y = seed;
        }

        public uint Generate () {
            y = y ^ (y << 13);
            y = y ^ (y >> 17);
            y = y ^ (y << 5);
            return y;
        }
    }

    #endregion

    #region Tiny Mersenne Twister RNG

    /*
    C# port of the Tiny Mersenne Twister - see 
    http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/TINYMT/index.html

    Copyright note from the original: 

    Copyright (c) 2011, 2013 Mutsuo Saito, Makoto Matsumoto,
    Hiroshima University and The University of Tokyo.
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are
    met:

        * Redistributions of source code must retain the above copyright
          notice, this list of conditions and the following disclaimer.
        * Redistributions in binary form must reproduce the above
          copyright notice, this list of conditions and the following
          disclaimer in the documentation and/or other materials provided
          with the distribution.
        * Neither the name of the Hiroshima University nor the names of
          its contributors may be used to endorse or promote products
          derived from this software without specific prior written
          permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
    "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
    LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
    A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
    OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
    SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
    LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
    DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
    THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
    (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
     */

    /// <summary>
    /// TinyMT is a small-footprint variant of a Mersenne Twister,
    /// with an internal state of just 128 bits, and period of 2^(127-1).
    /// 
    /// See: http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/TINYMT/index.html
    /// </summary>
    public class TinyMT : IRandom
    {
        private const int TINYMT32_MEXP = 127;
        private const int TINYMT32_SH0 = 1;
        private const int TINYMT32_SH1 = 10;
        private const int TINYMT32_SH8 = 8;
        private const uint TINYMT32_MASK = 0x7fffffff;
        private const float TINYMT32_MUL = (1.0f / 4294967296.0f);

        private const int MIN_LOOP = 8;
        private const int PRE_LOOP = 8;

        public uint[] s; // state

        private uint m1;
        private uint m2;
        private uint mt;

        public TinyMT () {
            s = new uint[4] { 0, 0, 0, 0 };
            // arbitrary numbers from the original paper's test suite
            Init(1, 0x8f7011ee, 0xfc78ff1f, 0x3793fdff);
        }

        /// <summary>
        /// This function changes internal state of tinymt32.
        /// Users should not call this function directly.
        /// </summary>
        private void NextState () {
            uint x;
            uint y;

            y = s[3];
            x = (s[0] & TINYMT32_MASK)
            ^ s[1]
            ^ s[2];
            x ^= (x << TINYMT32_SH0);
            y ^= (y >> TINYMT32_SH0) ^ x;
            s[0] = s[1];
            s[1] = s[2];
            s[2] = x ^ (y << TINYMT32_SH1);
            s[3] = y;
            uint mask = (uint) -((int) (y & 1));
            s[1] ^= mask & m1;
            s[2] ^= mask & m2;
        }

        /// <summary>
        /// This function outputs 32-bit unsigned integer from internal state.
        /// Users should not call this function directly.
        /// </summary>
        private uint Temper () {
            uint t0, t1;
            t0 = s[3];
            t1 = s[0] + (s[2] >> TINYMT32_SH8);
            t0 ^= t1;
            t0 ^= (uint) -((int) (t1 & 1)) & mt;
            return t0;
        }


        /// <summary>
        /// This function initializes the internal state array with a 32-bit
        /// unsigned integer seed and random number generator state variables.
        /// </summary>
        private void Init (uint seed, uint mat1, uint mat2, uint tmat) {
            s[0] = seed;
            s[1] = this.m1 = mat1;
            s[2] = this.m2 = mat2;
            s[3] = this.mt = tmat;
            for (uint i = 1; i < MIN_LOOP; i++) {
                s[i & 3] ^= i + (uint) (1812433253)
                    * (s[(i - 1) & 3]
                       ^ (s[(i - 1) & 3] >> 30));
            }

            for (int i = 0; i < PRE_LOOP; i++) {
                NextState();
            }
        }

        public void Init (uint seed) => Init(seed, m1, m2, mt);

        public uint Generate () {
            NextState();
            return Temper();
        }
    }

    #endregion

    #region Random range definitions

    /// <summary>
    /// Simple data structure that encapsulates both a range of numbers for a random number generator,
    /// and the number of passes (one pass to generate a uniform distribution, more passes to
    /// approximate normal distribution).
    /// </summary>
    public sealed class RandomRange
    {
        public int from;
        public int to;
        public byte passes;

        public RandomRange () : this(0, 1, 1) { }

        public RandomRange (int from, int to, byte passes = 1) {
            this.from = from;
            this.to = to;
            this.passes = passes;
        }
    }

    /// <summary>
    /// Simple data structure that encapsulates both a range of numbers for a random number generator,
    /// and the number of passes (one pass to generate a uniform distribution, more passes to
    /// approximate normal distribution).
    /// </summary>
    public sealed class RandomRangeF
    {
        public float from;
        public float to;
        public byte passes;

        public RandomRangeF () : this(0f, 1f, 1) { }

        public RandomRangeF (float from, float to, byte passes = 1) {
            this.from = from;
            this.to = to;
            this.passes = passes;
        }
    }

    #endregion

    #region Hashing utilities

    /// <summary>
    /// Utility methods for hashing via random number generators. These should produce 
    /// well-distributed hashes, such that consecutive inputs will result
    /// in very different outputs.
    /// </summary>
    public static class HashUtil
    {
        private static SplitMix64 _rng = new SplitMix64();

        /// <summary>
        /// Generates a uint hash of the specified uint.
        /// </summary>
        public static uint Hash (uint value) {
            _rng.Init(value);
            return _rng.Generate();
        }

        /// <summary>
        /// Generates a uint hash of the specified int.
        /// </summary>
        public static uint Hash (int value) {
            _rng.Init((uint) value);
            return _rng.Generate();
        }

        /// <summary>
        /// Generates a uint hash of the specified string.
        /// </summary>
        public static uint Hash (string value) {
            uint seed = string.IsNullOrEmpty(value) ? 0 : value.GetStableHashCode();
            _rng.Init(seed);
            return _rng.Generate();
        }

        /// <summary>
        /// Generates a float hash in the [0, 1) range of the specified uint.
        /// </summary>
        public static float HashToFloat (uint value) {
            _rng.Init(value);
            return _rng.GenerateFloat();
        }

        /// <summary>
        /// Generates a float hash in the [0, 1) range of the specified int.
        /// </summary>
        public static float HashToFloat (int value) {
            _rng.Init((uint) value);
            return _rng.GenerateFloat();
        }

        /// <summary>
        /// Generates a float hash in the [0, 1) range of the specified string.
        /// </summary>
        public static float HashToFloat (string value) {
            uint seed = string.IsNullOrEmpty(value) ? 0 : value.GetStableHashCode();
            _rng.Init(seed);
            return _rng.GenerateFloat();
        }
    }

    #endregion

    #region Random Extensions

    /// <summary>
    /// Extension methods for random number generators
    /// </summary>
    public static class RandomExtensions
    {
        public const float UINT_TO_FLOAT = (1.0f / 4294967296.0f);

        /// <summary>
        /// Initializes the RNG from the DateTime.ticks field of the specified time
        /// (the actual resolution of the ticks field is implementation-specific, see DateTime docs)
        /// </summary>
        public static void Init (this IRandom rng, DateTime time) {
            uint seed = (uint) time.Ticks;
            rng.Init(seed);
        }

        /// <summary>
        /// Initializes the RNG from a given uint seed and an additional salt value used to modify the seed.
        /// </summary>
        public static void Init (this IRandom rng, uint seed, uint salt) => rng.Init(seed ^ salt);

        /// <summary>
        /// Generates the next random value as a single-precision float in the half-open range [0, 1).
        /// </summary>
        /// <returns></returns>
        public static float GenerateFloat (this IRandom rng) => rng.Generate() * UINT_TO_FLOAT;

        /// <summary>
        /// Generates the next random value in the half-open int range [min, max)
        /// </summary>
        public static int Generate (this IRandom rng, int min, int max) {
            uint p = rng.Generate();
            if (max > min) {
                return min + (int) (p % (max - min));
            } else if (min > max) {
                return max + (int) (p % (min - max));
            } else {
                return min;
            }
        }

        /// <summary>
        /// Generates the next random value in the half-open int range [range.from, range.to)
        /// </summary>
        public static int Generate (this IRandom rng, IntRange range) =>
            Generate(rng, range.from, range.to);

        /// <summary>
        /// Generates the next random float in the closed float range [min, max]
        /// </summary>
        public static float Generate (this IRandom rng, float min, float max) {
            float p = rng.GenerateFloat();
            return (1 - p) * min + p * max;
        }

        /// <summary>
        /// Generates the next random value in the half-open int range [range.min, range.max)
        /// with a distribution specified by the range.passes value.
        /// </summary>
        public static int Generate (this IRandom rng, RandomRange range) {
            long total = 0;
            for (int i = 0; i < range.passes; i++) {
                total += rng.Generate(range.from, range.to);
            }
            return (int) (total / range.passes);
        }

        /// <summary>
        /// Generates the next random value in the closed float range [range.min, range.max]
        /// with a distribution specified by the range.passes value.
        /// </summary>
        public static float Generate (this IRandom rng, RandomRangeF range) {
            float total = 0f;
            for (int i = 0; i < range.passes; i++) {
                total += rng.Generate(range.from, range.to);
            }
            return (total / range.passes);
        }

        /// <summary>
        /// Generates the next value for the roll of a die with specified number of sides.
        /// For example, sides = 20 will generate values in the range [0, 19] for a d20 die.
        /// </summary>
        public static int DieRoll (this IRandom rng, int sides) => rng.Generate(0, sides);

        /// <summary>
        /// Returns true or false with equal probability (for a fair coin).
        /// For specific probabilities use CheckProbability() instead.
        /// </summary>
        public static bool CoinFlip (this IRandom rng) => CheckAndReturn(rng, 0.5f).pass;

        /// <summary>
        /// Produces a random float in the range [0, 1), and returns true if this float 
        /// is strictly less than the specified probability value.
        /// </summary>
        public static bool CheckProbability (this IRandom rng, float probability) => CheckAndReturn(rng, probability).pass;

        /// <summary>
        /// Produces a random float in the range [0, 1), and returns true if this float 
        /// is strictly less than the specified probability value.
        /// </summary>
        public static bool CheckProbability (this IRandom rng, Fixnum probability) => CheckProbability(rng, (float) probability);

        /// <summary>
        /// Produces a random float in the range [0, 1), and returns true if this float 
        /// is strictly less than the specified probability value, as well as the value itself.
        /// </summary>
        public static (bool pass, float random) CheckAndReturn (this IRandom rng, float probability) {
            var value = rng.GenerateFloat();
            return (value < probability, value);
        }

        /// <summary>
        /// Picks a random element from the collection and returns its index, or -1 if collection is empty.
        /// Collection cannot be null.
        /// </summary>
        public static int PickIndex<T> (this IRandom rng, T[] list) {
            if (list == null) {
                throw new ArgumentNullException(nameof(list), "Cannot pick random element from empty or null list");
            }

            return list.Length == 0 ? -1 : list.Length == 1 ? 0 : rng.Generate(0, list.Length);
        }

        /// <summary>
        /// Picks a random element from the collection and returns its index, or -1 if collection is empty.
        /// Collection cannot be null.
        /// </summary>
        public static int PickIndex<T> (this IRandom rng, List<T> list) {
            if (list == null) {
                throw new ArgumentNullException(nameof(list), "Cannot pick random element from empty or null list");
            }

            return list.Count == 0 ? -1 : list.Count == 1 ? 0 : rng.Generate(0, list.Count);
        }

        /// <summary>
        /// Picks a weighted random element from the first list, with probability based on weights from the second list,
        /// and returns its index, or -1 if the list is empty. Both lists must have the same length and cannot be null.
        ///
        /// Normally this function runs in O(n) in the size of the lists (one pass over each list).
        /// If the weights list is known to be normalized (all elements add up to one), pass in the normalized
        /// flag set to true to skip iterating over the weights array to sum it up.
        /// </summary>
        public static int PickIndexWeighed<T> (this IRandom rng, List<T> list, List<float> weights, bool normalized = false) {
            if (list == null) { throw new ArgumentNullException(nameof(list), "Cannot pick random element from null list"); }
            if (weights == null) { throw new ArgumentNullException(nameof(weights), "Cannot pick random element from null weight list"); }
            if (list.Count != weights.Count) { throw new ArgumentException("List of elements must have the same length as the list of weights"); }

            if (list.Count == 0) { return -1; }

            float sum = normalized ? 1 : weights.SumFast();
            float index = rng.GenerateFloat() * sum;

            for (int i = 0, count = list.Count; i < count; ++i) {
                float weight = weights[i];
                if (index <= weight) {
                    return i;
                } else {
                    index -= weight;
                }
            }

            // improperly normalized list? return the last one
            return list.Count - 1;
        }

        /// <summary>
        /// Picks a weighted random element with probability based on weights from the second field in the element,
        /// and returns its index, or -1 if the list is empty. Lists cannot be null.
        ///
        /// Normally this function runs in O(n) in the size of the lists (one pass over each list).
        /// If the weights list is known to be normalized (all elements add up to one), pass in the normalized
        /// flag set to true to skip iterating over the weights array to sum it up.
        /// </summary>
        public static int PickIndexWeighed<T> (this IRandom rng, List<(T, float)> list, bool normalized = false) {
            if (list == null) { throw new ArgumentNullException(nameof(list), "Cannot pick random element from null list"); }

            if (list.Count == 0) { return -1; }

            float sum = 0;
            if (normalized) {
                sum = 1;
            } else {
                for (int i = 0, count = list.Count; i < count; ++i) {
                    sum += list[i].Item2;
                }
            }

            float index = rng.GenerateFloat() * sum;
            for (int i = 0, count = list.Count; i < count; ++i) {
                float weight = list[i].Item2;
                if (index <= weight) {
                    return i;
                } else {
                    index -= weight;
                }
            }

            // improperly normalized list? return the last one
            return list.Count - 1;
        }

        /// <summary>
        /// Picks a random element from the collection. The collection cannot be empty or null.
        /// </summary>
        public static T PickElement<T> (this IRandom rng, List<T> list)
            => list[PickIndex(rng, list)];

        /// <summary>
        /// Picks a random element from the collection. The collection cannot be empty or null.
        /// </summary>
        public static T PickElement<T> (this IRandom rng, T[] list)
            => list[PickIndex(rng, list)];

        /// <summary>
        /// Picks a random element from the collection. If the collection is empty or null, returns the default value for T.
        /// </summary>
        public static T PickElementOrDefault<T> (this IRandom rng, List<T> list, T defaultValue = default)
            => (list == null || list.Count == 0) ? defaultValue : list[PickIndex(rng, list)];

        /// <summary>
        /// Picks a weighted random element from the first list, with probability based on weights from the second list.
        /// Both lists must have the same length. The lists are not allowed to be empty or null.
        ///
        /// Normally this function runs in O(n) in the size of the lists (one pass over each list).
        /// If the weights list is known to be normalized (all elements add up to one), pass in the normalized
        /// flag set to true to skip iterating over the weights array to sum it up.
        /// </summary>
        public static T PickElementWeighed<T> (this IRandom rng, List<T> list, List<float> weights, bool normalized = false) {
            var index = PickIndexWeighed(rng, list, weights, normalized);
            return list[index];
        }

        /// <summary>
        /// Picks a weighted random element from the list, with probability based on weights from the second element.
        /// The lists is not allowed to be empty or null.
        ///
        /// Normally this function runs in O(n) in the size of the lists (one pass over each list).
        /// If the weights list is known to be normalized (all elements add up to one), pass in the normalized
        /// flag set to true to skip iterating over the weights array to sum it up.
        /// </summary>
        public static T PickElementWeighed<T> (this IRandom rng, List<(T, float)> list, bool normalized = false) {
            var index = PickIndexWeighed(rng, list, normalized);
            return list[index].Item1;
        }

        /// <summary>
        /// Picks and removes a random element from the collection. The collection cannot be empty or null.
        /// By default, the function will perform O(1) removal by reordering items, but if 
        /// preserveOrdering is true, it will instead perform O(n) removal that preserves original list ordering.
        /// </summary>
        public static T PickAndRemoveElement<T> (this IRandom rng, List<T> list, bool preserveOrdering = false) {
            var index = PickIndex(rng, list);
            var result = list[index];
            if (preserveOrdering) {
                list.RemoveAt(index);
            } else {
                list.SwapRemoveAt(index);
            }
            return result;
        }

        /// <summary>
        /// Picks and removes  a weighted random element from the first list, with probability based on weights from
        /// the second list. The collection cannot be empty or null. 
        /// It's assumed (but not checked) that both lists have the same length.
        ///
        /// Normally this function runs in O(n) in the size of the lists (one pass over each list).
        /// If the weights list is known to be normalized (all elements add up to one), pass in the normalized
        /// flag set to true to skip iterating over the weights array to sum it up.
        ///
        /// By default, the function will perform O(1) removal by reordering items, but if 
        /// preserveOrdering is true, it will instead perform O(n) removal that preserves original list ordering.
        /// </summary>
        public static T PickAndRemoveElementWeighed<T> (this IRandom rng, List<T> list, List<float> weights, bool normalized = false, bool preserveOrdering = false) {
            var index = PickIndexWeighed(rng, list, weights, normalized: normalized);
            var result = list[index];
            if (preserveOrdering) {
                list.RemoveAt(index);
                weights.RemoveAt(index);
            } else {
                list.SwapRemoveAt(index);
                weights.SwapRemoveAt(index);
            }
            return result;
        }

        /// <summary>
        /// Performs an in-place Sattolo shuffle of all elements in the list. 
        /// See: https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        /// </summary>
        public static void Shuffle<T> (this IRandom rng, List<T> list) {
            int i = list.Count;
            while (i > 1) {
                i--;
                int j = rng.Generate(0, i);
                T temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }

        /// <summary> Creates a shallow copy of the source list and shuffles it. </summary>
        public static List<T> ShuffleCopy<T> (this IRandom rng, List<T> list) {
            var copy = new List<T>(list);
            rng.Shuffle(copy);
            return copy;
        }
    }

    #endregion

    #region Mock implementations

    /// <summary>
    /// Mock implementation of IRandom that produces values from the specified lists.
    /// </summary>
    public class ListMock : IRandom
    {
        public IList<uint> uints;
        public int index;

        public ListMock (IList<uint> uints) {
            this.uints = uints;
        }

        public void Init (uint seed) {
            // no op
        }

        public uint Generate () {
            uint result = uints[index++];
            if (index >= uints.Count) { index = 0; }
            return result;
        }
    }

    /// <summary>
    /// Mock implementation of IRandom that produces only the seed value, over and over.
    /// So it's not really random, you know. :)
    /// 
    /// See http://dilbert.com/strip/2001-10-25
    /// </summary>
    public class NonRandomMock : IRandom
    {
        public uint result;

        public void Init (uint seed) => this.result = seed;

        public uint Generate () => result;
    }

    #endregion
}
