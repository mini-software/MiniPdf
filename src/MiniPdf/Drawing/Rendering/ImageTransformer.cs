using System;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Rendering
{
    /// <summary>
    /// Renders a source rectangle into a destination parallelogram defined by 3 corner
    /// points (top-left, top-right, bottom-left) using an inverse-affine per-pixel scan.
    /// </summary>
    internal static class ImageTransformer
    {
        /// <param name="destPoints">
        /// Three points that define the parallelogram:
        ///   [0] = top-left, [1] = top-right, [2] = bottom-left.
        /// </param>
        /// <param name="srcRect">The source rectangle to render.</param>
        /// <param name="clip">The clipping rectangle, or null for no clipping.</param>
        /// <param name="mode">The interpolation mode.</param>
        /// <param name="compositingMode">The compositing mode.</param>
        /// <param name="attrs">The image attributes, or null for default.</param>
        public static void RenderAffine(
            Bitmap dst, Bitmap src,
            PointF[] destPoints, RectangleF srcRect,
            Rectangle? clip, InterpolationMode mode, CompositingMode compositingMode,
            Drawing2D.ImageAttributes? attrs)
        {
            if (destPoints == null || destPoints.Length < 3) return;
            if (srcRect.Width <= 0f || srcRect.Height <= 0f) return;

            // Forward map: dest = p0 + u*(p1-p0) + v*(p2-p0)
            // Inverse   : solve for (u, v) given (dx, dy)
            PointF p0 = destPoints[0], p1 = destPoints[1], p2 = destPoints[2];
            float ax = p1.X - p0.X, bx = p2.X - p0.X;
            float ay = p1.Y - p0.Y, by = p2.Y - p0.Y;
            float det = ax * by - bx * ay;
            if (Math.Abs(det) < 1e-10f) return; // degenerate

            float invDet = 1f / det;

            // Implicit 4th point of the parallelogram
            float p3x = p1.X + (p2.X - p0.X);
            float p3y = p1.Y + (p2.Y - p0.Y);

            float minX = Min4(p0.X, p1.X, p2.X, p3x);
            float minY = Min4(p0.Y, p1.Y, p2.Y, p3y);
            float maxX = Max4(p0.X, p1.X, p2.X, p3x);
            float maxY = Max4(p0.Y, p1.Y, p2.Y, p3y);

            Rectangle c = clip ?? new Rectangle(0, 0, dst.Width, dst.Height);
            int x0 = Math.Max((int)Math.Floor(minX), c.X);
            int y0 = Math.Max((int)Math.Floor(minY), c.Y);
            int x1 = Math.Min((int)Math.Ceiling(maxX), c.Right  - 1);
            int y1 = Math.Min((int)Math.Ceiling(maxY), c.Bottom - 1);

            for (int dy = y0; dy <= y1; dy++)
            {
                float ey = dy - p0.Y;
                for (int dx = x0; dx <= x1; dx++)
                {
                    float ex = dx - p0.X;
                    float u = ( by * ex - bx * ey) * invDet;
                    float v = (-ay * ex + ax * ey) * invDet;
                    if (u < 0f || u > 1f || v < 0f || v > 1f) continue;

                    float sx = srcRect.X + u * srcRect.Width;
                    float sy = srcRect.Y + v * srcRect.Height;
                    Color sc = ImageInterpolator.Sample(src, sx, sy, mode);
                    if (attrs != null) sc = attrs.ApplyToColor(sc);
                    PixelCompositor.BlendPixel(dst, dx, dy, sc, compositingMode);
                }
            }
        }

        private static float Min4(float a, float b, float c, float d)
        {
            float m = a < b ? a : b;
            m = m < c ? m : c;
            return m < d ? m : d;
        }

        private static float Max4(float a, float b, float c, float d)
        {
            float m = a > b ? a : b;
            m = m > c ? m : c;
            return m > d ? m : d;
        }
    }
}
