using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;
using MiniSoftware.Drawing.Pens;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Writes SVG text in two modes:
    /// <list type="bullet">
    ///   <item><b>Path mode</b> (default): vectorizes glyphs via
    ///     <see cref="GlyphOutlineRenderer"/> into <c>&lt;path&gt;</c> elements.
    ///     Pixel-identical to the rasterizer; no font dependency on viewer.</item>
    ///   <item><b>Text mode</b> (opt-in): emits <c>&lt;text&gt;</c>/<c>&lt;tspan&gt;</c>
    ///     with font attributes. Editable but best-effort fidelity.</item>
    /// </list>
    /// </summary>
    internal sealed class SvgTextWriter
    {
        private readonly SvgXmlWriter _w;
        private readonly SvgDefTable _defs;
        private readonly SvgEncoderParameters _params;

        public SvgTextWriter(SvgXmlWriter w, SvgDefTable defs, SvgEncoderParameters parameters)
        {
            _w = w;
            _defs = defs;
            _params = parameters;
        }

        /// <summary>
        /// Writes text for a DrawString command. Dispatches to path or text
        /// mode based on <see cref="SvgEncoderParameters.TextMode"/>.
        /// </summary>
        public void WriteText(string text, MiniSoftware.Drawing.Text.Font font, Brush brush,
                              RectangleF layoutRect, MiniSoftware.Drawing.Text.StringFormat? format)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (_params.TextMode == SvgTextMode.Text)
                WriteTextMode(text, font, brush, layoutRect, format);
            else
                WritePathMode(text, font, brush, layoutRect, format);
        }

        // ── Path mode: vectorize glyphs ───────────────────────────────────────

        /// <summary>
        /// Path mode: replays DrawString onto a capture Graphics that intercepts
        /// FillPath calls (which the rasterizer uses for each glyph outline),
        /// then emits each captured path as an SVG <c>&lt;path&gt;</c>.
        /// </summary>
        private void WritePathMode(string text, MiniSoftware.Drawing.Text.Font font, Brush brush,
                                   RectangleF layoutRect, MiniSoftware.Drawing.Text.StringFormat? format)
        {
            var capturedPaths = new List<GraphicsPath>();
            using var capture = new GlyphCaptureGraphics(capturedPaths);
            capture.DrawString(text, font, brush, layoutRect, format);

            string fill = SvgBrushWriter.WriteFillRef(brush, _defs);
            string? opacity = SvgBrushWriter.WriteFillOpacity(brush);

            foreach (var path in capturedPaths)
            {
                using (path)
                {
                    if (path.PointCount == 0) continue;
                    WriteFillPath(path, fill, opacity);
                }
            }
        }

        private void WriteFillPath(GraphicsPath path, string fill, string? opacity)
        {
            string d = SvgPathWriter.WritePathData(path, _params.CoordinatePrecision);
            if (string.IsNullOrEmpty(d)) return;

            _w.WriteStartElement("path");
            _w.WriteAttribute("d", d);
            _w.WriteAttribute("fill", fill);
            if (opacity != null)
                _w.WriteAttribute("fill-opacity", opacity);
            _w.WriteAttribute("stroke", "none");
            _w.WriteEndElement();
        }

        // ── Text mode: <text>/<tspan> ─────────────────────────────────────────

        private void WriteTextMode(string text, MiniSoftware.Drawing.Text.Font font, Brush brush,
                                   RectangleF layoutRect, MiniSoftware.Drawing.Text.StringFormat? format)
        {
            var lines = MiniSoftware.Drawing.Text.TextLayoutEngine.Layout(text, font, layoutRect, format, 96f, 96f);
            if (lines.Count == 0) return;

            string fill = SvgBrushWriter.WriteFillRef(brush, _defs);
            string? opacity = SvgBrushWriter.WriteFillOpacity(brush);

            _w.WriteStartElement("text");
            _w.WriteAttribute("font-family", font.FontFamily.Name ?? "Arial");
            _w.WriteAttribute("font-size", font.Size);
            if ((font.Style & FontStyle.Bold) != 0)
                _w.WriteAttribute("font-weight", "bold");
            if ((font.Style & FontStyle.Italic) != 0)
                _w.WriteAttribute("font-style", "italic");
            _w.WriteAttribute("fill", fill);
            if (opacity != null)
                _w.WriteAttribute("fill-opacity", opacity);

            var sf = format ?? MiniSoftware.Drawing.Text.StringFormat.GenericDefault;
            string anchor = sf.Alignment switch
            {
                MiniSoftware.Drawing.Text.StringAlignment.Near   => "start",
                MiniSoftware.Drawing.Text.StringAlignment.Center => "middle",
                MiniSoftware.Drawing.Text.StringAlignment.Far    => "end",
                _ => "start",
            };
            _w.WriteAttributeOptional("text-anchor", anchor, "start");

            string baseline = sf.LineAlignment switch
            {
                MiniSoftware.Drawing.Text.StringAlignment.Near   => "text-before-edge",
                MiniSoftware.Drawing.Text.StringAlignment.Center => "central",
                MiniSoftware.Drawing.Text.StringAlignment.Far    => "text-after-edge",
                _ => "text-before-edge",
            };
            _w.WriteAttributeOptional("dominant-baseline", baseline, "text-before-edge");

            foreach (var line in lines)
            {
                foreach (var run in line.Runs)
                {
                    _w.WriteStartElement("tspan");
                    _w.WriteAttribute("x", run.X);
                    _w.WriteAttribute("y", run.Baseline);
                    _w.Underlying.WriteString(run.Text);
                    _w.WriteEndElement();
                }
            }

            _w.WriteEndElement();
        }

        /// <summary>
        /// A Graphics subclass that intercepts FillPathCore to capture glyph
        /// outline paths during DrawString rendering.
        /// </summary>
        private sealed class GlyphCaptureGraphics : Graphics
        {
            private readonly List<GraphicsPath> _captured;

            public GlyphCaptureGraphics(List<GraphicsPath> captured)
                : base(CreateBlankBitmap())
            {
                _captured = captured;
            }

            private static MiniSoftware.Drawing.Imaging.Bitmap CreateBlankBitmap()
            {
                return new MiniSoftware.Drawing.Imaging.Bitmap(1, 1, PixelFormat.Format32bppArgb);
            }

            protected override void FillPathCore(Brush brush, GraphicsPath path, FillMode fillMode)
            {
                _captured.Add(path.Clone());
            }

            protected override void DrawPathCore(Pen pen, GraphicsPath path)
            {
            }
        }
    }
}