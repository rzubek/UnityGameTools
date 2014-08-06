using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SomaSim.Services
{
    public enum InputButton
    {
        None = 0,
        Left,
        Right,
        Middle,
        Wheel
    }

    public interface IInputHandler
    {
        void OnHandlerActivated ();
        void OnHandlerDeactivated ();

        bool OnTouch (Vector2 pos, InputPhase phase);
        //bool OnDown(InputButton button, Vector2 pos);
        //bool OnUp(InputButton button, Vector2 pos);
        bool OnZoom (Vector2 pos, float zoom, bool oneshot);
        bool OnCancel ();
    }


    /// <summary>
    /// This service unifies input handling between touch and mouse input. It keeps track
    /// of both kinds of activity, normalizes them into a mouse-compatible action type,
    /// and passes it to the input handler. 
    /// 
    /// Mouse and single-finger touch actions are collapsed into a mouse-like actions
    /// (move, down, drag, up, click), while mouse wheel and two-finger pinch actions 
    /// collapse into a zoom action. Other touch gestures are not currently supported.
    /// 
    /// Additionally, the service supports a stack of handlers. On every action,
    /// the topmost handler will be given the opportunity to handle it and stop
    /// propagation, or ignore the event and let it propagate to the next handler on the stack.
    /// </summary>
    public class InputService : IUpdateService
    {
        private List<InputSource> _sources;
        private LinkedList<IInputHandler> _handlers;

        public float dpi { get; private set; }

        public void Initialize () {
            dpi = (Screen.dpi > 0) ? Screen.dpi : 72;

            _sources = new List<InputSource>() { new MouseInputSource(), new TouchInputSource() };
            _sources.ForEach(src => src.Initialize(this));

            _handlers = new LinkedList<IInputHandler>();
        }

        public void Release () {
            while (_handlers.Count > 0) {
                Pop();
            }

            _handlers = null;

            _sources.ForEach(src => src.Release());
            _sources = null;
        }

        #region Handler stack management

        public void Push (IInputHandler handler) {
            if (_handlers.Count > 0) {
                _handlers.First.Value.OnHandlerDeactivated();
            }

            _handlers.AddFirst(handler);
            handler.OnHandlerActivated();
        }

        public IInputHandler Pop () {
            IInputHandler result = null;

            if (_handlers.Count > 0) {
                result = _handlers.First.Value;
                _handlers.RemoveFirst();
                result.OnHandlerDeactivated();
            }

            if (_handlers.Count > 0) {
                _handlers.First.Value.OnHandlerActivated();
            }

            return result;
        }

        public IInputHandler Peek () {
            return _handlers.First.Value;
        }

        public void Replace (IInputHandler handler) {
            while (_handlers.Count > 0) {
                Pop();
            }
            Push(handler);
        }

        #endregion

        #region Input handling

        public void Update () {
            _sources.ForEach(src => src.Update());
        }

        internal void OnInput (Vector2 pos, InputPhase phase) {
            _handlers.FirstOrDefault(h => h.OnTouch(pos, phase));
        }

        internal void OnZoom (Vector2 pos, float zoom, bool oneshot) {
            _handlers.FirstOrDefault(h => h.OnZoom(pos, zoom, oneshot));
        }

        internal void OnCancel () {
            _handlers.FirstOrDefault(h => h.OnCancel());
        }

        #endregion

    }
}
