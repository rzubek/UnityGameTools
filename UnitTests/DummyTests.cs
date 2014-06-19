using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SomaSim
{
    [TestClass]
    public class DummyTests

    {
        [TestMethod]
        public void TestValue()
        {
            Assert.AreEqual(4, Dummy.Value);
        }
    }
}
