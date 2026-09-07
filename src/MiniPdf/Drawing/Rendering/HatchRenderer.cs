using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;

namespace MiniPdf.Drawing.Rendering
{
    internal static class HatchRenderer
    {
        public static Color Sample(HatchBrush brush, int x, int y)
        {
            bool fg = IsForeground(brush.HatchStyle, x, y);
            return fg ? brush.ForegroundColor : brush.BackgroundColor;
        }

        private static bool IsForeground(HatchStyle style, int x, int y)
        {
            switch (style)
            {
                case HatchStyle.Horizontal:
                    return (y & 3) == 0;
                case HatchStyle.Vertical:
                    return (x & 3) == 0;
                case HatchStyle.ForwardDiagonal:
                    return ((x + y) & 3) == 0;
                case HatchStyle.BackwardDiagonal:
                    return ((x - y) & 3) == 0;
                case HatchStyle.Cross:
                    return (x & 3) == 0 || (y & 3) == 0;
                case HatchStyle.DiagonalCross:
                    return ((x + y) & 3) == 0 || ((x - y) & 3) == 0;
                case HatchStyle.Percent05:
                    return ((x * 17 + y * 31) & 31) == 0;
                case HatchStyle.Percent10:
                    return ((x * 17 + y * 31) & 15) == 0;
                case HatchStyle.Percent20:
                    return ((x * 17 + y * 31) & 7) <= 1;
                case HatchStyle.Percent25:
                    return ((x + y) & 3) == 0;
                case HatchStyle.Percent40:
                    return ((x * 5 + y * 3) & 7) <= 2;
                case HatchStyle.Percent50:
                    return ((x + y) & 1) == 0;
                case HatchStyle.Percent60:
                    return ((x * 5 + y * 3) & 7) <= 4;
                case HatchStyle.Percent75:
                    return ((x + y) & 3) != 0;
                case HatchStyle.Percent80:
                    return ((x * 17 + y * 31) & 7) >= 1;
                case HatchStyle.Percent90:
                    return ((x * 17 + y * 31) & 15) != 0;
                default:
                    // Deterministic fallback for unimplemented style variants.
                    return ((x * 11 + y * 7) & 3) == 0;
            }
        }
    }
}
