using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Font.Core;
using MiniPdf.Drawing.Font.Rendering;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Pens;
using MiniPdf.Drawing.Text;
using MiniPdf.Drawing.Text.Bidi;

namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Represents a series of connected lines and curves that form a closed or open shape.
    /// </summary>
    /// <remarks>
    /// A <see cref="GraphicsPath"/> can be used to draw and fill complex shapes.
    /// It supports lines, arcs, Bézier curves, rectangles, and ellipses.
    /// </remarks>
    public sealed class GraphicsPath : IDisposable
    {
        private readonly List<PointF> _points;
        private readonly List<byte>   _types;
        private FillMode              _fillMode;
        private bool                  _newFigure = true;

        private const float EllipseKappa = 0.5522847498f;
        private const float DefaultFlatness = 0.25f;

        // -----------------------------------------------------------------------
        // Constructors
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="GraphicsPath"/> with the default fill mode.
        /// </summary>
        public GraphicsPath() : this(FillMode.Alternate) { }

        /// <summary>
        /// Initializes a new <see cref="GraphicsPath"/> with the specified fill mode.
        /// </summary>
        /// <param name="fillMode">The fill mode that determines how the interior of the path is filled.</param>
        public GraphicsPath(FillMode fillMode)
        {
            _fillMode = fillMode;
            _points   = new List<PointF>();
            _types    = new List<byte>();
        }

        /// <summary>
        /// Initializes a new <see cref="GraphicsPath"/> from an array of points and types.
        /// </summary>
        /// <param name="pts">The array of points that define the path.</param>
        /// <param name="types">The array of point types that define the corresponding point types.</param>
        /// <param name="fillMode">The fill mode that determines how the interior of the path is filled.</param>
        public GraphicsPath(PointF[] pts, byte[] types, FillMode fillMode = FillMode.Alternate)
        {
            if (pts   == null) throw new ArgumentNullException(nameof(pts));
            if (types == null) throw new ArgumentNullException(nameof(types));
            if (pts.Length != types.Length)
                throw new ArgumentException("pts and types must be the same length.");
            _fillMode  = fillMode;
            _points    = new List<PointF>(pts);
            _types     = new List<byte>(types);
            _newFigure = _types.Count == 0
                      || (_types[_types.Count - 1] & (byte)PathPointType.CloseSubpath) != 0;
        }

        // -----------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the fill mode for this path.
        /// </summary>
        public FillMode FillMode { get => _fillMode; set => _fillMode = value; }

        /// <summary>
        /// Gets the number of points in this path.
        /// </summary>
        public int PointCount => _points.Count;

        /// <summary>
        /// Gets the array of points in this path.
        /// </summary>
        public PointF[] PathPoints => _points.ToArray();

        /// <summary>
        /// Gets the array of point types in this path.
        /// </summary>
        public byte[] PathTypes => _types.ToArray();

        /// <summary>
        /// Gets the path data for this path.
        /// </summary>
        public PathData PathData => new PathData { Points = _points.ToArray(), Types = _types.ToArray() };

        // -----------------------------------------------------------------------
        // Figure control
        // -----------------------------------------------------------------------

        /// <summary>
        /// Starts a new figure in this path.
        /// </summary>
        public void StartFigure() => _newFigure = true;

        /// <summary>
        /// Closes the current figure by connecting a line from the current point to the start point.
        /// </summary>
        public void CloseFigure()
        {
            if (_types.Count > 0 && !_newFigure)
                _types[_types.Count - 1] |= (byte)PathPointType.CloseSubpath;
            _newFigure = true;
        }

        /// <summary>
        /// Closes all open figures in this path.
        /// </summary>
        public void CloseAllFigures()
        {
            for (int i = 1; i < _types.Count; i++)
                if ((_types[i] & 0x07) == (byte)PathPointType.Start)
                    _types[i - 1] |= (byte)PathPointType.CloseSubpath;

            if (_types.Count > 0)
                _types[_types.Count - 1] |= (byte)PathPointType.CloseSubpath;

            _newFigure = true;
        }

        // -----------------------------------------------------------------------
        // Lines
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds a line to this path.
        /// </summary>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        public void AddLine(float x1, float y1, float x2, float y2)
            => AddLine(new PointF(x1, y1), new PointF(x2, y2));

        /// <summary>
        /// Adds a line to this path.
        /// </summary>
        /// <param name="pt1">The start point of the line.</param>
        /// <param name="pt2">The end point of the line.</param>
        public void AddLine(PointF pt1, PointF pt2)
        {
            _points.Add(pt1);
            _types.Add(_newFigure ? (byte)PathPointType.Start : (byte)PathPointType.Line);
            _newFigure = false;

            _points.Add(pt2);
            _types.Add((byte)PathPointType.Line);
        }

        /// <summary>
        /// Adds a series of connected lines to this path.
        /// </summary>
        /// <param name="points">The array of points that define the lines.</param>
        public void AddLines(PointF[] points)
        {
            if (points == null || points.Length == 0) return;

            for (int i = 0; i < points.Length; i++)
            {
                _points.Add(points[i]);
                _types.Add(i == 0 && _newFigure
                    ? (byte)PathPointType.Start
                    : (byte)PathPointType.Line);
            }
            _newFigure = false;
        }

        // -----------------------------------------------------------------------
        // Rectangles
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        /// <param name="rect">The rectangle to add.</param>
        public void AddRectangle(RectangleF rect)
        {
            _points.Add(new PointF(rect.X, rect.Y));
            _types.Add((byte)PathPointType.Start);

            _points.Add(new PointF(rect.X + rect.Width, rect.Y));
            _types.Add((byte)PathPointType.Line);

            _points.Add(new PointF(rect.X + rect.Width, rect.Y + rect.Height));
            _types.Add((byte)PathPointType.Line);

            _points.Add(new PointF(rect.X, rect.Y + rect.Height));
            _types.Add((byte)((byte)PathPointType.Line | (byte)PathPointType.CloseSubpath));

            _newFigure = true;
        }

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        /// <param name="rect">The rectangle to add.</param>
        public void AddRectangle(Rectangle rect)
            => AddRectangle(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        /// <param name="rects">The array of rectangles to add.</param>
        public void AddRectangles(RectangleF[] rects)
        {
            if (rects == null) return;
            foreach (var r in rects) AddRectangle(r);
        }

        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        /// <param name="rects">The array of rectangles to add.</param>
        public void AddRectangles(Rectangle[] rects)
        {
            if (rects == null) return;
            foreach (var r in rects) AddRectangle(r);
        }

        // -----------------------------------------------------------------------
        // Ellipse  (4 cubic Béziers, kappa = 0.5522847498)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds an ellipse to this path.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle.</param>
        /// <param name="width">The width of the ellipse.</param>
        /// <param name="height">The height of the ellipse.</param>
        public void AddEllipse(float x, float y, float width, float height)
            => AddEllipseCore(x + width * 0.5f, y + height * 0.5f, width * 0.5f, height * 0.5f);

        /// <summary>
        /// Adds an ellipse to this path.
        /// </summary>
        /// <param name="rect">The bounding rectangle of the ellipse.</param>
        public void AddEllipse(RectangleF rect)
            => AddEllipseCore(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.5f,
                              rect.Width * 0.5f, rect.Height * 0.5f);

        /// <summary>
        /// Adds an ellipse to this path.
        /// </summary>
        /// <param name="rect">The bounding rectangle of the ellipse.</param>
        public void AddEllipse(Rectangle rect)
            => AddEllipse(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

        private void AddEllipseCore(float cx, float cy, float rx, float ry)
        {
            float k = EllipseKappa;

            _points.Add(new PointF(cx + rx, cy)); _types.Add((byte)PathPointType.Start);

            // Segment 0 → (cx, cy+ry)
            AddBez(cx + rx, cy + k * ry,   cx + k * rx, cy + ry,   cx, cy + ry);
            // Segment 1 → (cx-rx, cy)
            AddBez(cx - k * rx, cy + ry,   cx - rx, cy + k * ry,   cx - rx, cy);
            // Segment 2 → (cx, cy-ry)
            AddBez(cx - rx, cy - k * ry,   cx - k * rx, cy - ry,   cx, cy - ry);
            // Segment 3 → (cx+rx, cy) — close
            AddBez(cx + k * rx, cy - ry,   cx + rx, cy - k * ry,   cx + rx, cy, close: true);

            _newFigure = true;
        }

        private void AddBez(float c1x, float c1y, float c2x, float c2y, float ex, float ey,
                            bool close = false)
        {
            _points.Add(new PointF(c1x, c1y)); _types.Add((byte)PathPointType.Bezier);
            _points.Add(new PointF(c2x, c2y)); _types.Add((byte)PathPointType.Bezier);
            _points.Add(new PointF(ex,  ey));
            _types.Add(close
                ? (byte)((byte)PathPointType.Bezier | (byte)PathPointType.CloseSubpath)
                : (byte)PathPointType.Bezier);
        }

        // -----------------------------------------------------------------------
        // Arc  (up to 4 Bézier segments, split at 90° boundaries)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds an arc to this path.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle.</param>
        /// <param name="width">The width of the bounding rectangle.</param>
        /// <param name="height">The height of the bounding rectangle.</param>
        /// <param name="startAngle">The starting angle of the arc in degrees.</param>
        /// <param name="sweepAngle">The sweep angle of the arc in degrees.</param>
        public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
            => AddArcCore(x + width * 0.5f, y + height * 0.5f, width * 0.5f, height * 0.5f,
                          startAngle, sweepAngle);

        /// <summary>
        /// Adds an arc to this path.
        /// </summary>
        /// <param name="rect">The bounding rectangle of the arc.</param>
        /// <param name="startAngle">The starting angle of the arc in degrees.</param>
        /// <param name="sweepAngle">The sweep angle of the arc in degrees.</param>
        public void AddArc(RectangleF rect, float startAngle, float sweepAngle)
            => AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

        /// <summary>
        /// Adds an arc to this path.
        /// </summary>
        /// <param name="rect">The bounding rectangle of the arc.</param>
        /// <param name="startAngle">The starting angle of the arc in degrees.</param>
        /// <param name="sweepAngle">The sweep angle of the arc in degrees.</param>
        public void AddArc(Rectangle rect, float startAngle, float sweepAngle)
            => AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

        private void AddArcCore(float cx, float cy, float rx, float ry,
                                float startAngle, float sweepAngle)
        {
            if (sweepAngle == 0f) return;
            if (sweepAngle >  360f) sweepAngle =  360f;
            if (sweepAngle < -360f) sweepAngle = -360f;

            int   sign      = sweepAngle >= 0 ? 1 : -1;
            float remaining = Math.Abs(sweepAngle);
            float cur       = startAngle;
            bool  isFirst   = true;

            while (remaining > 0f)
            {
                float seg = Math.Min(remaining, 90f) * sign;
                AddArcSegment(cx, cy, rx, ry, cur, seg, isFirst);
                isFirst    = false;
                cur       += seg;
                remaining -= Math.Abs(seg);
            }
        }

        private void AddArcSegment(float cx, float cy, float rx, float ry,
                                   float startDeg, float sweepDeg, bool isFirst)
        {
            double a0  = startDeg            * (Math.PI / 180.0);
            double a1  = (startDeg + sweepDeg) * (Math.PI / 180.0);
            double da  = a1 - a0;
            double k   = (4.0 / 3.0) * Math.Tan(da / 4.0);

            float cos0 = (float)Math.Cos(a0), sin0 = (float)Math.Sin(a0);
            float cos1 = (float)Math.Cos(a1), sin1 = (float)Math.Sin(a1);

            var p0 = new PointF(cx + rx * cos0, cy + ry * sin0);
            var c1 = new PointF(cx + rx * (cos0 - (float)(k * sin0)),
                                cy + ry * (sin0 + (float)(k * cos0)));
            var c2 = new PointF(cx + rx * (cos1 + (float)(k * sin1)),
                                cy + ry * (sin1 - (float)(k * cos1)));
            var p1 = new PointF(cx + rx * cos1, cy + ry * sin1);

            if (isFirst)
            {
                _points.Add(p0);
                _types.Add(_newFigure ? (byte)PathPointType.Start : (byte)PathPointType.Line);
                _newFigure = false;
            }

            _points.Add(c1); _types.Add((byte)PathPointType.Bezier);
            _points.Add(c2); _types.Add((byte)PathPointType.Bezier);
            _points.Add(p1); _types.Add((byte)PathPointType.Bezier);
        }

        // -----------------------------------------------------------------------
        // Béziers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds a cubic Bézier curve to this path.
        /// </summary>
        /// <param name="pt1">The starting point of the curve.</param>
        /// <param name="pt2">The first control point for the curve.</param>
        /// <param name="pt3">The second control point for the curve.</param>
        /// <param name="pt4">The ending point of the curve.</param>
        public void AddBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            _points.Add(pt1);
            _types.Add(_newFigure ? (byte)PathPointType.Start : (byte)PathPointType.Line);
            _newFigure = false;
            _points.Add(pt2); _types.Add((byte)PathPointType.Bezier);
            _points.Add(pt3); _types.Add((byte)PathPointType.Bezier);
            _points.Add(pt4); _types.Add((byte)PathPointType.Bezier);
        }

        /// <summary>
        /// Adds a cubic Bézier curve to this path.
        /// </summary>
        /// <param name="x1">The x-coordinate of the starting point.</param>
        /// <param name="y1">The y-coordinate of the starting point.</param>
        /// <param name="x2">The x-coordinate of the first control point.</param>
        /// <param name="y2">The y-coordinate of the first control point.</param>
        /// <param name="x3">The x-coordinate of the second control point.</param>
        /// <param name="y3">The y-coordinate of the second control point.</param>
        /// <param name="x4">The x-coordinate of the ending point.</param>
        /// <param name="y4">The y-coordinate of the ending point.</param>
        public void AddBezier(float x1, float y1, float x2, float y2,
                              float x3, float y3, float x4, float y4)
            => AddBezier(new PointF(x1, y1), new PointF(x2, y2),
                         new PointF(x3, y3), new PointF(x4, y4));

        /// <summary>
        /// Adds a series of connected Bézier curves to this path.
        /// </summary>
        /// <param name="points">The array of points that define the curves. Must have length 1 + 3n.</param>
        public void AddBeziers(PointF[] points)
        {
            if (points == null || points.Length < 4) return;
            if ((points.Length - 1) % 3 != 0)
                throw new ArgumentException("Length must be 1 + 3n.", nameof(points));

            _points.Add(points[0]);
            _types.Add(_newFigure ? (byte)PathPointType.Start : (byte)PathPointType.Line);
            _newFigure = false;

            for (int i = 1; i < points.Length; i++)
            {
                _points.Add(points[i]);
                _types.Add((byte)PathPointType.Bezier);
            }
        }

        // -----------------------------------------------------------------------
        // Polygon
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        /// <param name="points">The array of points that define the polygon.</param>
        public void AddPolygon(PointF[] points)
        {
            if (points == null || points.Length < 2) return;

            _points.Add(points[0]); _types.Add((byte)PathPointType.Start);
            for (int i = 1; i < points.Length; i++)
            {
                _points.Add(points[i]); _types.Add((byte)PathPointType.Line);
            }
            _types[_types.Count - 1] |= (byte)PathPointType.CloseSubpath;
            _newFigure = true;
        }

        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        /// <param name="points">The array of points that define the polygon.</param>
        public void AddPolygon(Point[] points)
        {
            if (points == null) return;
            var f = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++) f[i] = new PointF(points[i].X, points[i].Y);
            AddPolygon(f);
        }

        // -----------------------------------------------------------------------
        // Cardinal spline  (Catmull-Rom → Bézier conversion)
        // c1 = p[i] + t*(p[i+1]-p[i-1])/3   c2 = p[i+1] - t*(p[i+2]-p[i])/3
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds a cardinal spline curve to this path.
        /// </summary>
        /// <param name="points">The array of points that define the curve.</param>
        public void AddCurve(PointF[] points) => AddCurve(points, 0.5f);

        /// <summary>
        /// Adds a cardinal spline curve to this path with the specified tension.
        /// </summary>
        /// <param name="points">The array of points that define the curve.</param>
        /// <param name="tension">The tension of the curve (0.0 to 1.0).</param>
        public void AddCurve(PointF[] points, float tension)
        {
            if (points == null || points.Length < 2)
                throw new ArgumentException("Need ≥ 2 points.", nameof(points));
            AddCurve(points, 0, points.Length - 1, tension);
        }

        /// <summary>
        /// Adds a cardinal spline curve to this path with the specified offset, number of segments, and tension.
        /// </summary>
        /// <param name="points">The array of points that define the curve.</param>
        /// <param name="offset">The offset in the points array where the curve begins.</param>
        /// <param name="numberOfSegments">The number of segments in the curve.</param>
        /// <param name="tension">The tension of the curve (0.0 to 1.0).</param>
        public void AddCurve(PointF[] points, int offset, int numberOfSegments, float tension)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (numberOfSegments < 1)
                throw new ArgumentException("numberOfSegments ≥ 1 required.");
            if (offset < 0 || offset + numberOfSegments >= points.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            _points.Add(points[offset]);
            _types.Add(_newFigure ? (byte)PathPointType.Start : (byte)PathPointType.Line);
            _newFigure = false;

            for (int i = offset; i < offset + numberOfSegments; i++)
            {
                PointF p0  = points[i];
                PointF p1  = points[i + 1];
                PointF pm1 = i > 0               ? points[i - 1]    : points[i];
                PointF pp2 = i + 2 < points.Length ? points[i + 2]  : points[i + 1];

                var c1 = new PointF(p0.X + tension * (p1.X - pm1.X) / 3f,
                                    p0.Y + tension * (p1.Y - pm1.Y) / 3f);
                var c2 = new PointF(p1.X - tension * (pp2.X - p0.X) / 3f,
                                    p1.Y - tension * (pp2.Y - p0.Y) / 3f);

                _points.Add(c1); _types.Add((byte)PathPointType.Bezier);
                _points.Add(c2); _types.Add((byte)PathPointType.Bezier);
                _points.Add(p1); _types.Add((byte)PathPointType.Bezier);
            }
        }

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        /// <param name="points">The array of points that define the curve.</param>
        /// <param name="tension">The tension of the curve (0.0 to 1.0).</param>
        public void AddClosedCurve(PointF[] points, float tension = 0.5f)
        {
            if (points == null || points.Length < 3)
                throw new ArgumentException("Need ≥ 3 points.", nameof(points));

            int n = points.Length;
            _points.Add(points[0]); _types.Add((byte)PathPointType.Start);

            for (int i = 0; i < n; i++)
            {
                PointF p0  = points[i];
                PointF p1  = points[(i + 1) % n];
                PointF pm1 = points[(i + n - 1) % n];
                PointF pp2 = points[(i + 2) % n];

                var c1 = new PointF(p0.X + tension * (p1.X - pm1.X) / 3f,
                                    p0.Y + tension * (p1.Y - pm1.Y) / 3f);
                var c2 = new PointF(p1.X - tension * (pp2.X - p0.X) / 3f,
                                    p1.Y - tension * (pp2.Y - p0.Y) / 3f);

                _points.Add(c1); _types.Add((byte)PathPointType.Bezier);
                _points.Add(c2); _types.Add((byte)PathPointType.Bezier);
                byte endType = (byte)PathPointType.Bezier;
                if (i == n - 1) endType |= (byte)PathPointType.CloseSubpath;
                _points.Add(p1); _types.Add(endType);
            }
            _newFigure = true;
        }

        // -----------------------------------------------------------------------
        // Sub-path composition
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds the specified path to this path.
        /// </summary>
        /// <param name="addingPath">The path to add.</param>
        /// <param name="connect">Whether to connect the added path to the current path.</param>
        public void AddPath(GraphicsPath addingPath, bool connect)
        {
            if (addingPath == null) throw new ArgumentNullException(nameof(addingPath));
            if (addingPath._points.Count == 0) return;

            int insertAt = _points.Count;
            _points.AddRange(addingPath._points);
            _types.AddRange(addingPath._types);

            // If connecting and there is an open figure, change the added path's first
            // Start point into a Line so it continues the current subpath.
            if (connect && !_newFigure && insertAt > 0)
                _types[insertAt] = (byte)((_types[insertAt] & ~0x07) | (byte)PathPointType.Line);

            _newFigure = _types.Count > 0
                      && (_types[_types.Count - 1] & (byte)PathPointType.CloseSubpath) != 0;
        }

        // -----------------------------------------------------------------------
        // AddString — text layout engine (Batch 12)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        /// <param name="s">The string to add.</param>
        /// <param name="family">The font family.</param>
        /// <param name="style">The font style.</param>
        /// <param name="emSize">The em size of the font.</param>
        /// <param name="origin">The origin point.</param>
        /// <param name="format">The string format.</param>
        public void AddString(string s, FontFamily family, int style, float emSize,
                              PointF origin, StringFormat? format)
            => AddString(s, family, style, emSize,
                new RectangleF(origin.X, origin.Y, 1e7f, 1e7f),
                format ?? new StringFormat(StringFormatFlags.NoWrap));

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        /// <param name="s">The string to add.</param>
        /// <param name="family">The font family.</param>
        /// <param name="style">The font style.</param>
        /// <param name="emSize">The em size of the font.</param>
        /// <param name="layoutRect">The layout rectangle.</param>
        /// <param name="format">The string format.</param>
        public void AddString(string s, FontFamily family, int style, float emSize,
                              RectangleF layoutRect, StringFormat? format)
        {
            if (string.IsNullOrEmpty(s)) return;
            if (family == null) throw new ArgumentNullException(nameof(family));

            var drawingStyle = (Enums.FontStyle)style;
            using var font   = new Text.Font(family, emSize, drawingStyle, GraphicsUnit.Point);

            // Standard screen DPI for point-sized fonts.
            const float dpi = 96f;
            var lines = TextLayoutEngine.Layout(s, font, layoutRect, format, dpi, dpi);

            IFont? fontFace = family.GetFontFace(drawingStyle);
            if (fontFace == null) return;   // No real font — cannot produce glyph outlines.

            var metrics   = family.GetMetricsData(drawingStyle);
            float emPixels = emSize * dpi / 72f;          // Points → pixels at 96 DPI
            float emScale  = metrics.UnitsPerEm > 0
                ? (float)(emPixels / metrics.UnitsPerEm)
                : 1f;

            var renderer   = new GlyphOutlineRenderer(fontFace);
            var rasterizer = new GlyphRasterizer();

            float firstTabOffset;
            float[] tabStops = (format ?? StringFormat.GenericDefault).GetTabStops(out firstTabOffset);

            foreach (var line in lines)
            {
                foreach (var run in line.Runs)
                {
                    float curX = run.X;
                    foreach (char c in run.Text)
                    {
                        float adv = TextLayoutEngine.GetCharAdvance(
                            c, curX - run.X,
                            fontFace, emScale,
                            emPixels * TextLayoutEngine.DefaultCharWidthFactor,
                            firstTabOffset, tabStops, emPixels);

                        if (c != '\t')
                        {
                            var gid = fontFace.Encoding.DecodeToGid(c);
                            if (gid != null)
                            {
                                using var glyphPath = rasterizer.RenderGlyph(
                                    renderer, gid, emScale, curX, run.Baseline);
                                if (glyphPath.PointCount > 0)
                                    AddPath(glyphPath, false);
                            }
                        }

                        curX += adv;
                    }
                }
            }
        }

        // -----------------------------------------------------------------------
        // AddRichText — rich text (mixed fonts/styles/sizes/brushes)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds rich text to this path, allowing mixed fonts, styles, sizes, and brushes.
        /// </summary>
        public void AddRichText(string s, FontFamily family, int style, float emSize,
                                RichTextOptions? options, PointF origin, StringFormat? format)
            => AddRichText(s, family, style, emSize, options,
                new RectangleF(origin.X, origin.Y, 1e7f, 1e7f),
                format ?? new StringFormat(StringFormatFlags.NoWrap));

        /// <summary>
        /// Adds rich text to this path within the specified layout rectangle.
        /// </summary>
        public void AddRichText(string s, FontFamily family, int style, float emSize,
                                RichTextOptions? options, RectangleF layoutRect,
                                StringFormat? format)
        {
            if (string.IsNullOrEmpty(s)) return;
            if (family == null) throw new ArgumentNullException(nameof(family));

            var drawingStyle = (Enums.FontStyle)style;
            var sf = format ?? StringFormat.GenericDefault;
            const float dpi = 96f;

            using var defaultFont = new Text.Font(family, emSize, drawingStyle, GraphicsUnit.Point);
            var chain = options?.Fallback;
            var runs = RunSegmenter.Segment(s, defaultFont, options, sf, chain);
            var lines = TextLayoutEngine.Layout(runs, layoutRect, sf, dpi, dpi);
            if (lines.Count == 0) return;

            float firstTabOffset;
            float[] tabStops = sf.GetTabStops(out firstTabOffset);

            var rendererCache = new Dictionary<IFont, GlyphOutlineRenderer>();
            var rasterizer = new GlyphRasterizer();

            foreach (var line in lines)
            {
                foreach (var run in line.Runs)
                {
                    var fontFace = run.Font.FontFamily.GetFontFace(run.Font.Style);
                    if (fontFace == null) continue;

                    if (!rendererCache.TryGetValue(fontFace, out var renderer))
                    {
                        renderer = new GlyphOutlineRenderer(fontFace);
                        rendererCache[fontFace] = renderer;
                    }

                    var metrics = run.Font.FontFamily.GetMetricsData(run.Font.Style);
                    float emPixels = TextLayoutEngine.FontSizeToPixels(
                        run.Font.Size, run.Font.Unit, dpi);
                    float emScale = metrics.UnitsPerEm > 0
                        ? (float)(emPixels / metrics.UnitsPerEm) : 1f;
                    float fallbackW = emPixels * TextLayoutEngine.DefaultCharWidthFactor;

                    float curX = run.X;
                    foreach (char c in run.Text)
                    {
                        if (c == '\t' || c == '\r' || c == '\n') continue;

                        char renderChar = c;
                        char mirrored = c;
                        if (run.IsRTL && BidiMirroring.TryGetMirror(c, out mirrored))
                            renderChar = mirrored;

                        var gid = fontFace.Encoding.DecodeToGid(renderChar);
                        if (gid != null)
                        {
                            using var glyphPath = rasterizer.RenderGlyph(
                                renderer, gid, emScale, curX, run.Baseline);
                            if (glyphPath.PointCount > 0)
                                AddPath(glyphPath, false);
                        }

                        curX += TextLayoutEngine.GetCharAdvance(c, curX - run.X,
                            fontFace, emScale, fallbackW,
                            firstTabOffset, tabStops, emPixels);
                    }
                }
            }
        }

        // -----------------------------------------------------------------------
        // AddStringOnPath — text-on-path (Batch 21)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Adds a text string laid out along the specified baseline path.
        /// Each glyph is positioned on the path and rotated to follow the
        /// path tangent.
        /// </summary>
        /// <param name="s">The string to add.</param>
        /// <param name="family">The font family.</param>
        /// <param name="style">The font style.</param>
        /// <param name="emSize">The em size of the font in points.</param>
        /// <param name="baselinePath">The baseline path along which to lay out the text, in pixel coordinates.</param>
        public void AddStringOnPath(string s, FontFamily family, int style, float emSize,
                                    GraphicsPath baselinePath)
            => AddStringOnPath(s, family, style, emSize, baselinePath, null, null);

        /// <summary>
        /// Adds a text string laid out along the specified baseline path.
        /// </summary>
        /// <param name="s">The string to add.</param>
        /// <param name="family">The font family.</param>
        /// <param name="style">The font style.</param>
        /// <param name="emSize">The em size of the font in points.</param>
        /// <param name="baselinePath">The baseline path along which to lay out the text, in pixel coordinates.</param>
        /// <param name="format">The string format (only <see cref="StringFormat.Alignment"/> is used), or null for default.</param>
        public void AddStringOnPath(string s, FontFamily family, int style, float emSize,
                                    GraphicsPath baselinePath, StringFormat? format)
            => AddStringOnPath(s, family, style, emSize, baselinePath, format, null);

        /// <summary>
        /// Adds a text string laid out along the specified baseline path.
        /// </summary>
        /// <param name="s">The string to add.</param>
        /// <param name="family">The font family.</param>
        /// <param name="style">The font style.</param>
        /// <param name="emSize">The em size of the font in points.</param>
        /// <param name="baselinePath">The baseline path along which to lay out the text, in pixel coordinates.</param>
        /// <param name="format">The string format (only <see cref="StringFormat.Alignment"/> is used), or null for default.</param>
        /// <param name="options">Placement options (start offset, perpendicular offset, overflow), or null for defaults.</param>
        public void AddStringOnPath(string s, FontFamily family, int style, float emSize,
                                    GraphicsPath baselinePath,
                                    StringFormat? format, TextOnPathOptions? options)
        {
            if (family == null) throw new ArgumentNullException(nameof(family));
            if (baselinePath == null) throw new ArgumentNullException(nameof(baselinePath));
            if (string.IsNullOrEmpty(s)) return;

            var drawingStyle = (Enums.FontStyle)style;
            using var font = new Text.Font(family, emSize, drawingStyle, GraphicsUnit.Point);

            // AddString convention: 96 DPI for point-sized fonts.
            const float dpi = 96f;
            GraphicsPath result = TextOnPathLayout.BuildPath(
                s, font, baselinePath, format, options, dpi, dpi);

            if (result.PointCount > 0)
                AddPath(result, false);
        }

        // -----------------------------------------------------------------------
        // Transform
        // -----------------------------------------------------------------------

        /// <summary>
        /// Applies a transformation matrix to this path.
        /// </summary>
        /// <param name="matrix">The transformation matrix.</param>
        public void Transform(Matrix matrix)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            if (_points.Count == 0) return;
            var pts = _points.ToArray();
            matrix.TransformPoints(pts);
            _points.Clear();
            _points.AddRange(pts);
        }

        // -----------------------------------------------------------------------
        // Flatten  (de Casteljau subdivision until chord error < flatness)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Flattens this path using the default flatness.
        /// </summary>
        public void Flatten() => Flatten(null, DefaultFlatness);

        /// <summary>
        /// Flattens this path using the specified transformation matrix and default flatness.
        /// </summary>
        /// <param name="matrix">The transformation matrix to apply before flattening.</param>
        public void Flatten(Matrix? matrix) => Flatten(matrix, DefaultFlatness);

        /// <summary>
        /// Flattens this path using the specified transformation matrix and flatness.
        /// </summary>
        /// <param name="matrix">The transformation matrix to apply before flattening.</param>
        /// <param name="flatness">The maximum allowable error between the curve and the flattened line.</param>
        public void Flatten(Matrix? matrix, float flatness)
        {
            if (matrix != null) Transform(matrix);

            var newPts   = new List<PointF>(_points.Count);
            var newTypes = new List<byte>(_points.Count);
            int i = 0;

            while (i < _points.Count)
            {
                byte rawType = (byte)(_types[i] & (byte)PathPointType.PathTypeMask);

                if (rawType == (byte)PathPointType.Bezier
                    && newPts.Count > 0 && i + 2 < _points.Count)
                {
                    PointF p0      = newPts[newPts.Count - 1];
                    PointF c1      = _points[i];
                    PointF c2      = _points[i + 1];
                    PointF p1      = _points[i + 2];
                    bool   closeIt = (_types[i + 2] & (byte)PathPointType.CloseSubpath) != 0;

                    FlattenBezier(p0, c1, c2, p1, newPts, newTypes, flatness, closeIt);
                    i += 3;
                }
                else
                {
                    // Preserve point as-is; Bezier that isn't in a valid triplet → Line
                    byte flat = rawType == (byte)PathPointType.Bezier
                        ? (byte)PathPointType.Line
                        : rawType;
                    if ((_types[i] & (byte)PathPointType.CloseSubpath) != 0)
                        flat |= (byte)PathPointType.CloseSubpath;
                    newPts.Add(_points[i]);
                    newTypes.Add(flat);
                    i++;
                }
            }

            _points.Clear();  _points.AddRange(newPts);
            _types.Clear();   _types.AddRange(newTypes);
        }

        private static void FlattenBezier(PointF p0, PointF c1, PointF c2, PointF p1,
                                          List<PointF> pts, List<byte> types,
                                          float flatness, bool close)
        {
            float dx    = p1.X - p0.X, dy = p1.Y - p0.Y;
            float lenSq = dx * dx + dy * dy;
            float ftolSq = flatness * flatness * lenSq;

            // Perpendicular cross-product distances (squared)
            float d1sq = CrossSq(c1.X - p0.X, c1.Y - p0.Y, dx, dy);
            float d2sq = CrossSq(c2.X - p0.X, c2.Y - p0.Y, dx, dy);

            if (d1sq <= ftolSq && d2sq <= ftolSq)
            {
                byte t = (byte)PathPointType.Line;
                if (close) t |= (byte)PathPointType.CloseSubpath;
                pts.Add(p1);
                types.Add(t);
            }
            else
            {
                PointF m01   = Mid(p0, c1);
                PointF m12   = Mid(c1, c2);
                PointF m23   = Mid(c2, p1);
                PointF m012  = Mid(m01, m12);
                PointF m123  = Mid(m12, m23);
                PointF m0123 = Mid(m012, m123);

                FlattenBezier(p0,    m01,  m012,  m0123, pts, types, flatness, false);
                FlattenBezier(m0123, m123, m23,   p1,    pts, types, flatness, close);
            }
        }

        private static float CrossSq(float ax, float ay, float bx, float by)
        { float c = ax * by - ay * bx; return c * c; }

        private static PointF Mid(PointF a, PointF b)
            => new PointF((a.X + b.X) * 0.5f, (a.Y + b.Y) * 0.5f);

        // -----------------------------------------------------------------------
        // GetBounds
        // -----------------------------------------------------------------------

        /// <summary>
        /// Gets the bounding rectangle of this path.
        /// </summary>
        /// <returns>The bounding rectangle.</returns>
        public RectangleF GetBounds() => GetBounds(null, null);

        /// <summary>
        /// Gets the bounding rectangle of this path after applying the specified transformation.
        /// </summary>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The bounding rectangle.</returns>
        public RectangleF GetBounds(Matrix? matrix) => GetBounds(matrix, null);

        /// <summary>
        /// Gets the bounding rectangle of this path after applying the specified transformation and pen width.
        /// </summary>
        /// <param name="matrix">The transformation matrix.</param>
        /// <param name="pen">The pen used to determine the width of the bounding rectangle.</param>
        /// <returns>The bounding rectangle.</returns>
        public RectangleF GetBounds(Matrix? matrix, Pens.Pen? pen)
        {
            if (_points.Count == 0) return RectangleF.Empty;

            var pts = _points.ToArray();
            if (matrix != null) matrix.TransformPoints(pts);

            float minX = pts[0].X, minY = pts[0].Y;
            float maxX = pts[0].X, maxY = pts[0].Y;
            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].X < minX) minX = pts[i].X;
                if (pts[i].Y < minY) minY = pts[i].Y;
                if (pts[i].X > maxX) maxX = pts[i].X;
                if (pts[i].Y > maxY) maxY = pts[i].Y;
            }

            float pw = pen != null ? pen.Width * 0.5f : 0f;
            return new RectangleF(minX - pw, minY - pw,
                                  maxX - minX + pw * 2f, maxY - minY + pw * 2f);
        }

        // -----------------------------------------------------------------------
        // IsVisible / IsOutlineVisible
        // -----------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified point is contained within this path.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns><c>true</c> if the point is contained within the path; otherwise, <c>false</c>.</returns>
        public bool IsVisible(float x, float y)      => IsVisible(new PointF(x, y));

        /// <summary>
        /// Determines whether the specified point is contained within this path.
        /// </summary>
        /// <param name="pt">The point to test.</param>
        /// <returns><c>true</c> if the point is contained within the path; otherwise, <c>false</c>.</returns>
        public bool IsVisible(PointF pt)
        {
            if (_points.Count == 0) return false;
            var flat = Clone();
            flat.Flatten();
            return IsVisibleFlattened(flat, pt.X, pt.Y);
        }

        /// <summary>
        /// Determines whether the specified point is contained within the outline of this path.
        /// </summary>
        /// <param name="pt">The point to test.</param>
        /// <param name="pen">The pen used to determine the width of the outline.</param>
        /// <returns><c>true</c> if the point is contained within the outline; otherwise, <c>false</c>.</returns>
        public bool IsOutlineVisible(PointF pt, Pens.Pen pen)
        {
            if (pen == null || _points.Count == 0) return false;
            float halfW  = pen.Width * 0.5f;
            var   flat   = Clone();
            flat.Flatten();
            var   pts    = flat._points;
            var   types  = flat._types;
            int   subStart = -1;

            for (int i = 0; i < pts.Count; i++)
            {
                byte t = (byte)(types[i] & (byte)PathPointType.PathTypeMask);
                if (t == (byte)PathPointType.Start)
                {
                    subStart = i;
                }
                else if (i > 0 && SegDist(pt, pts[i - 1], pts[i]) <= halfW)
                    return true;

                if ((types[i] & (byte)PathPointType.CloseSubpath) != 0
                    && subStart >= 0 && subStart != i
                    && SegDist(pt, pts[i], pts[subStart]) <= halfW)
                    return true;
            }
            return false;
        }

        // -----------------------------------------------------------------------
        // Widen (stub — StrokeExpander in Batch 8)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Widens this path using the specified pen.
        /// </summary>
        /// <param name="pen">The pen used to widen the path.</param>
        public void Widen(Pen pen)              => Widen(pen, null, DefaultFlatness);

        /// <summary>
        /// Widens this path using the specified pen and transformation matrix.
        /// </summary>
        /// <param name="pen">The pen used to widen the path.</param>
        /// <param name="m">The transformation matrix.</param>
        public void Widen(Pen pen, Matrix? m)   => Widen(pen, m,    DefaultFlatness);

        /// <summary>
        /// Widens this path using the specified pen, transformation matrix, and flatness.
        /// </summary>
        /// <param name="pen">The pen used to widen the path.</param>
        /// <param name="m">The transformation matrix.</param>
        /// <param name="flatness">The flatness of the widened path.</param>
        public void Widen(Pen pen, Matrix? m, float flatness)
        {
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            var widened = Rendering.StrokeExpander.Expand(this, pen, m, flatness);
            _points.Clear();
            _types.Clear();
            _points.AddRange(widened.PathPoints);
            _types.AddRange(widened.PathTypes);
            _newFigure = _types.Count == 0
                      || (_types[_types.Count - 1] & (byte)PathPointType.CloseSubpath) != 0;
        }

        // -----------------------------------------------------------------------
        // Modify
        // -----------------------------------------------------------------------

        /// <summary>
        /// Clears all points and types from this path.
        /// </summary>
        public void Reset()
        {
            _points.Clear();
            _types.Clear();
            _newFigure = true;
        }

        /// <summary>
        /// Reverses the order of points in this path.
        /// </summary>
        public void Reverse()
        {
            if (_points.Count == 0) return;

            var outPts   = new List<PointF>(_points.Count);
            var outTypes = new List<byte>(_types.Count);

            // Collect subpath ranges
            var starts = new List<int>();
            var ends   = new List<int>();
            int s = -1;

            for (int i = 0; i < _types.Count; i++)
            {
                if ((_types[i] & 0x07) == (byte)PathPointType.Start)
                {
                    if (s >= 0) { starts.Add(s); ends.Add(i - 1); }
                    s = i;
                }
            }
            if (s >= 0) { starts.Add(s); ends.Add(_types.Count - 1); }

            foreach (var (from, to) in Zip(starts, ends))
            {
                bool closed = (_types[to] & (byte)PathPointType.CloseSubpath) != 0;
                for (int i = to; i >= from; i--)
                {
                    outPts.Add(_points[i]);
                    byte t = i == to      ? (byte)PathPointType.Start
                           : i == from && closed ? (byte)((byte)PathPointType.Line | (byte)PathPointType.CloseSubpath)
                           : (byte)PathPointType.Line;
                    outTypes.Add(t);
                }
            }

            _points.Clear();  _points.AddRange(outPts);
            _types.Clear();   _types.AddRange(outTypes);
        }

        // -----------------------------------------------------------------------
        // Clone / Dispose
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates an exact copy of this <see cref="GraphicsPath"/>.
        /// </summary>
        /// <returns>A new <see cref="GraphicsPath"/> that is a copy of this instance.</returns>
        public GraphicsPath Clone()
        {
            var c = new GraphicsPath(_fillMode) { _newFigure = _newFigure };
            c._points.AddRange(_points);
            c._types.AddRange(_types);
            return c;
        }

        /// <summary>
        /// Releases all resources used by this <see cref="GraphicsPath"/>.
        /// </summary>
        public void Dispose() { /* no unmanaged resources */ }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private static bool IsVisibleFlattened(GraphicsPath path, float px, float py)
        {
            var pts   = path._points;
            var types = path._types;
            if (pts.Count < 2) return false;

            int  crossings = 0, winding = 0;
            bool alternate = path._fillMode == FillMode.Alternate;
            int  subStart  = -1;

            for (int i = 0; i < pts.Count; i++)
            {
                byte t = (byte)(types[i] & (byte)PathPointType.PathTypeMask);
                if (t == (byte)PathPointType.Start)
                {
                    subStart = i;
                }
                else
                {
                    float ax = pts[i - 1].X, ay = pts[i - 1].Y;
                    float bx = pts[i].X,     by = pts[i].Y;
                    if (alternate) crossings += RayCross(px, py, ax, ay, bx, by);
                    else           winding   += WindingAdd(px, py, ax, ay, bx, by);
                }

                if ((types[i] & (byte)PathPointType.CloseSubpath) != 0
                    && subStart >= 0 && subStart != i)
                {
                    float ax = pts[i].X,          ay = pts[i].Y;
                    float bx = pts[subStart].X,   by = pts[subStart].Y;
                    if (alternate) crossings += RayCross(px, py, ax, ay, bx, by);
                    else           winding   += WindingAdd(px, py, ax, ay, bx, by);
                }
            }

            return alternate ? (crossings & 1) != 0 : winding != 0;
        }

        private static int RayCross(float px, float py,
                                    float ax, float ay, float bx, float by)
        {
            if ((ay <= py && by > py) || (by <= py && ay > py))
                if (px < ax + (py - ay) / (by - ay) * (bx - ax))
                    return 1;
            return 0;
        }

        private static int WindingAdd(float px, float py,
                                      float ax, float ay, float bx, float by)
        {
            if (ay <= py)
            {
                if (by > py && ax * by - ay * bx - px * (by - ay) + py * (bx - ax) > 0)
                    return 1;
            }
            else if (by <= py && ax * by - ay * bx - px * (by - ay) + py * (bx - ax) < 0)
                return -1;
            return 0;
        }

        private static float SegDist(PointF p, PointF a, PointF b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float lenSq = dx * dx + dy * dy;
            if (lenSq == 0f) return PtDist(p, a);
            float t = Math.Max(0f, Math.Min(1f,
                ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq));
            return PtDist(p, new PointF(a.X + t * dx, a.Y + t * dy));
        }

        private static float PtDist(PointF a, PointF b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private static IEnumerable<(int, int)> Zip(List<int> a, List<int> b)
        {
            for (int i = 0; i < a.Count; i++) yield return (a[i], b[i]);
        }
    }
}
