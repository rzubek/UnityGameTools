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

    /// <summary>
    /// Collection of color utilities
    /// </summary>
    public class ColorUtil
    {
        /// <summary>
        /// Converts a Color into an HTML compatible hex value in the "rrggbb" format. Alpha is ignored.
        /// Adopted from http://wiki.unity3d.com/index.php?title=HexConverter
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ColorToHex (Color32 color) {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        }

        /// <summary>
        /// Converts an HTML compatible hex string in the "rrggbb" format into a Color value.
        /// Alpha is set to full (1.0). Adopted from http://wiki.unity3d.com/index.php?title=HexConverter
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color32 HexToColor (string hex) {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }
    }
}
