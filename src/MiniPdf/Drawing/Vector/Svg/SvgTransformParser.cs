using System;
using System.Collections.Generic;
using System.Globalization;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Geometry;

namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// Parses an SVG <c>transform</c> attribute value (e.g.
    /// <c>translate(10,20) scale(2)</c>) into a <see cref="Matrix"/>.
    /// Supports matrix, translate, scale, rotate, skewX, and skewY.
    /// </summary>
    internal static class SvgTransformParser
    {
        /// <summary>
        /// Parses the transform list and returns the combined matrix.
        /// For a list <c>T1 T2 T3</c> the combined matrix is <c>T1 * T2 * T3</c>
        /// (SVG convention: rightmost applied first to points).
        /// </summary>
        public static Matrix Parse(string? transform)
        {
            var result = new Matrix();
            if (string.IsNullOrWhiteSpace(transform))
                return result;

            int i = 0;
            int len = transform.Length;

            while (i < len)
            {
                // Skip whitespace and commas
                SkipSeparators(transform, ref i, len);
                if (i >= len) break;

                // Read function name
                int start = i;
                while (i < len && (char.IsLetter(transform[i]) || transform[i] == '_'))
                    i++;

                if (i == start) { i++; continue; }

                string name = transform.Substring(start, i - start);

                // Skip whitespace and '('
                SkipSeparators(transform, ref i, len);
                if (i < len && transform[i] == '(') i++;
                SkipSeparators(transform, ref i, len);

                // Read arguments
                var args = new List<float>();
                while (i < len && transform[i] != ')')
                {
                    if (TryReadNumber(transform, ref i, len, out float val))
                        args.Add(val);
                    else
                        i++;

                    SkipSeparators(transform, ref i, len);
                }

                // Skip ')'
                if (i < len && transform[i] == ')') i++;

                // Apply transform (Append = SVG left-multiply, so T1 then T2 gives T1*T2)
                Matrix m = CreateTransform(name, args);
                result.Multiply(m, MatrixOrder.Append);
                m.Dispose();
            }

            return result;
        }

        private static Matrix CreateTransform(string name, List<float> args)
        {
            switch (name)
            {
                case "matrix":
                    if (args.Count >= 6)
                        return new Matrix(args[0], args[1], args[2], args[3], args[4], args[5]);
                    break;

                case "translate":
                    {
                        float tx = args.Count > 0 ? args[0] : 0f;
                        float ty = args.Count > 1 ? args[1] : 0f;
                        var m = new Matrix();
                        m.Translate(tx, ty);
                        return m;
                    }

                case "scale":
                    {
                        float sx = args.Count > 0 ? args[0] : 1f;
                        float sy = args.Count > 1 ? args[1] : sx;
                        var m = new Matrix();
                        m.Scale(sx, sy);
                        return m;
                    }

                case "rotate":
                    {
                        float angle = args.Count > 0 ? args[0] : 0f;
                        var m = new Matrix();
                        if (args.Count >= 3)
                            m.RotateAt(angle, new PointF(args[1], args[2]));
                        else
                            m.Rotate(angle);
                        return m;
                    }

                case "skewX":
                    {
                        float angle = args.Count > 0 ? args[0] : 0f;
                        float t = (float)Math.Tan(angle * Math.PI / 180.0);
                        var m = new Matrix();
                        m.Shear(t, 0f);
                        return m;
                    }

                case "skewY":
                    {
                        float angle = args.Count > 0 ? args[0] : 0f;
                        float t = (float)Math.Tan(angle * Math.PI / 180.0);
                        var m = new Matrix();
                        m.Shear(0f, t);
                        return m;
                    }
            }

            return new Matrix();
        }

        private static void SkipSeparators(string s, ref int i, int len)
        {
            while (i < len && (char.IsWhiteSpace(s[i]) || s[i] == ','))
                i++;
        }

        private static bool TryReadNumber(string s, ref int i, int len, out float value)
        {
            int start = i;

            // Optional sign
            if (i < len && (s[i] == '+' || s[i] == '-')) i++;

            // Integer part
            while (i < len && char.IsDigit(s[i])) i++;

            // Fractional part
            if (i < len && s[i] == '.')
            {
                i++;
                while (i < len && char.IsDigit(s[i])) i++;
            }

            // Exponent
            if (i < len && (s[i] == 'e' || s[i] == 'E'))
            {
                i++;
                if (i < len && (s[i] == '+' || s[i] == '-')) i++;
                while (i < len && char.IsDigit(s[i])) i++;
            }

            if (i == start)
            {
                value = 0;
                return false;
            }

            string numStr = s.Substring(start, i - start);
            return float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}