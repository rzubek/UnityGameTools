using UnityEngine;

namespace SomaSim.Util
{
    [TestClass]
    public class VectorUtilTest
    {
        [TestMethod]
        public void TestDistanceVector () {
            Vector2 a = new Vector2(0, 0);
            Vector2 b = new Vector2(3, 4);

            Assert.IsTrue(Vector2.Distance(a, b) == 5); // standard euclidean

            Assert.IsTrue(VectorUtil.DistanceTaxicab(a, b) == 7); // taxicab
            Assert.IsTrue(VectorUtil.DistanceSquared(a, b) == 25); // 5 * 5
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
