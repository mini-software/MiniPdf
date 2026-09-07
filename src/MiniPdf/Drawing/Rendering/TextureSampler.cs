using System;
using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Rendering
{
    internal static class TextureSampler
    {
        public static Color Sample(TextureBrush brush, float x, float y, InterpolationMode interpolation)
        {
            if (!(brush.Image is Bitmap bmp))
                return Color.Black;

            PointF p = TransformToBrushSpace(new PointF(x, y), brush.Transform);
            float u = p.X;
            float v = p.Y;

            bool flipX = brush.WrapMode == WrapMode.TileFlipX || brush.WrapMode == WrapMode.TileFlipXY;
            bool flipY = brush.WrapMode == WrapMode.TileFlipY || brush.WrapMode == WrapMode.TileFlipXY;
            bool clamp = brush.WrapMode == WrapMode.Clamp;

            u = WrapCoordinate(u, bmp.Width, clamp, flipX);
            v = WrapCoordinate(v, bmp.Height, clamp, flipY);

            switch (interpolation)
            {
                case InterpolationMode.Bilinear:
                case InterpolationMode.HighQualityBilinear:
                case InterpolationMode.Bicubic:
                case InterpolationMode.HighQualityBicubic:
                    return SampleBilinear(bmp, u, v);
                default:
                    return SampleNearest(bmp, u, v);
            }
        }

        private static PointF TransformToBrushSpace(PointF p, Matrix m)
        {
            Matrix inv = m.Clone();
            if (!inv.IsInvertible)
                return p;
            inv.Invert();
            PointF[] pts = new[] { p };
            inv.TransformPoints(pts);
            return pts[0];
        }

        private static float WrapCoordinate(float value, int size, bool clamp, bool flip)
        {
            if (size <= 0)
                return 0f;

            if (clamp)
                return Math.Max(0f, Math.Min(size - 1, value));

            float tile = value / size;
            int tileIndex = (int)Math.Floor(tile);
            float frac = value - tileIndex * size;
            if (frac < 0f)
                frac += size;

            if (flip && ((tileIndex & 1) != 0))
                frac = (size - 1) - frac;

            return frac;
        }

        private static Color SampleNearest(Bitmap bmp, float x, float y)
        {
            int ix = (int)Math.Round(x);
            int iy = (int)Math.Round(y);
            ix = Math.Max(0, Math.Min(bmp.Width - 1, ix));
            iy = Math.Max(0, Math.Min(bmp.Height - 1, iy));
            return bmp.GetPixel(ix, iy);
        }

        private static Color SampleBilinear(Bitmap bmp, float x, float y)
        {
            int x0 = (int)Math.Floor(x);
            int y0 = (int)Math.Floor(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float u = x - x0;
            float v = y - y0;

            x0 = Math.Max(0, Math.Min(bmp.Width - 1, x0));
            y0 = Math.Max(0, Math.Min(bmp.Height - 1, y0));
            x1 = Math.Max(0, Math.Min(bmp.Width - 1, x1));
            y1 = Math.Max(0, Math.Min(bmp.Height - 1, y1));

            Color c00 = bmp.GetPixel(x0, y0);
            Color c10 = bmp.GetPixel(x1, y0);
            Color c01 = bmp.GetPixel(x0, y1);
            Color c11 = bmp.GetPixel(x1, y1);

            return Lerp2D(c00, c10, c01, c11, u, v);
        }

        private static Color Lerp2D(Color c00, Color c10, Color c01, Color c11, float u, float v)
        {
            int a = Bilinear(c00.A, c10.A, c01.A, c11.A, u, v);
            int r = Bilinear(c00.R, c10.R, c01.R, c11.R, u, v);
            int g = Bilinear(c00.G, c10.G, c01.G, c11.G, u, v);
            int b = Bilinear(c00.B, c10.B, c01.B, c11.B, u, v);
            return Color.FromArgb(a, r, g, b);
        }

        private static int Bilinear(int c00, int c10, int c01, int c11, float u, float v)
        {
            float top = c00 * (1f - u) + c10 * u;
            float bot = c01 * (1f - u) + c11 * u;
            return (int)Math.Round(top * (1f - v) + bot * v);
        }
    }
}
