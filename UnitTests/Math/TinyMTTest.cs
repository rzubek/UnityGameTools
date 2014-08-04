using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SomaSim.Math
{
    [TestClass]
    public class TinyMTTest
    {
        [TestMethod]
        public void TestTinyMT()
        {
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
        public void TestTinyMTState()
        {
            TinyMT mt1 = new TinyMT();
            TinyMT mt2 = new TinyMT();
            
            mt1.Init(1);
            Assert.IsTrue(mt1.Generate() == 2545341989);
            Assert.IsTrue(mt1.Generate() == 981918433);

            // copy state to a brand new rng, and make sure it picks up where the first one left off
            mt1.s.CopyTo(mt2.s, 0);
            Assert.IsTrue(mt2.Generate() == 3715302833 && mt1.Generate() == 3715302833);
        }
    }
}
