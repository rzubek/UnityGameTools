using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Collections
{
    [TestClass]
    public class DequeTest
    {

        [TestMethod]
        public void TestAddAndGrow () {

            var d = new Deque<int>(0);
            Assert.IsTrue(d.Count == 0);
            Assert.IsTrue(d.Capacity == 0);

            d.AddFirst(1);
            Assert.IsTrue(d.Count == 1);
            Assert.IsTrue(d.Capacity == 4);

            d.AddFirst(0);
            Assert.IsTrue(d.Count == 2);
            Assert.IsTrue(d.Capacity == 4);

            d.AddLast(2);
            d.AddLast(3);
            Assert.IsTrue(d.Count == 4);
            Assert.IsTrue(d.Capacity == 4);

            d.AddLast(4);
            Assert.IsTrue(d.Count == 5);
            Assert.IsTrue(d.Capacity == 8);

            var arr = d.ToArray();
            var exp = new int[] { 0, 1, 2, 3, 4 };
            Assert.IsTrue(arr.Length == exp.Length);
            for (int i = 0; i < arr.Length; i++) {
                Assert.IsTrue(arr[i] == exp[i]);
            }
        }


        [TestMethod]
        public void TestRemoveAndTrim () {

            var d = new Deque<int>(4);
            Assert.IsTrue(d.Count == 0);
            Assert.IsTrue(d.Capacity == 4);

            d.AddLast(0);
            d.AddLast(1);
            d.AddLast(2);
            d.AddLast(3);
            d.AddLast(4);
            d.AddLast(5);
            Assert.IsTrue(d.Count == 6);
            Assert.IsTrue(d.Capacity == 8);

            d.RemoveLast();
            d.RemoveLast();
            d.RemoveLast();
            Assert.IsTrue(d.Count == 3);
            Assert.IsTrue(d.Capacity == 8);

            d.TrimExcess();
            Assert.IsTrue(d.Count == 3);
            Assert.IsTrue(d.Capacity == 3);

            var arr = d.ToArray();
            var exp = new int[] { 0, 1, 2 };
            Assert.IsTrue(arr.Length == exp.Length);
            for (int i = 0; i < arr.Length; i++) {
                Assert.IsTrue(arr[i] == exp[i]);
            }
        }

        [TestMethod]
        public void TestValuesAndWrapAround () {

            var d = new Deque<int>(4);

            d.AddLast(1);   // internally it contains [H:1, 2, 3, 4]
            d.AddLast(2);
            d.AddLast(3);
            d.AddLast(4);
            Assert.IsTrue(d.PeekFirst() == 1 & d.PeekLast() == 4);

            d.RemoveFirst();
            d.RemoveLast();  // now it's [0, H:2, 3, 0]
            Assert.IsTrue(d.Count == 2 && d.Capacity == 4);
            Assert.IsTrue(d.PeekFirst() == 2 & d.PeekLast() == 3);

            d.AddLast(4);
            d.RemoveFirst(); // now it's [0, 0, H:3, 4]
            Assert.IsTrue(d.Count == 2 && d.Capacity == 4);
            Assert.IsTrue(d.PeekFirst() == 3 & d.PeekLast() == 4);

            d.AddLast(5);
            d.RemoveFirst(); // now it's [5, 0, 0, H:4]
            Assert.IsTrue(d.Count == 2 && d.Capacity == 4);
            Assert.IsTrue(d.PeekFirst() == 4 & d.PeekLast() == 5);

            d.AddFirst(3);
            d.AddFirst(2);   // now it's [5, H:2, 3, 4]
            Assert.IsTrue(d.Count == 4 && d.Capacity == 4);
            Assert.IsTrue(d.PeekFirst() == 2 & d.PeekLast() == 5);

            d.AddFirst(1);   // reallocated to [H:1, 2, 3, 4, 5, 0, 0, 0]
            d.AddFirst(0);   // now it's [1, 2, 3, 4, 5, 0, 0, H:0]
            Assert.IsTrue(d.Count == 6 && d.Capacity == 8);
            Assert.IsTrue(d.PeekFirst() == 0 & d.PeekLast() == 5);

            var arr = d.ToArray();
            var exp = new int[] { 0, 1, 2, 3, 4, 5 };
            Assert.IsTrue(arr.Length == exp.Length);
            for (int i = 0; i < arr.Length; i++) {
                Assert.IsTrue(arr[i] == exp[i]);
            }
        }

        [TestMethod]
        public void TestEnumerator () {

            var exp = new int[] { 0, 1, 2, 3, 4, 5 };
            var d = new Deque<int>(4);
            d.AddLast(0);
            d.AddLast(1);
            d.AddLast(2);
            d.AddLast(3);
            d.AddLast(4);
            d.AddLast(5);

            int index = 0;
            foreach (var i in d) {
                Assert.IsTrue(i == exp[index++]);
            }
            Assert.IsTrue(index == 6);

            index = 0;
            var en = d.GetEnumerator();
            while (en.MoveNext()) {
                Assert.IsTrue(en.Current == exp[index++]);
            }
            Assert.IsTrue(index == 6);
        }
    }
}
