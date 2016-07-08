Deque
===

Double-ended queue that can be enqueued or dequeued from either end via O(1) operations.



Smart Collections
===

Smart collections (stack and queue) inform their elements about when they're being added
to or removed from the collection, and about whether they start or stop being the "active" element - 
at the top of the stack, or the head of the queue. 

Smart Stack
---

Smart stack simplify implementation of things like stacks of game screens (eg. main menu -> 
settings menu -> back to main menu -> scenario selection -> play game). 

In this example, a main menu stack element will display the main menu UI when it becomes
the topmost frame on the stack, but hides it when either another frame gets pushed on top of it,
or when it gets popped off the stack itself (eg. because the user is quitting the game):

```csharp
    public class GameScreenMainMenu : AbstractSmartStackElement
    {
        private BaseDialog dialog;

        public override void OnActivated () {
            base.OnActivated();
            dialog = Game.ui.ShowDialog(new MainScreenDialog());
        }

        public override void OnDeactivated () {
            dialog = Game.ui.HideDialog(dialog);
            base.OnDeactivated();
        }
    }
```

Then somewhere else:
```csharp
	Game.screens.Push(new GameScreenMainMenu());
```
... will be enough to show the main menu UI. If that UI then pushes another screen on
top of the stack, the UI will get hidden automatically.


Smart Queue
---

Smart queue is great for systems like NPC action queue, where each element in the queue
is a separate action that takes some time to complete, and then removes itself.
This can be used to easily sequence animations, pathfinding and navigation, communication
with the player, and so on, into an ordered series of events. 

Example coming soon!

