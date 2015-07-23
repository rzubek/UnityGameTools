using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SomaSim.Math
{
    /// <summary>
    /// Interface for random number generators
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Initializes the random number generator with an unsigned int seed.
        /// </summary>
        /// <param name="seed"></param>
        void Init (uint seed);

        /// <summary>
        /// Generates the next random value as an unsigned int
        /// </summary>
        /// <returns></returns>
        uint Generate ();

        /// <summary>
        /// Generates the next random value as a single-precision float in the half-open range [0, 1).
        /// </summary>
        /// <returns></returns>
        float GenerateFloat ();
    }

    /// <summary>
    /// Simple data structure that encapsulates both a range of numbers for a random number generator,
    /// and the number of passes (one pass to generate a uniform distribution, more passes to
    /// approximate normal distribution).
    /// </summary>
    public class RandomRange
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
    public class RandomRangeF
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

    /// <summary>
    /// Extension methods for random number generators
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Generates the next random value in the half-open range [min, max)
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Generate (this IRandom rand, int min, int max) {
            uint p = rand.Generate();
            uint mod = (uint) (p % (max - min));
			return (int) (mod + min);
        }

        /// <summary>
        /// Generates the next random float in the half-open range [min, max)
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Generate (this IRandom rand, float min, float max) {
            float p = rand.GenerateFloat();
            return (1 - p) * min + p * max;
        }

        /// <summary>
        /// Generates the next random value in the half-open range [range.min, range.max)
        /// with a distribution specified by the range.passes value.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static int Generate (this IRandom rng, RandomRange range) {
            long total = 0;
            for (int i = 0; i < range.passes; i++) {
                total += rng.Generate(range.from, range.to);
            }
            return (int)(total / range.passes);
        }

        /// <summary>
        /// Generates the next random value in the half-open range [range.min, range.max)
        /// with a distribution specified by the range.passes value.
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static float Generate (this IRandom rng, RandomRangeF range) {
            float total = 0f;
            for (int i = 0; i < range.passes; i++) {
                total += rng.Generate(range.from, range.to);
            }
            return (total / range.passes);
        }

        /// <summary>
        /// Generates the next value for the roll of a die with specified number of sides.
        /// For example, sides = 20 will generate values in the range [1, 20] for a d20 die.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="diesize"></param>
        /// <returns></returns>
        public static uint DieRoll (this IRandom rand, uint sides) {
            return (uint)(Generate(rand, 0, (int)sides) + 1);
        }

        /// <summary>
        /// Picks a random element from the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T PickElement<T> (this IRandom rand, IList<T> list) {
            if (list == null || list.Count == 0) {
                throw new System.Exception("Cannot pick random element from empty or null list");

            } else if (list.Count == 1) {
                return list[0];

            } else {
                int index = rand.Generate(0, list.Count);
                return list[index];
            }
        }

        /// <summary>
        /// Picks a random element from the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T PickElement<T> (this IRandom rand, IList list) where T : class {
            if (list == null || list.Count == 0) {
                throw new System.Exception("Cannot pick random element from empty or null list");

            } else if (list.Count == 1) {
                return list[0] as T;

            } else {
                int index = rand.Generate(0, list.Count);
                return list[index] as T;
            }
        }

        /// <summary>
        /// Picks a weighted random element from the first list, with probability based on weights from the second list.
        /// It's assumed (but not checked) that both lists have the same length.
        /// 
        /// Normally this function runs in O(n) in the size of the lists (one pass over each list).
        /// If the weights list is known to be normalized (all elements add up to one), pass in the normalized
        /// flag set to true to skip iterating over the weights array to sum it up.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rand"></param>
        /// <param name="list"></param>
        /// <param name="weights"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public static T PickElement<T> (this IRandom rand, IList<T> list, IList<float> weights, bool normalized = false) where T : class {
            float sum = normalized ? 1 : weights.Sum();
            float index = rand.GenerateFloat() * sum;
            
            for (int i = 0, count = list.Count; i < count; ++i) {
                float weight = weights[i];
                if (index <= weight) {
                    return list[i];
                } else {
                    index -= weight;
                }
            }

            return null;
        }

        /// <summary>
        /// Performs an in-place Sattolo shuffle of all elements in the list. 
        /// See: https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rand"></param>
        /// <param name="list"></param>
        public static void Shuffle<T> (this IRandom rand, IList<T> list) {
            int i = list.Count;
            while (i > 1) {
                i--;
                int j = rand.Generate(0, i);
                T temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }
    }

    /// <summary>
    /// Mock implementation of IRandom that produces values from the specified lists.
    /// </summary>
    public class RandomMock : IRandom
    {
        private const float UINT_MUL = (1.0f / 4294967296.0f);

        public IList<uint> uints;
        public int index;

        public RandomMock (IList<uint> uints) {
            this.uints = uints;
        }

        /// </inheritDoc>
        public void Init (uint seed) {
            // no op
        }

        /// </inheritDoc>
        public uint Generate () {
            uint result = uints[index++];
            if (index >= uints.Count) { index = 0; }
            return result;
        }

        /// </inheritDoc>
        public float GenerateFloat () {
            uint result = Generate();
            return result * UINT_MUL;
        }
    }
}
