// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

namespace SomaSim.Util
{
    [TestClass]
    public class FixnumTest
    {
        [TestMethod]
        public void TestScaling () {
            Fixnum num = 123;
            Assert.IsTrue(num.scaled == 12300);
            Assert.IsTrue(num.scaled == 123 * Fixnum.SCALE);

            Fixnum num2 = (Fixnum) 123.45f;
            Assert.IsTrue(num2.scaled == 12345);

            // check rounding
            Fixnum num3 = (Fixnum) 123.456f;
            Assert.IsTrue(num3.scaled == 12346);
        }

        [TestMethod]
        public void TestArithmetic () {
            Fixnum a = 100, b = (Fixnum) 1.2f;

            Assert.IsTrue(a == (Fixnum) 100);
            Assert.IsTrue(b == (Fixnum) 1.2f);

            Assert.IsTrue(a + b == (Fixnum) 101.2f);
            Assert.IsTrue(((float) (a + b)) == 101.2f);
            Assert.IsTrue((a + b).scaled == 10120);

            Assert.IsTrue((a * b) == 120);
            Assert.IsTrue(((float) (a * b)) == 120.0f);
            Assert.IsTrue((a * b).scaled == 12000);
        }
    }
}
