using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game.Util;

namespace SomaSim.Math
{
    [TestClass]
    public class MathUtilTest
    {
        [TestMethod]
        public void TestQuantize () {
            // floats
            Assert.IsTrue(MathUtil.Quantize(-10f, 5f) == -10f);
            Assert.IsTrue(MathUtil.Quantize(-5.01f, 5f) == -10f);
            Assert.IsTrue(MathUtil.Quantize(-5f, 5f) == -5f);
            Assert.IsTrue(MathUtil.Quantize(-0.01f, 5f) == -5f);
            Assert.IsTrue(MathUtil.Quantize(0f, 5f) == 0f);
            Assert.IsTrue(MathUtil.Quantize(4.99f, 5f) == 0f);
            Assert.IsTrue(MathUtil.Quantize(5f, 5f) == 5f);
            Assert.IsTrue(MathUtil.Quantize(9.99f, 5f) == 5f);

            // ints
            Assert.IsTrue(MathUtil.Quantize(-10, 5) == -10);
            Assert.IsTrue(MathUtil.Quantize(-6, 5) == -10);
            Assert.IsTrue(MathUtil.Quantize(-5, 5) == -5);
            Assert.IsTrue(MathUtil.Quantize(-1, 5) == -5);
            Assert.IsTrue(MathUtil.Quantize(0, 5) == 0);
            Assert.IsTrue(MathUtil.Quantize(4, 5) == 0);
            Assert.IsTrue(MathUtil.Quantize(5, 5) == 5);
            Assert.IsTrue(MathUtil.Quantize(9, 5) == 5);
        }

        [TestMethod]
        public void TestClamp () {
            // floats
            Assert.IsTrue(MathUtil.Clamp(-1f, 0f, 1f) == 0f);
            Assert.IsTrue(MathUtil.Clamp(0f, 0f, 1f) == 0f);
            Assert.IsTrue(MathUtil.Clamp(0.5f, 0f, 1f) == 0.5f);
            Assert.IsTrue(MathUtil.Clamp(1f, 0f, 1f) == 1f);
            Assert.IsTrue(MathUtil.Clamp(1.1f, 0f, 1f) == 1f);

            // ints
            Assert.IsTrue(MathUtil.Clamp(-1, 0, 10) == 0);
            Assert.IsTrue(MathUtil.Clamp(0, 0, 10) == 0);
            Assert.IsTrue(MathUtil.Clamp(5, 0, 10) == 5);
            Assert.IsTrue(MathUtil.Clamp(10, 0, 10) == 10);
            Assert.IsTrue(MathUtil.Clamp(11, 0, 10) == 10);
        }

        [TestMethod]
        public void TestEvenOdd () {
            Assert.IsTrue(MathUtil.IsEven(-2));
            Assert.IsTrue(MathUtil.IsEven(0));
            Assert.IsTrue(MathUtil.IsEven((uint)2));
            Assert.IsTrue(MathUtil.IsEven(2f));
            Assert.IsTrue(MathUtil.IsEven(2.1f));
            Assert.IsFalse(MathUtil.IsOdd(-2));
            Assert.IsFalse(MathUtil.IsOdd(0));
            Assert.IsFalse(MathUtil.IsOdd((uint)2));
            Assert.IsFalse(MathUtil.IsOdd(2f));
            Assert.IsFalse(MathUtil.IsOdd(2.1f));

            Assert.IsTrue(MathUtil.IsOdd(-1));
            Assert.IsTrue(MathUtil.IsOdd(1));
            Assert.IsTrue(MathUtil.IsOdd((uint)1));
            Assert.IsTrue(MathUtil.IsOdd(1f));
            Assert.IsTrue(MathUtil.IsOdd(1.1f));
            Assert.IsFalse(MathUtil.IsEven(-1));
            Assert.IsFalse(MathUtil.IsEven(1));
            Assert.IsFalse(MathUtil.IsEven((uint)1));
            Assert.IsFalse(MathUtil.IsEven(1f));
            Assert.IsFalse(MathUtil.IsEven(1.1f));
        }

        [TestMethod]
        public void TestInterpolateAndInverse () {
            Assert.IsTrue(MathUtil.Interpolate(  0f, -10, 10) == -10);
            Assert.IsTrue(MathUtil.Interpolate(0.5f, -10, 10) == 0);
            Assert.IsTrue(MathUtil.Interpolate(  1f, -10, 10) == 10);

            Assert.IsTrue(MathUtil.Uninterpolate(-10, -10, 10) == 0f);
            Assert.IsTrue(MathUtil.Uninterpolate(  0, -10, 10) == 0.5f);
            Assert.IsTrue(MathUtil.Uninterpolate( 10, -10, 10) == 1f);
        }
    }
}
