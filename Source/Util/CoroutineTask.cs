// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using UnityEngine;

namespace SomaSim.Util
{
    // more on coroutines and tasks: http://www.stevevermeulen.com/index.php/2017/09/using-async-await-in-unity3d-2017/

    /// <summary>
    /// Wraps coroutines in a container class, which lets us query whether the coroutine
    /// is running or finished, whether it threw an exception, and what kind of 
    /// value it yielded last. 
    /// 
    /// This class cannot be instantiated directly, use CoroutineExtensions.StartCoroutineTask() instead.
    /// </summary>
    public sealed class CoroutineTask
    {
        public bool IsFinished { get; private set; }
        public bool IsSuccessful { get; private set; }

        public object LastResult { get; private set; }
        public Exception LastException { get; private set; }

        private MonoBehaviour _behavior;
        private Coroutine _coroutine;
        private Action<CoroutineTask> _callback;

        internal CoroutineTask (MonoBehaviour behavior, IEnumerator coroutine, Action<CoroutineTask> onFinished = null) {
            this._behavior = behavior;
            this._callback = onFinished;
            this._coroutine = behavior.StartCoroutine(Wrap(coroutine));
        }

        public void Stop () {
            if (IsFinished) { throw new InvalidOperationException("Stopping a CoroutineTask that was already finished"); }

            this._behavior.StopCoroutine(_coroutine);
            ProcessFinished(null);
        }

        private IEnumerator Wrap (IEnumerator coroutine) {
            while (true) {
                try {
                    if (!coroutine.MoveNext()) {
                        ProcessFinished();
                        yield break;
                    }
                } catch (Exception e) {
                    Logger.Error("Coroutine task exception: " + e);
                    ProcessFinished(e);
                    yield break;
                }

                LastResult = coroutine.Current;
                yield return LastResult;
            }
        }

        private void ProcessFinished (Exception ex = null) {
            this.IsFinished = true;
            this.IsSuccessful = (ex == null);
            this.LastException = ex;

            this._callback?.Invoke(this);

            this._behavior = null;
            this._coroutine = null;
            this._callback = null;
        }
    }

    public static class CoroutineExtensions
    {
        public static CoroutineTask StartCoroutineTask (
            this MonoBehaviour behavior, IEnumerator coroutine, Action<CoroutineTask> onfinished = null) =>

            new CoroutineTask(behavior, coroutine, onfinished);
    }
}
