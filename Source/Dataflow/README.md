Dataflow
========

Dataflow package is a basic implementation of reactive / dataflow programming elements. For an overview of reactive programming, check out any number of articles, such as [Reactive Programming on Wikipedia](http://en.wikipedia.org/wiki/Reactive_programming).

One big difference for reactive programming in **games** is that game updates are timing-sensitive: for example, when someone's position changes between frames, we may need to know how much time passed between those frames, so that we can calculate velocity. Our API takes time change into account.

Here's a trivial averager example from unit tests:

```lang=csharp
    source.Add(intadder);
    source.Add(int2float).Add(averager);

    float dt = 0; // in this example dt doesn't matter
    source.Send(1, dt);
    source.Send(2, dt);
    source.Send(3, dt);

    Assert.IsTrue(intadder.sum == 6); // 1 + 2 + 3
    Assert.IsTrue(averager.average == 2); // (1 + 2 + 3) / 3
```

Another example in which time actually matters. This one uses a moving average implementation, with a time window of five seconds:

```lang=csharp
    var source = new BaseSender<float>();
    var average = new MovingAverage(5f); // 5 second averaging window
    source.Add(average);

    // let's send five values in every second: 5, 4, 3, 2, 1
    source.Send(5, 1); 
    source.Send(4, 1); 
    source.Send(3, 1);
    source.Send(2, 1);
    source.Send(1, 1); // five seconds in, the average will be (5 + 4 + 3 + 2 + 1) / 5 = 3
    AssertEqual(average.currentAverage, 3); 

    // now if we send two more values, the first two will have dropped off 
    // (because we're only averaging over a five second window)
    source.Send(2, 1);
    source.Send(2, 1); // (3 + 2 + 1 + 2 + 2) / 5 = 2
    AssertEqual(average.currentAverage, 2);
```

Now here's an example of a boolean barrier, which waits for all of its senders to report a true value before it turns true itself:

```lang=csharp
    var preconditionA = new BaseSender<bool>();
    var preconditionB = new BaseSender<bool>();
    var preconditionC = new BaseSender<bool>();

    var barrier = new Barrier();
    preconditionA.Add(barrier);
    preconditionB.Add(barrier);
    preconditionC.Add(barrier);

    var result = new LastValue<bool>();
    barrier.Add(result);

    // let two of the preconditions to trigger - this shouldn't change anything
    preconditionA.Send(true, 1f);
    preconditionC.Send(true, 1f);
    Assert.IsFalse(result.lastValue);

    // now once the last one sends true, the barrier becomes true as well
    preconditionB.Send(true, 1f);
    Assert.IsTrue(result.lastValue);
```

More examples available in unit tests.



