using System;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Rendering
{
    internal static class LineCapRenderer
    {
        public static void AddCap(GraphicsPath path, PointF point, PointF direction, float halfWidth, LineCap cap)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (halfWidth <= 0f || cap == LineCap.Flat)
                return;

            switch (cap)
            {
                case LineCap.Round:
                    path.AddEllipse(point.X - halfWidth, point.Y - halfWidth, halfWidth * 2f, halfWidth * 2f);
                    break;
                case LineCap.Square:
                    AddOrientedSquare(path, point, direction, halfWidth);
                    break;
                case LineCap.Triangle:
                    AddOrientedTriangle(path, point, direction, halfWidth);
                    break;
            }
        }

        private static void AddOrientedSquare(GraphicsPath path, PointF p, PointF dir, float half)
        {
            var d = Normalize(dir);
            var n = new PointF(-d.Y, d.X);

            PointF a = new PointF(p.X + (-d.X - n.X) * half, p.Y + (-d.Y - n.Y) * half);
            PointF b = new PointF(p.X + (-d.X + n.X) * half, p.Y + (-d.Y + n.Y) * half);
            PointF c = new PointF(p.X + ( d.X + n.X) * half, p.Y + ( d.Y + n.Y) * half);
            PointF dpt = new PointF(p.X + ( d.X - n.X) * half, p.Y + ( d.Y - n.Y) * half);
            path.AddPolygon(new[] { a, b, c, dpt });
        }

        private static void AddOrientedTriangle(GraphicsPath path, PointF p, PointF dir, float half)
        {
            var d = Normalize(dir);
            var n = new PointF(-d.Y, d.X);

            PointF tip = new PointF(p.X + d.X * (half * 1.5f), p.Y + d.Y * (half * 1.5f));
            PointF left = new PointF(p.X - d.X * half + n.X * half, p.Y - d.Y * half + n.Y * half);
            PointF right = new PointF(p.X - d.X * half - n.X * half, p.Y - d.Y * half - n.Y * half);
            path.AddPolygon(new[] { left, tip, right });
        }

        private static PointF Normalize(PointF p)
        {
            float len = (float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
            if (len <= 1e-6f)
                return new PointF(1f, 0f);
            return new PointF(p.X / len, p.Y / len);
        }
    }
}
