using System;
using System.Collections.Generic;
using System.Globalization;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Parses SVG shape elements (<c>&lt;rect&gt;</c>, <c>&lt;circle&gt;</c>,
    /// <c>&lt;ellipse&gt;</c>, <c>&lt;line&gt;</c>, <c>&lt;polyline&gt;</c>,
    /// <c>&lt;polygon&gt;</c>, <c>&lt;path&gt;</c>) into
    /// <see cref="GraphicsPath"/> objects.
    /// </summary>
    internal static class SvgShapeReader
    {
        /// <summary>
        /// Parses a shape element from its attributes and returns the
        /// corresponding <see cref="GraphicsPath"/>, or null if the element
        /// is not a recognized shape.
        /// </summary>
        public static GraphicsPath? ReadShape(string elementName,
            Dictionary<string, string> attrs, SvgStyle style)
        {
            switch (elementName)
            {
                case "rect":      return ReadRect(attrs);
                case "circle":    return ReadCircle(attrs);
                case "ellipse":   return ReadEllipse(attrs);
                case "line":      return ReadLine(attrs);
                case "polyline":  return ReadPolyline(attrs, close: false);
                case "polygon":   return ReadPolyline(attrs, close: true);
                case "path":      return ReadPath(attrs, style);
                default:          return null;
            }
        }

        /// <summary>
        /// Parses <c>&lt;rect x y width height rx ry/&gt;</c>.
        /// Rounded corners (rx/ry) are approximated with arcs.
        /// </summary>
        public static GraphicsPath ReadRect(Dictionary<string, string> attrs)
        {
            float x = GetFloat(attrs, "x");
            float y = GetFloat(attrs, "y");
            float w = GetFloat(attrs, "width");
            float h = GetFloat(attrs, "height");
            float rx = GetFloat(attrs, "rx", 0f);
            float ry = GetFloat(attrs, "ry", rx);

            var path = new GraphicsPath();

            if (rx <= 0 && ry <= 0)
            {
                path.AddRectangle(new RectangleF(x, y, w, h));
            }
            else
            {
                // Rounded rectangle using 4-point AddBezier (start, c1, c2, end)
                float erx = Math.Min(rx, w / 2f);
                float ery = Math.Min(ry, h / 2f);
                float kappa = 0.5522847498f;
                float kx = erx * kappa;
                float ky = ery * kappa;

                // Top edge
                path.AddLine(new PointF(x + erx, y), new PointF(x + w - erx, y));

                // Top-right corner: start=(x+w-erx,y) → end=(x+w, y+ery)
                path.AddBezier(
                    new PointF(x + w - erx, y),
                    new PointF(x + w - erx + kx, y),
                    new PointF(x + w, y + ery - ky),
                    new PointF(x + w, y + ery));

                // Right edge
                path.AddLine(new PointF(x + w, y + ery), new PointF(x + w, y + h - ery));

                // Bottom-right corner: start=(x+w, y+h-ery) → end=(x+w-erx, y+h)
                path.AddBezier(
                    new PointF(x + w, y + h - ery),
                    new PointF(x + w, y + h - ery + ky),
                    new PointF(x + w - erx + kx, y + h),
                    new PointF(x + w - erx, y + h));

                // Bottom edge
                path.AddLine(new PointF(x + w - erx, y + h), new PointF(x + erx, y + h));

                // Bottom-left corner: start=(x+erx, y+h) → end=(x, y+h-ery)
                path.AddBezier(
                    new PointF(x + erx, y + h),
                    new PointF(x + erx - kx, y + h),
                    new PointF(x, y + h - ery + ky),
                    new PointF(x, y + h - ery));

                // Left edge
                path.AddLine(new PointF(x, y + h - ery), new PointF(x, y + ery));

                // Top-left corner: start=(x, y+ery) → end=(x+erx, y)
                path.AddBezier(
                    new PointF(x, y + ery),
                    new PointF(x, y + ery - ky),
                    new PointF(x + erx - kx, y),
                    new PointF(x + erx, y));

                path.CloseFigure();
            }

            return path;
        }

        /// <summary>
        /// Parses <c>&lt;circle cx cy r/&gt;</c>.
        /// </summary>
        public static GraphicsPath ReadCircle(Dictionary<string, string> attrs)
        {
            float cx = GetFloat(attrs, "cx");
            float cy = GetFloat(attrs, "cy");
            float r = GetFloat(attrs, "r");

            var path = new GraphicsPath();
            path.AddEllipse(cx - r, cy - r, r * 2, r * 2);
            return path;
        }

        /// <summary>
        /// Parses <c>&lt;ellipse cx cy rx ry/&gt;</c>.
        /// </summary>
        public static GraphicsPath ReadEllipse(Dictionary<string, string> attrs)
        {
            float cx = GetFloat(attrs, "cx");
            float cy = GetFloat(attrs, "cy");
            float rx = GetFloat(attrs, "rx");
            float ry = GetFloat(attrs, "ry");

            var path = new GraphicsPath();
            path.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
            return path;
        }

        /// <summary>
        /// Parses <c>&lt;line x1 y1 x2 y2/&gt;</c>.
        /// </summary>
        public static GraphicsPath ReadLine(Dictionary<string, string> attrs)
        {
            float x1 = GetFloat(attrs, "x1");
            float y1 = GetFloat(attrs, "y1");
            float x2 = GetFloat(attrs, "x2");
            float y2 = GetFloat(attrs, "y2");

            var path = new GraphicsPath();
            path.AddLine(new PointF(x1, y1), new PointF(x2, y2));
            return path;
        }

        /// <summary>
        /// Parses <c>&lt;polyline points="x1,y1 x2,y2 ..."/&gt;</c> or
        /// <c>&lt;polygon&gt;</c> (same but closed).
        /// </summary>
        public static GraphicsPath ReadPolyline(Dictionary<string, string> attrs, bool close)
        {
            string? pointsStr = GetValue(attrs, "points");
            var points = ParsePoints(pointsStr);

            var path = new GraphicsPath();
            if (points.Length >= 2)
            {
                path.AddLines(points);
                if (close)
                    path.CloseFigure();
            }
            return path;
        }

        /// <summary>
        /// Parses <c>&lt;path d="..."/&gt;</c>.
        /// </summary>
        public static GraphicsPath ReadPath(Dictionary<string, string> attrs, SvgStyle style)
        {
            string? d = GetValue(attrs, "d");
            FillMode fillMode = string.Equals(GetValue(attrs, "fill-rule") ?? style.FillRule,
                "evenodd", StringComparison.OrdinalIgnoreCase)
                ? FillMode.Alternate
                : FillMode.Winding;
            return SvgPathParser.Parse(d, fillMode);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        internal static PointF[] ParsePoints(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Array.Empty<PointF>();

            var list = new List<PointF>();
            var tokens = s.Trim().Split(new[] { ' ', '\t', '\n', '\r', ',' },
                StringSplitOptions.RemoveEmptyEntries);

            // If even number of tokens, treat as x,y pairs
            int i = 0;
            while (i + 1 < tokens.Length)
            {
                if (float.TryParse(tokens[i], NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(tokens[i + 1], NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float y))
                {
                    list.Add(new PointF(x, y));
                }
                i += 2;
            }

            return list.ToArray();
        }

        internal static float GetFloat(Dictionary<string, string> attrs, string name, float defaultVal = 0f)
        {
            return SvgStyleParser.ParseFloat(GetValue(attrs, name), defaultVal);
        }

        internal static string? GetValue(Dictionary<string, string> attrs, string name)
        {
            attrs.TryGetValue(name, out string? v);
            return v;
        }
    }
}