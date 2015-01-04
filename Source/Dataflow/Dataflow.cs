using System;
using System.Collections.Generic;
using System.Linq;

namespace SomaSim.Dataflow
{
    #region Base classes and interfaces 

    /// <summary>
    /// Interface for objects that receive values of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReceiver<T> {
        /// <summary>
        /// Called after this receiver is attached to some sender
        /// </summary>
        /// <param name="sender"></param>
        void OnAdded (ISender<T> sender);

        /// <summary>
        /// Called before this receiver is detached from the sender 
        /// </summary>
        /// <param name="sender"></param>
        void OnRemoved (ISender<T> sender);

        /// <summary>
        /// Called by the sender with a new value, and some time delta value dt
        /// (for example, seconds passed since the last call to Receive)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dt"></param>
        void Receive (T value, float dt);
    }

    /// <summary>
    /// Interface for objects that send values of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISender<T> {

        /// <summary>
        /// Used to add a new receiver to listen to this sender's updates.
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        void Add (IReceiver<T> rec);

        /// <summary>
        /// Used to add a new transducer to listen to this sender's updates.
        /// Returns the transducer back as a sender, so that it can be chained 
        /// with subsequent calls to Add().
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="tr"></param>
        /// <returns></returns>
        ISender<B> Add<A, B> (ITransducer<A, B> tr) where A : T;

        /// <summary>
        /// Removes the specified receiver
        /// </summary>
        /// <param name="rec"></param>
        void Remove (IReceiver<T> rec);

        /// <summary>
        /// Removes all receivers
        /// </summary>
        void RemoveAll ();

        /// <summary>
        /// Sends the given value and time delta to all receivers
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dt"></param>
        void Send (T value, float dt);
    }

    /// <summary>
    /// Interface for objects that receive values of type A, 
    /// convert it to type B, and then send it to further receivers of type B.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    public interface ITransducer<A, B> : IReceiver<A>, ISender<B>
    {
        /// <summary>
        /// Converts incoming value of type A to outgoing value of type B
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        B Convert (A value, float dt);
    }

    /// <summary>
    /// Basic implementation of a sender of type T, which simply
    /// uses Send() to forward the value verbatim to all registered receivers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseSender<T> : ISender<T>
    {
        protected List<IReceiver<T>> _receivers = new List<IReceiver<T>>();

        // from ISender
        public ISender<B> Add<A, B> (ITransducer<A, B> tr) where A : T {
            IReceiver<T> receiver = (IReceiver<T>) tr;
            Add(receiver);
            return tr;
        }

        // from ISender
        virtual public void Add (IReceiver<T> rec) {
            _receivers.Add(rec);
            rec.OnAdded(this);
        }

        // from ISender
        virtual public void Remove (IReceiver<T> rec) {
            rec.OnRemoved(this);
            _receivers.Remove(rec);
        }

        // from ISender
        public void RemoveAll () {
            while (_receivers.Count > 0) {
                Remove(_receivers[_receivers.Count - 1]);
            }
        }

        // from ISender
        public void Send (T value, float dt) {
            foreach (var rec in _receivers) { rec.Receive(value, dt); }
        }
    }

    /// <summary>
    /// Simple base receiver that doesn't do anything. Subclasses must
    /// implement their own Receive().
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseReceiver<T> : IReceiver<T>
    {
        // from IReceier, subclasses must implement
        public abstract void Receive (T value, float dt);

        // from IReceiver
        virtual public void OnAdded (ISender<T> sender) { /* no op */ }

        // from IReceiver
        virtual public void OnRemoved (ISender<T> sender) { /* no op */ }
    }

    /// <summary>
    /// Sample base transducer implementation. Subclasses only have to implement
    /// their own versions of Convert(), this class takes care of receiving values,
    /// calling Convert(), and then forwarding results to any listeners.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    public abstract class BaseTransducer<A, B> : BaseSender<B>, ITransducer<A, B>
    {
        // subclasses will implement
        public abstract B Convert (A value, float dt);

        // from IReceiver
        public void Receive (A value, float dt) {
            B newvalue = this.Convert(value, dt);
            Send(newvalue, dt);
        }

        // from IReceiver
        virtual public void OnAdded (ISender<A> sender) {
            // nothing to do
        }

        // from IReceiver
        virtual public void OnRemoved (ISender<A> sender) {
            // clear out any of our listeners
            this.RemoveAll();
        }
    }

    #endregion 


    #region Useful utility classes

    /// <summary>
    /// Computes a simple moving average with a given time window (by convention measured in seconds)
    /// </summary>
    public class MovingAverage : BaseTransducer<float, float>
    {
        private struct Record {
            public float value;
            public float timestamp;
        }

        public readonly float windowSizeInSeconds;

        public float currentTime { get; private set; }
        public float currentAverage { get; private set; }

        private Queue<Record> _entries = new Queue<Record>();

        public MovingAverage (float windowSizeInSeconds) {
            this.windowSizeInSeconds = windowSizeInSeconds;
        }

        public override float Convert (float value, float dt) {
            currentTime += dt;
            float expirationTime = currentTime - windowSizeInSeconds;

            // drop any expired entries
            while (_entries.Count > 0 && _entries.Peek().timestamp <= expirationTime) {
                _entries.Dequeue();
            }

            // add this one
            _entries.Enqueue(new Record() { value = value, timestamp = currentTime });

            // now update the average...
            currentAverage = _entries.Average(record => record.value);

            // and pass the value along
            return value;
        }
    }

    /// <summary>
    /// Simple barrier implementation that can be added to multiple sources.
    /// 
    /// The barrier waits for all sources to send in a "true" boolean value.
    /// After it has received the same number of true values as there are sources, 
    /// it sends out true; before and after that, it sends out false.
    /// </summary>
    public class Barrier : BaseTransducer<bool, bool>
    {
        private int _senderCount = 0;
        private int _receivedTrueValues = 0;

        public override void OnAdded (ISender<bool> sender) {
            base.OnAdded(sender);
            _senderCount++;
        }

        public override void OnRemoved (ISender<bool> sender) {
            _senderCount--;
            base.OnRemoved(sender);
        }

        public override bool Convert (bool value, float dt) {
            if (value) {
                _receivedTrueValues++;
            }

            return _receivedTrueValues == _senderCount;
        }
    }


    #endregion

}
