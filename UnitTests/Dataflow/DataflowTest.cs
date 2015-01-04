using System;
using System.Collections.Generic;

namespace SomaSim.Dataflow
{
    [TestClass]
    public class DataflowTest
    {
        private static void AssertEqual (float value, float target) {
            Assert.IsTrue(value == target);
        }

        /// <summary>
        /// Test sink of type int, just adds whatever it gets
        /// </summary>
        public class IntAdder : BaseReceiver<int>
        {
            public int sum = 0;

            override public void Receive (int value, float dt) {
                sum += value;
            }
        }

        /// <summary>
        /// Produces a simple running average of all received inputs
        /// </summary>
        public class FloatAverager : BaseReceiver<float>
        {
            private float sum = 0f;
            private int count = 0;

            override public void Receive (float value, float dt) {
                sum += value;
                count++;
            }

            public float average { get { return sum / count; } }
        }

        /// <summary>
        /// Just converts ints to floats. It's only here to demonstrate 
        /// consuming and producing values of different types. 
        /// </summary>
        public class TrivialIntToFloatConverter : BaseTransducer<int, float>
        {
            public override float Convert (int value, float dt) {
                return (float)value;
            }
        }


        [TestMethod]
        public void TestBasicSourceAndSinkChains () {

            var source = new BaseSender<int>();
            var adder = new IntAdder();
            var averager = new FloatAverager();
            var int2float = new TrivialIntToFloatConverter();

            source.Add(adder);
            source.Add(int2float).Add(averager);

            // let's run it
            float dt = 0; // in this example dt doesn't matter
            source.Send(1, dt);
            source.Send(2, dt);
            source.Send(3, dt);

            Assert.IsTrue(adder.sum == 6); // 1 + 2 + 3
            Assert.IsTrue(averager.average == 2); // (1 + 2 + 3) / 3

            source.RemoveAll();
        }

        // ------------------------------------------------------------------------

        /// <summary>
        /// Test accumulator that decays the running sum at specified velocity,
        /// so that we can test how the passage of time affects thigs
        /// </summary>
        public class FloatAdderWithDecay : BaseReceiver<float>
        {
            private readonly float decayPerSecond = 0f;

            public float sum { get; private set; }

            public FloatAdderWithDecay  (float decayPerSecond) {
                this.decayPerSecond = decayPerSecond;
            }

            override public void Receive (float value, float dt) {
                // first apply decay to current sum
                float delta = dt * -decayPerSecond;
                // now add the new contribution
                sum = sum + value + delta;
            }

        }

        [TestMethod]
        public void TestTimeSensitiveChains () {

            var source = new BaseSender<float>();
            var adder = new FloatAdderWithDecay(0f);
            var leaky = new FloatAdderWithDecay(1f); // value decays by 1 per second

            source.Add(adder);
            source.Add(leaky);

            // initial value
            source.Send(100, 0);
            AssertEqual(adder.sum, 100);
            AssertEqual(leaky.sum, 100);

            // let one second pass, we add nothing
            source.Send(0, 1);
            AssertEqual(adder.sum, 100);  // adder is still at 100
            AssertEqual(leaky.sum, 99);   // leaky decays at 1/sec down to 99

            // let another second pass and add one
            source.Send(1, 1);
            AssertEqual(adder.sum, 101);  // adder just adds one
            AssertEqual(leaky.sum, 99);   // leaky decays by one more, but then gets one 
        }

        // ------------------------------------------------------------------------

        [TestMethod]
        public void TestMovingAverage () {

            var source = new BaseSender<float>();
            var average = new MovingAverage(5f); // 5 second averaging window

            source.Add(average);

            // let's send five values in every second: 5, 4, 3, 2, 1
            source.Send(5, 1); 
            source.Send(4, 1); 
            source.Send(3, 1); // three seconds in, the average will be (5 + 4 + 3) / 3 = 4
            AssertEqual(average.currentAverage, 4);

            source.Send(2, 1);
            source.Send(1, 1); // five seconds in, the average will be (5 + 4 + 3 + 2 + 1) / 5 = 3
            AssertEqual(average.currentAverage, 3); 

            // now if we send two more values, the first two will have dropped off 
            // (because we're only averaging over a five second window)
            source.Send(2, 1);
            source.Send(2, 1); // (3 + 2 + 1 + 2 + 2) / 5 = 2
            AssertEqual(average.currentAverage, 2);

            // and now we send just one value after four seconds.
            // now the moving average window only fits the last of the previous values
            source.Send(10, 4);
            AssertEqual(average.currentAverage, 6); // (2 + 10) / 2 = 6
        }

        // ------------------------------------------------------------------------

        public class LastValue<T> : BaseReceiver<T>
        {
            public T lastValue { get; private set; }
            public override void Receive (T value, float dt) {
                lastValue = value;
            }
        }
        
        [TestMethod]
        public void TestBarrier () {

            var preconditionA = new BaseSender<bool>();
            var preconditionB = new BaseSender<bool>();
            var preconditionC = new BaseSender<bool>();

            var barrier = new Barrier();
            preconditionA.Add(barrier);
            preconditionB.Add(barrier);
            preconditionC.Add(barrier);

            var result = new LastValue<bool>();
            barrier.Add(result);

            Assert.IsFalse(result.lastValue);

            // let two of the preconditions to trigger - this shouldn't change anything
            preconditionA.Send(true, 1f);
            Assert.IsFalse(result.lastValue);
            preconditionC.Send(true, 1f);
            Assert.IsFalse(result.lastValue);

            // let's make sure that sending false doesn't do anything
            preconditionA.Send(false, 1f);
            preconditionB.Send(false, 1f);
            preconditionC.Send(false, 1f);
            Assert.IsFalse(result.lastValue);

            // now the last one sends true, and the barrier becomes true as well
            preconditionB.Send(true, 1f);
            Assert.IsTrue(result.lastValue);
        }
    }
}
