using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SomaSim.Services
{
    public interface IInputHandler
    {
        void OnHandlerActivated();
        void OnHandlerDeactivated();

        bool OnMove(Vector2 pos);
        bool OnDown(int button, Vector2 pos);
        bool OnUp(int button, Vector2 pos);
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
        private LinkedList<IInputHandler> _handlers;

        public void Initialize()
        {
            _handlers = new LinkedList<IInputHandler>();
        }

        public void Release()
        {
            while (_handlers.Count > 0)
            {
                Pop();
            }

            _handlers = null;
        }

        #region Handler stack management

        public void Push(IInputHandler handler)
        {
            if (_handlers.Count > 0)
            {
                _handlers.First.Value.OnHandlerDeactivated();
            }

            _handlers.AddFirst(handler);
            handler.OnHandlerActivated();
        }

        public IInputHandler Pop()
        {
            IInputHandler result = null;

            if (_handlers.Count > 0)
            {
                result = _handlers.First.Value;
                _handlers.RemoveFirst();
                result.OnHandlerDeactivated();
            }

            if (_handlers.Count > 0)
            {
                _handlers.First.Value.OnHandlerActivated();
            }

            return result;
        }

        public IInputHandler Peek()
        {
            return _handlers.First.Value;
        }

        #endregion

        #region Input handling

        public void Update()
        {
            if (_handlers.Count == 0)
            {
                return; // nobody cares, wah wah
            }

            if (Input.mousePresent)
            {
                Vector2 mousepos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                _handlers.FirstOrDefault(h => h.OnMove(mousepos));

                if (Input.GetButtonDown("Fire1"))
                {
                    _handlers.FirstOrDefault(h => h.OnDown(0, mousepos));
                }
            }
        }


        #endregion

    }
}
