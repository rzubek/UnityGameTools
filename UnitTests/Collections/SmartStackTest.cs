using System;

namespace SomaSim.Collections
{
    [TestClass]
    public class SmartStackTest
    {
        internal class TestEntry : AbstractSmartStackElement
        {
            public int activationCount { get; private set; }
            public int deactivationCount { get; private set; }
            public int pushCount { get; private set; }
            public int popCount { get; private set; }

            public SmartStack<TestEntry> container { get { return this.Stack as SmartStack<TestEntry>; } }

            public override void OnPushed (object stack) {
                base.OnPushed(stack);
                pushCount++;
            }

            public override void OnActivated () {
                base.OnActivated();
                activationCount++;
            }

            public override void OnDeactivated () {
                deactivationCount++;
                base.OnDeactivated();
            }

            public override void OnPopped () {
                popCount++;
                base.OnPopped();
            }
        }

        [TestMethod]
        public void TestSmartStack () {

            SmartStack<TestEntry> stack = new SmartStack<TestEntry>();
            Assert.IsTrue(stack.IsEmpty && stack.Count == 0 && stack.Peek() == null);

            // make an empty something
            TestEntry e1 = new TestEntry();
            Assert.IsTrue(e1.pushCount == 0 && e1.popCount == 0 && e1.activationCount == 0 && e1.deactivationCount == 0);
            Assert.IsNull(e1.container);
            Assert.IsFalse(e1.IsActive);
            Assert.IsFalse(e1.IsOnStack);

            // push it! make sure it's there, and got notified appropriately
            stack.Push(e1);
            Assert.IsTrue(stack.Peek() == e1 && stack.Count == 1 && !stack.IsEmpty);
            Assert.IsTrue(e1.pushCount == 1 && e1.activationCount == 1);
            Assert.IsTrue(e1.IsOnStack);
            Assert.IsTrue(e1.IsActive);

            // push something else. this should deactivate but not pop the first one
            TestEntry e2 = new TestEntry();
            stack.Push(e2);
            Assert.IsTrue(stack.Peek() == e2 && stack.Count == 2 && !stack.IsEmpty);
            Assert.IsTrue(e1.pushCount == 1 && e1.activationCount == 1); // same as before, but
            Assert.IsTrue(e1.IsOnStack);
            Assert.IsFalse(e1.IsActive); // ...this is no longer true
            Assert.IsTrue(e1.deactivationCount == 1); // because it's been deactivated (not on top anymore)

            // pop that something else, let's verify e1 is activated 
            var popped = stack.Pop();
            Assert.IsTrue(popped == e2);
            Assert.IsTrue(stack.Peek() == e1 && stack.Count == 1 && !stack.IsEmpty);
            Assert.IsTrue(e1.pushCount == 1 && e1.activationCount == 2); // activations incremented
            Assert.IsTrue(e1.deactivationCount == 1); // deactivations unchanged
            Assert.IsTrue(e1.IsActive);

            // now pop the last thing
            var popped2 = stack.Pop();
            Assert.IsTrue(popped2 == e1);
            Assert.IsTrue(stack.Peek() == null && stack.Count == 0 && stack.IsEmpty);
            Assert.IsTrue(e1.pushCount == 1 && e1.activationCount == 2); // activations unchanged
            Assert.IsTrue(e1.deactivationCount == 2 && e1.popCount == 1); // deactivations and popped incremented
            Assert.IsFalse(e1.IsActive);     // none of these is true anymore
            Assert.IsFalse(e1.IsOnStack);
            Assert.IsNull(e1.container);
        }
    }
}
