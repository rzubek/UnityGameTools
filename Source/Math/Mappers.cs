﻿// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SomaSim.Util
{
    /// <summary>
    /// Piecewise linear function that takes a mapping from floats to T, 
    /// and can be queried for any float values in between those specified.
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
        public bool IsEmpty => x.Count == 0 || y.Count == 0;

        /// <summary>
        /// Given some input x value, find an appropriate y value given 
        /// the piecewise linear function. This function is linear in the number of points in the function.
        /// </summary>
        abstract public T Eval (float input);
    }

    /// <summary>
    /// Base class for piecewise linear functions that interpolate between specified points.
    /// For example, given points (0, 0) and (1, 1), the mapper will return y = 0 for
    /// x in [-inf, 0], y = x in [0, 1], and y = 1 for x in [1, inf]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class InterpolatingPiecewiseLinearMapper<T> : PiecewiseLinearMapper<T>
    {
        override public T Eval (float input) {
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
    public class FloatMapper : InterpolatingPiecewiseLinearMapper<float>
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
    public class ColorMapper : InterpolatingPiecewiseLinearMapper<Color32>
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

    /// <summary>
    /// Piecewise linear mapper, where each point defines a horizontal segment (dy = 0)
    /// between itself and its next neignbor.
    /// 
    /// For example, given points (0, 0) and (1, 1), the mapper will return y = 0 for
    /// x in [-inf, 1), and y = 1 for x in [1, inf]
    /// </summary>
    public class StepFunctionMapper : PiecewiseLinearMapper<float>
    {
        /// <summary>
        /// Given some input x value, find an appropriate y value given by the step function.
        /// This function is linear in the number of points in the function.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        override public float Eval (float input) {
            if (IsEmpty || x.Count != y.Count) {
                throw new Exception("Number mapping definition invalid");
            }

            float y0 = y[0];
            if (input <= x[0]) {
                return y0;
            }

            for (int i = 1, len = x.Count; i < len; i++) {
                if (input < x[i]) {
                    break;
                }
                y0 = y[i];
            }

            return y0;
        }

    }
}
