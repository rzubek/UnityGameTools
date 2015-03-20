using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Math
{
    public class BitwiseUtil
    {



        /// <summary>
        /// Reverses bits in the specified unsigned integer.
        /// 
        /// Example:
        /// <pre>
        /// input:   0x123489ab (binary 0001 0010 0011 0100 1000 1001 1010 1011)
        /// output:  0xd5912c48 (binary 1101 0101 1001 0001 0010 1100 0100 1000)
        /// </pre>
        /// </summary>
        public static uint ReverseBits (uint x) {
            x = (x & 0x55555555) << 1 | (x >> 1) & 0x55555555;
            x = (x & 0x33333333) << 2 | (x >> 2) & 0x33333333;
            x = (x & 0x0f0f0f0f) << 4 | (x >> 4) & 0x0f0f0f0f;
            x = (x << 24) | ((x & 0x0000ff00) << 8) | ((x >> 8) & 0x0000ff00) | (x >> 24);
            return x;
        }

        /// <summary>
        /// Performs a bitwise perfect outer shuffle. If the given 32-bit word is
        /// <b> abcd efgh ijkl mnop ABCD EFGH IJKL MNOP </b> (where each letter denotes a single bit),
        /// the result of shuffling will be <b> aAbB cCdD eEfF gGhH iIjJ kKlL mMnN oOpP </b>.   
        /// </summary>
        public static uint ShuffleBits (uint x) {
            uint t = 0;
            t = (x ^ (x >> 8)) & 0x0000ff00; x = x ^ t ^ (t << 8);
            t = (x ^ (x >> 4)) & 0x00f000f0; x = x ^ t ^ (t << 4);
            t = (x ^ (x >> 2)) & 0x0c0c0c0c; x = x ^ t ^ (t << 2);
            t = (x ^ (x >> 1)) & 0x22222222; x = x ^ t ^ (t << 1);
            return x;
        }

        /// <summary>
        /// Performs a bitwise perfect outer unshuffle. If the given 32-bit word is
        /// <b> aAbB cCdD eEfF gGhH iIjJ kKlL mMnN oOpP </b> (where each letter denotes a single bit),
        /// the result of unshuffling will be <b> abcd efgh ijkl mnop ABCD EFGH IJKL MNOP </b>.   
        /// </summary>
        public static uint UnshuffleBits (uint x) {
            uint t = 0;
            t = (x ^ (x >> 1)) & 0x22222222; x = x ^ t ^ (t << 1);
            t = (x ^ (x >> 2)) & 0x0c0c0c0c; x = x ^ t ^ (t << 2);
            t = (x ^ (x >> 4)) & 0x00f000f0; x = x ^ t ^ (t << 4);
            t = (x ^ (x >> 8)) & 0x0000ff00; x = x ^ t ^ (t << 8);
            return x;
        }
    }
}
