using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SomaSim.Math
{
    /// <summary>
    /// Piecewise linear function that maps floats to T
    /// </summary>
    public abstract class PiecewiseLinearMapper<T>
    {
        /// <summary>
        /// X values of the piecewise linear function
        /// </summary>
        public List<float> x = new List<float>();

        /// <summary>
        /// Y values for the function, have to be the same number as x values
        /// </summary>
        public List<T> y = new List<T>();

        /// <summary>
        /// Returns true if this mapper is empty, ie. contains no data points, and cannot be used.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty { get { return x.Count == 0 || y.Count == 0; } }

        /// <summary>
        /// Given some input x value, find an appropriate y value given 
        /// the piecewise linear function, interpolating between points as needed.
        /// This function is linear in the number of points in the function.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public T Eval (float input) {
            if (IsEmpty || x.Count != y.Count) {
                throw new Exception("Number mapping definition invalid");
            }

            float x0 = x[0];
            T y0 = y[0];
            if (input <= x[0]) {
                return y0;
            }

            for (int i = 1, len = x.Count; i < len; i++) {
                float x1 = x[i];
                T y1 = y[i];
                if (input <= x1) {
                    return Interpolate(input, x0, x1, y0, y1);
                }
                x0 = x1;
                y0 = y1;
            }

            return y0;
        }

        /** Interpolates between y values, given x values and a point between them. */
        protected abstract T Interpolate (float x, float x0, float x1, T y0, T y1);
    }

    /// <summary>
    /// Piecewise linear function that maps floats to floats
    /// </summary>
    public class FloatMapper : PiecewiseLinearMapper<float>
    {
        /// <inheritDoc/>
        override protected float Interpolate (float x, float x0, float x1, float y0, float y1) {
            float p = (x - x0) / (x1 - x0);
			return y0 + (y1 - y0) * p;
		}
    }


    /// <summary>
    /// Piecewise linear function that maps floats to colors
    /// </summary>
    public class ColorMapper : PiecewiseLinearMapper<Color32>
    {
        public void InitFromHexStrings (List<float> xs, List<string> colors) {
            this.x = xs;
            this.y = colors.Select(ColorUtil.HexToColor).ToList();
        }

        /// <inheritDoc/>
        override protected Color32 Interpolate (float x, float x0, float x1, Color32 y0, Color32 y1) {
            float p = (x - x0) / (x1 - x0);
            return Color32.Lerp(y0, y1, p);
        }
    }
}
