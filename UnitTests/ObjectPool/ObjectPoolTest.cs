using System;

namespace SomaSim.Utils
{
    public class TestElement : IObjectPoolElement
    {
        public int value;

        public void Reset () {
            value = 0;
        }
    }

    [TestClass]
    public class ObjectPoolTest
    {
        [TestMethod]
        public void TestAllocateAndFree () {
            var pool = new ObjectPool<TestElement>();
            pool.Initialize();

            Assert.IsTrue(pool.UsedListSize == 0 && pool.FreeListSize == 0);

            // allocate some instances
            var e1 = pool.Allocate();
            var e2 = pool.Allocate();

            Assert.IsTrue(pool.UsedListSize == 2 && pool.FreeListSize == 0);
            Assert.IsTrue(e1 != e2);
            Assert.IsTrue(e1.value == 0);

            // free both and then allocate two more again - we should get both back

            pool.Free(e1);
            pool.Free(e2);
            Assert.IsTrue(pool.UsedListSize == 0 && pool.FreeListSize == 2);

            var e3 = pool.Allocate();
            var e4 = pool.Allocate();
            Assert.IsTrue(pool.UsedListSize == 2 && pool.FreeListSize == 0);
            Assert.IsTrue(e3 == e2);
            Assert.IsTrue(e4 == e1);
        }

        [TestMethod]
        public void TestFactoryAndReset () {
            var pool = new ObjectPool<TestElement>();
            pool.Initialize(() => { var e = new TestElement(); e.value = 42; return e; });

            // allocate a new instance using this custom factory, make sure the factory code got called
            var e1 = pool.Allocate();
            Assert.IsTrue(e1.value == 42);

            // free it, and make sure it got reset
            pool.Free(e1);
            Assert.IsTrue(e1.value == 0);

            // now allocate again, this time since it's coming from the object pool, 
            // the factory code won't be called
            var e2 = pool.Allocate();
            Assert.IsTrue(e1 == e2);
            Assert.IsTrue(e2.value == 0); // because this is coming from the pool, not factory
        }

        [TestMethod]
        public void TestPoolRelease () {
            var pool = new ObjectPool<TestElement>();
            pool.Initialize(() => { var e = new TestElement(); e.value = 42; return e; });

            // allocate a new instance using this custom factory, make sure the factory code got called
            var e1 = pool.Allocate();
            Assert.IsTrue(e1.value == 42);
            Assert.IsTrue(pool.UsedListSize == 1);
            Assert.IsTrue(pool.FreeListSize == 0);

            // allocate and free an instance. this will grow the pool to size one
            pool.Free(pool.Allocate());
            Assert.IsTrue(pool.UsedListSize == 1);
            Assert.IsTrue(pool.FreeListSize == 1);

            // now release the entire object pool. this will clear out the free list, but will not
            // affect the object already in circulation, since we're not tracking them.
            pool.Release();
            Assert.IsTrue(e1.value == 42);
            Assert.IsTrue(pool.UsedListSize == 1);
            Assert.IsTrue(pool.FreeListSize == -1);
        }
    }
}
