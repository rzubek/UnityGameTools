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

        virtual public void Stop (bool success) {
            if (this.IsEnqueued && this.IsActive) {
                this.Script.StopCurrentAction(success, this);

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
                StopScript(true, null);
            }
        }

        /// <inheritDoc />
        virtual public void OnDeactivated (bool pushedback) {
            this.IsActive = false;
        }

        /// <inheritDoc />
        virtual public void OnDequeued (bool poppedtail) {
            // pop all remaining actions we contain
            Clear();

            this.Queue = null;
        }

        /// <summary>
        /// Adds a single action into the script. If it's the first one, it will be activated immediately.
        /// </summary>
        /// <param name="action"></param>
        new public void Add (Action action) {
            base.Enqueue(action);
        }

        /// <summary>
        /// Adds a list of actions to the script. If the script was empty, the first one will be activated
        /// immediately, before subsequent ones are added.
        /// </summary>
        /// <param name="actions"></param>
        public void Add (IEnumerable<Action> actions) {
            base.Enqueue(actions);
        }

        /// <summary>
        /// If success is true, stops the currently active action and advances to the next one or,
        /// if that was the last action in the script, stops the script as well.
        /// If success is false, stops the entire script.
        /// </summary>
        public void StopCurrentAction (bool success, object context) {
            if (this.IsActive && !success) {
                StopScript(false, context);
            } else {
                // remove the action
                base.Dequeue();
                // if we're out of actions, shut down
                if (this.IsEmpty && this.IsActive) {
                    StopScript(success, context);
                }
            }
        }

        /// <summary>
        /// Stops this script, which also has the effect of stopping the 
        /// currently active action, and cancelling any other ones that have not yet activated.
        /// </summary>
        public void StopScript (bool success, object context) {
            if (this.IsEnqueued && this.IsActive) {
                this.Queue.StopCurrentScript(success, context);
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
                StopScript(true, null); // we have nothing left, bail
                return;
            }

            // everything is okay, update 
            Action newhead = this.Head;
            try {
                if (!newhead._updatedOnce && newhead.IsActive) {
                    newhead._updatedOnce = true;
                    newhead.OnStarted();
                }
                if (newhead.IsActive) { // check isActive again just in case we got stopped in OnStarted
                    newhead.OnUpdate();
                }
            } catch (Exception ex) {
                // stop this script, something went wrong
                StopScript(false, newhead);

                if (ScriptQueue.DEBUG) { // maybe rethrow the exception
                    throw ex;
                }
            }
        }

        new public void Enqueue (Action action) {
            throw new InvalidOperationException("Enqueue not available, use Add instead");
        }

        new public void Dequeue () {
            throw new InvalidOperationException("Dequeue not available, use one of the Stop methods instead");
        }

        public override string ToString () {
            string elements = string.Join(", ", this.Select(action => action.ToString()).ToArray());
            if (Queue == null || Queue.IsEmpty) {
                elements = "(not in queue)" + elements;
            }

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
        virtual public void Run (Script script) {
            base.Enqueue(script);
        }

        /// <summary>
        /// Adds new script, and if the queue was empty, activates the first one
        /// immediately (before additional ones are added).
        /// </summary>
        /// <param name="scripts"></param>
        virtual public void Run (IEnumerable<Script> scripts) {
            base.Enqueue(scripts);
        }

        /// <summary>
        /// Stops and dequeues the currently active script (if any). If there was  
        /// another script in the queue behind it, activates it.
        /// </summary>
        virtual public void StopCurrentScript (bool success, object context) {
            base.Dequeue();
        }

        /// <summary>
        /// Stops and dequeues all scripts in the queue. This deactivates the currently
        /// active one, but does not activate or deactivate remaining ones. 
        /// </summary>
        virtual public void StopAllScripts (bool success, object context) {
            base.Clear();
        }

        new public void Enqueue (Script script) {
            throw new InvalidOperationException("Enqueue not available, use Run instead");
        }

        new public void Dequeue () {
            throw new InvalidOperationException("Dequeue not available, use one of the Stop methods instead");
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
