UnityGameTools
===

Collection of various tools we use in Unity3D game development.


Quick overview of what's here:
*   [Collections](Source/Collections) contains a few data structures, such as smart queues and stacks, that simplify gamedev life
*   [Math](Source/Math) contains a C# port of the Tiny Mersenne Twister random number generator, plus other math utilities
*   [ObjectPool](Source/ObjectPool) has a simple and straightforward object pool manager 
*   [Serializer](Source/Serializer) that will serialize typed objects into "typed" JSON and deserialize them back - useful for loading up game config files, or for save file implementation
*   [Services](Source/Services) contains basic setup for global services, and an input manager service
*   [Testing](Source/Testing) is a unit test harness that will load and run specially-marked-up class methods at game startup time



Setup 
---

Just drop this project under your Assets folder.

All the source code lives under the [Source](Source) directory.

There are also unit tests under the appropriately named [UnitTests](UnitTests) directory. To see them run, attach the [Source/Testing/UnitTestRunner](Source/Testing/UnitTestRunner.cs) component to some kind of a global game object, and it will run all unit tests marked with \[TestClass\] at the start of the game, in editor only.


