using System.Collections.Generic;

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
            uint p = rand.Generate();
            return p * (max - min) + min;
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
            int index = rand.Generate(0, list.Count);
            return list[index];
        }

    }
}
