// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;

namespace SomaSim.Util
{
    /// <summary>
    /// Implementation of the listener pattern that calls listeners in a predictable order,
    /// and allows for listener list modification in the middle of invocation 
    /// (any modification in the middle of an invocation will be observed during the next invocation.)
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
        private List<Action> _copy = new List<Action>();

        public void Invoke () {
            _copy.ClearAndAddRange(this);
            foreach (var a in _copy) { a.Invoke(); }
        }
    }

    /// <summary>
    /// Implementation of the listener pattern that calls listeners in a predictable order,
    /// and allows for listener list modification in the middle of invocation 
    /// (any modification in the middle of an invocation will be observed during the next invocation.)
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
        private List<Action<T>> _copy = new List<Action<T>>();

        public void Invoke (T arg) {
            _copy.ClearAndAddRange(this);
            foreach (var a in _copy) { a.Invoke(arg); }
        }
    }

    /// <summary>
    /// Implementation of the listener pattern that calls listeners in a predictable order,
    /// and allows for listener list modification in the middle of invocation 
    /// (any modification in the middle of an invocation will be observed during the next invocation.)
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
        private List<Action<T1, T2>> _copy = new List<Action<T1, T2>>();

        public void Invoke (T1 arg1, T2 arg2) {
            _copy.ClearAndAddRange(this);
            foreach (var a in _copy) { a.Invoke(arg1, arg2); }
        }
    }


    /// <summary>
    /// Implementation of the listener pattern that calls listeners in a predictable order,
    /// and allows for listener list modification in the middle of invocation 
    /// (any modification in the middle of an invocation will be observed during the next invocation.)
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
        private List<Action<T1, T2, T3>> _copy = new List<Action<T1, T2, T3>>();

        public void Invoke (T1 arg1, T2 arg2, T3 arg3) {
            _copy.ClearAndAddRange(this);
            foreach (var a in _copy) { a.Invoke(arg1, arg2, arg3); }
        }
    }
}
