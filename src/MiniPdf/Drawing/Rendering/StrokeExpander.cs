using System;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Rendering
{
    internal static class StrokeExpander
    {
        public static GraphicsPath Expand(GraphicsPath source, Pens.Pen pen, Matrix? transform = null, float flatness = 0.25f)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            GraphicsPath effective = DashExpander.Expand(source, pen, flatness);
            if (transform != null)
                effective.Transform(transform);

            effective.Flatten(null, flatness);

            PointF[] points = effective.PathPoints;
            byte[] types = effective.PathTypes;
            var outline = new GraphicsPath(FillMode.Winding);

            float halfWidth = Math.Max(0.5f, pen.Width * 0.5f);
            int subpathStart = -1;
            PointF? firstDir = null;
            PointF? lastDir = null;

            for (int i = 0; i < points.Length; i++)
            {
                byte raw = (byte)(types[i] & (byte)PathPointType.PathTypeMask);
                if (raw == (byte)PathPointType.Start)
                {
                    subpathStart = i;
                    firstDir = null;
                    lastDir = null;
                    continue;
                }

                PointF a = points[i - 1];
                PointF b = points[i];
                PointF dir = new PointF(b.X - a.X, b.Y - a.Y);
                if (Math.Abs(dir.X) < 1e-6f && Math.Abs(dir.Y) < 1e-6f)
                    continue;

                AddSegmentQuad(outline, a, b, halfWidth);
                if (firstDir == null)
                    firstDir = dir;
                lastDir = dir;

                bool closes = (types[i] & (byte)PathPointType.CloseSubpath) != 0;
                if (closes && subpathStart >= 0 && subpathStart != i)
                {
                    PointF c = points[subpathStart];
                    PointF closeDir = new PointF(c.X - b.X, c.Y - b.Y);
                    if (Math.Abs(closeDir.X) > 1e-6f || Math.Abs(closeDir.Y) > 1e-6f)
                        AddSegmentQuad(outline, b, c, halfWidth);
                }
                else if (subpathStart >= 0 && i == subpathStart + 1)
                {
                    LineCapRenderer.AddCap(outline, a, dir, halfWidth, pen.StartCap);
                }

                bool endOfOpenSubpath = !closes && (i + 1 >= points.Length || (types[i + 1] & (byte)PathPointType.PathTypeMask) == (byte)PathPointType.Start);
                if (endOfOpenSubpath && lastDir != null)
                    LineCapRenderer.AddCap(outline, b, lastDir.Value, halfWidth, pen.EndCap);
            }

            return outline;
        }

        private static void AddSegmentQuad(GraphicsPath path, PointF a, PointF b, float halfWidth)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len <= 1e-6f)
                return;

            float nx = -dy / len * halfWidth;
            float ny = dx / len * halfWidth;

            PointF p0 = new PointF(a.X + nx, a.Y + ny);
            PointF p1 = new PointF(a.X - nx, a.Y - ny);
            PointF p2 = new PointF(b.X - nx, b.Y - ny);
            PointF p3 = new PointF(b.X + nx, b.Y + ny);
            path.AddPolygon(new[] { p0, p1, p2, p3 });
        }
    }
}
