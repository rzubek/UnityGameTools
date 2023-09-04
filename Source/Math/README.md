Fixnum
===

`Fixnum` is a fixed-precision fractional type, currently optimized for numbers
with two decimal digits, e.g. `123.45`, such as currencies.

Fixnum struct stores its value in an int32, so that operations on fixnums
are int operations that don't lose precision (compared to floats)
and fractions can be compared exactly.

Thus the effective range of fixnum is [ -21,474,836.48, 21,474,836.47 ].



Random Number Generators
===

`IRandom` is a base interface for all number generators. It provides two functions,
`void Init (uint seed)` and `uint Generate()`. All of the following random number generators
implement this interface. 

Additionally, the `IRandom64` interface is implemented by RNGs that can produce
64-bit values in addition to 32-bit ones.

All RNG implementations expose their inner state as public variables,
so that it can be persisted to a save file along with the rest of game state.


Xorshift
---

Implementation of xorshift-32, a very fast random number generator
with a 2^32-1 period and a 32-bit state.

This is a great workhorse RNG, very fast, and with good properties. 
Note that the seed value must be non-zero.

See "Xorshift RNGs" by George Marsaglia, Journal of Statistical Software 8 (14), July 2003. 
http://www.jstatsoft.org/v08/i14/paper 


SplitMix64
---

This is a fixed-increment version of Java 8's SplittableRandom generator, 
see http://dx.doi.org/10.1145/2714064.2660195 and 
http://docs.oracle.com/javase/8/docs/api/java/util/SplittableRandom.html

It is a very fast generator passing BigCrush, with exactly 64 bits of state. 
Interestingly, it allows for the seed value to be zero, making it very useful for hashing.


Xoroshiro128+
---

Implementation of the Xoroshiro128+ RNG, a fast RNG with 2^128-1 period,
128-bit state, excellent random properties, and a far jump function.
See http://xoroshiro.di.unimi.it/xoroshiro128plus.c



Tiny Mersenne Twister
---

This is an implementation of the Tiny Mersenne Twister RNG.
It's much smaller and faster than a full Mersenne Twister, 
with an internal state of 128 bits, and period of 2^127-1.

See: http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/TINYMT/index.html


Random Number Extension Methods
---

Just a few extension methods to make RNGs easier to use.

```csharp
IRandom rand = new TinyMT();
var foo = rand.DieRoll(6); // random die roll between 1 and 6
var bar = rand.PickElement(new uint[] { 0, 1, 2, 3, 4 }); // picks one randomly
var baz = rand.Generate(50, 100); // integer from 50 up to but excluding 100
/// and more!
```


Hashing
===

`HashUtil` presents a number of functions for hashing numbers and strings.

These utility methods do hashing via random number generators. 
They produce well-distributed hashes, such that consecutive inputs 
(e.g. 1, 2, 3) will result in very different outputs.

```csharp
uint a = HashUtil.Hash(1), b = HashUtil.Hash(2), c = HashUtil.Hash(3);
Assert.IsTrue(a == 2298633409);
Assert.IsTrue(b == 479680206);
Assert.IsTrue(c == 3674312685);
```

String hashes are similarly well-distributed - even consecutive strings
produce very different hashes. 

Also, string hashes will remain
the same across different platforms and versions of .NET, unlike
the default `Object.GetHashCode()` implementation.

```csharp
uint s = HashUtil.Hash("Hello1"), t = HashUtil.Hash("Hello2");
Assert.IsTrue(s == 925636315);
Assert.IsTrue(t == 625902542);
```


Mappers
===

Helper classes for specifying piecewise linear functions and then querying them.
For example, one might have an interpolating linear function, which takes a series of
(x, y) points, and then interpolates anything in between, or a step linear function,
which takes (x, y) points and always returns the y value of the point immediately
preceding the queried location.

```csharp

var m = new FloatMapper() {
    x = new List<float> { 0, 1 },
    y = new List<float> { 0, 1 }
};

Assert.IsTrue(m.Eval(-10) == 0);
Assert.IsTrue(m.Eval(0) == 0);
Assert.IsTrue(m.Eval(0.5f) == 0.5f);
Assert.IsTrue(m.Eval(1) == 1);
Assert.IsTrue(m.Eval(10) == 1);

var m = new StepFunctionMapper() {
    x = new List<float> { 0, 1 },
    y = new List<float> { 0, 1 }
};

Assert.IsTrue(m.Eval(-10) == 0);
Assert.IsTrue(m.Eval(0) == 0);
Assert.IsTrue(m.Eval(0.5f) == 0);
Assert.IsTrue(m.Eval(1) == 1);
Assert.IsTrue(m.Eval(10) == 1);

```
