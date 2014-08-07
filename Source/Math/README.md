Random Number Generators
---

TinyMT is an implementation of the Tiny Mersenne Twister pseudo-random number generator.
It's much smaller and faster than a full Mersenne Twister, and its state is just 128 bits.

So save the RNG's state out to a savefile, just write out the contents of its 
```uint[] s``` array of four elements. To restore state, reload values into this array.

**Example:**

```csharp
IRandom rand = new TinyMT();
rand.Init(1); // seed 

uint firstRandomValue = rand.Generate();         // in range [0, 2^32)
float secondRandomValue = rand.GenerateFloat();  // in range [0, 1)

```

Random Number Extension Methods
---

Just a few extension methods to make TinyMT (and in the future, other RNGs) easier to use.

```csharp
IRandom rand = new TinyMT();
var foo = rand.DieRoll(6); // random die roll between 1 and 6
var bar = rand.PickElement(new uint[] { 0, 1, 2, 3, 4 }); // picks one randomly
var baz = rand.GenerateInRange(0, 100); // number from 0 up to but excluding 100

```








