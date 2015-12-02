using System;
using System.Collections.Generic;
using System.Linq;

namespace SomaSim.Util
{
    /// <summary>
    /// Very simple implementation of the listener or observable pattern.
    /// 
    /// Usage: 
    /// <code>
    /// // add an event listener
    /// OnSpecialEvent.Add(() => { doSomething(); });
    /// // ...
    /// // invoke the event
    /// OnSpecialEvent.Invoke(); // calls all listeners in order
    /// </code>
    /// </summary>
    public class Listeners : List<Action>
    {
        public void Invoke () {
            int count = this.Count;
            for (int i = 0; i < count; i++) {
                Action a = this[i];
                a.Invoke();
                Logger.AssertInEditor(count == this.Count, "Listeners list modified while iterating.");
            }
        }
    }

    /// <summary>
    /// Very simple implementation of the listener or observable pattern.
    /// 
    /// Usage: 
    /// <code>
    /// // add an event listener
    /// OnSpecialEvent.Add((int x) => { doSomething(x); });
    /// // ...
    /// // invoke the event
    /// OnSpecialEvent.Invoke(42); // calls all listeners in order
    /// </code>
    /// </summary>
    public class Listeners<T> : List<Action<T>>
    {
        public void Invoke (T arg) {
            int count = this.Count;
            for (int i = 0; i < count; i++) {
                var a = this[i];
                a.Invoke(arg);
                Logger.AssertInEditor(count == this.Count, "Listeners list modified while iterating.");
            }
        }
    }

    /// <summary>
    /// Very simple implementation of the listener or observable pattern.
    /// 
    /// Usage: 
    /// <code>
    /// // add an event listener
    /// OnSpecialEvent.Add((string key, int val) => { doSomething(key, val); });
    /// // ...
    /// // invoke the event
    /// OnSpecialEvent.Invoke("answer", 42); // calls all listeners in order
    /// </code>
    /// </summary>
    public class Listeners<T1, T2> : List<Action<T1, T2>>
    {
        public void Invoke (T1 arg1, T2 arg2) {
            int count = this.Count;
            for (int i = 0; i < count; i++) {
                var a = this[i];
                a.Invoke(arg1, arg2);
                Logger.AssertInEditor(count == this.Count, "Listeners list modified while iterating.");
            }
        }
    }


    /// <summary>
    /// Very simple implementation of the listener or observable pattern.
    /// 
    /// Usage: 
    /// <code>
    /// // add an event listener
    /// OnSpecialEvent.Add((int a, int b, int c) => { doSomething(a, b, c); });
    /// // ...
    /// // invoke the event
    /// OnSpecialEvent.Invoke(1, 2, 3); // calls all listeners in order
    /// </code>
    /// </summary>
    public class Listeners<T1, T2, T3> : List<Action<T1, T2, T3>>
    {
        public void Invoke (T1 arg1, T2 arg2, T3 arg3) {
            int count = this.Count;
            for (int i = 0; i < count; i++) {
                var a = this[i];
                a.Invoke(arg1, arg2, arg3);
                Logger.AssertInEditor(count == this.Count, "Listeners list modified while iterating.");
            }
        }
    }
}
