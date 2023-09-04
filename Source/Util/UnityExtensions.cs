// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using UnityEngine;
using UnityEngine.Events;

namespace SomaSim.Util
{
    /// <summary>
    /// Extensions for built-in unity classes
    /// </summary>
    public static class UnityExtensions
    {
        public static void SetListener (this UnityEvent evt, UnityAction call) {
            evt.RemoveAllListeners();
            evt.AddListener(call);
        }

        public static void SetListener<T> (this UnityEvent<T> evt, UnityAction<T> call) {
            evt.RemoveAllListeners();
            evt.AddListener(call);
        }

        public static void SetListener (this UnityEngine.UI.InputField.SubmitEvent evt, UnityAction<string> call) {
            evt.RemoveAllListeners();
            evt.AddListener(call);
        }

        public static void SetListener (this UnityEngine.UI.Slider.SliderEvent evt, UnityAction<float> call) {
            evt.RemoveAllListeners();
            evt.AddListener(call);
        }

        public static void SetListener (this UnityEngine.UI.Toggle.ToggleEvent evt, UnityAction<bool> call) {
            evt.RemoveAllListeners();
            evt.AddListener(call);
        }
    }

    /// <summary>
    /// Extensions for built-in Vector2
    /// </summary>
    public static class Vector2Extensions
    {
        public static Vector2 Add (this Vector2 vec, float x, float y) =>
            new Vector2(vec.x + x, vec.y + y);

        public static Vector2 NewX (this Vector2 vec, float x) =>
            new Vector2(x, vec.y);

        public static Vector2 NewY (this Vector2 vec, float y) =>
            new Vector2(vec.x, y);

        public static Vector2 Multiply (this Vector2 vec, float scale) =>
            new Vector2(vec.x * scale, vec.y * scale);
    }

    /// <summary>
    /// Extensions for built-in Vector3
    /// </summary>
    public static class Vector3Extensions
    {
        public static Vector3 Add (this Vector3 vec, float x, float y) =>
            new Vector3(vec.x + x, vec.y + y, vec.z);

        public static Vector3 Add (this Vector3 vec, float x, float y, float z) =>
            new Vector3(vec.x + x, vec.y + y, vec.z + z);

        public static Vector3 NewX (this Vector3 vec, float x) =>
            new Vector3(x, vec.y, vec.z);

        public static Vector3 NewY (this Vector3 vec, float y) =>
            new Vector3(vec.x, y, vec.z);

        public static Vector3 NewZ (this Vector3 vec, float z) =>
            new Vector3(vec.x, vec.y, z);

        public static Vector3 Multiply (this Vector3 vec, float scale) =>
            new Vector3(vec.x * scale, vec.y * scale, vec.z * scale);
    }

    /// <summary>
    /// Extensions for built-in Vector2
    /// </summary>
    public static class RectExtensions
    {
        public static Rect AddXY (this Rect rect, Vector2 xy) =>
            new Rect(rect.x + xy.x, rect.y + xy.y, rect.width, rect.height);

        public static Rect AddXY (this Rect rect, float x, float y) =>
            new Rect(rect.x + x, rect.y + y, rect.width, rect.height);

        public static Rect Add (this Rect rect, float x, float y, float width, float height) =>
            new Rect(rect.x + x, rect.y + y, rect.width + width, rect.height + height);

        public static Rect NewXY (this Rect rect, Vector2 xy) =>
            new Rect(xy.x, xy.y, rect.width, rect.height);

        public static Rect NewXY (this Rect rect, float x, float y) =>
            new Rect(x, y, rect.width, rect.height);

        public static Rect NewX (this Rect rect, float x) =>
            new Rect(x, rect.y, rect.width, rect.height);

        public static Rect NewY (this Rect rect, float y) =>
            new Rect(rect.x, y, rect.width, rect.height);

        public static Rect NewWH (this Rect rect, Vector2 wh) =>
            new Rect(rect.x, rect.y, wh.x, wh.y);

        public static Rect NewWH (this Rect rect, float width, float height) =>
            new Rect(rect.x, rect.y, width, height);

        public static Rect NewWidth (this Rect rect, float width) =>
            new Rect(rect.x, rect.y, width, rect.height);

        public static Rect NewHeight (this Rect rect, float height) =>
            new Rect(rect.x, rect.y, rect.width, height);
    }


    /// <summary>
    /// Collection of useful Vector utilities.
    /// </summary>
    public class VectorUtil
    {
        /// <summary>
        /// Returns Taxicab Distance (ie. Minkowski 1-norm distance) between two vectors.
        /// </summary>
        public static float DistanceTaxicab (Vector2 a, Vector2 b) {
            float absdx = Mathf.Abs(a.x - b.x);
            float absdy = Mathf.Abs(a.y - b.y);
            return absdx + absdy;
        }

        /// <summary>
        /// Returns the square of the distance between two vectors 
        /// (faster than regular distance which involves a square root)
        /// </summary>
        public static float DistanceSquared (Vector2 a, Vector2 b) {
            float dx = (a.x - b.x);
            float dy = (a.y - b.y);
            return dx * dx + dy * dy;
        }
    }
}
