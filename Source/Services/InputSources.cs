using UnityEngine;

namespace SomaSim.Services
{
    abstract internal class InputSource
    {
        protected InputService _service;
        virtual public void Initialize(InputService service) { _service = service; }
        virtual public void Release() { _service = null; }

        abstract public void Update();
    }

    internal class TouchInputSource : InputSource
    {
        public float TouchZoomSensitivity = 3f;

        override public void Update()
        {    
            if (Input.touchCount <= 0)
            {
                return; // nothing to process!
            }

            // at least one touch

            Vector2 pos = Input.GetTouch(0).position;
            TouchPhase phase = Input.GetTouch(0).phase;
            _service.OnTouch(pos, phase);

            // if there's a second one, do zoom

            if (Input.touchCount > 1)
            {
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

        override public void Update()
        {
            if (!Input.mousePresent)
            {
                return; // nothing to process!
            }

            Vector2 pos = Input.mousePosition;

            if (Input.GetMouseButtonUp(0) && _mouseDown)
            {
                _service.OnTouch(pos, TouchPhase.Ended);
                _mouseDown = false;
            }

            if (Input.GetMouseButton(0))
            {
                if (_mouseDown)
                {
                    _service.OnTouch(pos, TouchPhase.Moved);
                }
                else
                {
                    // we need something for hover
                }
            }

            if (Input.GetMouseButtonDown(0) && !_mouseDown)
            {
                _service.OnTouch(pos, TouchPhase.Began);
                _mouseDown = true;
            }

            float wheel = Input.GetAxis(MouseScrollWheelName);
            if (wheel != 0)
            {
                _service.OnZoom(pos, wheel * MouseWheelSensitivity, true);
            }
        }
    }
}
