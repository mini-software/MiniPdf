namespace MiniPdf.Drawing.Font.Rendering
{
    /// <summary>
    /// Receives decomposed glyph outline drawing commands from an outline renderer.
    /// Implement this interface to paint, record, or convert glyph paths
    /// (e.g. to a <c>GraphicsPath</c>, PDF stream, or SVG path).
    /// </summary>
    public interface IGlyphOutlinePainter
    {
        /// <summary>
        /// Lifts the pen and begins a new sub-path at the given point.
        /// Must be called at the start of each contour.
        /// </summary>
        void MoveTo(MoveTo cmd);

        /// <summary>Draws a straight line to the given endpoint.</summary>
        void LineTo(LineTo cmd);

        /// <summary>
        /// Draws a cubic Bézier curve using two control points.
        /// TrueType quadratic splines are pre-converted to this cubic form.
        /// </summary>
        void CurveTo(CurveTo cmd);

        /// <summary>
        /// Closes the current sub-path by drawing a straight line back to the
        /// point set by the most recent <see cref="MoveTo"/>.
        /// Called once at the end of each contour.
        /// </summary>
        void ClosePath();
    }
}
