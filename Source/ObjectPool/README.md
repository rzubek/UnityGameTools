ObjectPool
===

Implementation of an object pool.

Client code calls Allocate() to get a pooled object, and returns it back to the pool by calling Free(object). 

Pooled objects need to implement the interface IObjectPoolElement, which provides a function for resetting the element to a "clean" state after they've been returned to the pool (so that any resources they use can be released as soon as possible).

For a detailed discussion of several variants of object pools, see articles such as this one:
http://www.gamasutra.com/blogs/WendelinReich/20131127/203843/C_Memory_Management_for_Unity_Developers_part_3_of_3.php



Usage examples:
===

**Initialize and use the pool**

```csharp
var pool = new ObjectPool<TestElement>();
pool.Initialize();

// allocate a new object and use it
TestElement e1 = pool.Allocate();

// ... do stuff with element e1, and finally
pool.Free(e1);

// ... then the next time we allocate, like this:
TestElement e2 = pool.Allocate();
// we'll see that e2 is actually the same object as e1 was before,
// and that it has been reset
```

**Initialize a new pool with a custom factory instead of a default one**

```charp
// this will generate a unique ID for every instance created by the pool
var pool = new ObjectPool<TestElement>();
pool.Initialize(() => { var e = new SomeClass(); e.Id = Guid.NewGuid(); return e; });
```


