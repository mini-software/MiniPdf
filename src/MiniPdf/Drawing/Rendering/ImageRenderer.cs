using System;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Rendering
{
    /// <summary>
    /// Entry point for all DrawImage operations.
    /// Delegates to <see cref="ImageInterpolator"/> for sampling and
    /// <see cref="ImageTransformer"/> for affine warps.
    /// </summary>
    internal static class ImageRenderer
    {
        /// <summary>
        /// Scale-blit a portion of <paramref name="src"/> into <paramref name="destRect"/>
        /// on <paramref name="dst"/>, applying optional colour adjustments.
        /// </summary>
        public static void Render(
            Bitmap dst, Bitmap src,
            RectangleF destRect, RectangleF srcRect,
            Rectangle? clip, InterpolationMode mode, CompositingMode compositingMode,
            ImageAttributes? attrs)
        {
            if (destRect.Width <= 0 || destRect.Height <= 0) return;
            if (srcRect.Width  <= 0 || srcRect.Height  <= 0) return;

            Rectangle c = clip ?? new Rectangle(0, 0, dst.Width, dst.Height);
            int x0 = Math.Max((int)Math.Floor(destRect.X),        c.X);
            int y0 = Math.Max((int)Math.Floor(destRect.Y),        c.Y);
            int x1 = Math.Min((int)Math.Ceiling(destRect.Right),  c.Right)  - 1;
            int y1 = Math.Min((int)Math.Ceiling(destRect.Bottom), c.Bottom) - 1;

            float scaleX = srcRect.Width  / destRect.Width;
            float scaleY = srcRect.Height / destRect.Height;

            for (int dy = y0; dy <= y1; dy++)
            {
                float sy = srcRect.Y + (dy - destRect.Y) * scaleY;
                for (int dx = x0; dx <= x1; dx++)
                {
                    float sx = srcRect.X + (dx - destRect.X) * scaleX;
                    Color sc = ImageInterpolator.Sample(src, sx, sy, mode);
                    if (attrs != null) sc = attrs.ApplyToColor(sc);
                    PixelCompositor.BlendPixel(dst, dx, dy, sc, compositingMode);
                }
            }
        }

        /// <summary>
        /// Affine-warp blit into a parallelogram defined by 3 destination points.
        /// </summary>
        public static void RenderAffine(
            Bitmap dst, Bitmap src,
            PointF[] destPoints, RectangleF srcRect,
            Rectangle? clip, InterpolationMode mode, CompositingMode compositingMode,
            ImageAttributes? attrs)
            => ImageTransformer.RenderAffine(dst, src, destPoints, srcRect, clip, mode, compositingMode, attrs);
    }
}
