// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace SomaSim.Util
{
    public static class TaskUtil
    {
        #region Coroutine runner

        private class CoroutineRunnerComponent : MonoBehaviour { }

        /// <summary>
        /// This GameObject will be used to start/stop coroutines.
        /// </summary>
        private static MonoBehaviour _runner;

        /// <summary>
        /// Returns the singleton instance of the coroutine runner, creating a new instance if necessary.
        /// </summary>
        public static MonoBehaviour Runner {
            get {
                if (_runner == null) {
                    SetCoroutineRunner(new GameObject("Coroutine Runner").AddComponent<CoroutineRunnerComponent>());
                }
                return _runner;
            }
        }


        /// <summary>
        /// Overrides the default coroutine runner.
        /// </summary>
        /// <param name="runner"></param>
        public static void SetCoroutineRunner (MonoBehaviour runner) { _runner = runner; }

        #endregion


        #region Coroutines

        public static IEnumerator MakeCoroutine (Action a) {
            a();
            yield return null;
        }

        public static void StartParallel (this MonoBehaviour runner, IEnumerator a, IEnumerator b) {
            runner.StartCoroutine(a);
            runner.StartCoroutine(b);
        }

        public static void StartParallel (this MonoBehaviour runner, IEnumerator a, IEnumerator b, IEnumerator c) {
            runner.StartCoroutine(a);
            runner.StartCoroutine(b);
            runner.StartCoroutine(c);
        }

        public static void StartParallel (this MonoBehaviour runner, IEnumerator a, IEnumerator b, IEnumerator c, IEnumerator d) {
            runner.StartCoroutine(a);
            runner.StartCoroutine(b);
            runner.StartCoroutine(c);
            runner.StartCoroutine(d);
        }

        public static void StartParallel (this MonoBehaviour runner, params IEnumerator[] enums) {
            foreach (var e in enums) { runner.StartCoroutine(e); }
        }

        public static void StartSequence (this MonoBehaviour runner, IEnumerator a, IEnumerator b) {
            runner.StartCoroutine(MakeSequence(a, b));
        }

        public static void StartSequence (this MonoBehaviour runner, IEnumerator a, IEnumerator b, IEnumerator c) {
            runner.StartCoroutine(MakeSequence(a, b, c));
        }

        public static void StartSequence (this MonoBehaviour runner, IEnumerator a, IEnumerator b, IEnumerator c, IEnumerator d) {
            runner.StartCoroutine(MakeSequence(a, b, c, d));
        }

        public static void StartSequence (this MonoBehaviour runner, params IEnumerator[] enums) {
            runner.StartCoroutine(MakeSequence(enums));
        }

        public static IEnumerator MakeSequence (IEnumerator a, IEnumerator b) {
            yield return a;
            yield return b;
        }

        public static IEnumerator MakeSequence (IEnumerator a, IEnumerator b, IEnumerator c) {
            yield return a;
            yield return b;
            yield return c;
        }

        public static IEnumerator MakeSequence (IEnumerator a, IEnumerator b, IEnumerator c, IEnumerator d) {
            yield return a;
            yield return b;
            yield return c;
            yield return d;
        }

        public static IEnumerator MakeSequence (params IEnumerator[] enums) {
            foreach (var e in enums) { yield return e; }
        }

        public static IEnumerator AsyncToCoroutine (Func<Task> fn) {
            Task task = fn();
            while (!task.IsCompleted) {
                yield return null;
            }
        }

        #endregion


        #region Tasks

        public static void StartParallel (Func<Task> a, Func<Task> b) {
            a();
            b();
        }

        public static void StartParallel (Func<Task> a, Func<Task> b, Func<Task> c) {
            a();
            b();
            c();
        }

        public static void StartParallel (params Func<Task>[] fns) {
            foreach (var fn in fns) {
                fn();
            }
        }

        public static async Task StartSequence (Func<Task> a, Func<Task> b) {
            await a();
            await b();
        }

        public static async Task StartSequence (Func<Task> a, Func<Task> b, Func<Task> c) {
            await a();
            await b();
            await c();
        }

        public static async Task StartSequence (params Func<Task>[] fns) {
            foreach (var fn in fns) {
                await fn();
            }
        }

        public static async Task CoroutineToAsync (this MonoBehaviour runner, IEnumerator e) {
            var src = new TaskCompletionSource<bool>();
            StartSequence(runner, e, MakeFinalizer(src));
            await src.Task;
        }

        private static IEnumerator MakeFinalizer (TaskCompletionSource<bool> src) {
            src.SetResult(true);
            yield return null;
        }

        #endregion

        #region Task wrappers for standard async functions


        #endregion

    }
}
