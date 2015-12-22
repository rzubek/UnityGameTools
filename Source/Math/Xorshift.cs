using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Math
{
    /// <summary>
    /// Implementation of xorshift-32, a very fast random number generator
    /// with a 2^32-1 cycle and a 32-bit state.
    /// 
    /// See "Xorshift RNGs" by George Marsaglia, Journal of Statistical Software 8 (14), July 2003. 
    /// http://www.jstatsoft.org/v08/i14/paper 
    /// </summary>
    public class Xorshift : IRandom
    {
        private const float UINT_TO_UNIT_FLOAT = (1.0f / 4294967296.0f);

        /// <summary>
        /// Default value from the Marsaglia paper above, retained for historical purposes
        /// </summary>
        public const uint DEFAULT_SEED = 2463534242; 

        /// <summary>
        /// Current state of the RNG
        /// </summary>
        public uint y = DEFAULT_SEED;

        public void Init (uint seed) {
            y = DEFAULT_SEED ^ seed;
        }

        public uint Generate () {
            y = y ^ (y << 13);
            y = y ^ (y >> 17);
            y = y ^ (y << 5);
            return y;
        }

        public float GenerateFloat () {
            return Generate() * UINT_TO_UNIT_FLOAT;
        }
    }
}
