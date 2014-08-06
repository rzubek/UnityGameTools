using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SomaSim.Math
{
    /// <summary>
    /// Collection of useful Vector utilities.
    /// </summary>
    public class VectorUtil
    {
        /// <summary>
        /// Distance between two vectors using a specified norm.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p">p is the Minkowski p-norm of this distance function.
		///      p = 2 means the standard Euclidean distance (default). 
		///      p = 1 means taxicab distance.
		///      p = PositiveInfinity means the Chebyshev / chess king distance (max across all dimensions).
		///       Other p-norms are also supported, as long as p >= 1.</param>
        /// <returns></returns>
        public static float Distance (Vector2 a, Vector2 b, float p = 2) {

            if (p == 2) { // optimization for the default case
                return Vector2.Distance(a, b);
            }

            float absdx = Mathf.Abs(a.x - b.x);
            float absdy = Mathf.Abs(a.y - b.y);

            if (p == 1) {
                return absdx + absdy;
            } else if (float.IsPositiveInfinity(p)) {
                return Mathf.Max(absdx, absdy);
            } else { // generic case
                if (p < 1) {
                    throw new Exception("P-norm < 1 not supported!");
                }
                return Mathf.Pow((Mathf.Pow(absdx, p) + Mathf.Pow(absdy, p)), 1 / p);
            }
        }
    }
}
