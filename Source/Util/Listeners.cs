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
            foreach (Action a in this) {
                a.Invoke();
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
            foreach (Action<T> a in this) {
                a.Invoke(arg);
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
            foreach (Action<T1, T2> a in this) { 
                a.Invoke(arg1, arg2); 
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
            foreach (Action<T1, T2, T3> a in this) {
                a.Invoke(arg1, arg2, arg3);
            }
        }
    }
}
