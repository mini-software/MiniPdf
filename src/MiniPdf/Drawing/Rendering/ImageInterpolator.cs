using System;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Rendering
{
    /// <summary>
    /// Samples a source Bitmap at fractional pixel coordinates using NearestNeighbour,
    /// Bilinear (4-tap), or Bicubic (Catmull-Rom 16-tap) interpolation.
    /// </summary>
    internal static class ImageInterpolator
    {
        public static Color Sample(Bitmap src, float x, float y, InterpolationMode mode)
        {
            switch (mode)
            {
                case InterpolationMode.Bilinear:
                case InterpolationMode.HighQualityBilinear:
                    return SampleBilinear(src, x, y);
                case InterpolationMode.Bicubic:
                case InterpolationMode.HighQualityBicubic:
                    return SampleBicubic(src, x, y);
                default:
                    return SampleNearest(src, x, y);
            }
        }

        // ── Nearest-neighbour ────────────────────────────────────────────────

        private static Color SampleNearest(Bitmap src, float x, float y)
        {
            int ix = ClampInt((int)Math.Floor(x), 0, src.Width  - 1);
            int iy = ClampInt((int)Math.Floor(y), 0, src.Height - 1);
            return src.GetPixel(ix, iy);
        }

        // ── Bilinear (4-tap) ─────────────────────────────────────────────────

        private static Color SampleBilinear(Bitmap src, float x, float y)
        {
            int x0 = ClampInt((int)Math.Floor(x), 0, src.Width  - 1);
            int y0 = ClampInt((int)Math.Floor(y), 0, src.Height - 1);
            int x1 = ClampInt(x0 + 1, 0, src.Width  - 1);
            int y1 = ClampInt(y0 + 1, 0, src.Height - 1);

            float u = x - (float)Math.Floor(x);
            float v = y - (float)Math.Floor(y);

            Color p00 = src.GetPixel(x0, y0);
            Color p10 = src.GetPixel(x1, y0);
            Color p01 = src.GetPixel(x0, y1);
            Color p11 = src.GetPixel(x1, y1);

            float fu = 1f - u, fv = 1f - v;
            float w00 = fu * fv, w10 = u * fv, w01 = fu * v, w11 = u * v;

            return Color.FromArgb(
                ClampByte(p00.A * w00 + p10.A * w10 + p01.A * w01 + p11.A * w11),
                ClampByte(p00.R * w00 + p10.R * w10 + p01.R * w01 + p11.R * w11),
                ClampByte(p00.G * w00 + p10.G * w10 + p01.G * w01 + p11.G * w11),
                ClampByte(p00.B * w00 + p10.B * w10 + p01.B * w01 + p11.B * w11));
        }

        // ── Bicubic — Catmull-Rom 16-tap ─────────────────────────────────────

        private static Color SampleBicubic(Bitmap src, float x, float y)
        {
            float fx = x - (float)Math.Floor(x);
            float fy = y - (float)Math.Floor(y);
            int   px = (int)Math.Floor(x);
            int   py = (int)Math.Floor(y);

            float a = 0f, r = 0f, g = 0f, b = 0f;
            for (int row = -1; row <= 2; row++)
            {
                float wy = CatmullRom(row - fy);
                for (int col = -1; col <= 2; col++)
                {
                    float wx = CatmullRom(col - fx);
                    float  w = wx * wy;
                    Color  c = src.GetPixel(
                        ClampInt(px + col, 0, src.Width  - 1),
                        ClampInt(py + row, 0, src.Height - 1));
                    a += c.A * w;
                    r += c.R * w;
                    g += c.G * w;
                    b += c.B * w;
                }
            }

            return Color.FromArgb(ClampByte(a), ClampByte(r), ClampByte(g), ClampByte(b));
        }

        /// <summary>Catmull-Rom basis function (α = 0.5).</summary>
        private static float CatmullRom(float t)
        {
            float at = t < 0f ? -t : t;
            if (at < 1f) return  1.5f * at * at * at - 2.5f * at * at + 1f;
            if (at < 2f) return -0.5f * at * at * at + 2.5f * at * at - 4f * at + 2f;
            return 0f;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static int ClampInt(int v, int min, int max)
            => v < min ? min : v > max ? max : v;

        private static byte ClampByte(float v)
        {
            if (v <= 0f) return 0;
            if (v >= 255f) return 255;
            return (byte)Math.Round(v);
        }
    }
}
