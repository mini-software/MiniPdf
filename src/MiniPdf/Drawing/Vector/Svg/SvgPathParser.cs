using System;
using System.Collections.Generic;
using System.Globalization;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Parses an SVG path <c>d</c> attribute (SVG 1.1 §8.3.9) into a
    /// <see cref="GraphicsPath"/>. Supports all commands: M/m, L/l, H/h, V/v,
    /// C/c, S/s, Q/q, T/t, A/a, Z/z. Relative commands resolve to absolute
    /// using the current point. Arcs are converted to cubic Bézier segments.
    /// </summary>
    internal static class SvgPathParser
    {
        /// <summary>
        /// Parses the path data string and returns a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="d">The path data string.</param>
        /// <param name="fillMode">The fill mode for the resulting path.</param>
        /// <returns>A <see cref="GraphicsPath"/> built from the parsed commands.</returns>
        public static GraphicsPath Parse(string? d, FillMode fillMode = FillMode.Alternate)
        {
            if (string.IsNullOrWhiteSpace(d))
                return new GraphicsPath(fillMode);

            var points = new List<PointF>();
            var types = new List<byte>();
            var tok = new Tokenizer(d);

            PointF cur = PointF.Empty;
            PointF start = PointF.Empty;
            PointF? prevCtrl = null;
            char cmd = '\0';
            bool newSubpath = true;

            while (!tok.IsAtEnd)
            {
                char c = tok.PeekChar();

                if (IsCommandLetter(c))
                {
                    cmd = tok.ReadChar();
                    prevCtrl = null;
                }

                if (cmd == '\0')
                    break;

                switch (char.ToUpperInvariant(cmd))
                {
                    case 'M':
                        {
                            bool rel = char.IsLower(cmd);
                            PointF p = ReadPoint(tok, cur, rel);
                            if (newSubpath)
                            {
                                AddPoint(points, types, p, PathPointType.Start);
                                start = p;
                                newSubpath = false;
                            }
                            else
                            {
                                AddPoint(points, types, p, PathPointType.Line);
                            }
                            cur = p;
                            // Subsequent pairs are implicit L
                            cmd = rel ? 'l' : 'L';
                            prevCtrl = null;
                            break;
                        }

                    case 'L':
                        {
                            bool rel = char.IsLower(cmd);
                            PointF p = ReadPoint(tok, cur, rel);
                            AddPoint(points, types, p, PathPointType.Line);
                            cur = p;
                            prevCtrl = null;
                            break;
                        }

                    case 'H':
                        {
                            bool rel = char.IsLower(cmd);
                            float x = ReadNumber(tok);
                            float y = cur.Y;
                            float nx = rel ? cur.X + x : x;
                            AddPoint(points, types, new PointF(nx, y), PathPointType.Line);
                            cur = new PointF(nx, y);
                            prevCtrl = null;
                            break;
                        }

                    case 'V':
                        {
                            bool rel = char.IsLower(cmd);
                            float y = ReadNumber(tok);
                            float x = cur.X;
                            float ny = rel ? cur.Y + y : y;
                            AddPoint(points, types, new PointF(x, ny), PathPointType.Line);
                            cur = new PointF(x, ny);
                            prevCtrl = null;
                            break;
                        }

                    case 'C':
                        {
                            bool rel = char.IsLower(cmd);
                            PointF c1 = ReadPoint(tok, cur, rel);
                            PointF c2 = ReadPoint(tok, cur, rel);
                            PointF e  = ReadPoint(tok, cur, rel);
                            AddBezier(points, types, c1, c2, e);
                            cur = e;
                            prevCtrl = c2;
                            break;
                        }

                    case 'S':
                        {
                            bool rel = char.IsLower(cmd);
                            PointF c2 = ReadPoint(tok, cur, rel);
                            PointF e  = ReadPoint(tok, cur, rel);
                            PointF c1 = prevCtrl.HasValue ? Reflect(prevCtrl.Value, cur) : cur;
                            AddBezier(points, types, c1, c2, e);
                            cur = e;
                            prevCtrl = c2;
                            break;
                        }

                    case 'Q':
                        {
                            bool rel = char.IsLower(cmd);
                            PointF q1 = ReadPoint(tok, cur, rel);
                            PointF e  = ReadPoint(tok, cur, rel);
                            // Degree-elevate quadratic to cubic
                            PointF c1 = Lerp(cur, q1, 2f / 3f);
                            PointF c2 = Lerp(e, q1, 2f / 3f);
                            AddBezier(points, types, c1, c2, e);
                            cur = e;
                            prevCtrl = q1;
                            break;
                        }

                    case 'T':
                        {
                            bool rel = char.IsLower(cmd);
                            PointF e = ReadPoint(tok, cur, rel);
                            PointF q1 = prevCtrl.HasValue ? Reflect(prevCtrl.Value, cur) : cur;
                            PointF c1 = Lerp(cur, q1, 2f / 3f);
                            PointF c2 = Lerp(e, q1, 2f / 3f);
                            AddBezier(points, types, c1, c2, e);
                            cur = e;
                            prevCtrl = q1;
                            break;
                        }

                    case 'A':
                        {
                            bool rel = char.IsLower(cmd);
                            float rx = ReadNumber(tok);
                            float ry = ReadNumber(tok);
                            float angle = ReadNumber(tok);
                            bool large = ReadFlag(tok);
                            bool sweep = ReadFlag(tok);
                            PointF p = ReadPoint(tok, cur, rel);
                            ArcToBezier(points, types, cur, p, rx, ry, angle, large, sweep);
                            cur = p;
                            prevCtrl = null;
                            break;
                        }

                    case 'Z':
                        {
                            if (types.Count > 0)
                                types[types.Count - 1] |= (byte)PathPointType.CloseSubpath;
                            cur = start;
                            newSubpath = true;
                            prevCtrl = null;
                            tok.SkipSeparators();
                            break;
                        }

                    default:
                        // Unknown command: skip rest
                        goto done;
                }
            }

            done:
            if (points.Count == 0)
                return new GraphicsPath(fillMode);

            return new GraphicsPath(points.ToArray(), types.ToArray(), fillMode);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static bool IsCommandLetter(char c)
        {
            return "MmLlHhVvCcSsQqTtAaZz".IndexOf(c) >= 0;
        }

        private static void AddPoint(List<PointF> points, List<byte> types, PointF p, PathPointType type)
        {
            points.Add(p);
            types.Add((byte)type);
        }

        private static void AddBezier(List<PointF> points, List<byte> types,
                                       PointF c1, PointF c2, PointF e)
        {
            points.Add(c1); types.Add((byte)PathPointType.Bezier);
            points.Add(c2); types.Add((byte)PathPointType.Bezier);
            points.Add(e);  types.Add((byte)PathPointType.Bezier);
        }

        private static PointF Reflect(PointF ctrl, PointF pivot)
            => new PointF(2f * pivot.X - ctrl.X, 2f * pivot.Y - ctrl.Y);

        private static PointF Lerp(PointF a, PointF b, float t)
            => new PointF(a.X + t * (b.X - a.X), a.Y + t * (b.Y - a.Y));

        private static PointF ReadPoint(Tokenizer tok, PointF cur, bool rel)
        {
            float x = ReadNumber(tok);
            float y = ReadNumber(tok);
            return rel ? new PointF(cur.X + x, cur.Y + y) : new PointF(x, y);
        }

        private static float ReadNumber(Tokenizer tok) => tok.ReadNumber();

        private static bool ReadFlag(Tokenizer tok) => tok.ReadFlag();

        // ── Arc-to-Bézier conversion (SVG 1.1 impl. note F.6.5) ───────────────

        private static void ArcToBezier(List<PointF> points, List<byte> types,
            PointF start, PointF end, float rx, float ry, float angleDeg,
            bool largeArc, bool sweep)
        {
            if (rx == 0 || ry == 0)
            {
                AddPoint(points, types, end, PathPointType.Line);
                return;
            }

            // If endpoints are identical, arc is a no-op
            if (start.X == end.X && start.Y == end.Y)
                return;

            rx = Math.Abs(rx);
            ry = Math.Abs(ry);

            double angle = angleDeg * Math.PI / 180.0;
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);

            // Step 1: compute (x1', y1')
            double dx = (start.X - end.X) / 2.0;
            double dy = (start.Y - end.Y) / 2.0;
            double x1p =  cosA * dx + sinA * dy;
            double y1p = -sinA * dx + cosA * dy;

            // Step 2: compute (cx', cy')
            double rx2 = rx * rx;
            double ry2 = ry * ry;
            double x1p2 = x1p * x1p;
            double y1p2 = y1p * y1p;

            double lambda = (x1p2 / rx2) + (y1p2 / ry2);
            if (lambda > 1.0)
            {
                double s = Math.Sqrt(lambda);
                rx *= (float)s;
                ry *= (float)s;
                rx2 = rx * rx;
                ry2 = ry * ry;
            }

            double sign = (largeArc != sweep) ? 1.0 : -1.0;
            double num = rx2 * ry2 - rx2 * y1p2 - ry2 * x1p2;
            double den = rx2 * y1p2 + ry2 * x1p2;
            den = Math.Max(den, 1e-12);
            double coef = sign * Math.Sqrt(Math.Max(0.0, num / den));
            double cxp =  coef * (rx * y1p / ry);
            double cyp = -coef * (ry * x1p / rx);

            // Step 3: compute (cx, cy)
            double cx = cosA * cxp - sinA * cyp + (start.X + end.X) / 2.0;
            double cy = sinA * cxp + cosA * cyp + (start.Y + end.Y) / 2.0;

            // Step 4: compute theta1 and delta-theta
            double ux = (x1p - cxp) / rx;
            double uy = (y1p - cyp) / ry;
            double vx = (-x1p - cxp) / rx;
            double vy = (-y1p - cyp) / ry;

            double theta1 = VectorAngle(1, 0, ux, uy);
            double delta = VectorAngle(ux, uy, vx, vy);

            if (!sweep && delta > 0)
                delta -= 2 * Math.PI;
            else if (sweep && delta < 0)
                delta += 2 * Math.PI;

            // Split into segments of at most 90°
            int segments = Math.Max(1, (int)Math.Ceiling(Math.Abs(delta) / (Math.PI / 2)));
            double segAngle = delta / segments;

            for (int i = 0; i < segments; i++)
            {
                double a0 = theta1 + segAngle * i;
                double a1 = theta1 + segAngle * (i + 1);

                // Bézier approximation of an elliptical arc segment
                double t = 4.0 / 3.0 * Math.Tan(segAngle / 4.0);

                double cos0 = Math.Cos(a0), sin0 = Math.Sin(a0);
                double cos1 = Math.Cos(a1), sin1 = Math.Sin(a1);

                // Points on the unit circle, then scale/rotate/translate
                double p0x = cos0, p0y = sin0;
                double p1x = cos0 + t * sin0, p1y = sin0 - t * cos0;
                double p2x = cos1 - t * sin1, p2y = sin1 + t * cos1;
                double p3x = cos1, p3y = sin1;

                // Scale by rx, ry
                p0x *= rx; p0y *= ry;
                p1x *= rx; p1y *= ry;
                p2x *= rx; p2y *= ry;
                p3x *= rx; p3y *= ry;

                // Rotate by angle
                double rp0x = cosA * p0x - sinA * p0y;
                double rp0y = sinA * p0x + cosA * p0y;
                double rp1x = cosA * p1x - sinA * p1y;
                double rp1y = sinA * p1x + cosA * p1y;
                double rp2x = cosA * p2x - sinA * p2y;
                double rp2y = sinA * p2x + cosA * p2y;
                double rp3x = cosA * p3x - sinA * p3y;
                double rp3y = sinA * p3x + cosA * p3y;

                // Translate by (cx, cy)
                var c1 = new PointF((float)(rp1x + cx), (float)(rp1y + cy));
                var c2 = new PointF((float)(rp2x + cx), (float)(rp2y + cy));
                var ep = new PointF((float)(rp3x + cx), (float)(rp3y + cy));

                AddBezier(points, types, c1, c2, ep);
            }
        }

        private static double VectorAngle(double ux, double uy, double vx, double vy)
        {
            double dot = ux * vx + uy * vy;
            double len = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
            if (len == 0) return 0;
            double cos = Math.Max(-1.0, Math.Min(1.0, dot / len));
            double a = Math.Acos(cos);
            if (ux * vy - uy * vx < 0)
                a = -a;
            return a;
        }

        // ── Tokenizer ─────────────────────────────────────────────────────────

        private sealed class Tokenizer
        {
            private readonly string _s;
            private int _i;

            public Tokenizer(string s) { _s = s; _i = 0; }

            public bool IsAtEnd => _i >= _s.Length;

            public char PeekChar() => _i < _s.Length ? _s[_i] : '\0';

            public char ReadChar() => _s[_i++];

            public void SkipSeparators()
            {
                while (_i < _s.Length && (char.IsWhiteSpace(_s[_i]) || _s[_i] == ','))
                    _i++;
            }

            public float ReadNumber()
            {
                SkipSeparators();
                int start = _i;

                if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++;
                while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                if (_i < _s.Length && _s[_i] == '.')
                {
                    _i++;
                    while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                }
                if (_i < _s.Length && (_s[_i] == 'e' || _s[_i] == 'E'))
                {
                    _i++;
                    if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++;
                    while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                }

                if (_i == start)
                {
                    _i++;
                    return 0f;
                }

                float.TryParse(_s.Substring(start, _i - start), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float val);
                return val;
            }

            public bool ReadFlag()
            {
                // Flags are exactly one character ('0' or '1') with no separators
                // consumed before them; but whitespace between args is allowed.
                SkipSeparators();
                if (_i >= _s.Length) return false;
                char c = _s[_i];
                if (c == '0' || c == '1')
                {
                    _i++;
                    return c == '1';
                }
                // Tolerant: try reading a number
                return ReadNumber() != 0f;
            }
        }
    }
}