using System;
using System.Collections.Generic;
using System.Globalization;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Geometry;

namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// Represents a parsed SVG gradient definition (linear or radial) stored in
    /// the defs table, used to construct a MiniPdf.Drawing brush on demand.
    /// </summary>
    internal sealed class SvgGradientDef
    {
        public string Id = "";
        public bool IsRadial;
        // Linear
        public float X1, Y1, X2, Y2;
        // Radial
        public float Cx, Cy, R, Fx, Fy;
        // Common
        public string GradientUnits = "objectBoundingBox";
        public string SpreadMethod = "pad";
        public Matrix? GradientTransform;
        public List<(float Offset, Color Color)> Stops = new List<(float, Color)>();
    }

    /// <summary>
    /// Parses SVG fill/stroke values (colors, <c>url(#id)</c> references) and
    /// gradient/pattern def elements into MiniPdf.Drawing brushes.
    /// </summary>
    internal static class SvgBrushReader
    {
        /// <summary>
        /// Resolves a paint value (e.g. <c>red</c>, <c>#ff0000</c>,
        /// <c>rgb(255,0,0)</c>, <c>url(#g1)</c>) to a brush, or null for
        /// "none"/"inherit".
        /// </summary>
        public static Brush? ResolveBrush(string? paint, Dictionary<string, object> defs)
        {
            if (string.IsNullOrWhiteSpace(paint)) return null;
            paint = paint.Trim();

            if (paint.Equals("none", StringComparison.OrdinalIgnoreCase) ||
                paint.Equals("inherit", StringComparison.OrdinalIgnoreCase))
                return null;

            // url(#id) reference
            if (paint.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
            {
                int start = paint.IndexOf('#');
                int end = paint.IndexOf(')', start);
                if (start < 0 || end < 0) return null;
                string id = paint.Substring(start + 1, end - start - 1).Trim();
                if (defs.TryGetValue(id, out var def))
                    return BrushFromDef(def);
                return null;
            }

            // Named color, hex, rgb(), etc.
            Color color = ParseColor(paint);
            if (color.IsEmpty) return null;
            return new SolidBrush(color);
        }

        /// <summary>
        /// Parses an SVG color string: named color, <c>#rgb</c>, <c>#rrggbb</c>,
        /// <c>rgb(r,g,b)</c>, or <c>rgba(r,g,b,a)</c>.
        /// </summary>
        public static Color ParseColor(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return Color.Empty;
            s = s.Trim();

            // Hex
            if (s.StartsWith("#"))
            {
                string hex = s.Substring(1);
                if (hex.Length == 3)
                {
                    byte r = (byte)(HexVal(hex[0]) * 17);
                    byte g = (byte)(HexVal(hex[1]) * 17);
                    byte b = (byte)(HexVal(hex[2]) * 17);
                    return Color.FromArgb(255, r, g, b);
                }
                if (hex.Length == 6)
                {
                    int r = HexVal(hex[0]) * 16 + HexVal(hex[1]);
                    int g = HexVal(hex[2]) * 16 + HexVal(hex[3]);
                    int b = HexVal(hex[4]) * 16 + HexVal(hex[5]);
                    return Color.FromArgb(255, r, g, b);
                }
                if (hex.Length == 8)
                {
                    int a = HexVal(hex[0]) * 16 + HexVal(hex[1]);
                    int r = HexVal(hex[2]) * 16 + HexVal(hex[3]);
                    int g = HexVal(hex[4]) * 16 + HexVal(hex[5]);
                    int b = HexVal(hex[6]) * 16 + HexVal(hex[7]);
                    return Color.FromArgb(a, r, g, b);
                }
                return Color.Empty;
            }

            // rgb(r,g,b) or rgba(r,g,b,a)
            if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
            {
                string inner = s.Substring(4).TrimEnd(')', ' ');
                var parts = inner.Split(',');
                if (parts.Length >= 3)
                {
                    int r = ParseChannel(parts[0].Trim());
                    int g = ParseChannel(parts[1].Trim());
                    int b = ParseChannel(parts[2].Trim());
                    int a = 255;
                    if (parts.Length >= 4)
                        a = (int)(ParseChannel(parts[3].Trim()));
                    return Color.FromArgb(a, r, g, b);
                }
                return Color.Empty;
            }

            // Named color
            Color named = Color.FromName(s);
            if (!named.IsEmpty)
                return named;

            // "currentColor" — default to black
            if (s.Equals("currentColor", StringComparison.OrdinalIgnoreCase))
                return Color.Black;

            return Color.Empty;
        }

        /// <summary>
        /// Applies opacity to a brush by returning a new brush with modified alpha.
        /// For solid brushes, scales the alpha channel. For gradient brushes,
        /// scales all stop alphas.
        /// </summary>
        public static Brush? ApplyOpacity(Brush? brush, float opacity)
        {
            if (brush == null || opacity >= 1f) return brush;
            if (opacity <= 0f) return new SolidBrush(Color.FromArgb(0, 0, 0, 0));

            if (brush is SolidBrush sb)
            {
                int a = (int)(sb.Color.A * opacity);
                return new SolidBrush(Color.FromArgb(a, sb.Color.R, sb.Color.G, sb.Color.B));
            }

            // For gradient brushes, we'd need to scale stop alphas.
            // For now, return as-is (opacity is best-effort for complex brushes).
            return brush;
        }

        /// <summary>
        /// Constructs a MiniPdf.Drawing brush from a parsed gradient def.
        /// </summary>
        private static Brush? BrushFromDef(object def)
        {
            if (def is SvgGradientDef gd)
                return GradientToBrush(gd);
            // Patterns and other def types: return null for now
            return null;
        }

        /// <summary>
        /// Converts a <see cref="SvgGradientDef"/> to a
        /// <see cref="LinearGradientBrush"/> or <see cref="PathGradientBrush"/>.
        /// </summary>
        public static Brush? GradientToBrush(SvgGradientDef gd)
        {
            if (gd.Stops.Count == 0) return null;

            if (!gd.IsRadial)
            {
                Color c1 = gd.Stops[0].Color;
                Color c2 = gd.Stops[gd.Stops.Count - 1].Color;
                var brush = new LinearGradientBrush(
                    new PointF(gd.X1, gd.Y1),
                    new PointF(gd.X2, gd.Y2),
                    c1, c2);

                if (gd.SpreadMethod.Equals("repeat", StringComparison.OrdinalIgnoreCase))
                    brush.WrapMode = WrapMode.Tile;
                else if (gd.SpreadMethod.Equals("reflect", StringComparison.OrdinalIgnoreCase))
                    brush.WrapMode = WrapMode.TileFlipXY;
                else
                    brush.WrapMode = WrapMode.Clamp;

                // Multi-stop interpolation
                if (gd.Stops.Count > 2)
                {
                    var blend = new ColorBlend(gd.Stops.Count);
                    for (int i = 0; i < gd.Stops.Count; i++)
                    {
                        blend.Colors[i] = gd.Stops[i].Color;
                        blend.Positions[i] = gd.Stops[i].Offset;
                    }
                    brush.InterpolationColors = blend;
                }

                if (gd.GradientTransform != null && !gd.GradientTransform.IsIdentity)
                    brush.Transform = gd.GradientTransform.Clone();

                return brush;
            }
            else
            {
                // Radial gradient → PathGradientBrush
                float r = Math.Max(gd.R, 1f);
                var center = new PointF(gd.Cx, gd.Cy);
                var points = new PointF[]
                {
                    new PointF(gd.Cx + r, gd.Cy),
                    new PointF(gd.Cx, gd.Cy + r),
                    new PointF(gd.Cx - r, gd.Cy),
                    new PointF(gd.Cx, gd.Cy - r),
                };
                var brush = new PathGradientBrush(points)
                {
                    CenterPoint = center,
                    CenterColor = gd.Stops[0].Color,
                };
                var surround = new Color[gd.Stops.Count > 1 ? gd.Stops.Count - 1 : 1];
                if (gd.Stops.Count > 1)
                {
                    for (int i = 1; i < gd.Stops.Count; i++)
                        surround[i - 1] = gd.Stops[i].Color;
                }
                else
                {
                    surround[0] = gd.Stops[0].Color;
                }
                brush.SurroundColors = surround;

                // Focus point
                if (gd.Fx != gd.Cx || gd.Fy != gd.Cy)
                {
                    float fxOffset = gd.Fx - gd.Cx;
                    float fyOffset = gd.Fy - gd.Cy;
                    brush.FocusScales = new PointF(fxOffset / r, fyOffset / r);
                }

                if (gd.GradientTransform != null && !gd.GradientTransform.IsIdentity)
                    brush.Transform = gd.GradientTransform.Clone();

                return brush;
            }
        }

        /// <summary>
        /// Parses a gradient element (linearGradient or radialGradient) from the
        /// XmlReader into a <see cref="SvgGradientDef"/>.
        /// The reader must be positioned on the gradient start element.
        /// Reads through the end element.
        /// </summary>
        public static SvgGradientDef ReadGradient(System.Xml.XmlReader reader, bool isRadial)
        {
            var gd = new SvgGradientDef { IsRadial = isRadial };
            gd.Id = reader.GetAttribute("id") ?? "";

            // Default values depend on gradientUnits (objectBoundingBox vs userSpaceOnUse)
            bool userSpace = string.Equals(reader.GetAttribute("gradientUnits"),
                "userSpaceOnUse", StringComparison.OrdinalIgnoreCase);
            gd.GradientUnits = userSpace ? "userSpaceOnUse" : "objectBoundingBox";

            if (isRadial)
            {
                gd.Cx = SvgStyleParser.ParseFloat(reader.GetAttribute("cx"), userSpace ? 0f : 0.5f);
                gd.Cy = SvgStyleParser.ParseFloat(reader.GetAttribute("cy"), userSpace ? 0f : 0.5f);
                gd.R  = SvgStyleParser.ParseFloat(reader.GetAttribute("r"),  userSpace ? 0f : 0.5f);
                gd.Fx = SvgStyleParser.ParseFloat(reader.GetAttribute("fx"), gd.Cx);
                gd.Fy = SvgStyleParser.ParseFloat(reader.GetAttribute("fy"), gd.Cy);
            }
            else
            {
                gd.X1 = SvgStyleParser.ParseFloat(reader.GetAttribute("x1"), userSpace ? 0f : 0f);
                gd.Y1 = SvgStyleParser.ParseFloat(reader.GetAttribute("y1"), userSpace ? 0f : 0f);
                gd.X2 = SvgStyleParser.ParseFloat(reader.GetAttribute("x2"), userSpace ? 0f : 1f);
                gd.Y2 = SvgStyleParser.ParseFloat(reader.GetAttribute("y2"), userSpace ? 0f : 0f);
            }

            string? spread = reader.GetAttribute("spreadMethod");
            if (!string.IsNullOrEmpty(spread)) gd.SpreadMethod = spread;

            string? transformAttr = reader.GetAttribute("gradientTransform");
            if (!string.IsNullOrWhiteSpace(transformAttr))
                gd.GradientTransform = SvgTransformParser.Parse(transformAttr);

            // Read stops
            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.EndElement) break;
                    if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                        reader.LocalName == "stop")
                    {
                        float offset = ParseOffset(reader.GetAttribute("offset"));
                        Color color = ParseColor(reader.GetAttribute("stop-color"));

                        string? stopOpacity = reader.GetAttribute("stop-opacity");
                        if (!string.IsNullOrWhiteSpace(stopOpacity) &&
                            float.TryParse(stopOpacity, NumberStyles.Float,
                                CultureInfo.InvariantCulture, out float op))
                        {
                            int a = (int)(color.A * Math.Max(0f, Math.Min(1f, op)));
                            color = Color.FromArgb(a, color.R, color.G, color.B);
                        }

                        // Also check style attribute
                        string? styleAttr = reader.GetAttribute("style");
                        if (!string.IsNullOrWhiteSpace(styleAttr))
                        {
                            foreach (var pair in styleAttr.Split(';'))
                            {
                                int colon = pair.IndexOf(':');
                                if (colon < 0) continue;
                                string pname = pair.Substring(0, colon).Trim();
                                string pval = pair.Substring(colon + 1).Trim();
                                if (pname == "stop-color")
                                    color = ParseColor(pval);
                                else if (pname == "stop-opacity" &&
                                    float.TryParse(pval, NumberStyles.Float,
                                        CultureInfo.InvariantCulture, out float so))
                                {
                                    int a = (int)(color.A * Math.Max(0f, Math.Min(1f, so)));
                                    color = Color.FromArgb(a, color.R, color.G, color.B);
                                }
                                else if (pname == "offset")
                                    offset = ParseOffset(pval);
                            }
                        }

                        gd.Stops.Add((offset, color));
                    }
                }
            }

            return gd;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static int HexVal(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return 0;
        }

        private static int ParseChannel(string s)
        {
            s = s.Trim();
            if (s.EndsWith("%"))
            {
                float v = SvgStyleParser.ParseFloat(s.TrimEnd('%'));
                return (int)Math.Round(v * 255f / 100f);
            }
            return (int)SvgStyleParser.ParseFloat(s);
        }

        private static float ParseOffset(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0f;
            s = s.Trim();
            bool isPercent = s.EndsWith("%");
            if (isPercent) s = s.TrimEnd('%');
            float v = SvgStyleParser.ParseFloat(s);
            return isPercent ? v / 100f : Math.Max(0f, Math.Min(1f, v));
        }
    }
}