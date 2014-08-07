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
        public static int GenerateInRange (this IRandom rand, int min, int max) {
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
        public static float GenerateInRange (this IRandom rand, float min, float max) {
            uint p = rand.Generate();
            return p * (max - min) + min;
        }

        /// <summary>
        /// Picks a random element from the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T PickElement<T> (this IRandom rand, IList<T> list) {
            int index = rand.GenerateInRange(0, list.Count);
            return list[index];
        }
    }
}
