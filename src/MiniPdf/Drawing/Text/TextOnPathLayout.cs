using System;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Font.Core;
using MiniPdf.Drawing.Font.Rendering;
using MiniPdf.Drawing.Geometry;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Core text-on-path layout engine. Renders a string along a baseline
    /// <see cref="GraphicsPath"/>, producing a fillable <see cref="GraphicsPath"/>
    /// whose glyphs are positioned and rotated to follow the baseline.
    /// </summary>
    internal static class TextOnPathLayout
    {
        /// <summary>
        /// Build a <see cref="GraphicsPath"/> containing the glyphs of
        /// <paramref name="text"/> placed along <paramref name="baseline"/>.
        /// </summary>
        /// <param name="text">Text to place. Empty returns an empty path.</param>
        /// <param name="font">Font describing size, style, and family.</param>
        /// <param name="baseline">Baseline path in pixel coordinates.</param>
        /// <param name="sf">String format (null → default). Only <see cref="StringFormat.Alignment"/> is used.</param>
        /// <param name="options">Placement options (null → defaults).</param>
        /// <param name="dpiX">Horizontal DPI of the render target.</param>
        /// <param name="dpiY">Vertical DPI of the render target.</param>
        public static GraphicsPath BuildPath(
            string text,
            Font font,
            GraphicsPath baseline,
            StringFormat? sf,
            TextOnPathOptions? options,
            float dpiX,
            float dpiY)
        {
            var result = new GraphicsPath();
            if (string.IsNullOrEmpty(text)) return result;
            if (baseline == null) throw new ArgumentNullException(nameof(baseline));
            if (font == null) throw new ArgumentNullException(nameof(font));
            if (baseline.PointCount == 0) return result;

            sf = sf ?? StringFormat.GenericDefault;
            options = options ?? new TextOnPathOptions();

            // ── 1. Font metrics in pixels ─────────────────────────────────────
            var metrics = font.FontFamily.GetMetricsData(font.Style);
            float emPixels = TextLayoutEngine.FontSizeToPixels(font.Size, font.Unit, dpiY);
            float emScale = metrics.UnitsPerEm > 0
                ? (float)(emPixels / metrics.UnitsPerEm)
                : 1f;

            IFont? fontFace = font.FontFamily.GetFontFace(font.Style);
            if (fontFace == null) return result; // no outlines available

            // ── 2. Arc-length parameterize the baseline ───────────────────────
            var arc = PathArcLength.Build(baseline);
            if (arc.TotalLength <= 0f) return result;

            // ── 3. Measure total text advance width ───────────────────────────
            float firstTabOffset;
            float[] tabStops = sf.GetTabStops(out firstTabOffset);
            float fallbackWidth = emPixels * TextLayoutEngine.DefaultCharWidthFactor;

            float textWidth = 0f;
            foreach (char c in text)
            {
                textWidth += TextLayoutEngine.GetCharAdvance(
                    c, textWidth, fontFace, emScale, fallbackWidth,
                    firstTabOffset, tabStops, emPixels);
            }

            // ── 4. Compute start distance from alignment + StartOffset ────────
            float usableLength = arc.TotalLength - options.StartOffset;
            StringAlignment alignment = options.Alignment ?? sf.Alignment;
            float start;
            switch (alignment)
            {
                case StringAlignment.Center:
                    start = options.StartOffset + Math.Max(0f, (usableLength - textWidth) / 2f);
                    break;
                case StringAlignment.Far:
                    start = options.StartOffset + Math.Max(0f, usableLength - textWidth);
                    break;
                default: // Near
                    start = options.StartOffset;
                    break;
            }
            if (start < 0f) start = 0f;

            // ── 5. Render glyphs ──────────────────────────────────────────────
            var renderer = new GlyphOutlineRenderer(fontFace);
            var rasterizer = new GlyphRasterizer();
            using var m = new Matrix();
            float currentDist = start;

            foreach (char c in text)
            {
                // Tab / CR / LF: advance width only, no glyph.
                if (c == '\t' || c == '\r' || c == '\n')
                {
                    currentDist += TextLayoutEngine.GetCharAdvance(
                        c, currentDist - start, fontFace, emScale, fallbackWidth,
                        firstTabOffset, tabStops, emPixels);
                    continue;
                }

                float adv = TextLayoutEngine.GetCharAdvance(
                    c, currentDist - start, fontFace, emScale, fallbackWidth,
                    firstTabOffset, tabStops, emPixels);

                // Overflow check.
                if (currentDist > arc.TotalLength && options.Overflow == TextOnPathOverflow.Stop)
                    break;

                var gid = fontFace.Encoding.DecodeToGid(c);
                if (gid != null)
                {
                    bool ok = arc.GetPointAt(currentDist, out PointF pos, out float angle);
                    if (ok)
                    {
                        // Perpendicular offset: "above" = (sin, -cos) * offset.
                        float rad = angle * (float)(Math.PI / 180.0);
                        float perpX = (float)Math.Sin(rad) * options.PerpendicularOffset;
                        float perpY = -(float)Math.Cos(rad) * options.PerpendicularOffset;

                        // Render glyph at local origin (0,0), baseline on X axis.
                        using var glyphPath = rasterizer.RenderGlyph(
                            renderer, gid, emScale, 0f, 0f);
                        if (glyphPath.PointCount > 0)
                        {
                            // Matrix = R * T  (rotate around glyph origin, then translate).
                            m.Reset();
                            m.Translate(pos.X + perpX, pos.Y + perpY, MatrixOrder.Append);
                            m.Rotate(angle, MatrixOrder.Prepend);
                            glyphPath.Transform(m);
                            result.AddPath(glyphPath, false);
                        }
                    }
                }

                currentDist += adv;
            }

            return result;
        }
    }
}