using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SomaSim.Collections
{
    [TestClass]
    public class SmartQueueTest
    {
        internal class TestEntry : AbstractSmartQueueElement
        {
            public int activationCount { get; private set; }
            public int deactivationCount { get; private set; }
            public int enqueuedCount { get; private set; }
            public int dequeuedCount { get; private set; }

            public int pushedBackCount { get; private set; }
            public int poppedTailCount { get; private set; }

            public SmartQueue<TestEntry> container { get { return this.Queue as SmartQueue<TestEntry>; } }

            public override void OnEnqueued (object queue) {
                base.OnEnqueued(queue);
                enqueuedCount++;
            }

            public override void OnActivated () {
                base.OnActivated();
                activationCount++;
            }

            public override void OnDeactivated (bool pushedback) {
                if (pushedback) { pushedBackCount++; }
                deactivationCount++;
                base.OnDeactivated(pushedback);
            }

            public override void OnDequeued (bool poppedtail) {
                if (poppedtail) { poppedTailCount++; }
                dequeuedCount++;
                base.OnDequeued(poppedtail);
            }
        }

        [TestMethod]
        public void TestSmartQueueEnqueueAndDequeue () {

            SmartQueue<TestEntry> q = new SmartQueue<TestEntry>();
            Assert.IsTrue(q.Empty && q.Count == 0 && q.Head == null);

            // make an empty something
            TestEntry e1 = new TestEntry();
            Assert.IsTrue(e1.enqueuedCount == 0 && e1.dequeuedCount == 0 && e1.activationCount == 0 && e1.deactivationCount == 0);
            Assert.IsNull(e1.container);
            Assert.IsFalse(e1.IsHead);
            Assert.IsFalse(e1.IsEnqueued);

            // enqueue it. make sure it's there, and got notified appropriately
            q.Enqueue(e1);
            Assert.IsTrue(q.Head == e1 && q.Tail == e1 && q.Count == 1 && !q.Empty);
            Assert.IsTrue(e1.enqueuedCount == 1 && e1.activationCount == 1);
            Assert.IsTrue(e1.IsEnqueued);
            Assert.IsTrue(e1.IsHead);

            // enqueue something else behind it. the first one should be unaffected
            TestEntry e2 = new TestEntry();
            q.Enqueue(e2);
            Assert.IsTrue(q.Head == e1 && q.Tail == e2 && q.Count == 2 && !q.Empty);
            Assert.IsTrue(e1.enqueuedCount == 1 && e1.activationCount == 1); // same as before
            Assert.IsTrue(e1.IsEnqueued);
            Assert.IsTrue(e1.IsHead); 
            // but the second one shouldn't be active yet
            Assert.IsTrue(e2.enqueuedCount == 1 && e2.activationCount == 0); // enq'd, inactive
            Assert.IsTrue(e2.IsEnqueued);
            Assert.IsFalse(e2.IsHead);

            // dequeue e1, verify it cleaned up
            var popped = q.Dequeue();
            Assert.IsTrue(popped == e1);
            Assert.IsTrue(q.Head == e2 && q.Count == 1 && !q.Empty);
            Assert.IsTrue(e1.enqueuedCount == 1 && e1.activationCount == 1); // activations unchanged
            Assert.IsTrue(e1.deactivationCount == 1 && e1.dequeuedCount == 1); // deactivations updated
            Assert.IsFalse(e1.IsHead);
            Assert.IsFalse(e1.IsEnqueued);
            Assert.IsTrue(e1.pushedBackCount == 0 && e1.poppedTailCount == 0); // nothing unexpected here
            // verify e2 activated
            Assert.IsTrue(e2.enqueuedCount == 1 && e2.activationCount == 1); 
            Assert.IsTrue(e2.IsHead);

            // now deq e2 and verify things cleaned up
            var popped2 = q.Dequeue();
            Assert.IsTrue(popped2 == e2);
            Assert.IsTrue(q.Head == null && q.Count == 0 && q.Empty);
            Assert.IsTrue(e2.enqueuedCount == 1 && e2.activationCount == 1); // activations unchanged
            Assert.IsTrue(e2.deactivationCount == 1 && e2.dequeuedCount == 1); // deactivations updated
            Assert.IsTrue(e2.pushedBackCount == 0);
            Assert.IsFalse(e2.IsHead);     // none of these is true anymore
            Assert.IsFalse(e2.IsEnqueued);
            Assert.IsNull(e2.container);
        }

        [TestMethod]
        public void TestSmartQueuePushHead () {

            SmartQueue<TestEntry> q = new SmartQueue<TestEntry>();
            Assert.IsTrue(q.Empty && q.Count == 0 && q.Head == null);

            // enqueue e1 and e2 onto the queue
            TestEntry e1 = new TestEntry();
            TestEntry e2 = new TestEntry();
            q.Enqueue(e1);
            q.Enqueue(e2);
            Assert.IsTrue(q.Head == e1 && q.Tail == e2 && q.Count == 2);
            Assert.IsTrue(e1.IsEnqueued);
            Assert.IsTrue(e1.IsHead);

            // now push back something in front of e1
            TestEntry h = new TestEntry();
            q.PushHead(h);
            Assert.IsTrue(q.Head == h && q.Tail == e2 && q.Count == 3);
            Assert.IsTrue(h.IsHead && h.IsEnqueued);
            Assert.IsTrue(e1.IsEnqueued);
            Assert.IsFalse(e1.IsHead); // no longer the head
            Assert.IsTrue(e1.pushedBackCount == 1); // it knows it got pushed back

            // now let's dequeue it and verify we're back to previous state
            var popped = q.Dequeue();
            Assert.IsTrue(popped == h);
            Assert.IsFalse(h.IsHead);
            Assert.IsFalse(h.IsEnqueued);
            Assert.IsTrue(q.Head == e1 && q.Tail == e2 && q.Count == 2);
            Assert.IsTrue(e1.IsHead && e1.IsEnqueued);
            Assert.IsTrue(e1.pushedBackCount == 1);
        }

        [TestMethod]
        public void TestSmartQueuePopTail () {

            SmartQueue<TestEntry> q = new SmartQueue<TestEntry>();
            Assert.IsTrue(q.Empty && q.Count == 0 && q.Head == null);

            // enqueue e1 and e2 onto the queue
            TestEntry e1 = new TestEntry();
            TestEntry e2 = new TestEntry();
            q.Enqueue(e1);
            q.Enqueue(e2);
            Assert.IsTrue(q.Head == e1 && q.Tail == e2 && q.Count == 2);
            Assert.IsTrue(e1.IsEnqueued);
            Assert.IsTrue(e1.IsHead);

            // now pop e2 from the tail, make sure it never got activated or deactivated
            var popped = q.PopTail();
            Assert.IsTrue(q.Head == e1 && q.Tail == e1 && q.Count == 1);
            Assert.IsFalse(e2.IsEnqueued);
            Assert.IsFalse(e2.IsHead);
            Assert.IsTrue(e2.enqueuedCount == 1 && e2.dequeuedCount == 1);
            Assert.IsTrue(e2.activationCount == 0 && e2.deactivationCount == 0); // never activated or deactivated
            Assert.IsTrue(e2.poppedTailCount == 1); // it knows it got popped from the tail

            // now let's pop the head element from the tail, make sure it's cleaned up
            var popped2 = q.PopTail();
            Assert.IsTrue(popped2 == e1);
            Assert.IsTrue(q.Empty);
            Assert.IsFalse(e1.IsHead);
            Assert.IsFalse(e1.IsEnqueued);
            Assert.IsTrue(e1.enqueuedCount == 1 && e1.dequeuedCount == 1);
            Assert.IsTrue(e1.activationCount == 1 && e1.deactivationCount == 1); // both activated and deactivated
            Assert.IsTrue(e1.poppedTailCount == 1); // it also knows it got popped from the tail
        }

    }
}
