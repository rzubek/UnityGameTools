using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Collections
{
    [TestClass]
    public class LRUCacheTest
    {
        [TestMethod]
        public void TestLRUCache () {

            var cache = new LRUCache<string, int>(3); // make a tiny cache

            Assert.IsTrue(cache.Capacity == 3 && cache.Count == 0);

            Assert.IsFalse(cache.ContainsKey("one"));
            Assert.IsTrue(cache.Get("one") == 0);
            cache.Add("one", 1);
            Assert.IsTrue(cache.ContainsKey("one"));
            Assert.IsTrue(cache.Get("one") == 1);
            Assert.IsTrue(cache.Count == 1);

            Assert.IsFalse(cache.ContainsKey("two"));
            Assert.IsTrue(cache.Get("two") == 0);
            cache.Add("two", 2);
            Assert.IsTrue(cache.ContainsKey("two"));
            Assert.IsTrue(cache.Get("two") == 2);
            Assert.IsTrue(cache.Count == 2);

            Assert.IsFalse(cache.ContainsKey("three"));
            Assert.IsTrue(cache.Get("three") == 0);
            cache.Add("three", 3);
            Assert.IsTrue(cache.ContainsKey("three"));
            Assert.IsTrue(cache.Get("three") == 3);
            Assert.IsTrue(cache.Count == 3);

            // we're at capacity. if we add another element, 
            // "one" will get evicted since it's least recently used

            Assert.IsTrue(cache.Count == cache.Capacity);

            cache.Add("four", 4);
            Assert.IsTrue(cache.Get("four") == 4);

            Assert.IsTrue(cache.Count == 3);
            Assert.IsTrue(cache.ContainsKey("four"));
            Assert.IsTrue(cache.ContainsKey("three"));
            Assert.IsTrue(cache.ContainsKey("two"));
            Assert.IsFalse(cache.ContainsKey("one"));

            // now let's touch "two" because that's the least recently used one.
            // by doing that, we promote "three" to be the least recently used one,
            // and adding a new entry will then evict it.

            Assert.IsTrue(cache.Get("two") == 2); // touch the oldest one

            Assert.IsTrue(cache.Count == cache.Capacity);

            cache.Add("five", 5);
            Assert.IsTrue(cache.Get("five") == 5);

            Assert.IsTrue(cache.Count == 3);
            Assert.IsTrue(cache.ContainsKey("five"));
            Assert.IsTrue(cache.ContainsKey("four"));
            Assert.IsFalse(cache.ContainsKey("three")); // evicted as lru
            Assert.IsTrue(cache.ContainsKey("two"));    // still there

            // finally we remove one item, dropping the count.
            // adding another item will not cause evictions

            Assert.IsTrue(cache.Remove("four"));
            Assert.IsFalse(cache.ContainsKey("four"));
            Assert.IsTrue(cache.Get("four") == 0);

            Assert.IsTrue(cache.Count == cache.Capacity - 1);

            cache.Add("six", 6);
            Assert.IsTrue(cache.Get("six") == 6);

            Assert.IsTrue(cache.Count == 3);
            Assert.IsTrue(cache.ContainsKey("six"));
            Assert.IsTrue(cache.ContainsKey("five"));
            Assert.IsFalse(cache.ContainsKey("four"));   // removed manually
            Assert.IsTrue(cache.ContainsKey("two"));    // still there

            // test clearing

            cache.Clear();
            Assert.IsTrue(cache.Count == 0);

        }
    }
}
