using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.AI
{
    [TestClass]
    public class ActionsTest
    {
        public class TestAction : Action
        {
            public string name;
            public bool stopOnStart = false;
            public int countActivate;
            public int countDeactivate;
            public int countUpdate;

            public TestAction (string name, bool stopOnStart = false) {
                this.name = name;
                this.stopOnStart = stopOnStart;
            }

            public override void OnActivated () {
                base.OnActivated();
                this.countActivate++;
            }

            public override void OnDeactivated (bool pushedback) {
                this.countDeactivate++;
                base.OnDeactivated(pushedback);
            }

            internal override void OnStarted () {
                base.OnStarted();
                if (stopOnStart) {
                    Stop(true);
                }
            }

            internal override void OnUpdate () {
                base.OnUpdate();
                this.countUpdate++;
            }

            public override string ToString () {
                return "TestAction " + name;
            }
        }

        public class TestScript : Script
        {
            public int countActivate;
            public int countDeactivate;

            public TestScript (string name, params Action[] actions) : base(name, actions.ToList()) { }

            public override void OnActivated () {
                base.OnActivated();
                this.countActivate++;
            }

            public override void OnDeactivated (bool pushedback) {
                this.countDeactivate++;
                base.OnDeactivated(pushedback);
            }
        }

        [TestMethod]
        public void TestActionsInScript () {

            // make some actions
            var a = new TestAction("a");
            var b = new TestAction("b");
            var c = new TestAction("c");
            var d = new TestAction("d");
            var e = new TestAction("e");
            var f = new TestAction("f");

            var seq = new TestScript("test");
            seq.Enqueue(new List<Action>() { a, b, c });

            // make sure they're all enqueued but only A is activated
            Assert.IsTrue(!seq.IsEmpty && seq.Head == a);
            Assert.IsTrue(a.IsEnqueued && a.IsActive);
            Assert.IsTrue(b.IsEnqueued && !b.IsActive);
            Assert.IsTrue(c.IsEnqueued && !c.IsActive);

            // stop A, this should dequeue it and activate B
            a.Stop(true);
            Assert.IsTrue(!seq.IsEmpty && seq.Head == b);
            Assert.IsTrue(!a.IsEnqueued && !a.IsActive);
            Assert.IsTrue( b.IsEnqueued &&  b.IsActive);
            Assert.IsTrue( c.IsEnqueued && !c.IsActive);

            // stop the script, this should pop B and C without activating the latter
            seq.Clear();
            Assert.IsTrue(seq.IsEmpty && seq.Head == null);
            Assert.IsTrue(!a.IsEnqueued && !a.IsActive && a.countActivate == 1 && a.countDeactivate == 1);
            Assert.IsTrue(!b.IsEnqueued && !b.IsActive && b.countActivate == 1 && b.countDeactivate == 1);
            Assert.IsTrue(!c.IsEnqueued && !c.IsActive && c.countActivate == 0 && c.countDeactivate == 0);

            // push two more 
            seq.Enqueue(new List<Action>() { d, e });
            Assert.IsTrue(d.IsEnqueued &&  d.IsActive && d.countActivate == 1 && d.countDeactivate == 0);
            Assert.IsTrue(e.IsEnqueued && !e.IsActive && e.countActivate == 0 && e.countDeactivate == 0);

            // pop E from the end, this should not affect D, or activate/deactivate E
            seq.PopTail();
            Assert.IsTrue( d.IsEnqueued &&  d.IsActive && d.countActivate == 1 && d.countDeactivate == 0);
            Assert.IsTrue(!e.IsEnqueued && !e.IsActive && e.countActivate == 0 && e.countDeactivate == 0);

            // push F to the front, this should deactivate D but keep it in the queue
            seq.PushHead(f);
            Assert.IsTrue(f.IsEnqueued &&  f.IsActive && f.countActivate == 1 && f.countDeactivate == 0);
            Assert.IsTrue(d.IsEnqueued && !d.IsActive && d.countActivate == 1 && d.countDeactivate == 1);

            // now dequeue F from the front, this should pop it, and re-activate D
            seq.StopCurrentAction(true, null);
            Assert.IsTrue(!f.IsEnqueued && !f.IsActive && f.countActivate == 1 && f.countDeactivate == 1);
            Assert.IsTrue( d.IsEnqueued &&  d.IsActive && d.countActivate == 2 && d.countDeactivate == 1);
        }

        [TestMethod]
        public void TestActionStartupAndUpdate () {

            // make some actions
            var a = new TestAction("a");
            var b = new TestAction("b");
            var c = new TestAction("c", true); // this one stops before first update
            var seq = new TestScript("test", a, b, c);

            Assert.IsTrue(a.IsEnqueued && a.IsActive && !a.IsStarted);
            Assert.IsTrue(a.countUpdate == 0);
            
            // fake an update cycle
            seq.OnUpdate();

            Assert.IsTrue(a.IsEnqueued && a.IsActive && a.IsStarted);
            Assert.IsTrue(a.countUpdate == 1);

            // stop. this will remove and deactivate A, and activate B, 
            // but not start it yet until an update
            a.Stop(true);
            Assert.IsTrue(!a.IsEnqueued && !a.IsActive && !a.IsStarted);
            Assert.IsTrue( b.IsEnqueued &&  b.IsActive && !b.IsStarted);
            Assert.IsTrue(a.countUpdate == 1);
            Assert.IsTrue(b.countUpdate == 0);

            seq.OnUpdate();
            Assert.IsTrue(b.IsEnqueued && b.IsActive && b.IsStarted);
            Assert.IsTrue(b.countUpdate == 1);

            // stop b. this will activate c, which will stop itself before first update
            b.Stop(true);
            Assert.IsTrue(c.IsEnqueued && c.IsActive && !c.IsStarted);
            Assert.IsTrue(c.countUpdate == 0);
            seq.OnUpdate();
            Assert.IsTrue(!c.IsEnqueued && !c.IsActive && !c.IsStarted);
            Assert.IsTrue(c.countUpdate == 0); // this update never got a chance to run
        }

        [TestMethod]
        public void TestScriptsInQueue () {
            // make some scripts

            var a = new TestAction("a");
            var b = new TestAction("b");
            var c = new TestAction("c");
            var abc = new TestScript("abc", a, b, c);
            var empty = new TestScript("empty");
            var d = new TestAction("d");
            var e = new TestAction("e");
            var f = new TestAction("f");
            var def = new TestScript("def", d, e, f );

            var q = new ScriptQueue();
            q.Enqueue(new List<Script>() { abc, empty, def });

            // verify the first script and action are active 
            Assert.IsTrue(!q.IsEmpty && q.Head == abc && q.Head.Head == a);
            Assert.IsTrue(  abc.IsEnqueued &&  abc.IsActive);
            Assert.IsTrue(empty.IsEnqueued && !empty.IsActive);
            Assert.IsTrue(  def.IsEnqueued && !def.IsActive);

            // stop the first action, keeps the same script active
            a.Stop(true);
            Assert.IsTrue(!q.IsEmpty && q.Head == abc && q.Head.Head == b);

            // finish off actions. this will remove ABC from the queue
            b.Stop(true);
            c.Stop(true);
            Assert.IsTrue(!q.IsEmpty && q.Head != abc);
            Assert.IsTrue(abc.countActivate == 1 && abc.countDeactivate == 1);

            // the Empty script was activated but then immediately removed, because it's empty
            Assert.IsTrue(!empty.IsEnqueued && !empty.IsActive);
            Assert.IsTrue(empty.countActivate == 1 && empty.countDeactivate == 1);
            
            // and let's make sure that DEF is the active one now
            Assert.IsTrue(!q.IsEmpty && q.Head == def && q.Head.Head == d);
            Assert.IsTrue(  !abc.IsEnqueued &&   !abc.IsActive);
            Assert.IsTrue(!empty.IsEnqueued && !empty.IsActive);
            Assert.IsTrue(   def.IsEnqueued &&    def.IsActive);
            Assert.IsTrue(def.countActivate == 1 && def.countDeactivate == 0);

            // then if we force-stop DEF, it should clean out of the queue completely
            def.StopScript(true, null);
            Assert.IsTrue(q.IsEmpty && q.Head == null);
            Assert.IsTrue(!def.IsEnqueued && !def.IsActive);
            Assert.IsTrue(def.countActivate == 1 && def.countDeactivate == 1);

        }

        [TestMethod]
        public void TestStopOnFailure () {

            // make some scripts

            var a = new TestAction("a");
            var b = new TestAction("b");
            var c = new TestAction("c");
            var abc = new TestScript("abc", a, b, c);
            var d = new TestAction("d");
            var e = new TestAction("e");
            var f = new TestAction("f");
            var def = new TestScript("def", d, e, f);

            var q = new ScriptQueue();
            q.Enqueue(new List<Script>() { abc, def });

            q.OnUpdate();
            Assert.IsTrue(a.IsEnqueued && a.IsActive && a.IsStarted);

            // if we stop just action a with the failure flag set, it should stop the entire script
            // and advance to the next one
            abc.StopCurrentAction(false, null);

            Assert.IsTrue(!a.IsEnqueued && !a.IsActive);
            Assert.IsTrue(!abc.IsEnqueued && !abc.IsActive);
            Assert.IsTrue(def.IsActive && d.IsActive);

            // similary stopping the script will just remove it
            def.StopScript(false, null);
            Assert.IsTrue(!def.IsActive && !d.IsActive);
        }

        [TestMethod]
        public void TestQueueUpdate () {
            // make some scripts

            var a = new TestAction("a");
            var b = new TestAction("b");
            var c = new TestAction("c");
            var abc = new TestScript("abc", a, b, c);
            var d = new TestAction("d");
            var e = new TestAction("e");
            var f = new TestAction("f");
            var def = new TestScript("def", d, e, f);

            var q = new ScriptQueue();
            q.Enqueue(new List<Script>() { abc, def });

            Assert.IsTrue(a.IsEnqueued && a.IsActive && !a.IsStarted);

            // fake a queue update
            q.OnUpdate();

            Assert.IsTrue(a.IsEnqueued && a.IsActive && a.IsStarted);

            // stop the script. the first action in the next script
            // will be activated, but won't be started until another update
            q.StopCurrentScript(true, null);
            Assert.IsTrue(!abc.IsEnqueued && !abc.IsActive && !a.IsActive && !a.IsStarted);
            Assert.IsTrue(def.IsEnqueued && def.IsActive && d.IsActive && !d.IsStarted);

            q.OnUpdate();
            Assert.IsTrue(def.IsEnqueued && def.IsActive && d.IsActive && d.IsStarted);

            // clear everything. no more updates
            q.StopAllScripts(true, null);
            Assert.IsTrue(!def.IsEnqueued && !def.IsActive && !d.IsActive && !d.IsStarted);
            Assert.IsTrue(q.IsEmpty);
        }

    }
}
