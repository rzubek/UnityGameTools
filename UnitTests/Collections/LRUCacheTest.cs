// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

namespace SomaSim.Util
{
    [TestClass]
    public class LRUCacheTest
    {
        [TestMethod]
        public void TestLRUCache () {

            var cache = new LRUCache<string, int>(3); // make a tiny cache of three elements

            Assert.IsTrue(cache.Capacity == 3 && cache.Count == 0);

            // add three elements

            Assert.IsFalse(cache.ContainsKey("one"));
            Assert.IsTrue(cache.Get("one") == 0);
            cache.Add("one", 1);
            Assert.IsTrue(cache.ContainsKeyAt("one", 0));
            Assert.IsTrue(cache.Get("one") == 1);
            Assert.IsTrue(cache.Count == 1);

            Assert.IsFalse(cache.ContainsKey("two"));
            Assert.IsTrue(cache.Get("two") == 0);
            cache.Add("two", 2);
            Assert.IsTrue(cache.ContainsKeyAt("two", 1));
            Assert.IsTrue(cache.Get("two") == 2);
            Assert.IsTrue(cache.Count == 2);

            Assert.IsFalse(cache.ContainsKey("three"));
            Assert.IsTrue(cache.Get("three") == 0);
            cache.Add("three", 3);
            Assert.IsTrue(cache.ContainsKeyAt("three", 2));
            Assert.IsTrue(cache.Get("three") == 3);
            Assert.IsTrue(cache.Count == 3);

            // we're at capacity. if we add another element, 
            // "one" will get evicted since it's least recently used

            Assert.IsTrue(cache.Count == cache.Capacity);

            cache.Add("four", 4);
            Assert.IsTrue(cache.Get("four") == 4);

            Assert.IsTrue(cache.Count == 3);
            Assert.IsTrue(cache.ContainsKeyAt("four", 2));  // from the youngest
            Assert.IsTrue(cache.ContainsKeyAt("three", 1)); // ...
            Assert.IsTrue(cache.ContainsKeyAt("two", 0));   // to the oldest
            Assert.IsFalse(cache.ContainsKey("one"));

            // now let's touch "two" because that's the least recently used one.
            // by doing that, we demote "three" to be the least recently used one,
            // and adding a new entry will then evict it.

            Assert.IsTrue(cache.Get("two") == 2); // reading the key will touch it
            Assert.IsTrue(cache.ContainsKeyAt("two", 2));   // now two is the youngest
            Assert.IsTrue(cache.ContainsKeyAt("four", 1));  // ...
            Assert.IsTrue(cache.ContainsKeyAt("three", 0)); // and three is the oldest

            Assert.IsTrue(cache.Count == cache.Capacity);

            cache.Add("five", 5);
            Assert.IsTrue(cache.Get("five") == 5);

            Assert.IsTrue(cache.Count == 3);
            Assert.IsTrue(cache.ContainsKeyAt("five", 2)); // youngest
            Assert.IsTrue(cache.ContainsKeyAt("two", 1));  // ...
            Assert.IsTrue(cache.ContainsKeyAt("four", 0)); // oldest
            Assert.IsFalse(cache.ContainsKey("three")); // evicted as lru


            // finally we remove one item, dropping the count.
            // adding another item will not cause evictions

            Assert.IsTrue(cache.Remove("four"));
            Assert.IsFalse(cache.ContainsKey("four"));
            Assert.IsTrue(cache.Get("four") == 0);

            Assert.IsTrue(cache.Count == cache.Capacity - 1);

            cache.Add("six", 6);
            Assert.IsTrue(cache.Get("six") == 6);

            Assert.IsTrue(cache.Count == 3);
            Assert.IsTrue(cache.ContainsKeyAt("six", 2));  // youngest
            Assert.IsTrue(cache.ContainsKeyAt("five", 1)); // ...
            Assert.IsTrue(cache.ContainsKeyAt("two", 0));  // oldest
            Assert.IsFalse(cache.ContainsKey("four"));   // removed manually

            // test clearing

            cache.Clear();
            Assert.IsTrue(cache.Count == 0);
        }
    }
}
