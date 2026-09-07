using System.Globalization;
using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Pens;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Writes SVG stroke attributes for a <see cref="Pen"/>.
    /// </summary>
    internal static class SvgPenWriter
    {
        /// <summary>
        /// Writes stroke-related attributes onto the current element.
        /// Writes: stroke, stroke-width, stroke-opacity, stroke-linecap,
        /// stroke-linejoin, stroke-miterlimit, stroke-dasharray, stroke-dashoffset.
        /// </summary>
        public static void WriteStroke(SvgXmlWriter w, Pen? pen, SvgDefTable defs)
        {
            if (pen == null)
            {
                w.WriteAttribute("stroke", "none");
                return;
            }

            // stroke color or brush ref
            if (pen.PenType == PenType.SolidColor)
            {
                w.WriteAttribute("stroke", SvgBrushWriter.ColorToCss(pen.Color));
                if (pen.Color.A < 255)
                    w.WriteAttribute("stroke-opacity", Fmt(pen.Color.A / 255f));
            }
            else
            {
                string? id = defs?.RegisterBrush(pen.Brush);
                w.WriteAttribute("stroke", id != null ? "url(#" + id + ")" : SvgBrushWriter.ColorToCss(pen.Color));
            }

            // stroke-width (emit only if non-default for compactness, but always
            // emit to be safe since SVG default is 1)
            w.WriteAttribute("stroke-width", Fmt(pen.Width));

            // line caps
            w.WriteAttributeOptional("stroke-linecap", LineCapToString(pen.StartCap), "butt");
            if (pen.StartCap != pen.EndCap)
                w.WriteAttributeOptional("stroke-linecap", LineCapToString(pen.EndCap), "butt");

            // line join
            w.WriteAttributeOptional("stroke-linejoin", LineJoinToString(pen.LineJoin), "miter");

            // miter limit (SVG default is 4; emit if different)
            if (System.Math.Abs(pen.MiterLimit - 10f) > 0.001f && System.Math.Abs(pen.MiterLimit - 4f) > 0.001f)
                w.WriteAttribute("stroke-miterlimit", Fmt(pen.MiterLimit));

            // dash style
            WriteDashStyle(w, pen);

            // dash offset
            if (pen.DashOffset != 0f)
                w.WriteAttribute("stroke-dashoffset", Fmt(pen.DashOffset));
        }

        private static void WriteDashStyle(SvgXmlWriter w, Pen pen)
        {
            switch (pen.DashStyle)
            {
                case DashStyle.Solid:
                    // default, no dasharray
                    break;
                case DashStyle.Dash:
                    w.WriteAttribute("stroke-dasharray", "4 2");
                    break;
                case DashStyle.Dot:
                    w.WriteAttribute("stroke-dasharray", "1 2");
                    break;
                case DashStyle.DashDot:
                    w.WriteAttribute("stroke-dasharray", "4 2 1 2");
                    break;
                case DashStyle.DashDotDot:
                    w.WriteAttribute("stroke-dasharray", "4 2 1 2 1 2");
                    break;
                case DashStyle.Custom:
                    var pattern = pen.DashPattern;
                    if (pattern != null && pattern.Length > 0)
                    {
                        var sb = new System.Text.StringBuilder();
                        for (int i = 0; i < pattern.Length; i++)
                        {
                            if (i > 0) sb.Append(' ');
                            sb.Append(Fmt(pattern[i] * pen.Width));
                        }
                        w.WriteAttribute("stroke-dasharray", sb.ToString());
                    }
                    break;
            }
        }

        private static string LineCapToString(LineCap cap)
        {
            return cap switch
            {
                LineCap.Flat     => "butt",
                LineCap.Round    => "round",
                LineCap.Square   => "square",
                LineCap.Triangle => "butt", // approximated
                _ => "butt",
            };
        }

        private static string LineJoinToString(LineJoin join)
        {
            return join switch
            {
                LineJoin.Miter => "miter",
                LineJoin.Bevel => "bevel",
                LineJoin.Round => "round",
                _ => "miter",
            };
        }

        private static string Fmt(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v)) v = 0f;
            string s = v.ToString("F4", CultureInfo.InvariantCulture);
            if (s.IndexOf('.') >= 0)
                s = s.TrimEnd('0').TrimEnd('.');
            if (s.Length == 0 || s == "-") s = "0";
            return s;
        }
    }
}