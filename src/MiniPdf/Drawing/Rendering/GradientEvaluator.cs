using System;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Rendering
{
    internal static class GradientEvaluator
    {
        public static Color SampleLinear(LinearGradientBrush brush, float x, float y)
        {
            PointF p = TransformToBrushSpace(new PointF(x, y), brush.Transform);
            PointF p0 = brush.Point1;
            PointF p1 = brush.Point2;

            float dx = p1.X - p0.X;
            float dy = p1.Y - p0.Y;
            float lenSq = dx * dx + dy * dy;
            float t = lenSq <= 1e-6f ? 0f : ((p.X - p0.X) * dx + (p.Y - p0.Y) * dy) / lenSq;

            t = ApplyWrap(t, brush.WrapMode);
            t = ApplyBlend(brush.Blend, t);

            ColorBlend cb = brush.InterpolationColors;
            Color color = SampleColorBlend(cb, t);

            if (!brush.GammaCorrection)
                return color;

            return Color.FromArgb(
                color.A,
                GammaEncode(color.R),
                GammaEncode(color.G),
                GammaEncode(color.B));
        }

        public static Color SamplePath(PathGradientBrush brush, float x, float y)
        {
            PointF p = TransformToBrushSpace(new PointF(x, y), brush.Transform);
            RectangleF rect = brush.Rectangle;
            PointF center = brush.CenterPoint;

            float rx = Math.Max(1e-6f, rect.Width * 0.5f);
            float ry = Math.Max(1e-6f, rect.Height * 0.5f);
            float nx = (p.X - center.X) / rx;
            float ny = (p.Y - center.Y) / ry;
            float t = (float)Math.Sqrt(nx * nx + ny * ny);
            t = Math.Max(0f, Math.Min(1f, t));
            t = ApplyBlend(brush.Blend, t);

            if (brush.InterpolationColors != null && brush.InterpolationColors.Colors.Length > 0)
                return SampleColorBlend(brush.InterpolationColors, t);

            Color edge = Average(brush.SurroundColors);
            return Lerp(brush.CenterColor, edge, t);
        }

        internal static Color SampleColorBlend(ColorBlend blend, float t)
        {
            if (blend.Colors == null || blend.Positions == null || blend.Colors.Length == 0 || blend.Positions.Length == 0)
                return Color.Black;

            if (blend.Colors.Length == 1)
                return blend.Colors[0];

            int n = Math.Min(blend.Colors.Length, blend.Positions.Length);
            if (n == 1)
                return blend.Colors[0];

            t = Math.Max(0f, Math.Min(1f, t));
            if (t <= blend.Positions[0]) return blend.Colors[0];
            if (t >= blend.Positions[n - 1]) return blend.Colors[n - 1];

            for (int i = 1; i < n; i++)
            {
                float p0 = blend.Positions[i - 1];
                float p1 = blend.Positions[i];
                if (t > p1) continue;

                float u = Math.Abs(p1 - p0) < 1e-6f ? 0f : (t - p0) / (p1 - p0);
                return Lerp(blend.Colors[i - 1], blend.Colors[i], u);
            }

            return blend.Colors[n - 1];
        }

        private static float ApplyBlend(Blend? blend, float t)
        {
            if (blend == null || blend.Positions == null || blend.Factors == null)
                return Math.Max(0f, Math.Min(1f, t));

            int n = Math.Min(blend.Positions.Length, blend.Factors.Length);
            if (n == 0)
                return Math.Max(0f, Math.Min(1f, t));

            t = Math.Max(0f, Math.Min(1f, t));
            if (t <= blend.Positions[0]) return blend.Factors[0];
            if (t >= blend.Positions[n - 1]) return blend.Factors[n - 1];

            for (int i = 1; i < n; i++)
            {
                float p0 = blend.Positions[i - 1];
                float p1 = blend.Positions[i];
                if (t > p1) continue;

                float u = Math.Abs(p1 - p0) < 1e-6f ? 0f : (t - p0) / (p1 - p0);
                float v = blend.Factors[i - 1] + (blend.Factors[i] - blend.Factors[i - 1]) * u;
                return Math.Max(0f, Math.Min(1f, v));
            }

            return Math.Max(0f, Math.Min(1f, blend.Factors[n - 1]));
        }

        private static float ApplyWrap(float t, WrapMode wrap)
        {
            if (wrap == WrapMode.Clamp)
                return Math.Max(0f, Math.Min(1f, t));

            double floor = Math.Floor(t);
            float frac = (float)(t - floor);
            if (frac < 0f) frac += 1f;

            bool odd = (((int)floor) & 1) != 0;
            switch (wrap)
            {
                case WrapMode.Tile:
                    return frac;
                case WrapMode.TileFlipX:
                case WrapMode.TileFlipY:
                case WrapMode.TileFlipXY:
                    return odd ? 1f - frac : frac;
                default:
                    return Math.Max(0f, Math.Min(1f, t));
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

        private static Color Average(Color[] colors)
        {
            if (colors == null || colors.Length == 0)
                return Color.Black;

            int a = 0, r = 0, g = 0, b = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                a += colors[i].A;
                r += colors[i].R;
                g += colors[i].G;
                b += colors[i].B;
            }

            return Color.FromArgb(a / colors.Length, r / colors.Length, g / colors.Length, b / colors.Length);
        }

        private static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            int aa = (int)Math.Round(a.A + (b.A - a.A) * t);
            int rr = (int)Math.Round(a.R + (b.R - a.R) * t);
            int gg = (int)Math.Round(a.G + (b.G - a.G) * t);
            int bb = (int)Math.Round(a.B + (b.B - a.B) * t);
            return Color.FromArgb(aa, rr, gg, bb);
        }

        private static byte GammaEncode(int v)
        {
            float lin = Math.Max(0f, Math.Min(255f, v)) / 255f;
            float g = (float)Math.Pow(lin, 1.0 / 2.2);
            return (byte)Math.Round(g * 255f);
        }
    }
}
