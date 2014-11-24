using SomaSim.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.AI
{
    abstract public class Action : AbstractSmartQueueElement
    {
        internal bool _updatedOnce = false;

        /// <summary>
        /// Returns true while the action is active and also has had at least one update cycle. 
        /// Ceases to be true when the action is deactivated or dequeued.
        /// </summary>
        public bool IsStarted { get { return this.IsEnqueued && this.IsActive && _updatedOnce; } }

        public Script Script { get { return Queue as Script; } }

        virtual internal void OnStarted () {
            // no op
        }

        virtual internal void OnUpdate () {
            // no op
        }

        virtual public void Stop () {
            if (this.IsEnqueued && this.IsActive) {
                this.Script.StopCurrentAction();

            } else if (ScriptQueue.DEBUG) {
                // this action is not running, so something went wrong if someone is calling stop()
                throw new Exception("Can't stop action that isn't running: " + this);
            }
        }
    }

    public class Script : SmartQueue<Action>, ISmartQueueElement
    {
        public string Name;

        public ScriptQueue Queue { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsEnqueued { get { return this.Queue != null; } }

        public Script (string name = null, IEnumerable<Action> actions = null) { 
            this.Name = name;
            if (actions != null) {
                this.Enqueue(actions);
            }
        }

        /// <inheritDoc />
        virtual public void OnEnqueued (object queue) {
            this.Queue = queue as ScriptQueue;
        }

        /// <inheritDoc />
        virtual public void OnActivated () {
            this.IsActive = true;
            if (IsEmpty) {
                // if empty, we shouldn't be here, pop ourselves off
                StopScript();
            }
        }

        /// <inheritDoc />
        virtual public void OnDeactivated (bool pushedback) {
            // deactivate and pop all actions we contain
            Clear();

            this.IsActive = false;
        }

        /// <inheritDoc />
        virtual public void OnDequeued (bool poppedtail) {
            this.Queue = null;
        }

        /// <summary>
        /// Stops the currently active action and advances to the next one or,
        /// if that was the last action in the script, stops the script as well.
        /// </summary>
        public void StopCurrentAction () {
            // remove the action
            this.Dequeue(); 
            // if we're out of actions, shut down
            if (this.IsEmpty && this.IsActive) {
                StopScript();
            }
        }

        /// <summary>
        /// Stops this script, which also has the effect of stopping the 
        /// currently active action, and cancelling any other ones that have not yet activated.
        /// </summary>
        public void StopScript () {
            if (this.IsEnqueued && this.IsActive) {
                this.Queue.Dequeue();
            } else if (ScriptQueue.DEBUG) {
                throw new InvalidOperationException("Can't stop a script that isn't running: " + this);
            }
        }

        /// <summary>
        /// Called by the action queue on a regular basis. Calls update on the currently active action.
        /// </summary>
        internal void OnUpdate () {
            if (this.Count == 0) {
                if (ScriptQueue.DEBUG) {
                    throw new InvalidOperationException("Can't update an empty script, it should have been dequened already");
                }
                StopScript(); // we have nothing left, bail
                return;
            }

            // everything is okay, update 
            Action a = this.Head;
            if (!a._updatedOnce) {
                a._updatedOnce = true;
                a.OnStarted();
            }
            a.OnUpdate();
        }

        public override string ToString () {
            string elements = (Queue == null || Queue.IsEmpty) ? "(none)" :
                string.Join(", ", this.Select(action => action.ToString()).ToArray());
            return "Script " + Name + ": " + elements;
        }
    }

    public class ScriptQueue : SmartQueue<Script>
    {
        #if UNITY_EDITOR
        public static bool DEBUG = true;
        #else
        public static bool DEBUG = false;
        #endif

        /// <summary>
        /// Adds a new script, and if it's the only one, activates it immedately.
        /// </summary>
        /// <param name="script"></param>
        public void Run (Script script) {
            Enqueue(script);
        }

        /// <summary>
        /// Adds new script, and if the queue was empty, activates the first one
        /// immediately (before additional ones are added).
        /// </summary>
        /// <param name="scripts"></param>
        public void Run (IEnumerable<Script> scripts) {
            Enqueue(scripts);
        }

        /// <summary>
        /// Stops and dequeues the currently active script (if any). If there was  
        /// another script in the queue behind it, activates it.
        /// </summary>
        public void StopCurrentScript () {
            Dequeue();
        }

        /// <summary>
        /// Stops and dequeues all scripts in the queue. This deactivates the currently
        /// active one, but does not activate or deactivate remaining ones. 
        /// </summary>
        public void StopAllScripts () {
            Clear();
        }

        public void OnUpdate () {
            if (!IsEmpty) {
                this.Head.OnUpdate();
            }
        }

        public override string ToString () {
            return "ScriptQueue: " + string.Join(", ", this.Select(seq => seq.Name).ToArray());
        }
    }
}
