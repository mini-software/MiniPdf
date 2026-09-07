using System;
using System.Globalization;
using System.Text;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Geometry;

namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// Writes SVG brush representations: inline color for solid brushes, and
    /// <c>&lt;linearGradient&gt;</c>/<c>&lt;radialGradient&gt;</c>/
    /// <c>&lt;pattern&gt;</c> defs for gradient/hatch/texture brushes.
    /// </summary>
    internal static class SvgBrushWriter
    {
        // ── Inline fill/stroke color ──────────────────────────────────────────

        /// <summary>
        /// Returns the inline <c>fill</c> or <c>stroke</c> value for a brush.
        /// Solid brushes return <c>rgb(r,g,b)</c>; non-solid brushes return
        /// <c>url(#id)</c> (the def must already be registered). Null brush
        /// returns <c>none</c>.
        /// </summary>
        public static string WriteFillRef(Brush? brush, SvgDefTable defs)
        {
            if (brush == null) return "none";
            if (brush is SolidBrush sb)
                return ColorToCss(sb.Color);

            string? id = defs?.RegisterBrush(brush);
            return id != null ? "url(#" + id + ")" : "none";
        }

        /// <summary>
        /// Returns the opacity (0–1) for a brush, or "1" if fully opaque.
        /// </summary>
        public static string? WriteFillOpacity(Brush? brush)
        {
            if (brush is SolidBrush sb)
            {
                if (sb.Color.A < 255)
                    return Fmt(sb.Color.A / 255f);
            }
            return null;
        }

        // ── Def emission ──────────────────────────────────────────────────────

        /// <summary>
        /// Emits all registered defs as SVG <c>&lt;defs&gt;</c> children.
        /// </summary>
        public static void WriteDefs(SvgXmlWriter w, SvgDefTable defs)
        {
            if (defs == null || defs.Defs.Count == 0) return;
            w.WriteStartElement("defs");
            foreach (var def in defs.Defs)
                WriteDef(w, def);
            w.WriteEndElement();
        }

        private static void WriteDef(SvgXmlWriter w, SvgDef def)
        {
            switch (def.Kind)
            {
                case SvgDefKind.LinearGradient:
                    WriteLinearGradient(w, (LinearGradientBrush)def.Source!, def.Id);
                    break;
                case SvgDefKind.RadialGradient:
                    WriteRadialGradient(w, (PathGradientBrush)def.Source!, def.Id);
                    break;
                case SvgDefKind.HatchPattern:
                    WriteHatchPattern(w, (HatchBrush)def.Source!, def.Id);
                    break;
                case SvgDefKind.TexturePattern:
                    WriteTexturePattern(w, (TextureBrush)def.Source!, def.Id);
                    break;
                case SvgDefKind.ClipPath:
                    WriteClipPath(w, def);
                    break;
            }
        }

        // ── Linear gradient ───────────────────────────────────────────────────

        private static void WriteLinearGradient(SvgXmlWriter w, LinearGradientBrush lg, string id)
        {
            w.WriteStartElement("linearGradient");
            w.WriteAttribute("id", id);
            w.WriteAttribute("x1", lg.Point1.X);
            w.WriteAttribute("y1", lg.Point1.Y);
            w.WriteAttribute("x2", lg.Point2.X);
            w.WriteAttribute("y2", lg.Point2.Y);
            w.WriteAttribute("gradientUnits", "userSpaceOnUse");
            WriteSpreadMethod(w, lg.WrapMode);
            var matrix = lg.Transform;
            if (matrix != null && !matrix.IsIdentity)
                w.WriteAttribute("gradientTransform", SvgTransformWriter.WriteTransform(matrix));

            var blend = lg.InterpolationColors;
            if (blend != null && blend.Colors.Length >= 2)
            {
                for (int i = 0; i < blend.Colors.Length; i++)
                    WriteStop(w, blend.Positions[i], blend.Colors[i]);
            }
            else
            {
                var colors = lg.LinearColors;
                WriteStop(w, 0f, colors[0]);
                WriteStop(w, 1f, colors[1]);
            }
            w.WriteEndElement();
        }

        // ── Radial gradient ───────────────────────────────────────────────────

        private static void WriteRadialGradient(SvgXmlWriter w, PathGradientBrush pg, string id)
        {
            w.WriteStartElement("radialGradient");
            w.WriteAttribute("id", id);

            var rect = pg.Rectangle;
            float cx = pg.CenterPoint.X;
            float cy = pg.CenterPoint.Y;
            // radius = max distance from center to bounds edge
            float r = Math.Max(
                Math.Max(Math.Abs(rect.Right - cx), Math.Abs(cx - rect.X)),
                Math.Max(Math.Abs(rect.Bottom - cy), Math.Abs(cy - rect.Y)));
            if (r <= 0f) r = 1f;

            w.WriteAttribute("cx", cx);
            w.WriteAttribute("cy", cy);
            w.WriteAttribute("r", r);
            // Focus point (fx, fy) — only emit if different from center
            float fx = cx + pg.FocusScales.X;
            float fy = cy + pg.FocusScales.Y;
            if (Math.Abs(fx - cx) > 0.001f || Math.Abs(fy - cy) > 0.001f)
            {
                w.WriteAttribute("fx", fx);
                w.WriteAttribute("fy", fy);
            }
            w.WriteAttribute("gradientUnits", "userSpaceOnUse");
            var matrix = pg.Transform;
            if (matrix != null && !matrix.IsIdentity)
                w.WriteAttribute("gradientTransform", SvgTransformWriter.WriteTransform(matrix));

            WriteStop(w, 0f, pg.CenterColor);
            var surround = pg.SurroundColors;
            if (surround.Length > 0)
            {
                // Single surround color → stop at 1
                if (surround.Length == 1)
                    WriteStop(w, 1f, surround[0]);
                else
                    for (int i = 0; i < surround.Length; i++)
                        WriteStop(w, (float)(i + 1) / surround.Length, surround[i]);
            }
            w.WriteEndElement();
        }

        // ── Hatch pattern ─────────────────────────────────────────────────────

        private static void WriteHatchPattern(SvgXmlWriter w, HatchBrush hb, string id)
        {
            w.WriteStartElement("pattern");
            w.WriteAttribute("id", id);
            w.WriteAttribute("width", 8);
            w.WriteAttribute("height", 8);
            w.WriteAttribute("patternUnits", "userSpaceOnUse");

            // Background rect
            w.WriteStartElement("rect");
            w.WriteAttribute("x", 0);
            w.WriteAttribute("y", 0);
            w.WriteAttribute("width", 8);
            w.WriteAttribute("height", 8);
            w.WriteAttribute("fill", ColorToCss(hb.BackgroundColor));
            w.WriteEndElement();

            // Foreground hatch geometry (simplified: a few common styles)
            WriteHatchForeground(w, hb);
            w.WriteEndElement();
        }

        private static void WriteHatchForeground(SvgXmlWriter w, HatchBrush hb)
        {
            string fg = ColorToCss(hb.ForegroundColor);
            switch (hb.HatchStyle)
            {
                case HatchStyle.Horizontal:
                    WriteLine(w, 0, 0, 8, 0, fg);
                    break;
                case HatchStyle.Vertical:
                    WriteLine(w, 0, 0, 0, 8, fg);
                    break;
                case HatchStyle.ForwardDiagonal:
                    WriteLine(w, 0, 0, 8, 8, fg);
                    break;
                case HatchStyle.BackwardDiagonal:
                    WriteLine(w, 0, 8, 8, 0, fg);
                    break;
                case HatchStyle.Cross:
                    WriteLine(w, 0, 0, 8, 0, fg);
                    WriteLine(w, 0, 0, 0, 8, fg);
                    break;
                case HatchStyle.DiagonalCross:
                    WriteLine(w, 0, 0, 8, 8, fg);
                    WriteLine(w, 0, 8, 8, 0, fg);
                    break;
                default:
                    // Default: solid foreground fill for unknown styles
                    w.WriteStartElement("rect");
                    w.WriteAttribute("x", 0); w.WriteAttribute("y", 0);
                    w.WriteAttribute("width", 8); w.WriteAttribute("height", 8);
                    w.WriteAttribute("fill", fg);
                    w.WriteEndElement();
                    break;
            }
        }

        private static void WriteLine(SvgXmlWriter w, float x1, float y1, float x2, float y2, string stroke)
        {
            w.WriteStartElement("line");
            w.WriteAttribute("x1", x1); w.WriteAttribute("y1", y1);
            w.WriteAttribute("x2", x2); w.WriteAttribute("y2", y2);
            w.WriteAttribute("stroke", stroke);
            w.WriteAttribute("stroke-width", 1);
            w.WriteEndElement();
        }

        // ── Texture pattern ───────────────────────────────────────────────────

        private static void WriteTexturePattern(SvgXmlWriter w, TextureBrush tb, string id)
        {
            w.WriteStartElement("pattern");
            w.WriteAttribute("id", id);
            w.WriteAttribute("width", tb.Image.Width);
            w.WriteAttribute("height", tb.Image.Height);
            w.WriteAttribute("patternUnits", "userSpaceOnUse");
            var matrix = tb.Transform;
            if (matrix != null && !matrix.IsIdentity)
                w.WriteAttribute("patternTransform", SvgTransformWriter.WriteTransform(matrix));

            // Embedded image
            w.WriteStartElement("image");
            w.WriteAttribute("x", 0);
            w.WriteAttribute("y", 0);
            w.WriteAttribute("width", tb.Image.Width);
            w.WriteAttribute("height", tb.Image.Height);
            w.WriteXLinkHref(SvgImageWriter.EmbedImage(tb.Image));
            w.WriteEndElement();

            w.WriteEndElement();
        }

        // ── Clip path ─────────────────────────────────────────────────────────

        private static void WriteClipPath(SvgXmlWriter w, SvgDef def)
        {
            w.WriteStartElement("clipPath");
            w.WriteAttribute("id", def.Id);
            if (def.Source is Region region)
            {
                var scans = region.GetRegionScans(new Matrix());
                foreach (var r in scans)
                {
                    w.WriteStartElement("rect");
                    w.WriteAttribute("x", r.X);
                    w.WriteAttribute("y", r.Y);
                    w.WriteAttribute("width", r.Width);
                    w.WriteAttribute("height", r.Height);
                    w.WriteEndElement();
                }
            }
            else if (def.ClipPath != null)
            {
                w.WriteStartElement("path");
                w.WriteAttribute("d", SvgPathWriter.WritePathData(def.ClipPath));
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void WriteStop(SvgXmlWriter w, float offset, Color color)
        {
            w.WriteStartElement("stop");
            w.WriteAttribute("offset", Fmt(offset));
            w.WriteAttribute("stop-color", ColorToCss(color));
            if (color.A < 255)
                w.WriteAttribute("stop-opacity", Fmt(color.A / 255f));
            w.WriteEndElement();
        }

        private static void WriteSpreadMethod(SvgXmlWriter w, WrapMode wrap)
        {
            string s = wrap switch
            {
                WrapMode.Tile      => "repeat",
                WrapMode.Clamp     => "pad",
                WrapMode.TileFlipX => "repeat",
                WrapMode.TileFlipY => "repeat",
                WrapMode.TileFlipXY => "repeat",
                _ => "pad",
            };
            w.WriteAttribute("spreadMethod", s);
        }

        /// <summary>Converts a Color to an SVG CSS color string: <c>rgb(r,g,b)</c>.</summary>
        public static string ColorToCss(Color color)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "rgb({0},{1},{2})", color.R, color.G, color.B);
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