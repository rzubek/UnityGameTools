Utils
===

[Listeners](Listeners.cs) is a straightforward implementation of the listener / observable pattern.

Though C# supports listeners natively using the *event* keyword, this implementation gets around several problems:
1. Vestigial warts, such as always having to null check the event handler before dispatch, in case there are no listerners subscribed to it, which throws an exception
2. Thread safety guards when compiling with Visual Studio, which causes compilation errors with the MONO AOT compiler for iOS

This implementation sidesteps these issues completely, because it's just a straightforward list of closures that get invoked in order as needed.

Usage: 

```csharp
	Listeners<string, int> OnDataChange = new Listeners<string, int>();

	// later we add a listener
	OnDataChange.Add((string k, int v) => { ProcessKey(k); ProcessValue(v); });

	// and then to invoke it later:
	OnDataChange.Invoke("answer", 42);
```






