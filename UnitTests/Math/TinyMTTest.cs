using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SomaSim.Math
{
    [TestClass]
    public class TinyMTTest
    {
        [TestMethod]
        public void TestTinyMT () {
            IRandom rand = new TinyMT();

            // these magic constants courtesy of the original C implementation test suite

            rand.Init(1);
            Assert.IsTrue(rand.Generate() == 2545341989);
            Assert.IsTrue(rand.Generate() == 981918433);
            Assert.IsTrue(rand.Generate() == 3715302833);
            Assert.IsTrue(rand.Generate() == 2387538352);
            Assert.IsTrue(rand.Generate() == 3591001365);
        }

        [TestMethod]
        public void TestTinyMTState () {
            TinyMT mt1 = new TinyMT();
            TinyMT mt2 = new TinyMT();

            mt1.Init(1);
            Assert.IsTrue(mt1.Generate() == 2545341989);
            Assert.IsTrue(mt1.Generate() == 981918433);

            // copy state to a brand new rng, and make sure it picks up where the first one left off
            mt1.s.CopyTo(mt2.s, 0);
            Assert.IsTrue(mt2.Generate() == 3715302833 && mt1.Generate() == 3715302833);
        }

        [TestMethod]
        public void TestIRandomExtensionMethods () {
            IRandom rand = new TinyMT();

            rand.Init(1); // 2545341989 will be the first number
            Assert.IsTrue(rand.GenerateInRange(0, 5) == 4); // 2545341989 % 5 = 4

            rand.Init(1);
            Assert.IsTrue(rand.GenerateInRange(1, 6) == 5); // 2545341989 % 5 + 1 = 5

            rand.Init(1);
            Assert.IsTrue(rand.PickElement(new uint[] { 0, 1, 2, 3, 4 }) == 4); // 2545341989 % 5 = 4, so 4th element

            rand.Init(1); // 2545341989 will be the first number
            Assert.IsTrue(rand.DieRoll(10) == 10); // last digit of 2545341989 plus one
        }
    }
}
