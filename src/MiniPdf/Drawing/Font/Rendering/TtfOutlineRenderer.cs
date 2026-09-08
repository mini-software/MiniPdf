using System;
using MiniSoftware.Drawing.Font.Ttf.Glyphs;
using MiniSoftware.Drawing.Font.Ttf.Tables;

namespace MiniSoftware.Drawing.Font.Rendering
{
    /// <summary>
    /// Decomposes a TrueType glyph outline (<see cref="GlyphOutlineData"/>) into
    /// <see cref="IGlyphOutlinePainter"/> commands.
    ///
    /// <para><b>Simple glyphs</b> — each closed contour is walked and quadratic
    /// Bézier control points are converted to cubic form before being emitted.</para>
    ///
    /// <para><b>Composite glyphs</b> — each component is resolved recursively via
    /// the supplied <see cref="TtfGlyfTable"/> and rendered with the component's
    /// 2×3 affine transform composed into the current transform.</para>
    ///
    /// <para><b>Quadratic → cubic conversion</b> (standard formula):
    /// <code>
    ///   C1 = (Q0 + 2·Q1) / 3
    ///   C2 = (2·Q1 + Q2) / 3
    /// </code></para>
    /// </summary>
    public static class TtfOutlineRenderer
    {
        private const int MaxRecursionDepth = 8; // guard against malformed fonts

        // ── Public entry ──────────────────────────────────────────────────────

        /// <summary>
        /// Renders <paramref name="outline"/> using the identity transform.
        /// </summary>
        /// <param name="outline">Parsed glyph outline from <see cref="TtfGlyfTable"/>.</param>
        /// <param name="painter">Target that receives drawing commands.</param>
        /// <param name="glyfTable">
        /// Required for composite glyphs (component look-up); may be null for simple glyphs.
        /// </param>
        public static void Render(
            GlyphOutlineData outline,
            IGlyphOutlinePainter painter,
            TtfGlyfTable? glyfTable = null)
        {
            if (outline is null) throw new ArgumentNullException(nameof(outline));
            if (painter is null) throw new ArgumentNullException(nameof(painter));

            RenderInternal(outline, painter, glyfTable,
                           a: 1f, b: 0f, c: 0f, d: 1f, e: 0f, f: 0f, depth: 0);
        }

        // ── Internal recursive dispatcher ─────────────────────────────────────

        private static void RenderInternal(
            GlyphOutlineData outline,
            IGlyphOutlinePainter painter,
            TtfGlyfTable? glyfTable,
            float a, float b, float c, float d, float e, float f,
            int depth)
        {
            if (depth > MaxRecursionDepth) return;

            if (outline.IsComposite)
            {
                if (outline.Composite == null || glyfTable == null) return;
                RenderComposite(outline.Composite, painter, glyfTable, a, b, c, d, e, f, depth);
            }
            else
            {
                if (outline.Simple == null) return;
                RenderSimple(outline.Simple, painter, a, b, c, d, e, f);
            }
        }

        // ── Composite glyph ───────────────────────────────────────────────────

        private static void RenderComposite(
            TtfCompositeGlyph composite,
            IGlyphOutlinePainter painter,
            TtfGlyfTable glyfTable,
            float a, float b, float c, float d, float e, float f,
            int depth)
        {
            foreach (var component in composite.Components)
            {
                var compOutline = glyfTable.GetGlyphOutline(component.GlyphIndex);
                if (compOutline == null) continue;

                // Component local transform: T  (2×3 affine matrix)
                //   row 0: T[0,0]=ca, T[0,1]=cb, T[0,2]=ce
                //   row 1: T[1,0]=cc, T[1,1]=cd, T[1,2]=cf
                // Transform order: design → component → outer (current).
                // Point: x'' = a*(ca*x + cb*y + ce) + b*(cc*x + cd*y + cf) + e
                var T  = component.Transform;
                float ca = T[0, 0], cb = T[0, 1], ce = T[0, 2];
                float cc = T[1, 0], cd = T[1, 1], cf = T[1, 2];

                float na = a * ca + b * cc;
                float nb = a * cb + b * cd;
                float ne = a * ce + b * cf + e;

                float nc = c * ca + d * cc;
                float nd = c * cb + d * cd;
                float nf = c * ce + d * cf + f;

                RenderInternal(compOutline, painter, glyfTable, na, nb, nc, nd, ne, nf, depth + 1);
            }
        }

        // ── Simple glyph ──────────────────────────────────────────────────────

        private static void RenderSimple(
            TtfSimpleGlyph glyph,
            IGlyphOutlinePainter painter,
            float a, float b, float c, float d, float e, float f)
        {
            if (glyph.NumberOfContours <= 0) return;

            int from = 0;
            for (int ci = 0; ci < glyph.NumberOfContours; ci++)
            {
                int end   = glyph.EndPtsOfContours[ci];
                int count = end - from + 1;
                if (count > 0)
                    RenderContour(glyph.Points, from, count, painter, a, b, c, d, e, f);
                from = end + 1;
            }
        }

        // ── Contour rendering ─────────────────────────────────────────────────

