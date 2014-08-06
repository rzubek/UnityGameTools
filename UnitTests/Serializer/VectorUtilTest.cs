using Microsoft.VisualStudio.TestTools.UnitTesting;
using SomaSim.Math;
using UnityEngine;

namespace SomaSim.Serializer
{
    [TestClass]
    public class VectorUtilTest
    {
        [TestMethod]
        public void TestDistance () {
            Vector2 a = new Vector2(0, 0);
            Vector2 b = new Vector2(3, 4);

            Assert.IsTrue(VectorUtil.Distance(a, b, 1) == 7); // taxicab
            Assert.IsTrue(VectorUtil.Distance(a, b, 2) == 5); // euclidean
            Assert.IsTrue(VectorUtil.Distance(a, b, float.PositiveInfinity) == 4); // chebyshev

            Assert.IsTrue(VectorUtil.Distance(a, b) == 5); // euclidean by default
        }
    }
}
