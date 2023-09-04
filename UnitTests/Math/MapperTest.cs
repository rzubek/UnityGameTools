// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System.Collections.Generic;

namespace SomaSim.Util
{
    [TestClass]
    public class MapperTest
    {
        [TestMethod]
        public void TestFloatMapper () {
            var m = new FloatMapper() {
                x = new List<float> { 0, 1 },
                y = new List<float> { 0, 1 }
            };

            Assert.IsTrue(m.Eval(-10) == 0);
            Assert.IsTrue(m.Eval(0) == 0);
            Assert.IsTrue(m.Eval(0.5f) == 0.5f);
            Assert.IsTrue(m.Eval(1) == 1);
            Assert.IsTrue(m.Eval(10) == 1);
        }

        [TestMethod]
        public void TestStepMapper () {
            var m = new StepFunctionMapper() {
                x = new List<float> { 0, 1 },
                y = new List<float> { 0, 1 }
            };

            Assert.IsTrue(m.Eval(-10) == 0);
            Assert.IsTrue(m.Eval(0) == 0);
            Assert.IsTrue(m.Eval(0.5f) == 0);
            Assert.IsTrue(m.Eval(1) == 1);
            Assert.IsTrue(m.Eval(10) == 1);
        }

    }
}



