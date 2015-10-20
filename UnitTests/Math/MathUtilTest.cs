using System;
using Game.Util;

namespace SomaSim.Math
{
    [TestClass]
    public class MathUtilTest
    {
        [TestMethod]
        public void TestSnapDown () {
            // floats
            Assert.IsTrue(MathUtil.SnapDown(-10f, 5f) == -10f);
            Assert.IsTrue(MathUtil.SnapDown(-5.01f, 5f) == -10f);
            Assert.IsTrue(MathUtil.SnapDown(-5f, 5f) == -5f);
            Assert.IsTrue(MathUtil.SnapDown(-0.01f, 5f) == -5f);
            Assert.IsTrue(MathUtil.SnapDown(0f, 5f) == 0f);
            Assert.IsTrue(MathUtil.SnapDown(4.99f, 5f) == 0f);
            Assert.IsTrue(MathUtil.SnapDown(5f, 5f) == 5f);
            Assert.IsTrue(MathUtil.SnapDown(9.99f, 5f) == 5f);

            // ints
            Assert.IsTrue(MathUtil.SnapDown(-10, 5) == -10);
            Assert.IsTrue(MathUtil.SnapDown(-6, 5) == -10);
            Assert.IsTrue(MathUtil.SnapDown(-5, 5) == -5);
            Assert.IsTrue(MathUtil.SnapDown(-1, 5) == -5);
            Assert.IsTrue(MathUtil.SnapDown(0, 5) == 0);
            Assert.IsTrue(MathUtil.SnapDown(4, 5) == 0);
            Assert.IsTrue(MathUtil.SnapDown(5, 5) == 5);
            Assert.IsTrue(MathUtil.SnapDown(9, 5) == 5);
        }

        [TestMethod]
        public void TestSnapUp () {
            // floats
            Assert.IsTrue(MathUtil.SnapUp(-10f, 5f) == -10f);
            Assert.IsTrue(MathUtil.SnapUp(-9.99f, 5f) == -5f);
            Assert.IsTrue(MathUtil.SnapUp(-5f, 5f) == -5f);
            Assert.IsTrue(MathUtil.SnapUp(-4.99f, 5f) == 0f);
            Assert.IsTrue(MathUtil.SnapUp(-0.01f, 5f) == 0f);
            Assert.IsTrue(MathUtil.SnapUp(0f, 5f) == 0f);
            Assert.IsTrue(MathUtil.SnapUp(0.01f, 5f) == 5f);
            Assert.IsTrue(MathUtil.SnapUp(5f, 5f) == 5f);
            Assert.IsTrue(MathUtil.SnapUp(5.01f, 5f) == 10f);

            // ints
            Assert.IsTrue(MathUtil.SnapUp(-10, 5) == -10);
            Assert.IsTrue(MathUtil.SnapUp(-9, 5) == -5);
            Assert.IsTrue(MathUtil.SnapUp(-5, 5) == -5);
            Assert.IsTrue(MathUtil.SnapUp(-4, 5) == 0);
            Assert.IsTrue(MathUtil.SnapUp(0, 5) == 0);
            Assert.IsTrue(MathUtil.SnapUp(1, 5) == 5);
            Assert.IsTrue(MathUtil.SnapUp(5, 5) == 5);
            Assert.IsTrue(MathUtil.SnapUp(6, 5) == 10);
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

        [TestMethod]
        public void TestModulus () {
            Assert.IsTrue(MathUtil.Modulus(5, 3) == 2);
            Assert.IsTrue(MathUtil.Modulus(4, 3) == 1);
            Assert.IsTrue(MathUtil.Modulus(3, 3) == 0);
            Assert.IsTrue(MathUtil.Modulus(2, 3) == 2);
            Assert.IsTrue(MathUtil.Modulus(1, 3) == 1);
            Assert.IsTrue(MathUtil.Modulus(0, 3) == 0);
            Assert.IsTrue(MathUtil.Modulus(-1, 3) == 2);
            Assert.IsTrue(MathUtil.Modulus(-2, 3) == 1);
            Assert.IsTrue(MathUtil.Modulus(-3, 3) == 0);
            Assert.IsTrue(MathUtil.Modulus(-4, 3) == 2);
        }

        [TestMethod]
        public void TestCountIntervals () {
            Assert.IsTrue(MathUtil.CountIntervals(3, 4, 5) == 0);
            Assert.IsTrue(MathUtil.CountIntervals(3, 5, 5) == 1);
            Assert.IsTrue(MathUtil.CountIntervals(5, 9, 5) == 0);
            Assert.IsTrue(MathUtil.CountIntervals(5, 10, 5) == 1);
            Assert.IsTrue(MathUtil.CountIntervals(5, 11, 5) == 1);
            Assert.IsTrue(MathUtil.CountIntervals(3, 10, 5) == 2);

            Assert.IsTrue(MathUtil.CountIntervals(3f, 4.9999f, 5f) == 0);
            Assert.IsTrue(MathUtil.CountIntervals(3f, 5f, 5f) == 1);
            Assert.IsTrue(MathUtil.CountIntervals(5f, 9.99f, 5f) == 0);
            Assert.IsTrue(MathUtil.CountIntervals(5f, 10f, 5f) == 1);
            Assert.IsTrue(MathUtil.CountIntervals(5f, 11f, 5f) == 1);
            Assert.IsTrue(MathUtil.CountIntervals(3f, 10f, 5f) == 2);
        }
    }
}
