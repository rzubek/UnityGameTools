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









