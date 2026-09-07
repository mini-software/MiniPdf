using System;
using System.Collections.Generic;
using System.Xml;
using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Text;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Parses SVG <c>&lt;text&gt;</c> and <c>&lt;tspan&gt;</c> elements into
    /// text rendering information (best-effort). Complex text flow features
    /// (textLength, writing-mode, complex BiDi) fall back to approximate positioning.
    /// </summary>
    internal sealed class SvgTextReader
    {
        private readonly Dictionary<string, object> _defs;

        public SvgTextReader(Dictionary<string, object> defs)
        {
            _defs = defs;
        }

        /// <summary>
        /// Reads a <c>&lt;text&gt;</c> element and returns the text content,
        /// font, brush, and layout rectangle for a <c>DrawString</c> call.
        /// The reader must be positioned on the text start element; reads
        /// through the end element.
        /// </summary>
        public (string Text, MiniSoftware.Drawing.Text.Font Font, Brush? Brush, RectangleF LayoutRect, StringFormat? Format)?
            ReadText(XmlReader reader, SvgStyle style)
        {
            var attrs = SvgStyleParser.ApplyAttributes(reader, style);

            float x = SvgShapeReader.GetFloat(attrs, "x");
            float y = SvgShapeReader.GetFloat(attrs, "y");
            float dx = SvgShapeReader.GetFloat(attrs, "dx");
            float dy = SvgShapeReader.GetFloat(attrs, "dy");

            // Font
            float fontSize = SvgStyleParser.ParseLength(style.FontSize, 16f);
            FontStyle fontStyle = ParseFontStyle(style.FontStyleVal, style.FontWeight);
            var font = new MiniSoftware.Drawing.Text.Font(style.FontFamily, fontSize, fontStyle, GraphicsUnit.Pixel);

            // Brush
            float fillOpacity = SvgStyleParser.ParseFloat(style.FillOpacity, 1f) *
                                SvgStyleParser.ParseFloat(style.Opacity, 1f);
            Brush? brush = SvgBrushReader.ResolveBrush(style.Fill, _defs);
            brush = SvgBrushReader.ApplyOpacity(brush, fillOpacity);

            // String format from text-anchor
            var format = new StringFormat
            {
                Alignment = style.TextAnchor.ToLowerInvariant() switch
                {
                    "middle" => StringAlignment.Center,
                    "end"    => StringAlignment.Far,
                    _        => StringAlignment.Near,
                },
            };

            // Read text content (including tspans)
            float textX = x + dx;
            float textY = y + dy;
            var sb = new System.Text.StringBuilder();
            float curX = textX;

            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.EndElement) break;

                    if (reader.NodeType == XmlNodeType.Text ||
                        reader.NodeType == XmlNodeType.Whitespace ||
                        reader.NodeType == XmlNodeType.SignificantWhitespace)
                    {
                        sb.Append(reader.Value);
                    }
                    else if (reader.NodeType == XmlNodeType.Element &&
                             reader.LocalName == "tspan")
                    {
                        var tspanAttrs = SvgStyleParser.ApplyAttributes(reader, style.Clone());
                        float tsX = tspanAttrs.TryGetValue("x", out var xVal)
                            ? SvgStyleParser.ParseFloat(xVal) : curX;
                        float tsY = tspanAttrs.TryGetValue("y", out var yVal)
                            ? SvgStyleParser.ParseFloat(yVal) : textY;

                        if (sb.Length > 0 && !sb[sb.Length - 1].Equals(' '))
                            sb.Append(' ');

                        string tspanText = ReadElementText(reader);
                        sb.Append(tspanText);
                        // Approximate advance width
                        curX = tsX + tspanText.Length * fontSize * 0.5f;
                    }
                }
            }

            string text = sb.ToString().Trim();
            if (string.IsNullOrEmpty(text) || brush == null)
            {
                font.Dispose();
                brush?.Dispose();
                return null;
            }

            // Layout rect: start at (textX, textY), wide enough for the text
            var layoutRect = new RectangleF(textX, textY - font.Height, 10000f, font.Height);

            return (text, font, brush, layoutRect, format);
        }

        private static string ReadElementText(XmlReader reader)
        {
            if (reader.IsEmptyElement) return "";
            var sb = new System.Text.StringBuilder();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement) break;
                if (reader.NodeType == XmlNodeType.Text ||
                    reader.NodeType == XmlNodeType.Whitespace ||
                    reader.NodeType == XmlNodeType.SignificantWhitespace)
                {
                    sb.Append(reader.Value);
                }
            }
            return sb.ToString();
        }

        private static FontStyle ParseFontStyle(string fontStyle, string fontWeight)
        {
            var result = FontStyle.Regular;
            if (fontStyle.Equals("italic", StringComparison.OrdinalIgnoreCase) ||
                fontStyle.Equals("oblique", StringComparison.OrdinalIgnoreCase))
                result |= FontStyle.Italic;
            if (fontWeight.Equals("bold", StringComparison.OrdinalIgnoreCase) ||
                (int.TryParse(fontWeight, out int w) && w >= 600))
                result |= FontStyle.Bold;
            return result;
        }
    }
}