using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        void Init(uint seed);

        /// <summary>
        /// Generates the next random value as an unsigned int
        /// </summary>
        /// <returns></returns>
        uint Generate();

        /// <summary>
        /// Generates the next random value as a single-precision float in the half-open range [0, 1).
        /// </summary>
        /// <returns></returns>
        float GenerateFloat();

    }
}
