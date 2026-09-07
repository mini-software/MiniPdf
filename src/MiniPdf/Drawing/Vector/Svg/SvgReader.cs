using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Pens;
using MiniPdf.Drawing.Rendering;
using MiniPdf.Drawing.Text;

namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// Top-level SVG reader: parses an SVG document from a stream using a
    /// forward-only <see cref="XmlReader"/> and produces a
    /// <see cref="VectorScene"/> by driving a <see cref="RecordingGraphics"/>.
    /// </summary>
    internal static class SvgReader
    {
        /// <summary>
        /// Reads an SVG document from the specified stream and returns a
        /// <see cref="VectorScene"/>.
        /// </summary>
        public static VectorScene Read(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            using var reader = XmlReader.Create(stream, settings);
            var ctx = new SvgReadContext();
            ctx.Parse(reader);
            return ctx.Scene;
        }

        /// <summary>
        /// Reads an SVG document from a string.
        /// </summary>
        public static VectorScene ReadString(string svg)
        {
            using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svg));
            return Read(ms);
        }
    }

    /// <summary>
    /// Internal parsing context that maintains the state stack, defs table,
    /// and the recording graphics surface.
    /// </summary>
    internal sealed class SvgReadContext : IDisposable
    {
        public VectorScene Scene { get; } = new VectorScene();
        public Dictionary<string, object> Defs { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public SvgStyle CurrentStyle { get; private set; } = new SvgStyle();

        private RecordingGraphics? _graphics;
        private readonly Stack<(SvgStyle Style, GraphicsState? State)> _stateStack
            = new Stack<(SvgStyle, GraphicsState?)>();

        private bool _disposed;

        // ── Top-level parse ────────────────────────────────────────────────────

        public void Parse(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    string name = reader.LocalName;
                    if (name.Equals("svg", StringComparison.OrdinalIgnoreCase))
                    {
                        ReadRootSvg(reader);
                    }
                    else if (_graphics != null)
                    {
                        ReadElement(reader, name);
                    }
                }
            }
        }

        // ── Root <svg> ────────────────────────────────────────────────────────

        private void ReadRootSvg(XmlReader reader)
        {
            var style = new SvgStyle();
            var attrs = SvgStyleParser.ApplyAttributes(reader, style);

            // Dimensions
            int width = ResolveDimension(attrs, "width", 0);
            int height = ResolveDimension(attrs, "height", 0);

            // If no width/height, use viewBox
            if (width <= 0 || height <= 0)
            {
                string? viewBox = GetValue(attrs, "viewBox");
                if (!string.IsNullOrWhiteSpace(viewBox))
                {
                    var parts = viewBox.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        float vbW = SvgStyleParser.ParseFloat(parts[2]);
                        float vbH = SvgStyleParser.ParseFloat(parts[3]);
                        if (width <= 0) width = (int)Math.Round(vbW);
                        if (height <= 0) height = (int)Math.Round(vbH);
                    }
                }
            }

            if (width <= 0) width = 800;
            if (height <= 0) height = 600;

            Scene.Width = width;
            Scene.Height = height;

            _graphics = new RecordingGraphics(Scene, width, height);
            CurrentStyle = style;

            // Root transform
            string? transform = GetValue(attrs, "transform");
            if (!string.IsNullOrWhiteSpace(transform))
            {
                var m = SvgTransformParser.Parse(transform);
                _graphics.MultiplyTransform(m, MatrixOrder.Prepend);
                m.Dispose();
            }

            // Process children
            if (!reader.IsEmptyElement)
            {
                ReadChildren(reader);
            }
        }

        // ── Element dispatch ──────────────────────────────────────────────────

        private void ReadElement(XmlReader reader, string name)
        {
            // Save current style and apply this element's attributes
            var savedStyle = CurrentStyle;
            var style = savedStyle.Clone();
            var attrs = SvgStyleParser.ApplyAttributes(reader, style);

            // Check display/visibility
            if (style.Display.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                SkipChildren(reader);
                CurrentStyle = savedStyle;
                return;
            }

            switch (name)
            {
                case "g":
                    PushGroup(reader, attrs, style, savedStyle);
                    break;

                case "defs":
                    ReadDefs(reader);
                    CurrentStyle = savedStyle;
                    break;

                case "linearGradient":
                case "radialGradient":
                    ReadGradientDef(reader, name == "radialGradient");
                    CurrentStyle = savedStyle;
                    break;

                case "clipPath":
                    ReadClipPathDef(reader);
                    CurrentStyle = savedStyle;
                    break;

                case "use":
                    ReadUse(reader, attrs, style);
                    CurrentStyle = savedStyle;
                    break;

                case "image":
                    ReadImageElement(attrs, style);
                    SkipChildren(reader);
                    CurrentStyle = savedStyle;
                    break;

                case "text":
                    ReadTextElement(reader, style);
                    CurrentStyle = savedStyle;
                    break;

                case "rect":
                case "circle":
                case "ellipse":
                case "line":
                case "polyline":
                case "polygon":
                case "path":
                    ReadShapeElement(name, attrs, style);
                    SkipChildren(reader);
                    CurrentStyle = savedStyle;
                    break;

                case "title":
                case "desc":
                case "metadata":
                case "symbol":
                case "mask":
                    // Skip non-rendering elements
                    SkipChildren(reader);
                    CurrentStyle = savedStyle;
                    break;

                default:
                    // Unknown element: skip gracefully
                    SkipChildren(reader);
                    CurrentStyle = savedStyle;
                    break;
            }
        }

        // ── <g> group ────────────────────────────────────────────────────────

        private void PushGroup(XmlReader reader, Dictionary<string, string> attrs,
            SvgStyle style, SvgStyle savedStyle)
        {
            GraphicsState? state = null;

            // Save graphics state for transform/clip
            state = _graphics!.Save();
            CurrentStyle = style;

            // Apply transform
            string? transform = GetValue(attrs, "transform");
            if (!string.IsNullOrWhiteSpace(transform))
            {
                var m = SvgTransformParser.Parse(transform);
                _graphics.MultiplyTransform(m, MatrixOrder.Prepend);
                m.Dispose();
            }

            // Apply clip-path
            ApplyClipPath(style.ClipPath);

            _stateStack.Push((savedStyle, state));

            // Process children
            if (!reader.IsEmptyElement)
            {
                ReadChildren(reader);
            }

            // Pop: restore state
            _stateStack.Pop();
            CurrentStyle = savedStyle;
            _graphics.Restore(state!);
        }

        // ── <defs> ────────────────────────────────────────────────────────────

        private void ReadDefs(XmlReader reader)
        {
            if (reader.IsEmptyElement) return;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement) break;
                if (reader.NodeType != XmlNodeType.Element) continue;

                string name = reader.LocalName;
                switch (name)
                {
                    case "linearGradient":
                        ReadGradientDef(reader, isRadial: false);
                        break;
                    case "radialGradient":
                        ReadGradientDef(reader, isRadial: true);
                        break;
                    case "clipPath":
                        ReadClipPathDef(reader);
                        break;
                    default:
                        // Store other defs (shapes for <use>) by id
                        string? id = reader.GetAttribute("id");
                        if (!string.IsNullOrEmpty(id))
                        {
                            StoreUseTarget(reader, id, name);
                        }
                        else
                        {
                            SkipChildren(reader);
                        }
                        break;
                }
            }
        }

        private void ReadGradientDef(XmlReader reader, bool isRadial)
        {
            var gd = SvgBrushReader.ReadGradient(reader, isRadial);
            if (!string.IsNullOrEmpty(gd.Id))
                Defs[gd.Id] = gd;
        }

        private void ReadClipPathDef(XmlReader reader)
        {
            string? id = reader.GetAttribute("id");
            var clipPath = SvgClipReader.ReadClipPath(reader);
            if (!string.IsNullOrEmpty(id))
                Defs[id] = clipPath;
        }

        // ── <use> ──────────────────────────────────────────────────────────────

        private void StoreUseTarget(XmlReader reader, string id, string elementName)
        {
            // For shapes in defs, parse and store the path + style
            var style = new SvgStyle();
            var attrs = SvgStyleParser.ApplyAttributes(reader, style);
            var path = SvgShapeReader.ReadShape(elementName, attrs, style);
            if (path != null)
            {
                Defs[id] = (path, style);
            }
            SkipChildren(reader);
        }

        private void ReadUse(XmlReader reader, Dictionary<string, string> attrs, SvgStyle style)
        {
            string? href = GetValue(attrs, "href") ?? GetValue(attrs, "xlink:href");
            if (string.IsNullOrWhiteSpace(href)) return;

            // Extract id from url(#id) or #id
            string id = href.Trim();
            if (id.StartsWith("#")) id = id.Substring(1);

            if (!Defs.TryGetValue(id, out var def))
                return;

            // Apply use's transform (x, y → translate, plus transform attr)
            GraphicsState? state = _graphics!.Save();

            float useX = SvgShapeReader.GetFloat(attrs, "x");
            float useY = SvgShapeReader.GetFloat(attrs, "y");
            if (useX != 0 || useY != 0)
                _graphics.TranslateTransform(useX, useY, MatrixOrder.Prepend);

            string? transform = GetValue(attrs, "transform");
            if (!string.IsNullOrWhiteSpace(transform))
            {
                var m = SvgTransformParser.Parse(transform);
                _graphics.MultiplyTransform(m, MatrixOrder.Prepend);
                m.Dispose();
            }

            if (def is ValueTuple<GraphicsPath, SvgStyle> shapeDef)
            {
                // Draw the referenced shape with the use's style (or the shape's style)
                var effectiveStyle = style.Clone();
                // Use the shape's own style where the use doesn't override
                // (simplified: use current style)
                DrawShape(shapeDef.Item1, effectiveStyle);
            }
            else if (def is GraphicsPath clipPath)
            {
                DrawShape(clipPath, style);
            }

            _graphics.Restore(state);
        }

        // ── Shape elements ─────────────────────────────────────────────────────

        private void ReadShapeElement(string name, Dictionary<string, string> attrs, SvgStyle style)
        {
            GraphicsPath? path = SvgShapeReader.ReadShape(name, attrs, style);
            if (path == null || path.PointCount == 0) return;

            DrawShape(path, style);
        }

        private void DrawShape(GraphicsPath path, SvgStyle style)
        {
            float fillOpacity = SvgStyleParser.ParseFloat(style.FillOpacity, 1f) *
                                SvgStyleParser.ParseFloat(style.Opacity, 1f);
            float strokeOpacity = SvgStyleParser.ParseFloat(style.StrokeOpacity, 1f) *
                                   SvgStyleParser.ParseFloat(style.Opacity, 1f);

            // Fill
            if (style.HasFill)
            {
                Brush? fillBrush = SvgBrushReader.ResolveBrush(style.Fill, Defs);
                fillBrush = SvgBrushReader.ApplyOpacity(fillBrush, fillOpacity);
                if (fillBrush != null)
                {
                    FillMode fillMode = string.Equals(style.FillRule, "evenodd",
                        StringComparison.OrdinalIgnoreCase)
                        ? FillMode.Alternate : FillMode.Winding;
                    _graphics!.FillPath(fillBrush, path, fillMode);
                    fillBrush.Dispose();
                }
            }

            // Stroke
            if (style.HasStroke)
            {
                Pen? pen = CreatePen(style, strokeOpacity);
                if (pen != null)
                {
                    _graphics!.DrawPath(pen, path);
                    pen.Dispose();
                }
            }
        }

        private Pen? CreatePen(SvgStyle style, float opacity)
        {
            Brush? strokeBrush = SvgBrushReader.ResolveBrush(style.Stroke, Defs);
            strokeBrush = SvgBrushReader.ApplyOpacity(strokeBrush, opacity);
            if (strokeBrush == null) return null;

            float width = SvgStyleParser.ParseFloat(style.StrokeWidth, 1f);
            var pen = new Pen(strokeBrush, width);

            // Line cap
            pen.StartCap = ParseLineCap(style.StrokeLinecap);
            pen.EndCap = pen.StartCap;

            // Line join
            pen.LineJoin = style.StrokeLinejoin.ToLowerInvariant() switch
            {
                "round" => LineJoin.Round,
                "bevel" => LineJoin.Bevel,
                _       => LineJoin.Miter,
            };

            // Miter limit
            pen.MiterLimit = SvgStyleParser.ParseFloat(style.StrokeMiterlimit, 4f);

            // Dash array
            if (!string.IsNullOrWhiteSpace(style.StrokeDashArray) &&
                !style.StrokeDashArray.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                var dashParts = style.StrokeDashArray.Split(new[] { ' ', ',' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (dashParts.Length > 0)
                {
                    var pattern = new float[dashParts.Length];
                    bool allZero = true;
                    for (int i = 0; i < dashParts.Length; i++)
                    {
                        pattern[i] = SvgStyleParser.ParseFloat(dashParts[i], 0f);
                        if (pattern[i] > 0) allZero = false;
                    }
                    if (!allZero)
                    {
                        pen.DashStyle = DashStyle.Custom;
                        pen.DashPattern = pattern;
                    }
                }
            }

            // Dash offset
            float dashOffset = SvgStyleParser.ParseFloat(style.StrokeDashoffset, 0f);
            if (dashOffset != 0f)
                pen.DashOffset = dashOffset;

            strokeBrush.Dispose();
            return pen;
        }

        private static LineCap ParseLineCap(string s)
        {
            return s.ToLowerInvariant() switch
            {
                "round"  => LineCap.Round,
                "square" => LineCap.Square,
                _        => LineCap.Flat,
            };
        }

        // ── <image> ────────────────────────────────────────────────────────────

        private void ReadImageElement(Dictionary<string, string> attrs, SvgStyle style)
        {
            var result = SvgImageReader.ReadImage(attrs);
            if (result == null) return;

            var (image, destRect) = result.Value;
            if (image == null) return;

            GraphicsState? state = null;
            bool needRestore = false;

            string? transform = GetValue(attrs, "transform");
            if (!string.IsNullOrWhiteSpace(transform))
            {
                state = _graphics!.Save();
                needRestore = true;
                var m = SvgTransformParser.Parse(transform);
                _graphics.MultiplyTransform(m, MatrixOrder.Prepend);
                m.Dispose();
            }

            var srcRect = new RectangleF(0, 0, image.Width, image.Height);
            _graphics!.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel, null);

            if (needRestore)
                _graphics.Restore(state!);

            image.Dispose();
        }

        // ── <text> ─────────────────────────────────────────────────────────────

        private void ReadTextElement(XmlReader reader, SvgStyle style)
        {
            var textReader = new SvgTextReader(Defs);
            var result = textReader.ReadText(reader, style);
            if (result == null) return;

            var (text, font, brush, layoutRect, format) = result.Value;
            if (brush != null)
            {
                _graphics!.DrawString(text, font, brush, layoutRect, format);
                brush.Dispose();
            }
            font.Dispose();
            format?.Dispose();
        }

        // ── Clip path ──────────────────────────────────────────────────────────

        private void ApplyClipPath(string? clipPathRef)
        {
            if (string.IsNullOrWhiteSpace(clipPathRef) ||
                clipPathRef.Equals("none", StringComparison.OrdinalIgnoreCase))
                return;

            // Extract id from url(#id)
            string id = clipPathRef;
            int hash = id.IndexOf('#');
            if (hash >= 0)
            {
                int end = id.IndexOf(')', hash);
                id = end >= 0
                    ? id.Substring(hash + 1, end - hash - 1)
                    : id.Substring(hash + 1);
            }

            if (Defs.TryGetValue(id, out var def))
            {
                if (def is GraphicsPath clipPath)
                {
                    using var region = new Region(clipPath);
                    _graphics!.IntersectClip(region);
                }
            }
        }

        // ── Children reading ───────────────────────────────────────────────────

        private void ReadChildren(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement) break;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    ReadElement(reader, reader.LocalName);
                }
            }
        }

        private static void SkipChildren(XmlReader reader)
        {
            if (reader.IsEmptyElement) return;
            int depth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth)
                    break;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static int ResolveDimension(Dictionary<string, string> attrs, string name, int defaultVal)
        {
            string? val = GetValue(attrs, name);
            if (string.IsNullOrWhiteSpace(val)) return defaultVal;
            return (int)Math.Round(SvgStyleParser.ParseLength(val, defaultVal));
        }

        private static string? GetValue(Dictionary<string, string> attrs, string name)
        {
            attrs.TryGetValue(name, out string? v);
            return v;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _graphics?.Dispose();
        }
    }
}