// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

namespace SomaSim.Util
{
    [TestClass]
    public class BitwiseUtilTest
    {
        [TestMethod]
        public void TestReverseBits () {
            uint original = 0x123489ab; // (binary 0001 0010 0011 0100 1000 1001 1010 1011)
            uint reversed = 0xd5912c48; // (binary 1101 0101 1001 0001 0010 1100 0100 1000)

            Assert.IsTrue(reversed == BitwiseUtil.ReverseBits(original));
            Assert.IsTrue(original == BitwiseUtil.ReverseBits(reversed));
            Assert.IsTrue(original == BitwiseUtil.ReverseBits(BitwiseUtil.ReverseBits(original)));
        }

        [TestMethod]
        public void TestShuffleUnshuffle () {
            uint original = 0x0f0f55aa; // (binary 0000 1111 0000 1111 0101 0101 1010 1010)
            uint shuffled = 0x11bb44ee; // (binary 0001 0001 1011 1011 0100 0100 1110 1110)

            uint forward = BitwiseUtil.ShuffleBits(original);
            uint backward = BitwiseUtil.UnshuffleBits(forward);

            Assert.IsTrue(shuffled == forward);
            Assert.IsTrue(original == backward);
        }
    }
}
