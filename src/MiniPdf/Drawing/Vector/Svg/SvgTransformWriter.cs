using System.Globalization;
using MiniPdf.Drawing.Drawing2D;

namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// Converts a <see cref="Matrix"/> to an SVG <c>transform</c> attribute
    /// value. Always emits the matrix form for lossless round-tripping.
    /// </summary>
    internal static class SvgTransformWriter
    {
        /// <summary>
        /// Returns <c>matrix(a b c d e f)</c> for the given matrix, or an empty
        /// string if the matrix is the identity.
        /// </summary>
        public static string WriteTransform(Matrix matrix, int precision = 4)
        {
            if (matrix == null || matrix.IsIdentity) return string.Empty;

            var e = matrix.Elements;
            // SVG matrix(a b c d e f) maps to GDI+ [M11 M12 M21 M22 DX DY]
            return string.Format(CultureInfo.InvariantCulture,
                "matrix({0} {1} {2} {3} {4} {5})",
                Fmt(e[0], precision), Fmt(e[1], precision),
                Fmt(e[2], precision), Fmt(e[3], precision),
                Fmt(e[4], precision), Fmt(e[5], precision));
        }

        private static string Fmt(float v, int precision)
        {
            if (float.IsNaN(v) || float.IsInfinity(v)) v = 0f;
            string s = v.ToString("F" + precision, CultureInfo.InvariantCulture);
            if (s.IndexOf('.') >= 0)
                s = s.TrimEnd('0').TrimEnd('.');
            if (s.Length == 0 || s == "-") s = "0";
            return s;
        }
    }
}