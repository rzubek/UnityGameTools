using Microsoft.VisualStudio.TestTools.UnitTesting;
using SomaSim.Math;
using UnityEngine;

namespace SomaSim.Math
{
    [TestClass]
    public class VectorUtilTest
    {
        [TestMethod]
        public void TestDistanceVector () {
            Vector2 a = new Vector2(0, 0);
            Vector2 b = new Vector2(3, 4);

            Assert.IsTrue(VectorUtil.Distance(a, b, 1) == 7); // taxicab
            Assert.IsTrue(VectorUtil.Distance(a, b, 2) == 5); // euclidean
            Assert.IsTrue(VectorUtil.Distance(a, b, float.PositiveInfinity) == 4); // chebyshev

            Assert.IsTrue(VectorUtil.Distance(a, b) == 5); // euclidean by default
        }

        [TestMethod]
        public void TestDistancePoint () {
            Vector2 a = new Vector2(0, 0);
            Vector2 b = new Vector2(3, 4);

            Assert.IsTrue(VectorUtil.Distance(a.x, a.y, b.x, b.y, 1) == 7); // taxicab
            Assert.IsTrue(VectorUtil.Distance(a.x, a.y, b.x, b.y, 2) == 5); // euclidean
            Assert.IsTrue(VectorUtil.Distance(a.x, a.y, b.x, b.y, float.PositiveInfinity) == 4); // chebyshev

            Assert.IsTrue(VectorUtil.Distance(a.x, a.y, b.x, b.y) == 5); // euclidean by default
        }
    }

    [TestClass]
    public class ColorUtilTest
    {
        [TestMethod]
        public void TestColorToHexToColor () {
            Color c = new Color32(0, 128, 255, 0);

            string h = ColorUtil.ColorToHex(c);
            Assert.IsTrue(h == "0080FF");

            Color32 c2 = ColorUtil.HexToColor(h);
            Assert.IsTrue(c2.r == 0 && c2.g == 128 && c2.b == 255);
            Assert.IsTrue(c2.a == 255); // we lost alpha, as expected
        }
    }
}
