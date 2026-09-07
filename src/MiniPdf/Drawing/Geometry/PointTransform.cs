using MiniPdf.Drawing.Drawing2D;

namespace MiniPdf.Drawing.Geometry
{
    /// <summary>
    /// Internal helpers for applying a <see cref="Matrix"/> to point(s).
    /// Used by GraphicsPath, Region, and the rasterizer.
    /// </summary>
    internal static class PointTransform
    {
        internal static PointF Transform(Matrix matrix, PointF point)
        {
            var pts = new[] { point };
            matrix.TransformPoints(pts);
            return pts[0];
        }

        internal static PointF[] TransformAll(Matrix matrix, PointF[] points)
        {
            if (points == null || points.Length == 0) return System.Array.Empty<PointF>();
            var copy = (PointF[])points.Clone();
            matrix.TransformPoints(copy);
            return copy;
        }

        internal static void TransformInPlace(Matrix matrix, PointF[] points)
        {
            if (points != null && points.Length > 0)
                matrix.TransformPoints(points);
        }
    }
}
