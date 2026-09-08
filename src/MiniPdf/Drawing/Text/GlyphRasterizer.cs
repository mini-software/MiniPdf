using System.Collections.Generic;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Font.Rendering;
using MiniSoftware.Drawing.Font.Glyphs;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Implements <see cref="IGlyphOutlinePainter"/> to convert a glyph's vector outline
    /// into a <see cref="GraphicsPath"/> positioned at a given screen coordinate.
    /// <para>
    /// Font design space: X increases rightward, Y increases upward.
    /// Screen space:      X increases rightward, Y increases downward.
    /// The Y axis is therefore flipped: <c>screenY = baselineY − fontY × emScale</c>.
    /// </para>
    /// </summary>
    internal sealed class GlyphRasterizer : IGlyphOutlinePainter
    {
        private readonly List<PointF> _pts   = new List<PointF>();
        private readonly List<byte>   _types = new List<byte>();
        private float _scale;
        private float _dx;
        private float _dy;

        // ── Coordinate transforms ──────────────────────────────────────────────

        private float Sx(double fx) => _dx + (float)fx * _scale;
        private float Sy(double fy) => _dy - (float)fy * _scale;

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Decompose one glyph and return its outline as a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="GlyphOutlineRenderer"/> wrapping the font.</param>
        /// <param name="gid">Glyph identifier to render.</param>
        /// <param name="emScale">Ratio <c>emSizePixels / unitsPerEm</c>.</param>
        /// <param name="glyphX">Left edge of the glyph origin on screen, in pixels.</param>
        /// <param name="baselineY">Y of the text baseline on screen, in pixels.</param>
        public GraphicsPath RenderGlyph(
            GlyphOutlineRenderer renderer,
            GlyphId              gid,
            float                emScale,
            float                glyphX,
            float                baselineY)
        {
            _pts.Clear();
            _types.Clear();
            _scale = emScale;
            _dx    = glyphX;
            _dy    = baselineY;

            renderer.RenderGlyph(gid, this);

            if (_pts.Count == 0)
                return new GraphicsPath();

            return new GraphicsPath(_pts.ToArray(), _types.ToArray());
        }

        // ── IGlyphOutlinePainter ───────────────────────────────────────────────

        public void MoveTo(MoveTo cmd)
        {
            _pts.Add(new PointF(Sx(cmd.X), Sy(cmd.Y)));
            _types.Add((byte)PathPointType.Start);
        }

        public void LineTo(LineTo cmd)
        {
            if (_types.Count == 0)
            {
                // Well-formed glyph data always starts with MoveTo; guard anyway.
                _pts.Add(new PointF(_dx, _dy));
                _types.Add((byte)PathPointType.Start);
            }
            _pts.Add(new PointF(Sx(cmd.X), Sy(cmd.Y)));
            _types.Add((byte)PathPointType.Line);
        }

        public void CurveTo(CurveTo cmd)
        {
            if (_types.Count == 0)
            {
                _pts.Add(new PointF(_dx, _dy));
                _types.Add((byte)PathPointType.Start);
            }
            _pts.Add(new PointF(Sx(cmd.X1), Sy(cmd.Y1))); _types.Add((byte)PathPointType.Bezier);
            _pts.Add(new PointF(Sx(cmd.X2), Sy(cmd.Y2))); _types.Add((byte)PathPointType.Bezier);
            _pts.Add(new PointF(Sx(cmd.X3), Sy(cmd.Y3))); _types.Add((byte)PathPointType.Bezier);
        }

        public void ClosePath()
        {
            if (_types.Count > 0)
                _types[_types.Count - 1] |= (byte)PathPointType.CloseSubpath;
        }
    }
}
