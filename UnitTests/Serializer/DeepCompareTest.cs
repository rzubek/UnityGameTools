// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SomaSim.SION
{
    [TestClass]
    public class DeepCompareTest
    {
        internal enum TestEnum
        {
            Zero = 0,
            One = 1,
        }

        internal struct TestStruct
        {
            public int i;
            public string s;
        }

        internal class SimpleTestClass
        {
            public int ifield;
            public string sfield;
            public SimpleTestClass next;
            public TestEnum en = TestEnum.Zero;

            public string sprop { get { return _sprop; } set { _sprop = value; } }
            private string _sprop;
        }

        internal class LargeTestClass
        {
            public int ifield;
            public List<int> ilist;
            public int[] iarray;
            public ArrayList arraylist;

            public Dictionary<int, string> dict;
            public Hashtable hash;
        }

        [TestMethod]
        public void TestComparePrimitives () {
            Assert.IsTrue(DeepCompare.DeepEquals(1, 1));
            Assert.IsTrue(DeepCompare.DeepEquals(1f, 1f));
            Assert.IsTrue(DeepCompare.DeepEquals("1", "1"));
            Assert.IsTrue(DeepCompare.DeepEquals('1', '1'));
            Assert.IsTrue(DeepCompare.DeepEquals(true, true));
            Assert.IsTrue(DeepCompare.DeepEquals(null, null));

            Assert.IsFalse(DeepCompare.DeepEquals(1, 2));
            Assert.IsFalse(DeepCompare.DeepEquals(1, 1f));
            Assert.IsFalse(DeepCompare.DeepEquals("1", '1'));
            Assert.IsFalse(DeepCompare.DeepEquals('1', 1));
            Assert.IsFalse(DeepCompare.DeepEquals(true, false));
            Assert.IsFalse(DeepCompare.DeepEquals(true, null));
        }

        [TestMethod]
        public void TestCompareSimpleClass () {

            var a = new SimpleTestClass() { ifield = 1, sfield = "2", sprop = "3", en = TestEnum.One, next = new SimpleTestClass() { ifield = 4 } };
            var b = new SimpleTestClass() { ifield = 1, sfield = "2", sprop = "3", en = TestEnum.One, next = new SimpleTestClass() { ifield = 4 } };

            Assert.IsTrue(DeepCompare.DeepEquals(a, a));
            Assert.IsTrue(DeepCompare.DeepEquals(a, b));

            var bad1 = new SimpleTestClass() { ifield = 1, sfield = "2", sprop = "3", next = new SimpleTestClass() { ifield = 0 } };
            var bad2 = new SimpleTestClass() { ifield = 1, sfield = "2", sprop = "3" };
            var bad3 = new SimpleTestClass() { ifield = 1, sfield = "2", next = new SimpleTestClass() { ifield = 0 } };
            var bad4 = new SimpleTestClass() { ifield = 1, sfield = "2", sprop = "3", en = TestEnum.One, next = new SimpleTestClass() { ifield = 0 } };

            Assert.IsFalse(DeepCompare.DeepEquals(a, bad1));
            Assert.IsFalse(DeepCompare.DeepEquals(a, bad2));
            Assert.IsFalse(DeepCompare.DeepEquals(a, bad3));
            Assert.IsFalse(DeepCompare.DeepEquals(a, bad4));
            Assert.IsFalse(DeepCompare.DeepEquals(a, null));
        }

        [TestMethod]
        public void TestCompareArrays () {
            int[] a = new int[] { 1, 2, 3 };

            Assert.IsTrue(DeepCompare.DeepEquals(a, new int[] { 1, 2, 3 }));

            Assert.IsFalse(DeepCompare.DeepEquals(a, new int[] { 1, 2 }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, new int[] { 1, 2, 3, 4 }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, new List<int>() { 1, 2, 3 }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, null));
        }

        [TestMethod]
        public void TestCompareIEnumerables () {
            List<int> a = new List<int> { 1, 2, 3 };

            Assert.IsTrue(DeepCompare.DeepEquals(a, new List<int> { 1, 2, 3 }));

            Assert.IsFalse(DeepCompare.DeepEquals(a, new List<int> { 1, 2 }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, new List<int> { 1, 2, 3, 4 }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, new int[] { 1, 2, 3 }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, new ArrayList() { 1, 2, 3 }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, null));


            ArrayList b = new ArrayList() { 1, 2, 3 };

            Assert.IsTrue(DeepCompare.DeepEquals(b, new ArrayList() { 1, 2, 3 }));

            Assert.IsFalse(DeepCompare.DeepEquals(b, new List<int> { 1, 2, 3 }));


            ArrayList c = new ArrayList() { 1, "1", new char[] { 'a', 'b' }, null };

            Assert.IsTrue(DeepCompare.DeepEquals(c, new ArrayList() { 1, "1", new char[] { 'a', 'b' }, null }));

            Assert.IsFalse(DeepCompare.DeepEquals(c, new ArrayList() { 1, "1", new char[] { 'a' }, null }));
            Assert.IsFalse(DeepCompare.DeepEquals(c, new ArrayList() { 1, "1", new ArrayList() { 'a', 'b' }, null }));
        }

        [TestMethod]
        public void TestCompareIDictionaries () {
            Dictionary<string, int> a = new Dictionary<string, int>() { { "one", 1 }, { "two", 2 } };

            Assert.IsTrue(DeepCompare.DeepEquals(a, new Dictionary<string, int>() { { "one", 1 }, { "two", 2 } }));

            Assert.IsFalse(DeepCompare.DeepEquals(a, new Dictionary<string, int>() { { "one", 1 } }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, new Dictionary<string, int>() { { "one", 1 }, { "two", 2 }, { "three", 3 } }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, new Dictionary<string, int>() { { "one", 1 }, { "foo", 2 } }));


            Dictionary<string, List<int>> b = new Dictionary<string, List<int>>() { { "one", new List<int>() { 1 } }, { "two", new List<int>() { 2 } } };

            Assert.IsTrue(DeepCompare.DeepEquals(b, new Dictionary<string, List<int>>() { { "one", new List<int>() { 1 } }, { "two", new List<int>() { 2 } } }));

            Assert.IsFalse(DeepCompare.DeepEquals(b, new Dictionary<string, List<int>>() { { "one", new List<int>() { 0 } }, { "two", new List<int>() { 2 } } }));
            Assert.IsFalse(DeepCompare.DeepEquals(b, new Dictionary<string, List<int>>() { { "one", new List<int>() { 1 } }, { "foo", new List<int>() { 2 } } }));
            Assert.IsFalse(DeepCompare.DeepEquals(a, b));

        }

        [TestMethod]
        public void TestCompareLargeClass () {
            Func<LargeTestClass> factory = () => {
                return new LargeTestClass() {
                    ifield = 42,
                    ilist = new List<int>() { 1, 2, 3 },
                    arraylist = new ArrayList() { 'a', 'b', 'c' },
                    iarray = new int[] { 0, 1, 2 },
                    hash = new Hashtable() { { "foo", "bar" }, { 1, 2 }, { "test", new TestStruct() { i = 1, s = "one" } } },
                    dict = new Dictionary<int, string>() { { 1, "one" }, { 2, "two" } }
                };
            };

            var a = factory();

            Assert.IsTrue(DeepCompare.DeepEquals(a, factory()));

            var bad = factory();
            bad.ilist[2] = 42;
            Assert.IsFalse(DeepCompare.DeepEquals(a, bad));

            bad = factory();
            bad.hash["test"] = new TestStruct() { i = 42, s = "fortytwo" };
            Assert.IsFalse(DeepCompare.DeepEquals(a, bad));

        }
    }
}