        /// <summary>
        /// Renders one closed contour from <paramref name="pts"/>[<paramref name="from"/>
        /// .. from+count−1].
        /// </summary>
        /// <remarks>
        /// Algorithm:
        /// <list type="number">
        ///   <item>Find the first on-curve point to use as the MoveTo start.
        ///         If no on-curve point exists (all-off-curve contour), use the midpoint
        ///         of the last and first point as the implicit start.</item>
        ///   <item>Walk <em>count</em> points cyclically from the start; for each:
        ///     <list type="bullet">
        ///       <item>on-curve after on-curve → LineTo</item>
        ///       <item>on-curve after off-curve → emit quad→cubic CurveTo</item>
        ///       <item>off-curve after off-curve → emit quad→cubic to implicit midpoint,
        ///             then begin new control point</item>
        ///     </list>
        ///   </item>
        ///   <item>If a control point is pending after the loop, emit the closing curve
        ///         back to the start.</item>
        ///   <item>ClosePath.</item>
        /// </list>
        /// </remarks>
        private static void RenderContour(
            TtfPoint[] pts, int from, int count,
            IGlyphOutlinePainter painter,
            float a, float b, float c, float d, float e, float f)
        {
            if (count <= 0) return;

            // Inline affine transform: x' = a·x + b·y + e,  y' = c·x + d·y + f
            void Tx(double px, double py, out double rx, out double ry)
            {
                rx = a * px + b * py + e;
                ry = c * px + d * py + f;
            }

            // ── Find start ────────────────────────────────────────────────────
            int firstOnCurve = -1;
            for (int i = 0; i < count; i++)
                if (pts[from + i].OnCurve) { firstOnCurve = i; break; }

            double startX, startY;
            int startIndex; // first index to process in the cyclic walk

            if (firstOnCurve < 0)
            {
                // All-off-curve contour: implicit start at midpoint of last and first point.
                double mx = (pts[from].X + pts[from + count - 1].X) / 2.0;
                double my = (pts[from].Y + pts[from + count - 1].Y) / 2.0;
                Tx(mx, my, out startX, out startY);
                startIndex = 0; // process all n off-curve points
            }
            else
            {
                Tx(pts[from + firstOnCurve].X, pts[from + firstOnCurve].Y, out startX, out startY);
                startIndex = firstOnCurve + 1;
            }

            painter.MoveTo(new MoveTo(startX, startY));

            double curX  = startX;
            double curY  = startY;
            double ctrlX = 0;
            double ctrlY = 0;
            bool   hasCtrl = false;

            // ── Walk count points cyclically (natural close-back) ─────────────
            for (int step = 0; step < count; step++)
            {
                var pt = pts[from + (startIndex + step) % count];
                Tx(pt.X, pt.Y, out double px, out double py);

                if (pt.OnCurve)
                {
                    if (hasCtrl)
                    {
                        EmitQuadAsCubic(curX, curY, ctrlX, ctrlY, px, py, painter);
                        hasCtrl = false;
                    }
                    else
                    {
                        painter.LineTo(new LineTo(px, py));
                    }
                    curX = px;
                    curY = py;
                }
                else // off-curve
                {
                    if (hasCtrl)
                    {
                        // Two consecutive off-curve points → implicit on-curve at midpoint.
                        // Midpoint in transformed space = transform of midpoint in design space
                        // (affine transforms preserve midpoints).
                        double midX = (ctrlX + px) / 2.0;
                        double midY = (ctrlY + py) / 2.0;
                        EmitQuadAsCubic(curX, curY, ctrlX, ctrlY, midX, midY, painter);
                        curX = midX;
                        curY = midY;
                    }
                    ctrlX  = px;
                    ctrlY  = py;
                    hasCtrl = true;
                }
            }

            // If a pending control remains (only possible in all-off-curve contours),
            // close back to the start with a final curve.
            if (hasCtrl)
                EmitQuadAsCubic(curX, curY, ctrlX, ctrlY, startX, startY, painter);

            painter.ClosePath();
        }

        // ── Quadratic → cubic Bézier conversion ──────────────────────────────

        /// <summary>
        /// Converts a quadratic Bézier (Q0 = current, Q1 = control, Q2 = end) to a
        /// cubic Bézier and emits it.
        /// <para>
        /// C1 = (Q0 + 2·Q1) / 3 <br/>
        /// C2 = (2·Q1 + Q2) / 3
        /// </para>
        /// </summary>
        private static void EmitQuadAsCubic(
            double q0X, double q0Y,  // current position (Q0)
            double q1X, double q1Y,  // quadratic off-curve control (Q1)
            double q2X, double q2Y,  // segment endpoint (Q2)
            IGlyphOutlinePainter painter)
        {
            double c1X = (q0X + 2.0 * q1X) / 3.0;
            double c1Y = (q0Y + 2.0 * q1Y) / 3.0;
            double c2X = (2.0 * q1X + q2X) / 3.0;
            double c2Y = (2.0 * q1Y + q2Y) / 3.0;

            painter.CurveTo(new CurveTo(c1X, c1Y, c2X, c2Y, q2X, q2Y));
        }
    }
}
