// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

namespace SomaSim.Util
{
    [TestClass]
    public class FactoryPoolTest
    {
        [TestMethod]
        public void TestAllocateAndFree () {
            StringBuilderPool.Reset();
            StringBuilderPool pool = StringBuilderPool.Instance;

            Assert.IsTrue(pool.UsedListSize == 0 && pool.FreeListSize == 0);

            // allocate some instances
            var sb1 = pool.Allocate();
            var sb2 = pool.Allocate();

            Assert.IsTrue(pool.UsedListSize == 2 && pool.FreeListSize == 0);
            Assert.IsTrue(sb1 != sb2);
            Assert.IsTrue(sb1.Length == 0);

            // free both and then allocate two more again - we should get both back

            Assert.IsTrue(pool.ProduceValueAndFree(sb1) == "");
            Assert.IsTrue(pool.ProduceValueAndFree(sb2) == "");
            Assert.IsTrue(pool.UsedListSize == 0 && pool.FreeListSize == 2);

            var sb3 = pool.Allocate();
            var sb4 = pool.Allocate();
            Assert.IsTrue(pool.UsedListSize == 2 && pool.FreeListSize == 0);
            Assert.IsTrue(sb3 == sb2);
            Assert.IsTrue(sb4 == sb1);

            StringBuilderPool.Reset();
        }

        [TestMethod]
        public void TestReset () {
            StringBuilderPool.Reset();
            StringBuilderPool pool = StringBuilderPool.Instance;

            // allocate a new string builder and make sure we use it
            var sb1 = pool.Allocate();
            Assert.IsTrue(sb1.Length == 0);
            sb1.Append("abc");
            Assert.IsTrue(sb1.Length == 3);

            // finalize it, and make sure it got reset
            Assert.IsTrue(pool.ProduceValueAndFree(sb1) == "abc");
            Assert.IsTrue(sb1.Length == 0);

            // now allocate again, this time since it's coming from the object pool, 
            // we'll get back the same instance
            var sb2 = pool.Allocate();
            Assert.IsTrue(sb1 == sb2);
            Assert.IsTrue(sb2.Length == 0); // and it's been reset

            StringBuilderPool.Reset();
        }

        [TestMethod]
        public void TestPoolRelease () {
            StringBuilderPool.Reset();
            StringBuilderPool pool = StringBuilderPool.Instance;

            // allocate a new instance using this custom factory, make sure the factory code got called
            var sb1 = pool.Allocate();
            sb1.Append("abc");
            Assert.IsTrue(pool.UsedListSize == 1);
            Assert.IsTrue(pool.FreeListSize == 0);

            // allocate and free an instance. this will grow the pool to size one
            Assert.IsTrue(pool.ProduceValueAndFree(pool.Allocate()) == "");
            Assert.IsTrue(pool.UsedListSize == 1);
            Assert.IsTrue(pool.FreeListSize == 1);

            // now release the entire object pool. this will clear out the free list, but will not
            // affect the object already in circulation, since we're not tracking them.
            pool.Release();
            Assert.IsTrue(sb1.Length == 3);
            Assert.IsTrue(pool.UsedListSize == 1);
            Assert.IsTrue(pool.FreeListSize == -1);

            StringBuilderPool.Reset();
        }
    }
}
