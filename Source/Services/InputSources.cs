using UnityEngine;

namespace SomaSim.Services
{
    public enum InputPhase
    {
        None,
        HoverMoved,
        TouchBegan,
        TouchMoved,
        TouchEnded
    }

    abstract internal class InputSource
    {
        protected InputService _service;
        virtual public void Initialize (InputService service) { _service = service; }
        virtual public void Release () { _service = null; }

        abstract public void Update ();
    }

    internal class TouchInputSource : InputSource
    {
        public float TouchZoomSensitivity = 3f;

        private InputPhase TouchToInput (TouchPhase phase) {
            switch (phase) {
                case TouchPhase.Began: return InputPhase.TouchBegan;
                case TouchPhase.Ended: return InputPhase.TouchEnded;
                case TouchPhase.Moved: return InputPhase.TouchMoved;
                case TouchPhase.Stationary: return InputPhase.TouchMoved;
                case TouchPhase.Canceled: return InputPhase.TouchEnded;
                default: return InputPhase.None;
            }
        }

        override public void Update () {
            if (Input.touchCount <= 0) {
                return; // nothing to process!
            }

            // exactly one touch
            if (Input.touchCount == 1) {
                Vector2 pos = Input.GetTouch(0).position;
                TouchPhase phase = Input.GetTouch(0).phase;
                _service.OnInput(pos, TouchToInput(phase));
            }

            // if there's a second one, do zoom
            if (Input.touchCount > 1) {
                Vector2 q0 = Input.GetTouch(0).position, q1 = Input.GetTouch(1).position;
                Vector2 p0 = q0 + Input.GetTouch(0).deltaPosition, p1 = q1 + Input.GetTouch(1).deltaPosition;
                float dp = Vector2.Distance(p0, p1);
                float dq = Vector2.Distance(q0, q1);
                float zoom = dp - dq;
                zoom *= TouchZoomSensitivity / _service.dpi; // needs to be DPI dependent
                _service.OnZoom(q0, zoom, false);
            }
        }
    }

    internal class MouseInputSource : InputSource
    {
        public string MouseScrollWheelName = "Mouse ScrollWheel";
        public float MouseWheelSensitivity = 2f;

        private bool _mouseDown = false;

        override public void Update () {
            if (!Input.mousePresent) {
                return; // nothing to process!
            }

            Vector2 pos = Input.mousePosition;

            if ((Input.GetMouseButtonUp(0) && _mouseDown) || // regular mouse up, or
                (!Input.GetMouseButton(0) && _mouseDown))    // we lost the mouse up event somewhere
            {
                _service.OnInput(pos, InputPhase.TouchEnded);
                _mouseDown = false;
            }

            if (Input.GetMouseButton(0) && _mouseDown) {
                _service.OnInput(pos, InputPhase.TouchMoved);
            }

            if (Input.GetMouseButtonDown(0) && !_mouseDown) {
                _service.OnInput(pos, InputPhase.TouchBegan);
                _mouseDown = true;
            }

            if (!Input.GetMouseButton(0)) {
                _service.OnInput(pos, InputPhase.HoverMoved);
            }

            if (Input.GetMouseButtonDown(1)) // right button cancels
            {
                _service.OnCancel();
            }

            float wheel = Input.GetAxis(MouseScrollWheelName);
            if (wheel != 0) {
                _service.OnZoom(pos, wheel * MouseWheelSensitivity, true);
            }
        }
    }
}
