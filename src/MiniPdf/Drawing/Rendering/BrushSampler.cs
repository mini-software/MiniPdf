using System;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;

namespace MiniPdf.Drawing.Rendering
{
    internal static class BrushSampler
    {
        public static Color Sample(Brush brush, float x, float y, InterpolationMode interpolation)
        {
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            if (brush is SolidBrush solid)
                return solid.Color;

            if (brush is LinearGradientBrush linear)
                return GradientEvaluator.SampleLinear(linear, x, y);

            if (brush is PathGradientBrush path)
                return GradientEvaluator.SamplePath(path, x, y);

            if (brush is TextureBrush texture)
                return TextureSampler.Sample(texture, x, y, interpolation);

            if (brush is HatchBrush hatch)
                return HatchRenderer.Sample(hatch, (int)Math.Floor(x), (int)Math.Floor(y));

            return Color.Black;
        }
    }
}
