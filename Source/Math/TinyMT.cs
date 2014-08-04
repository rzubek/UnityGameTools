using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Math
{
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

        public uint[] s;
        public uint m1;
        public uint m2;
        public uint mt;

        public TinyMT()
        {
            s = new uint[4] { 0, 0, 0, 0 };
            // arbitrary numbers from the original paper's test suite
            Init(1, 0x8f7011ee, 0xfc78ff1f, 0x3793fdff);
        }

        /// <summary>
        /// This function changes internal state of tinymt32.
        /// Users should not call this function directly.
        /// </summary>
        private void NextState()
        {
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
            uint mask = (uint)-((int)(y & 1));
            s[1] ^= mask & m1;
            s[2] ^= mask & m2;
        }

        /// <summary>
        /// This function outputs 32-bit unsigned integer from internal state.
        /// Users should not call this function directly.
        /// </summary>
        private uint Temper()
        {
            uint t0, t1;
            t0 = s[3];
            t1 = s[0] + (s[2] >> TINYMT32_SH8);
            t0 ^= t1;
            t0 ^= (uint)-((int)(t1 & 1)) & mt;
            return t0;
        }


        /// <summary>
        /// This function initializes the internal state array with a 32-bit
        /// unsigned integer seed and random number generator state variables.
        /// </summary>
        private void Init(uint seed, uint mat1, uint mat2, uint tmat)
        {
            s[0] = seed;
            s[1] = this.m1 = mat1;
            s[2] = this.m2 = mat2;
            s[3] = this.mt = tmat;
            for (uint i = 1; i < MIN_LOOP; i++)
            {
                s[i & 3] ^= i + (uint)(1812433253)
                    * (s[(i - 1) & 3]
                       ^ (s[(i - 1) & 3] >> 30));
            }

            for (int i = 0; i < PRE_LOOP; i++)
            {
                NextState();
            }
        }

        /// <inheritDoc/>
        public void Init(uint seed)
        {
            Init(seed, m1, m2, mt);
        }

        /// <inheritDoc/>
        public uint Generate()
        {
            NextState();
            return Temper();
        }

        /// <inheritDoc/>
        public float GenerateFloat()
        {
            NextState();
            return Temper() * TINYMT32_MUL;
        }

    }
}
