// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using UnityEngine;

namespace SomaSim.Util
{
    /// <summary>
    /// Collection of color utilities
    /// </summary>
    public static class ColorUtil
    {
        /// <summary>
        /// Converts a Color into an HTML compatible hex value in the "rrggbb" format. Alpha is ignored.
        /// Adopted from http://wiki.unity3d.com/index.php?title=HexConverter
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ColorToHex (Color32 color, bool includeAlpha = false) =>
            color.r.ToString("X2") +
            color.g.ToString("X2") +
            color.b.ToString("X2") +
            (includeAlpha ? color.a.ToString("X2") : "");

        /// <summary>
        /// Converts an HTML compatible hex string in the "rrggbb[aa]" or "#rrggbb[aa]" format into a Color value.
        /// Alpha is set to full (1.0). Adopted from http://wiki.unity3d.com/index.php?title=HexConverter
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color32 HexToColor (string hex) {
            int i = hex[0] == '#' ? 1 : 0;
            byte r = byte.Parse(hex.Substring(i + 0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(i + 2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(i + 4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = (hex.Length == 8 || hex.Length == 9) ?
                byte.Parse(hex.Substring(i + 6, 2), System.Globalization.NumberStyles.HexNumber) :
                (byte) 255;
            return new Color32(r, g, b, a);
        }

        /// <summary> Returns the same color with alpha set to specified value </summary>
        public static Color32 SetAlpha (this Color32 c, byte alpha) => new Color32(c.r, c.g, c.b, alpha);

        /// <summary> Returns the same color with alpha set to specified value </summary>
        public static Color32 SetAlpha (this Color32 c, float alpha) => SetAlpha(c, (byte) (alpha * 255));

        /// <summary> Returns the same color with alpha set to specified value </summary>
        public static Color SetAlpha (this Color c, float alpha) => new Color(c.r, c.g, c.b, alpha);
    }
}
