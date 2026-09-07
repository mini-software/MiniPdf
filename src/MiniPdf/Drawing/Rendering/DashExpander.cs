using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Rendering
{
    internal static class DashExpander
    {
        public static GraphicsPath Expand(GraphicsPath path, Pens.Pen pen, float flatness = 0.25f)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            float[] pattern = GetPattern(pen);
            if (pattern.Length == 0)
                return path.Clone();

            GraphicsPath flat = path.Clone();
            flat.Flatten(null, flatness);
            PointF[] points = flat.PathPoints;
            byte[] types = flat.PathTypes;

            var dashed = new GraphicsPath(flat.FillMode);
            int subpathStart = -1;
            float patternPos = pen.DashOffset * pen.Width;
            int patternIndex = 0;
            bool draw = true;

            for (int i = 0; i < points.Length; i++)
            {
                byte raw = (byte)(types[i] & (byte)PathPointType.PathTypeMask);
                if (raw == (byte)PathPointType.Start)
                {
                    subpathStart = i;
                    continue;
                }

                if (i > 0)
                    EmitDashedSegment(dashed, points[i - 1], points[i], pattern, ref patternIndex, ref patternPos, ref draw);

                bool closes = (types[i] & (byte)PathPointType.CloseSubpath) != 0;
                if (closes && subpathStart >= 0 && subpathStart != i)
                    EmitDashedSegment(dashed, points[i], points[subpathStart], pattern, ref patternIndex, ref patternPos, ref draw);
            }

            return dashed;
        }

        private static void EmitDashedSegment(
            GraphicsPath output,
            PointF a,
            PointF b,
            float[] pattern,
            ref int patternIndex,
            ref float patternPos,
            ref bool draw)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len <= 1e-6f)
                return;

            float ux = dx / len;
            float uy = dy / len;
            float consumed = 0f;

            while (consumed < len)
            {
                float dashLen = Math.Max(1e-6f, pattern[patternIndex]);
                float remainInDash = dashLen - patternPos;
                float run = Math.Min(remainInDash, len - consumed);

                if (draw)
                {
                    PointF p0 = new PointF(a.X + ux * consumed, a.Y + uy * consumed);
                    PointF p1 = new PointF(a.X + ux * (consumed + run), a.Y + uy * (consumed + run));
                    output.StartFigure();
                    output.AddLine(p0, p1);
                }

                consumed += run;
                patternPos += run;

                if (patternPos >= dashLen - 1e-6f)
                {
                    patternPos = 0f;
                    patternIndex = (patternIndex + 1) % pattern.Length;
                    draw = !draw;
                }
            }
        }

        private static float[] GetPattern(Pens.Pen pen)
        {
            if (pen.DashStyle == DashStyle.Custom)
            {
                float[] custom = pen.DashPattern;
                if (custom != null && custom.Length > 0)
                    return ScalePattern(custom, pen.Width);
                return Array.Empty<float>();
            }

            switch (pen.DashStyle)
            {
                case DashStyle.Solid:
                    return Array.Empty<float>();
                case DashStyle.Dash:
                    return new[] { 3f * pen.Width, 1f * pen.Width };
                case DashStyle.Dot:
                    return new[] { 1f * pen.Width, 1f * pen.Width };
                case DashStyle.DashDot:
                    return new[] { 3f * pen.Width, 1f * pen.Width, 1f * pen.Width, 1f * pen.Width };
                case DashStyle.DashDotDot:
                    return new[] { 3f * pen.Width, 1f * pen.Width, 1f * pen.Width, 1f * pen.Width, 1f * pen.Width, 1f * pen.Width };
                default:
                    return Array.Empty<float>();
            }
        }

        private static float[] ScalePattern(float[] pattern, float width)
        {
            var result = new float[pattern.Length];
            for (int i = 0; i < pattern.Length; i++)
                result[i] = Math.Max(1e-6f, pattern[i] * width);
            return result;
        }
    }
}
