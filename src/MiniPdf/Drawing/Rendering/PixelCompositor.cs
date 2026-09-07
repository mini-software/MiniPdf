using System;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Rendering
{
    internal static class PixelCompositor
    {
        public static void BlendPixel(Bitmap target, int x, int y, Color src, CompositingMode mode)
        {
            if ((uint)x >= (uint)target.Width || (uint)y >= (uint)target.Height)
                return;

            if (mode == CompositingMode.SourceCopy)
            {
                target.SetPixel(x, y, src);
                return;
            }

            Color dst = target.GetPixel(x, y);
            Color outColor = SourceOver(src, dst);
            target.SetPixel(x, y, outColor);
        }

        public static Color SourceOver(Color src, Color dst)
        {
            float sa = src.A / 255f;
            float da = dst.A / 255f;

            float outA = sa + da * (1f - sa);
            if (outA <= 0f)
                return Color.FromArgb(0, 0, 0, 0);

            float outR = (src.R * sa + dst.R * da * (1f - sa)) / outA;
            float outG = (src.G * sa + dst.G * da * (1f - sa)) / outA;
            float outB = (src.B * sa + dst.B * da * (1f - sa)) / outA;

            return Color.FromArgb(
                ClampToByte(outA * 255f),
                ClampToByte(outR),
                ClampToByte(outG),
                ClampToByte(outB));
        }

        private static byte ClampToByte(float v)
        {
            if (v <= 0f) return 0;
            if (v >= 255f) return 255;
            return (byte)Math.Round(v);
        }
    }
}
