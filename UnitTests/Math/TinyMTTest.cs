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
    }
}
