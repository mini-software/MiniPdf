using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Font.Core;
using MiniPdf.Drawing.Font.Rendering;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;
using MiniPdf.Drawing.Pens;
using MiniPdf.Drawing.Rendering;
using MiniPdf.Drawing.Text;
using MiniPdf.Drawing.Text.Bidi;

namespace MiniPdf.Drawing
{
    /// <summary>
    /// Encapsulates a GDI+ drawing surface and provides methods for rendering shapes, text, and images.
    /// </summary>
    /// <remarks>
    /// A <see cref="Graphics"/> object is associated with a specific device context and is used to
    /// perform drawing operations. It is typically obtained from a <see cref="Bitmap"/> via <see cref="FromImage"/>.
    /// </remarks>
    public class Graphics : IDrawingContext
    {
        private readonly Bitmap? _target;
        private Matrix _transform;
        private Region _clip;
        private Point _renderingOrigin;
        private bool _disposed;

        // ── Rendering-hint backing fields ──
        private SmoothingMode      _smoothingMode;
        private InterpolationMode  _interpolationMode;
        private CompositingMode    _compositingMode;
        private CompositingQuality _compositingQuality;
        private PixelOffsetMode    _pixelOffsetMode;
        private TextRenderingHint  _textRenderingHint;
        private GraphicsUnit       _pageUnit;
        private float              _pageScale;

        private readonly List<GraphicsState> _stateStack = new List<GraphicsState>();
        private readonly List<GraphicsContainer> _containerStack = new List<GraphicsContainer>();

        /// <summary>
        /// Gets a value indicating whether this graphics surface is a recording
        /// (non-raster) surface. When true, <see cref="_target"/> is null and
        /// raster core methods are never called.
        /// </summary>
        protected virtual bool IsRecording => false;

        /// <summary>
        /// Initializes a new <see cref="Graphics"/> instance associated with the specified bitmap.
        /// </summary>
        /// <param name="target">The target bitmap for drawing operations.</param>
        protected Graphics(Bitmap target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _transform = new Matrix();
            _clip = new Region(new RectangleF(0f, 0f, target.Width, target.Height));

            _pageUnit = GraphicsUnit.Pixel;
            _pageScale = 1f;
            _smoothingMode = SmoothingMode.None;
            _interpolationMode = InterpolationMode.NearestNeighbor;
            _compositingMode = CompositingMode.SourceOver;
            _compositingQuality = CompositingQuality.Default;
            _pixelOffsetMode = PixelOffsetMode.Default;
            _textRenderingHint = TextRenderingHint.SystemDefault;
        }

        /// <summary>
        /// Sentinel constructor for recording surfaces that have no bitmap target.
        /// Initializes transform and clip to safe defaults without allocating a bitmap.
        /// </summary>
        internal Graphics()
        {
            _target = null;
            _transform = new Matrix();
            _clip = new Region();
            _clip.MakeInfinite();

            _pageUnit = GraphicsUnit.Pixel;
            _pageScale = 1f;
            _smoothingMode = SmoothingMode.None;
            _interpolationMode = InterpolationMode.NearestNeighbor;
            _compositingMode = CompositingMode.SourceOver;
            _compositingQuality = CompositingQuality.Default;
            _pixelOffsetMode = PixelOffsetMode.Default;
            _textRenderingHint = TextRenderingHint.SystemDefault;
        }

        /// <summary>
        /// Creates a <see cref="Graphics"/> object from the specified image.
        /// </summary>
        /// <param name="image">The image to create the graphics context from.</param>
        /// <returns>A new <see cref="Graphics"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when the image is not a <see cref="Bitmap"/>.</exception>
        public static Graphics FromImage(Image image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (!(image is Bitmap bmp))
                throw new NotSupportedException("Only Bitmap-backed images are currently supported.");
            return new Graphics(bmp);
        }

        /// <summary>
        /// Creates a <see cref="Graphics"/> object from a device context handle.
        /// </summary>
        /// <param name="hdc">The device context handle.</param>
        /// <returns>A new <see cref="Graphics"/> instance.</returns>
        /// <exception cref="PlatformNotSupportedException">Always thrown as HDC-backed Graphics is not supported.</exception>
        public static Graphics FromHdc(IntPtr hdc)
            => throw new PlatformNotSupportedException("HDC-backed Graphics is not supported.");

        /// <summary>
        /// Creates a <see cref="Graphics"/> object from a window handle.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <returns>A new <see cref="Graphics"/> instance.</returns>
        /// <exception cref="PlatformNotSupportedException">Always thrown as HWND-backed Graphics is not supported.</exception>
        public static Graphics FromHwnd(IntPtr hwnd)
            => throw new PlatformNotSupportedException("HWND-backed Graphics is not supported.");

        /// <summary>
        /// Gets or sets the world transformation matrix for this <see cref="Graphics"/>.
        /// </summary>
        public Matrix Transform
        {
            get { ThrowIfDisposed(); return _transform.Clone(); }
            set
            {
                ThrowIfDisposed();
                if (value == null) throw new ArgumentNullException(nameof(value));
                _transform = value.Clone();
                OnTransformChanged();
            }
        }

        /// <summary>
        /// Gets or sets the unit of measure for page coordinates.
        /// </summary>
        public GraphicsUnit PageUnit
        {
            get => _pageUnit;
            set { _pageUnit = value; OnHintChanged(value); }
        }

        /// <summary>
        /// Gets or sets the scaling factor for page coordinates.
        /// </summary>
        public float PageScale
        {
            get => _pageScale;
            set { _pageScale = value; OnHintChanged(value); }
        }

        public Text.FontFallbackChain? FontFallback { get; set; }

        /// <summary>
        /// Gets or sets the smoothing mode for rendering lines and curves.
        /// </summary>
        public SmoothingMode SmoothingMode
        {
            get => _smoothingMode;
            set { _smoothingMode = value; OnHintChanged(value); }
        }

        /// <summary>
        /// Gets or sets the interpolation mode for scaling and rotating images.
        /// </summary>
        public InterpolationMode InterpolationMode
        {
            get => _interpolationMode;
            set { _interpolationMode = value; OnHintChanged(value); }
        }

        /// <summary>
        /// Gets or sets the compositing mode for rendering.
        /// </summary>
        public CompositingMode CompositingMode
        {
            get => _compositingMode;
            set { _compositingMode = value; OnHintChanged(value); }
        }

        /// <summary>
        /// Gets or sets the compositing quality for rendering.
        /// </summary>
        public CompositingQuality CompositingQuality
        {
            get => _compositingQuality;
            set { _compositingQuality = value; OnHintChanged(value); }
        }

        /// <summary>
        /// Gets or sets the pixel offset mode for rendering.
        /// </summary>
        public PixelOffsetMode PixelOffsetMode
        {
            get => _pixelOffsetMode;
            set { _pixelOffsetMode = value; OnHintChanged(value); }
        }

        /// <summary>
        /// Gets or sets the text rendering hint for text quality.
        /// </summary>
        public TextRenderingHint TextRenderingHint
        {
            get => _textRenderingHint;
            set { _textRenderingHint = value; OnHintChanged(value); }
        }

        /// <summary>
        /// Gets or sets the clipping region for this <see cref="Graphics"/>.
        /// </summary>
        public Region Clip
        {
            get { ThrowIfDisposed(); return _clip.Clone(); }
            set
            {
                ThrowIfDisposed();
                if (value == null) throw new ArgumentNullException(nameof(value));
                ReplaceClip(value.Clone());
                OnClipChanged();
            }
        }

        /// <summary>
        /// Gets the bounding rectangle of the clipping region.
        /// </summary>
        public RectangleF ClipBounds
        {
            get { ThrowIfDisposed(); return _clip.GetBounds(this); }
        }

        /// <summary>
        /// Gets a value indicating whether the clipping region is empty.
        /// </summary>
        public bool IsClipEmpty
        {
            get { ThrowIfDisposed(); return _clip.IsEmpty(this); }
        }

        /// <summary>
        /// Gets the visible clipping bounds.
        /// </summary>
        public RectangleF VisibleClipBounds
        {
            get
            {
                ThrowIfDisposed();
                var b = ClipBounds;
                var s = new RectangleF(0f, 0f, Width, Height);
                return RectangleF.Intersect(b, s);
            }
        }

        /// <summary>
        /// Gets the width of the drawing surface in pixels.
        /// </summary>
        public virtual int Width => _target!.Width;

        /// <summary>
        /// Gets the height of the drawing surface in pixels.
        /// </summary>
        public virtual int Height => _target!.Height;

        /// <summary>
        /// Gets the horizontal resolution of the graphics context in pixels per inch.
        /// </summary>
        public virtual float DpiX => _target!.HorizontalResolution;

        /// <summary>
        /// Gets the vertical resolution of the graphics context in pixels per inch.
        /// </summary>
        public virtual float DpiY => _target!.VerticalResolution;

        /// <summary>
        /// Gets or sets the rendering origin point for the graphics context.
        /// </summary>
        public Point RenderingOrigin
        {
            get { ThrowIfDisposed(); return _renderingOrigin; }
            set { ThrowIfDisposed(); _renderingOrigin = value; OnHintChanged(value); }
        }

        /// <summary>
        /// Resets the world transformation matrix to the identity matrix.
        /// </summary>
        public void ResetTransform()
        {
            ThrowIfDisposed();
            _transform.Reset();
            OnTransformChanged();
        }

        /// <summary>
        /// Multiplies the world transformation matrix by the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix to multiply.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void MultiplyTransform(Matrix matrix, MatrixOrder order = MatrixOrder.Prepend)
        {
            ThrowIfDisposed();
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            _transform.Multiply(matrix, order);
            OnTransformChanged();
        }

        /// <summary>
        /// Rotates the world transformation matrix by the specified angle.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend)
        {
            ThrowIfDisposed();
            _transform.Rotate(angle, order);
            OnTransformChanged();
        }

        /// <summary>
        /// Scales the world transformation matrix by the specified factors.
        /// </summary>
        /// <param name="sx">The horizontal scale factor.</param>
        /// <param name="sy">The vertical scale factor.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)
        {
            ThrowIfDisposed();
            _transform.Scale(sx, sy, order);
            OnTransformChanged();
        }

        /// <summary>
        /// Translates the world transformation matrix by the specified offsets.
        /// </summary>
        /// <param name="dx">The horizontal translation.</param>
        /// <param name="dy">The vertical translation.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend)
        {
            ThrowIfDisposed();
            _transform.Translate(dx, dy, order);
            OnTransformChanged();
        }

        /// <summary>
        /// Draws a line connecting two points with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            using var path = new GraphicsPath();
            path.AddLine(ToPointF(x1, y1), ToPointF(x2, y2));
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a line connecting two points with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="pt1">The first point.</param>
        /// <param name="pt2">The second point.</param>
        public void DrawLine(Pen pen, PointF pt1, PointF pt2)
            => DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);

        /// <summary>
        /// Draws a line connecting two points with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
            => DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);

        /// <summary>
        /// Draws a series of line segments connecting an array of points.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="points">The array of points to connect.</param>
        public void DrawLines(Pen pen, PointF[] points)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Length < 2) return;

            using var path = new GraphicsPath();
            path.AddLines(ToPointFs(points));
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a rectangle with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="rect">The rectangle to draw.</param>
        public void DrawRectangle(Pen pen, Rectangle rect)
            => DrawRectangle(pen, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

        /// <summary>
        /// Draws a rectangle with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="x">The x-coordinate of the rectangle.</param>
        /// <param name="y">The y-coordinate of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
            => DrawRectangle(pen, new RectangleF(x, y, width, height));

        /// <summary>
        /// Draws a rectangle with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="rect">The rectangle to draw.</param>
        public void DrawRectangle(Pen pen, RectangleF rect)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            using var path = new GraphicsPath();
            path.AddRectangle(ToRectangleF(rect));
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a series of rectangles with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="rects">The array of rectangles to draw.</param>
        public void DrawRectangles(Pen pen, RectangleF[] rects)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));
            if (rects == null) throw new ArgumentNullException(nameof(rects));

            using var path = new GraphicsPath();
            for (int i = 0; i < rects.Length; i++)
                path.AddRectangle(ToRectangleF(rects[i]));
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding rectangle with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="rect">The bounding rectangle of the ellipse.</param>
        public void DrawEllipse(Pen pen, Rectangle rect)
            => DrawEllipse(pen, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

        /// <summary>
        /// Draws an ellipse defined by a bounding rectangle with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="rect">The bounding rectangle of the ellipse.</param>
        public void DrawEllipse(Pen pen, RectangleF rect)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            using var path = new GraphicsPath();
            path.AddEllipse(ToRectangleF(rect));
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws an arc defined by a bounding rectangle, start angle, and sweep angle with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="rect">The bounding rectangle of the arc.</param>
        /// <param name="startAngle">The start angle of the arc in degrees.</param>
        /// <param name="sweepAngle">The sweep angle of the arc in degrees.</param>
        public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            var r = ToRectangleF(rect);
            using var path = new GraphicsPath();
            path.AddArc(r, startAngle, sweepAngle);
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a Bézier spline defined by four points with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="pt1">The starting point.</param>
        /// <param name="pt2">The first control point.</param>
        /// <param name="pt3">The second control point.</param>
        /// <param name="pt4">The ending point.</param>
        public void DrawBezier(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            using var path = new GraphicsPath();
            path.AddBezier(ToPointF(pt1), ToPointF(pt2), ToPointF(pt3), ToPointF(pt4));
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a series of Bézier splines with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="points">The array of points defining the splines.</param>
        public void DrawBeziers(Pen pen, PointF[] points)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));
            if (points == null) throw new ArgumentNullException(nameof(points));

            using var path = new GraphicsPath();
            path.AddBeziers(ToPointFs(points));
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a curve through a series of points with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="points">The array of points to draw through.</param>
        /// <param name="tension">The tension of the curve (default 0.5).</param>
        public void DrawCurve(Pen pen, PointF[] points, float tension = 0.5f)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));
            if (points == null) throw new ArgumentNullException(nameof(points));

            using var path = new GraphicsPath();
            path.AddCurve(ToPointFs(points), tension);
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a closed curve through a series of points with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="points">The array of points to draw through.</param>
        /// <param name="tension">The tension of the curve (default 0.5).</param>
        /// <param name="fillMode">The fill mode for the curve (default Alternate).</param>
        public void DrawClosedCurve(Pen pen, PointF[] points, float tension = 0.5f, FillMode fillMode = FillMode.Alternate)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));
            if (points == null) throw new ArgumentNullException(nameof(points));

            using var path = new GraphicsPath(fillMode);
            path.AddClosedCurve(ToPointFs(points), tension);
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a polygon defined by an array of points with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="points">The array of points defining the polygon.</param>
        public void DrawPolygon(Pen pen, PointF[] points)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));
            if (points == null) throw new ArgumentNullException(nameof(points));

            using var path = new GraphicsPath();
            path.AddPolygon(ToPointFs(points));
            DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a path defined by a GraphicsPath object with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="path">The GraphicsPath to draw.</param>
        public void DrawPath(Pen pen, GraphicsPath path)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));
            if (path == null) throw new ArgumentNullException(nameof(path));
            DrawPathCore(pen, path);
        }

        /// <summary>
        /// Core implementation that strokes a graphics path. Override in a
        /// recording surface to capture the command instead of rasterizing.
        /// </summary>
        /// <param name="pen">The pen to stroke with.</param>
        /// <param name="path">The path to stroke.</param>
        protected virtual void DrawPathCore(Pen pen, GraphicsPath path)
        {
            Rasterizer.DrawPath(
                _target!,
                path,
                pen,
                _transform,
                GetClipRect(),
                SmoothingMode,
                CompositingMode,
                InterpolationMode);
        }

        /// <summary>
        /// Draws a pie shape defined by an ellipse and two radial lines with the specified pen.
        /// </summary>
        /// <param name="pen">The pen to draw with.</param>
        /// <param name="rect">The bounding rectangle of the pie.</param>
        /// <param name="startAngle">The start angle of the pie in degrees.</param>
        /// <param name="sweepAngle">The sweep angle of the pie in degrees.</param>
        public void DrawPie(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
        {
            ThrowIfDisposed();
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            var r = ToRectangleF(rect);
            float cx = r.X + r.Width * 0.5f;
            float cy = r.Y + r.Height * 0.5f;
            float a0 = startAngle * ((float)Math.PI / 180f);
            float a1 = (startAngle + sweepAngle) * ((float)Math.PI / 180f);

            var p0 = new PointF(cx + (float)Math.Cos(a0) * r.Width * 0.5f,
                                cy + (float)Math.Sin(a0) * r.Height * 0.5f);
            var p1 = new PointF(cx + (float)Math.Cos(a1) * r.Width * 0.5f,
                                cy + (float)Math.Sin(a1) * r.Height * 0.5f);

            using var path = new GraphicsPath();
            path.StartFigure();
            path.AddLine(new PointF(cx, cy), p0);
            path.AddArc(r, startAngle, sweepAngle);
            path.AddLine(p1, new PointF(cx, cy));
            path.CloseFigure();

            DrawPath(pen, path);
        }

        /// <summary>
        /// Fills the interior of a rectangle with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="rect">The rectangle to fill.</param>
        public void FillRectangle(Brush brush, Rectangle rect)
            => FillRectangle(brush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

        /// <summary>
        /// Fills the interior of a rectangle with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="x">The x-coordinate of the rectangle.</param>
        /// <param name="y">The y-coordinate of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public void FillRectangle(Brush brush, float x, float y, float width, float height)
            => FillRectangle(brush, new RectangleF(x, y, width, height));

        /// <summary>
        /// Fills the interior of a rectangle with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="rect">The rectangle to fill.</param>
        public void FillRectangle(Brush brush, RectangleF rect)
        {
            ThrowIfDisposed();
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            using var path = new GraphicsPath();
            path.AddRectangle(ToRectangleF(rect));
            FillPath(brush, path);
        }

        /// <summary>
        /// Fills the interiors of a series of rectangles with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="rects">The array of rectangles to fill.</param>
        public void FillRectangles(Brush brush, RectangleF[] rects)
        {
            ThrowIfDisposed();
            if (brush == null) throw new ArgumentNullException(nameof(brush));
            if (rects == null) throw new ArgumentNullException(nameof(rects));

            using var path = new GraphicsPath();
            for (int i = 0; i < rects.Length; i++)
                path.AddRectangle(ToRectangleF(rects[i]));
            FillPath(brush, path);
        }

        /// <summary>
        /// Fills the interior of an ellipse defined by a bounding rectangle with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="rect">The bounding rectangle of the ellipse.</param>
        public void FillEllipse(Brush brush, Rectangle rect)
            => FillEllipse(brush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

        /// <summary>
        /// Fills the interior of an ellipse defined by a bounding rectangle with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="rect">The bounding rectangle of the ellipse.</param>
        public void FillEllipse(Brush brush, RectangleF rect)
        {
            ThrowIfDisposed();
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            using var path = new GraphicsPath();
            path.AddEllipse(ToRectangleF(rect));
            FillPath(brush, path);
        }

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="points">The array of points defining the polygon.</param>
        /// <param name="fillMode">The fill mode for the polygon (default Alternate).</param>
        public void FillPolygon(Brush brush, PointF[] points, FillMode fillMode = FillMode.Alternate)
        {
            ThrowIfDisposed();
            if (brush == null) throw new ArgumentNullException(nameof(brush));
            if (points == null) throw new ArgumentNullException(nameof(points));

            using var path = new GraphicsPath(fillMode);
            path.AddPolygon(ToPointFs(points));
            FillPath(brush, path);
        }

        /// <summary>
        /// Fills the interior of a pie shape defined by an ellipse and two radial lines with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="rect">The bounding rectangle of the pie.</param>
        /// <param name="startAngle">The start angle of the pie in degrees.</param>
        /// <param name="sweepAngle">The sweep angle of the pie in degrees.</param>
        public void FillPie(Brush brush, RectangleF rect, float startAngle, float sweepAngle)
        {
            ThrowIfDisposed();
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            var r = ToRectangleF(rect);
            float cx = r.X + r.Width * 0.5f;
            float cy = r.Y + r.Height * 0.5f;
            float a0 = startAngle * ((float)Math.PI / 180f);
            float a1 = (startAngle + sweepAngle) * ((float)Math.PI / 180f);

            var p0 = new PointF(cx + (float)Math.Cos(a0) * r.Width * 0.5f,
                                cy + (float)Math.Sin(a0) * r.Height * 0.5f);
            var p1 = new PointF(cx + (float)Math.Cos(a1) * r.Width * 0.5f,
                                cy + (float)Math.Sin(a1) * r.Height * 0.5f);

            using var path = new GraphicsPath();
            path.StartFigure();
            path.AddLine(new PointF(cx, cy), p0);
            path.AddArc(r, startAngle, sweepAngle);
            path.AddLine(p1, new PointF(cx, cy));
            path.CloseFigure();

            FillPath(brush, path);
        }

        /// <summary>
        /// Fills the interior of a path defined by a GraphicsPath object with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="path">The GraphicsPath to fill.</param>
        public void FillPath(Brush brush, GraphicsPath path)
            => FillPath(brush, path, path.FillMode);

        /// <summary>
        /// Fills the interior of a path defined by a GraphicsPath object with the specified brush and fill mode.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="path">The GraphicsPath to fill.</param>
        /// <param name="fillMode">The fill mode.</param>
        public void FillPath(Brush brush, GraphicsPath path, FillMode fillMode)
        {
            ThrowIfDisposed();
            if (brush == null) throw new ArgumentNullException(nameof(brush));
            if (path == null) throw new ArgumentNullException(nameof(path));
            FillPathCore(brush, path, fillMode);
        }

        /// <summary>
        /// Core implementation that fills a graphics path. Override in a
        /// recording surface to capture the command instead of rasterizing.
        /// </summary>
        protected virtual void FillPathCore(Brush brush, GraphicsPath path, FillMode fillMode)
        {
            Rasterizer.FillPath(
                _target!,
                path,
                brush,
                _transform,
                fillMode,
                GetClipRect(),
                SmoothingMode,
                CompositingMode,
                InterpolationMode);
        }

        /// <summary>
        /// Fills the interior of a closed curve with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="points">The array of points defining the curve.</param>
        /// <param name="fillMode">The fill mode for the curve (default Alternate).</param>
        /// <param name="tension">The tension of the curve (default 0.5).</param>
        public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillMode = FillMode.Alternate, float tension = 0.5f)
        {
            ThrowIfDisposed();
            if (brush == null) throw new ArgumentNullException(nameof(brush));
            if (points == null) throw new ArgumentNullException(nameof(points));

            using var path = new GraphicsPath(fillMode);
            path.AddClosedCurve(ToPointFs(points), tension);
            FillPath(brush, path);
        }

        /// <summary>
        /// Fills the interior of a region with the specified brush.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="region">The region to fill.</param>
        public void FillRegion(Brush brush, Region region)
        {
            ThrowIfDisposed();
            if (brush == null) throw new ArgumentNullException(nameof(brush));
            if (region == null) throw new ArgumentNullException(nameof(region));

            using var identity = new Matrix();
            var scans = region.GetRegionScans(identity);
            for (int i = 0; i < scans.Length; i++)
                FillRectangle(brush, scans[i]);
        }

        /// <summary>
        /// Sets the clipping region to the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to clip to.</param>
        /// <param name="combineMode">The combine mode for the clipping operation (default Replace).</param>
        public void SetClip(Rectangle rect, CombineMode combineMode = CombineMode.Replace)
            => SetClip(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), combineMode);

        /// <summary>
        /// Sets the clipping region to the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to clip to.</param>
        /// <param name="combineMode">The combine mode for the clipping operation (default Replace).</param>
        public void SetClip(RectangleF rect, CombineMode combineMode = CombineMode.Replace)
        {
            ThrowIfDisposed();
            using var region = new Region(ToRectangleF(rect));
            ApplyClip(region, combineMode);
            OnClipChanged();
        }

        /// <summary>
        /// Sets the clipping region to the specified path.
        /// </summary>
        /// <param name="path">The path to clip to.</param>
        /// <param name="combineMode">The combine mode for the clipping operation (default Replace).</param>
        public void SetClip(GraphicsPath path, CombineMode combineMode = CombineMode.Replace)
        {
            ThrowIfDisposed();
            if (path == null) throw new ArgumentNullException(nameof(path));
            using var region = new Region(path);
            ApplyClip(region, combineMode);
            OnClipChanged();
        }

        /// <summary>
        /// Sets the clipping region to the specified region.
        /// </summary>
        /// <param name="region">The region to clip to.</param>
        /// <param name="combineMode">The combine mode for the clipping operation (default Replace).</param>
        public void SetClip(Region region, CombineMode combineMode = CombineMode.Replace)
        {
            ThrowIfDisposed();
            if (region == null) throw new ArgumentNullException(nameof(region));
            ApplyClip(region, combineMode);
            OnClipChanged();
        }

        /// <summary>
        /// Intersects the clipping region with the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to intersect with.</param>
        public void IntersectClip(Rectangle rect) => SetClip(rect, CombineMode.Intersect);

        /// <summary>
        /// Intersects the clipping region with the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to intersect with.</param>
        public void IntersectClip(RectangleF rect) => SetClip(rect, CombineMode.Intersect);

        /// <summary>
        /// Intersects the clipping region with the specified region.
        /// </summary>
        /// <param name="region">The region to intersect with.</param>
        public void IntersectClip(Region region) => SetClip(region, CombineMode.Intersect);

        /// <summary>
        /// Excludes the specified rectangle from the clipping region.
        /// </summary>
        /// <param name="rect">The rectangle to exclude.</param>
        public void ExcludeClip(Rectangle rect) => SetClip(rect, CombineMode.Exclude);

        /// <summary>
        /// Excludes the specified region from the clipping region.
        /// </summary>
        /// <param name="region">The region to exclude.</param>
        public void ExcludeClip(Region region) => SetClip(region, CombineMode.Exclude);

        /// <summary>
        /// Resets the clipping region to the entire graphics surface.
        /// </summary>
public void ResetClip()
        {
            ThrowIfDisposed();
            ReplaceClip(new Region(new RectangleF(0f, 0f, Width, Height)));
            OnClipChanged();
        }

        /// <summary>
        /// Translates the clipping region by the specified amount.
        /// </summary>
        /// <param name="dx">The horizontal translation.</param>
        /// <param name="dy">The vertical translation.</param>
        public void TranslateClip(float dx, float dy)
        {
            ThrowIfDisposed();
            _clip.Translate(ToPixels(dx, PageUnit), ToPixels(dy, PageUnit));
            OnClipChanged();
        }

        /// <summary>
        /// Saves the current graphics state and returns a GraphicsState object.
        /// </summary>
        /// <returns>The saved graphics state.</returns>
        public virtual GraphicsState Save()
        {
            ThrowIfDisposed();
            var state = CreateState();
            _stateStack.Add(state);
            return state;
        }

        /// <summary>
        /// Restores the graphics state to the specified state.
        /// </summary>
        /// <param name="graphicsState">The graphics state to restore.</param>
        public virtual void Restore(GraphicsState graphicsState)
        {
            ThrowIfDisposed();
            if (graphicsState == null) throw new ArgumentNullException(nameof(graphicsState));
            ApplyState(graphicsState);

            int idx = _stateStack.LastIndexOf(graphicsState);
            if (idx >= 0)
                _stateStack.RemoveRange(idx, _stateStack.Count - idx);
        }

        /// <summary>
        /// Begins a graphics container and saves the current state.
        /// </summary>
        /// <returns>The graphics container.</returns>
        public virtual GraphicsContainer BeginContainer()
        {
            ThrowIfDisposed();
            var state = Save();
            var container = new GraphicsContainer(state);
            _containerStack.Add(container);
            return container;
        }

        /// <summary>
        /// Begins a graphics container with the specified destination and source rectangles and unit.
        /// </summary>
        /// <param name="dstRect">The destination rectangle.</param>
        /// <param name="srcRect">The source rectangle.</param>
        /// <param name="unit">The graphics unit for the rectangles.</param>
        /// <returns>The graphics container.</returns>
        public virtual GraphicsContainer BeginContainer(RectangleF dstRect, RectangleF srcRect, GraphicsUnit unit)
        {
            ThrowIfDisposed();
            var container = BeginContainer();

            float sx = srcRect.Width == 0f ? 1f : dstRect.Width / srcRect.Width;
            float sy = srcRect.Height == 0f ? 1f : dstRect.Height / srcRect.Height;

            var m = new Matrix();
            m.Translate(dstRect.X, dstRect.Y, MatrixOrder.Append);
            m.Scale(sx, sy, MatrixOrder.Append);
            m.Translate(-srcRect.X, -srcRect.Y, MatrixOrder.Append);
            MultiplyTransform(m, MatrixOrder.Append);

            PageUnit = unit;
            return container;
        }

        /// <summary>
        /// Ends a graphics container and restores the previous state.
        /// </summary>
        /// <param name="container">The graphics container to end.</param>
        public virtual void EndContainer(GraphicsContainer container)
        {
            ThrowIfDisposed();
            if (container == null) throw new ArgumentNullException(nameof(container));
            Restore(container.State);
            _containerStack.Remove(container);
        }

        /// <summary>
        /// Clears the graphics surface with the specified color.
        /// </summary>
        /// <param name="color">The color to clear with.</param>
        public void Clear(Color color)
        {
            ThrowIfDisposed();
            ClearCore(color);
        }

        /// <summary>
        /// Core implementation that clears the surface. Override in a recording
        /// surface to capture the command instead of rasterizing.
        /// </summary>
        protected virtual void ClearCore(Color color)
        {
            int bpp = Bitmap.GetBytesPerPixel(_target!._format);
            if (bpp < 3)
            {
                for (int y = 0; y < _target!._height; y++)
                for (int x = 0; x < _target!._width; x++)
                    _target!.SetPixel(x, y, color);
                return;
            }

            for (int y = 0; y < _target!._height; y++)
            {
                int row = y * _target!._stride;
                for (int x = 0; x < _target!._width; x++)
                {
                    int o = row + x * bpp;
                    _target!._pixels[o] = color.B;
                    _target!._pixels[o + 1] = color.G;
                    _target!._pixels[o + 2] = color.R;
                    if (bpp >= 4)
                        _target!._pixels[o + 3] = color.A;
                }
            }
        }

        /// <summary>
        /// Flushes the graphics buffer to the output device.
        /// </summary>
        public void Flush() { ThrowIfDisposed(); }

        /// <summary>
        /// Flushes the graphics buffer to the output device with the specified intention.
        /// </summary>
        /// <param name="intention">The flush intention.</param>
        public void Flush(FlushIntention intention) { ThrowIfDisposed(); }

        /// <summary>
        /// Copies a block of pixels from the screen to the graphics surface.
        /// </summary>
        /// <param name="sourceX">The x-coordinate of the source location.</param>
        /// <param name="sourceY">The y-coordinate of the source location.</param>
        /// <param name="destinationX">The x-coordinate of the destination location.</param>
        /// <param name="destinationY">The y-coordinate of the destination location.</param>
        /// <param name="blockRegionSize">The size of the block to copy.</param>
        /// <exception cref="PlatformNotSupportedException">Always thrown as screen capture is not supported.</exception>
        public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize)
            => throw new PlatformNotSupportedException("Screen capture is not supported in this implementation.");

        // ── DrawImage overloads ──────────────────────────────────────────────────

        /// <summary>
        /// Draws the specified image at the specified point.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="point">The point where the image will be drawn.</param>
        public void DrawImage(Image image, Point point)
            => DrawImage(image, new Rectangle(point.X, point.Y, image?.Width ?? 0, image?.Height ?? 0));

        /// <summary>
        /// Draws the specified image at the specified point.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="point">The point where the image will be drawn.</param>
        public void DrawImage(Image image, PointF point)
            => DrawImage(image, new RectangleF(point.X, point.Y, image?.Width ?? 0, image?.Height ?? 0));

        /// <summary>
        /// Draws the specified image at the specified coordinates.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="x">The x-coordinate of the location.</param>
        /// <param name="y">The y-coordinate of the location.</param>
        public void DrawImage(Image image, int x, int y)
            => DrawImage(image, new Rectangle(x, y, image?.Width ?? 0, image?.Height ?? 0));

        /// <summary>
        /// Draws the specified image at the specified coordinates.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="x">The x-coordinate of the location.</param>
        /// <param name="y">The y-coordinate of the location.</param>
        public void DrawImage(Image image, float x, float y)
            => DrawImage(image, new RectangleF(x, y, image?.Width ?? 0, image?.Height ?? 0));

        /// <summary>
        /// Draws the specified image scaled to fit the specified rectangle.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        public void DrawImage(Image image, Rectangle destRect)
            => DrawImage(image, (RectangleF)destRect,
                         new RectangleF(0, 0, image?.Width ?? 0, image?.Height ?? 0),
                         GraphicsUnit.Pixel, null);

        /// <summary>
        /// Draws the specified image scaled to fit the specified rectangle.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        public void DrawImage(Image image, RectangleF destRect)
            => DrawImage(image, destRect,
                         new RectangleF(0, 0, image?.Width ?? 0, image?.Height ?? 0),
                         GraphicsUnit.Pixel, null);

        /// <summary>
        /// Draws the specified image transformed to fit the specified array of points.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destPoints">An array of points that define a parallelogram.</param>
        public void DrawImage(Image image, Point[] destPoints)
        {
            if (destPoints == null) throw new ArgumentNullException(nameof(destPoints));
            var pts = new PointF[destPoints.Length];
            for (int i = 0; i < destPoints.Length; i++)
                pts[i] = new PointF(destPoints[i].X, destPoints[i].Y);
            DrawImage(image, pts);
        }

        /// <summary>
        /// Draws the specified image transformed to fit the specified array of points.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destPoints">An array of points that define a parallelogram.</param>
        public void DrawImage(Image image, PointF[] destPoints)
            => DrawImage(image, destPoints,
                         new RectangleF(0, 0, image?.Width ?? 0, image?.Height ?? 0),
                         GraphicsUnit.Pixel, null);

        /// <summary>
        /// Draws the specified portion of the image scaled to fit the specified destination rectangle.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        /// <param name="srcRect">The portion of the image to draw.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
            => DrawImage(image, (RectangleF)destRect,
                         ToSrcPixelRect(new RectangleF(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height), srcUnit, image),
                         GraphicsUnit.Pixel, null);

        /// <summary>
        /// Draws the specified portion of the image scaled to fit the specified destination rectangle.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        /// <param name="srcRect">The portion of the image to draw.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
            => DrawImage(image, destRect, ToSrcPixelRect(srcRect, srcUnit, image), GraphicsUnit.Pixel, null);

        /// <summary>
        /// Draws the specified portion of the image scaled to fit the specified destination rectangle with the specified attributes.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        /// <param name="srcRect">The portion of the image to draw.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        /// <param name="imageAttr">The image attributes to use for drawing.</param>
        public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes? imageAttr)
            => DrawImage(image, (RectangleF)destRect,
                         ToSrcPixelRect(new RectangleF(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height), srcUnit, image),
                         GraphicsUnit.Pixel, imageAttr);

        /// <summary>
        /// Draws the specified portion of the image scaled to fit the specified destination rectangle with the specified attributes.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        /// <param name="srcRect">The portion of the image to draw.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        /// <param name="imageAttr">The image attributes to use for drawing.</param>
        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes? imageAttr)
        {
            ThrowIfDisposed();
            if (image == null) throw new ArgumentNullException(nameof(image));
            DrawImageCore(image, destRect, srcRect, srcUnit, imageAttr);
        }

        /// <summary>
        /// Core implementation that draws a scaled image. Override in a recording
        /// surface to capture the command instead of rasterizing.
        /// </summary>
        protected virtual void DrawImageCore(Image image, RectangleF destRect, RectangleF srcRect,
                                             GraphicsUnit srcUnit, ImageAttributes? imageAttr)
        {
            bool disposeSrc;
            Bitmap src = GetBitmapSource(image, out disposeSrc);
            try
            {
                var d  = ToRectangleF(destRect);
                var sr = ToSrcPixelRect(srcRect, srcUnit, image);
                ImageRenderer.Render(_target!, src, d, sr, GetClipRect(), InterpolationMode, CompositingMode, imageAttr);
            }
            finally
            {
                if (disposeSrc) src.Dispose();
            }
        }

        /// <summary>
        /// Draws the specified portion of the image at the specified point.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destPoint">The point where the image will be drawn.</param>
        /// <param name="srcRect">The portion of the image to draw.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        public void DrawImage(Image image, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit)
            => DrawImage(image, destPoint, srcRect, srcUnit, null);

        /// <summary>
        /// Draws the specified portion of the image at the specified point with the specified attributes.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destPoint">The point where the image will be drawn.</param>
        /// <param name="srcRect">The portion of the image to draw.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        /// <param name="imageAttr">The image attributes to use for drawing.</param>
        public void DrawImage(Image image, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes? imageAttr)
            => DrawImage(image, new RectangleF(destPoint.X, destPoint.Y, srcRect.Width, srcRect.Height),
                         srcRect, srcUnit, imageAttr);

        /// <summary>
        /// Draws the specified portion of the image transformed to fit the specified array of points.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destPoints">An array of points that define a parallelogram.</param>
        /// <param name="srcRect">The portion of the image to draw.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
            => DrawImage(image, destPoints, srcRect, srcUnit, null);

        /// <summary>
        /// Draws the specified portion of the image transformed to fit the specified array of points with the specified attributes.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destPoints">An array of points that define a parallelogram.</param>
        /// <param name="srcRect">The portion of the image to draw.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        /// <param name="imageAttr">The image attributes to use for drawing.</param>
        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes? imageAttr)
        {
            ThrowIfDisposed();
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (destPoints == null) throw new ArgumentNullException(nameof(destPoints));
            DrawImageCore(image, destPoints, srcRect, srcUnit, imageAttr);
        }

        /// <summary>
        /// Core implementation that draws an affine-transformed image. Override in
        /// a recording surface to capture the command instead of rasterizing.
        /// </summary>
        protected virtual void DrawImageCore(Image image, PointF[] destPoints, RectangleF srcRect,
                                             GraphicsUnit srcUnit, ImageAttributes? imageAttr)
        {
            bool disposeSrc;
            Bitmap src = GetBitmapSource(image, out disposeSrc);
            try
            {
                var pts = ToPointFs(destPoints);
                var sr  = ToSrcPixelRect(srcRect, srcUnit, image);
                ImageRenderer.RenderAffine(_target!, src, pts, sr, GetClipRect(), InterpolationMode, CompositingMode, imageAttr);
            }
            finally
            {
                if (disposeSrc) src.Dispose();
            }
        }

        /// <summary>
        /// Draws the specified portion of the image scaled to fit the specified destination rectangle.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        /// <param name="srcX">The x-coordinate of the source rectangle.</param>
        /// <param name="srcY">The y-coordinate of the source rectangle.</param>
        /// <param name="srcWidth">The width of the source rectangle.</param>
        /// <param name="srcHeight">The height of the source rectangle.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight,
            GraphicsUnit srcUnit)
            => DrawImage(image, (RectangleF)destRect,
                ToSrcPixelRect(new RectangleF(srcX, srcY, srcWidth, srcHeight), srcUnit, image),
                GraphicsUnit.Pixel, null);

        /// <summary>
        /// Draws the specified portion of the image scaled to fit the specified destination rectangle with the specified attributes.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        /// <param name="srcX">The x-coordinate of the source rectangle.</param>
        /// <param name="srcY">The y-coordinate of the source rectangle.</param>
        /// <param name="srcWidth">The width of the source rectangle.</param>
        /// <param name="srcHeight">The height of the source rectangle.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        /// <param name="imageAttr">The image attributes to use for drawing.</param>
        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight,
            GraphicsUnit srcUnit, ImageAttributes? imageAttr)
            => DrawImage(image, (RectangleF)destRect,
                ToSrcPixelRect(new RectangleF(srcX, srcY, srcWidth, srcHeight), srcUnit, image),
                GraphicsUnit.Pixel, imageAttr);

        /// <summary>
        /// Draws the specified portion of the image scaled to fit the specified destination rectangle.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        /// <param name="srcX">The x-coordinate of the source rectangle.</param>
        /// <param name="srcY">The y-coordinate of the source rectangle.</param>
        /// <param name="srcWidth">The width of the source rectangle.</param>
        /// <param name="srcHeight">The height of the source rectangle.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight,
            GraphicsUnit srcUnit)
            => DrawImage(image, (RectangleF)destRect,
                ToSrcPixelRect(new RectangleF(srcX, srcY, srcWidth, srcHeight), srcUnit, image),
                GraphicsUnit.Pixel, null);

        /// <summary>
        /// Draws the specified portion of the image scaled to fit the specified destination rectangle with the specified attributes.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="destRect">The rectangle that specifies the location and size of the drawn image.</param>
        /// <param name="srcX">The x-coordinate of the source rectangle.</param>
        /// <param name="srcY">The y-coordinate of the source rectangle.</param>
        /// <param name="srcWidth">The width of the source rectangle.</param>
        /// <param name="srcHeight">The height of the source rectangle.</param>
        /// <param name="srcUnit">The unit of measure for the source rectangle.</param>
        /// <param name="imageAttr">The image attributes to use for drawing.</param>
        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight,
            GraphicsUnit srcUnit, ImageAttributes? imageAttr)
            => DrawImage(image, (RectangleF)destRect,
                ToSrcPixelRect(new RectangleF(srcX, srcY, srcWidth, srcHeight), srcUnit, image),
                GraphicsUnit.Pixel, imageAttr);

        /// <summary>
        /// Draws the specified image at the specified point without scaling.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="point">The point where the image will be drawn.</param>
        public void DrawImageUnscaled(Image image, Point point)
        {
            ThrowIfDisposed();
            if (image == null) throw new ArgumentNullException(nameof(image));
            DrawImage(image, new Rectangle(point.X, point.Y, image.Width, image.Height));
        }

        /// <summary>
        /// Draws the specified image at the specified point without scaling and clips if necessary.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="rect">The rectangle that specifies the location and size of the drawn image.</param>
        public void DrawImageUnscaledAndClipped(Image image, Rectangle rect)
        {
            ThrowIfDisposed();
            if (image == null) throw new ArgumentNullException(nameof(image));
            var dst = new Rectangle(rect.X, rect.Y,
                Math.Min(rect.Width, image.Width),
                Math.Min(rect.Height, image.Height));
            DrawImage(image, dst);
        }

        private static Bitmap GetBitmapSource(Image image, out bool disposeAfterUse)
        {
            if (image is Bitmap bitmap)
            {
                disposeAfterUse = false;
                return bitmap;
            }

            if (image is Imaging.Metafile metafile)
            {
                disposeAfterUse = true;
                return metafile.RenderToBitmap();
            }

            throw new NotSupportedException("Only Bitmap and Metafile images are currently supported.");
        }

        // ── DrawImage source-rect unit conversion ─────────────────────────────

        private static RectangleF ToSrcPixelRect(RectangleF srcRect, GraphicsUnit srcUnit, Image src)
        {
            if (srcUnit == GraphicsUnit.Pixel) return srcRect;
            float dpiX = src.HorizontalResolution;
            float dpiY = src.VerticalResolution;
            return new RectangleF(
                SrcUnitToPixel(srcRect.X,      srcUnit, dpiX),
                SrcUnitToPixel(srcRect.Y,      srcUnit, dpiY),
                SrcUnitToPixel(srcRect.Width,  srcUnit, dpiX),
                SrcUnitToPixel(srcRect.Height, srcUnit, dpiY));
        }

        private static float SrcUnitToPixel(float value, GraphicsUnit unit, float dpi)
        {
            switch (unit)
            {
                case GraphicsUnit.Pixel:      return value;
                case GraphicsUnit.Point:      return value * dpi / 72f;
                case GraphicsUnit.Inch:       return value * dpi;
                case GraphicsUnit.Document:   return value * dpi / 300f;
                case GraphicsUnit.Millimeter: return value * dpi / 25.4f;
                default:                      return value;
            }
        }

        /// <summary>
        /// Gets the nearest color to the specified color.
        /// </summary>
        /// <param name="color">The color to find the nearest match for.</param>
        /// <returns>The nearest color.</returns>
        public Color GetNearestColor(Color color)
        {
            ThrowIfDisposed();
            return color;
        }

        /// <summary>
        /// Determines whether the specified point is visible within the clipping region.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is visible; otherwise, false.</returns>
        public bool IsVisible(PointF point)
        {
            ThrowIfDisposed();
            return _clip.IsVisible(point, this);
        }

        /// <summary>
        /// Determines whether the specified rectangle is visible within the clipping region.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns>True if the rectangle is visible; otherwise, false.</returns>
        public bool IsVisible(RectangleF rect)
        {
            ThrowIfDisposed();
            return _clip.IsVisible(rect, this);
        }

        // ── Virtual hooks for recording surfaces ──────────────────────────────────

        /// <summary>
        /// Called after the world transform changes. Override in a recording
        /// surface to capture the new transform state.
        /// </summary>
        protected virtual void OnTransformChanged() { }

        /// <summary>
        /// Called after the clip region changes. Override in a recording
        /// surface to capture the new clip state.
        /// </summary>
        protected virtual void OnClipChanged() { }

        /// <summary>
        /// Called after a rendering hint, page unit, page scale, or rendering
        /// origin changes. Override in a recording surface to capture the new value.
        /// </summary>
        /// <param name="hintValue">The new hint value.</param>
        protected virtual void OnHintChanged(object hintValue) { }

        /// <summary>
        /// Releases all resources used by the Graphics object.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _clip.Dispose();
            _transform.Dispose();
        }

        private GraphicsState CreateState()
            => new GraphicsState(
                _transform.Clone(),
                _clip.Clone(),
                PageUnit,
                PageScale,
                SmoothingMode,
                InterpolationMode,
                CompositingMode,
                CompositingQuality,
                PixelOffsetMode,
                TextRenderingHint,
                _renderingOrigin);

        private void ApplyState(GraphicsState state)
        {
            ReplaceClip(state.Clip.Clone());
            _transform.Dispose();
            _transform = state.Transform.Clone();
            PageUnit = state.PageUnit;
            PageScale = state.PageScale;
            SmoothingMode = state.SmoothingMode;
            InterpolationMode = state.InterpolationMode;
            CompositingMode = state.CompositingMode;
            CompositingQuality = state.CompositingQuality;
            PixelOffsetMode = state.PixelOffsetMode;
            TextRenderingHint = state.TextRenderingHint;
            _renderingOrigin = state.RenderingOrigin;
        }

        private void ApplyClip(Region region, CombineMode mode)
        {
            switch (mode)
            {
                case CombineMode.Replace:
                    ReplaceClip(region.Clone());
                    break;
                case CombineMode.Intersect:
                    _clip.Intersect(region);
                    break;
                case CombineMode.Union:
                    _clip.Union(region);
                    break;
                case CombineMode.Xor:
                    _clip.Xor(region);
                    break;
                case CombineMode.Exclude:
                    _clip.Exclude(region);
                    break;
                case CombineMode.Complement:
                    _clip.Complement(region);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private void ReplaceClip(Region newClip)
        {
            _clip.Dispose();
            _clip = newClip;
        }

        private Rectangle? GetClipRect()
        {
            var r = VisibleClipBounds;
            if (r.IsEmpty) return Rectangle.Empty;

            int x = Clamp((int)Math.Floor(r.X), 0, Width);
            int y = Clamp((int)Math.Floor(r.Y), 0, Height);
            int right = Clamp((int)Math.Ceiling(r.Right), 0, Width);
            int bottom = Clamp((int)Math.Ceiling(r.Bottom), 0, Height);

            if (right <= x || bottom <= y)
                return Rectangle.Empty;

            return Rectangle.FromLTRB(x, y, right, bottom);
        }

        private float ToPixels(float value, GraphicsUnit unit)
        {
            float px;
            switch (unit)
            {
                case GraphicsUnit.Pixel:
                    px = value;
                    break;
                case GraphicsUnit.Point:
                    px = value * DpiX / 72f;
                    break;
                case GraphicsUnit.Inch:
                    px = value * DpiX;
                    break;
                case GraphicsUnit.Document:
                    px = value * DpiX / 300f;
                    break;
                case GraphicsUnit.Millimeter:
                    px = value * DpiX / 25.4f;
                    break;
                default:
                    px = value;
                    break;
            }

            return px * PageScale;
        }

        private PointF ToPointF(float x, float y)
            => new PointF(ToPixels(x, PageUnit), ToPixels(y, PageUnit));

        private PointF ToPointF(PointF p)
            => new PointF(ToPixels(p.X, PageUnit), ToPixels(p.Y, PageUnit));

        private PointF[] ToPointFs(PointF[] points)
        {
            var arr = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
                arr[i] = ToPointF(points[i]);
            return arr;
        }

        private RectangleF ToRectangleF(RectangleF r)
            => new RectangleF(ToPixels(r.X, PageUnit), ToPixels(r.Y, PageUnit), ToPixels(r.Width, PageUnit), ToPixels(r.Height, PageUnit));

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Graphics));
        }

        // ── DrawString overloads ─────────────────────────────────────────────

        /// <summary>
        /// Draws the specified text at the specified location with the specified brush and font.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="x">The x-coordinate of the text location.</param>
        /// <param name="y">The y-coordinate of the text location.</param>
        public void DrawString(string s,Text.Font font, Brush brush, float x, float y)
            => DrawStringCore(s, font, brush,
                new RectangleF(x, y, 1e7f, 1e7f),
                new StringFormat(StringFormatFlags.NoWrap));

        /// <summary>
        /// Draws the specified text at the specified point with the specified brush and font.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="point">The point representing the text location.</param>
        public void DrawString(string s, Text.Font font, Brush brush, PointF point)
            => DrawStringCore(s, font, brush,
                new RectangleF(point.X, point.Y, 1e7f, 1e7f),
                new StringFormat(StringFormatFlags.NoWrap));

        /// <summary>
        /// Draws the specified text in the specified layout rectangle with the specified brush and font.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="layoutRect">The layout rectangle for the text.</param>
        public void DrawString(string s, Text.Font font, Brush brush, RectangleF layoutRect)
            => DrawStringCore(s, font, brush, layoutRect, null);

        /// <summary>
        /// Draws the specified text at the specified location with the specified brush, font, and format.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="x">The x-coordinate of the text location.</param>
        /// <param name="y">The y-coordinate of the text location.</param>
        /// <param name="format">The string format, or null for default.</param>
        public void DrawString(string s, Text.Font font, Brush brush, float x, float y, StringFormat? format)
            => DrawStringCore(s, font, brush,
                new RectangleF(x, y, 1e7f, 1e7f),
                format ?? new StringFormat(StringFormatFlags.NoWrap));

        /// <summary>
        /// Draws the specified text at the specified point with the specified brush, font, and format.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="point">The point representing the text location.</param>
        /// <param name="format">The string format, or null for default.</param>
        public void DrawString(string s, Text.Font font, Brush brush, PointF point, StringFormat? format)
            => DrawStringCore(s, font, brush,
                new RectangleF(point.X, point.Y, 1e7f, 1e7f),
                format ?? new StringFormat(StringFormatFlags.NoWrap));

        /// <summary>
        /// Draws the specified text in the specified layout rectangle with the specified brush, font, and format.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="layoutRect">The layout rectangle for the text.</param>
        /// <param name="format">The string format, or null for default.</param>
        public void DrawString(string s, Text.Font font, Brush brush, RectangleF layoutRect, StringFormat? format)
            => DrawStringCore(s, font, brush, layoutRect, format);

        protected virtual void DrawStringCore(
            string        s,
            Text.Font          font,
            Brush         brush,
            RectangleF    layoutRect,
            StringFormat? format)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(s)) return;
            if (font  == null) throw new ArgumentNullException(nameof(font));
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            var pixelRect = ToRectangleF(layoutRect);
            var lines     = TextLayoutEngine.Layout(s, font, pixelRect, format, DpiX, DpiY);
            if (lines.Count == 0) return;

            var   metrics  = font.FontFamily.GetMetricsData(font.Style);
            float emPixels = TextLayoutEngine.FontSizeToPixels(font.Size, font.Unit, DpiY);
            float emScale  = metrics.UnitsPerEm > 0
                ? (float)(emPixels / metrics.UnitsPerEm)
                : 1f;

            IFont?               fontFace  = font.FontFamily.GetFontFace(font.Style);
            GlyphOutlineRenderer? renderer  = fontFace != null
                ? new GlyphOutlineRenderer(fontFace)
                : null;
            GlyphRasterizer?     rasterizer = renderer != null ? new GlyphRasterizer() : null;

            // Decoration metrics (design units → pixels)
            float ulY  = (float)(metrics.UnderlinePosition  * emScale);  // typically negative (below baseline)
            float ulH  = Math.Max(1f, (float)(metrics.UnderlineThickness * emScale));
            float soY  = (float)(metrics.StrikeoutPosition  * emScale);  // typically positive (above baseline)
            float soH  = Math.Max(1f, (float)(metrics.StrikeoutSize      * emScale));

            float firstTabOffset;
            float[] tabStops = (format ?? StringFormat.GenericDefault).GetTabStops(out firstTabOffset);

            foreach (var line in lines)
            {
                foreach (var run in line.Runs)
                {
                    if (renderer != null && rasterizer != null)
                    {
                        float curX = run.X;
                        foreach (char c in run.Text)
                        {
                            float adv = TextLayoutEngine.GetCharAdvance(
                                c, curX - run.X,
                                fontFace, emScale, emPixels * TextLayoutEngine.DefaultCharWidthFactor,
                                firstTabOffset, tabStops, emPixels);

                            if (c != '\t')
                            {
                                var gid = fontFace!.Encoding.DecodeToGid(c);
                                if (gid != null)
                                {
                                    using var glyphPath = rasterizer.RenderGlyph(
                                        renderer, gid, emScale, curX, run.Baseline);
                                    if (glyphPath.PointCount > 0)
                                        FillPath(brush, glyphPath);
                                }
                            }

                            curX += adv;
                        }
                    }

                    // Underline
                    if (font.Underline && run.Width > 0f)
                    {
                        float ry = run.Baseline - ulY;
                        using var ulPath = new GraphicsPath();
                        ulPath.AddRectangle(new RectangleF(run.X, ry, run.Width, ulH));
                        FillPath(brush, ulPath);
                    }

                    // Strikeout
                    if (font.Strikeout && run.Width > 0f)
                    {
                        float ry = run.Baseline - soY;
                        using var soPath = new GraphicsPath();
                        soPath.AddRectangle(new RectangleF(run.X, ry, run.Width, soH));
                        FillPath(brush, soPath);
                    }
                }
            }
        }

        // ── DrawStringOnPath overloads ───────────────────────────────────────

        /// <summary>
        /// Draws the specified text along the given baseline path.
        /// Each glyph is positioned on the path and rotated to follow the
        /// path tangent.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="baselinePath">The baseline path along which to lay out the text, in the current page units.</param>
        public void DrawStringOnPath(string s, Text.Font font, Brush brush,
                                     Drawing2D.GraphicsPath baselinePath)
            => DrawStringOnPath(s, font, brush, baselinePath, null, null);

        /// <summary>
        /// Draws the specified text along the given baseline path.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="baselinePath">The baseline path along which to lay out the text, in the current page units.</param>
        /// <param name="format">The string format (only <see cref="StringFormat.Alignment"/> is used), or null for default.</param>
        public void DrawStringOnPath(string s, Text.Font font, Brush brush,
                                     Drawing2D.GraphicsPath baselinePath, StringFormat? format)
            => DrawStringOnPath(s, font, brush, baselinePath, format, null);

        /// <summary>
        /// Draws the specified text along the given baseline path.
        /// </summary>
        /// <param name="s">The text to draw.</param>
        /// <param name="font">The font to draw with.</param>
        /// <param name="brush">The brush to draw the text with.</param>
        /// <param name="baselinePath">The baseline path along which to lay out the text, in the current page units.</param>
        /// <param name="format">The string format (only <see cref="StringFormat.Alignment"/> is used), or null for default.</param>
        /// <param name="options">Placement options (start offset, perpendicular offset, overflow), or null for defaults. Offsets are in the current page units.</param>
        public void DrawStringOnPath(string s, Text.Font font, Brush brush,
                                     Drawing2D.GraphicsPath baselinePath,
                                     StringFormat? format, TextOnPathOptions? options)
        {
            ThrowIfDisposed();
            if (font == null) throw new ArgumentNullException(nameof(font));
            if (brush == null) throw new ArgumentNullException(nameof(brush));
            if (baselinePath == null) throw new ArgumentNullException(nameof(baselinePath));
            if (string.IsNullOrEmpty(s)) return;

            // Convert page-unit offsets to pixels (consistent with DrawString).
            TextOnPathOptions? pixelOptions = options != null
                ? new TextOnPathOptions
                {
                    StartOffset         = ToPixels(options.StartOffset, PageUnit),
                    PerpendicularOffset = ToPixels(options.PerpendicularOffset, PageUnit),
                    Alignment           = options.Alignment,
                    Overflow            = options.Overflow,
                }
                : null;

            GraphicsPath result = TextOnPathLayout.BuildPath(
                s, font, baselinePath, format, pixelOptions, DpiX, DpiY);

            if (result.PointCount > 0)
                FillPath(brush, result);
        }

        // ── MeasureString overloads ──────────────────────────────────────────

        /// <summary>
        /// Measures the specified text when drawn with the specified font.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font to measure with.</param>
        /// <returns>The size of the text in pixels.</returns>
        public SizeF MeasureString(string text, Text.Font font)
            => MeasureStringCore(text, font,
                new RectangleF(0f, 0f, float.MaxValue / 2f, float.MaxValue / 2f),
                new StringFormat(StringFormatFlags.NoWrap));

        /// <summary>
        /// Measures the specified text when drawn with the specified font within the specified width.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font to measure with.</param>
        /// <param name="width">The maximum width in pixels.</param>
        /// <returns>The size of the text in pixels.</returns>
        public SizeF MeasureString(string text, Text.Font font, int width)
            => MeasureStringCore(text, font,
                new RectangleF(0f, 0f, width, float.MaxValue / 2f),
                null);

        /// <summary>
        /// Measures the specified text when drawn with the specified font within the specified layout area.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font to measure with.</param>
        /// <param name="layoutArea">The layout area in the current page units.</param>
        /// <returns>The size of the text in pixels.</returns>
        public SizeF MeasureString(string text, Text.Font font, SizeF layoutArea)
            => MeasureStringCore(text, font,
                new RectangleF(0f, 0f,
                    ToPixels(layoutArea.Width,  PageUnit),
                    ToPixels(layoutArea.Height, PageUnit)),
                null);

        /// <summary>
        /// Measures the specified text when drawn with the specified font within the specified layout area and format.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font to measure with.</param>
        /// <param name="layoutArea">The layout area in the current page units.</param>
        /// <param name="format">The string format, or null for default.</param>
        /// <returns>The size of the text in pixels.</returns>
        public SizeF MeasureString(string text, Text.Font font, SizeF layoutArea, StringFormat? format)
            => MeasureStringCore(text, font,
                new RectangleF(0f, 0f,
                    ToPixels(layoutArea.Width,  PageUnit),
                    ToPixels(layoutArea.Height, PageUnit)),
                format);

        /// <summary>
        /// Measures the specified text when drawn with the specified font at the specified origin and format.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font to measure with.</param>
        /// <param name="origin">The origin point in the current page units.</param>
        /// <param name="format">The string format, or null for default.</param>
        /// <returns>The size of the text in pixels.</returns>
        public SizeF MeasureString(string text, Text.Font font, PointF origin, StringFormat? format)
            => MeasureStringCore(text, font,
                new RectangleF(origin.X, origin.Y, float.MaxValue / 2f, float.MaxValue / 2f),
                format ?? new StringFormat(StringFormatFlags.NoWrap));

        /// <summary>
        /// Core implementation for measuring text.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font to measure with.</param>
        /// <param name="pixelRect">The layout rectangle in pixels.</param>
        /// <param name="format">The string format, or null for default.</param>
        /// <returns>The size of the text in pixels.</returns>
        private SizeF MeasureStringCore(
            string        text,
            Text.Font          font,
            RectangleF    pixelRect,
            StringFormat? format)
        {
            if (string.IsNullOrEmpty(text)) return SizeF.Empty;
            if (font == null) throw new ArgumentNullException(nameof(font));

            var lines = TextLayoutEngine.Layout(text, font, pixelRect, format, DpiX, DpiY);
            if (lines.Count == 0) return SizeF.Empty;

            float maxW = 0f;
            foreach (var line in lines)
                if (line.Width > maxW) maxW = line.Width;

            float totalH = lines.Count * lines[0].LineHeight;
            return new SizeF(maxW, totalH);
        }

        // ── Rich text methods ───────────────────────────────────────────────

        public void DrawRichText(string text, RichTextOptions options, Brush defaultBrush,
                                  RectangleF layoutRect)
            => DrawRichText(text, options, defaultBrush, layoutRect, null);

        public void DrawRichText(string text, RichTextOptions options, Brush defaultBrush,
                                  RectangleF layoutRect, StringFormat? format)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(text)) return;
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (defaultBrush == null) throw new ArgumentNullException(nameof(defaultBrush));

            var pixelRect = ToRectangleF(layoutRect);
            var sf = format ?? StringFormat.GenericDefault;

            using var defaultFont = new Text.Font(options.FontFamily, options.EmSize,
                                                   options.FontStyle, options.Unit);
            var chain = options.Fallback ?? FontFallback;
            var runs = RunSegmenter.Segment(text, defaultFont, options, sf, chain);
            var lines = TextLayoutEngine.Layout(runs, pixelRect, sf, DpiX, DpiY);
            if (lines.Count == 0) return;

            RenderMultiRunLines(lines, defaultBrush, sf);
        }

        public SizeF MeasureRichText(string text, RichTextOptions options, SizeF layoutArea)
            => MeasureRichText(text, options, layoutArea, null);

        public SizeF MeasureRichText(string text, RichTextOptions options, SizeF layoutArea,
                                      StringFormat? format)
        {
            if (string.IsNullOrEmpty(text)) return SizeF.Empty;
            if (options == null) throw new ArgumentNullException(nameof(options));

            var pixelRect = new RectangleF(0f, 0f,
                ToPixels(layoutArea.Width, PageUnit),
                ToPixels(layoutArea.Height, PageUnit));
            var sf = format ?? StringFormat.GenericDefault;

            using var defaultFont = new Text.Font(options.FontFamily, options.EmSize,
                                                   options.FontStyle, options.Unit);
            var chain = options.Fallback ?? FontFallback;
            var runs = RunSegmenter.Segment(text, defaultFont, options, sf, chain);
            var lines = TextLayoutEngine.Layout(runs, pixelRect, sf, DpiX, DpiY);
            if (lines.Count == 0) return SizeF.Empty;

            float maxW = 0f;
            foreach (var line in lines)
                if (line.Width > maxW) maxW = line.Width;

            float totalH = 0f;
            foreach (var line in lines)
                totalH += line.LineHeight;

            return new SizeF(maxW, totalH);
        }

        private void RenderMultiRunLines(List<TextLine> lines, Brush defaultBrush,
                                          StringFormat sf)
        {
            float firstTabOffset;
            float[] tabStops = sf.GetTabStops(out firstTabOffset);

            var rendererCache = new Dictionary<IFont,
                (GlyphOutlineRenderer renderer, GlyphRasterizer rasterizer)>();

            foreach (var line in lines)
            {
                foreach (var run in line.Runs)
                {
                    var fontFace = run.Font.FontFamily.GetFontFace(run.Font.Style);
                    if (fontFace == null) continue;

                    if (!rendererCache.TryGetValue(fontFace, out var cached))
                    {
                        cached = (new GlyphOutlineRenderer(fontFace), new GlyphRasterizer());
                        rendererCache[fontFace] = cached;
                    }
                    var (renderer, rasterizer) = cached;

                    var metrics = run.Font.FontFamily.GetMetricsData(run.Font.Style);
                    float emPixels = TextLayoutEngine.FontSizeToPixels(
                        run.Font.Size, run.Font.Unit, DpiY);
                    float emScale = metrics.UnitsPerEm > 0
                        ? (float)(emPixels / metrics.UnitsPerEm) : 1f;
                    float fallbackW = emPixels * TextLayoutEngine.DefaultCharWidthFactor;

                    var brush = run.Brush ?? defaultBrush;
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
                                FillPath(brush, glyphPath);
                        }

                        curX += TextLayoutEngine.GetCharAdvance(c, curX - run.X,
                            fontFace, emScale, fallbackW,
                            firstTabOffset, tabStops, emPixels);
                    }

                    if (run.Font.Underline && run.Width > 0f)
                    {
                        float ulY = run.Baseline - (float)(metrics.UnderlinePosition * emScale);
                        float ulH = Math.Max(1f, (float)(metrics.UnderlineThickness * emScale));
                        using var ulPath = new GraphicsPath();
                        ulPath.AddRectangle(new RectangleF(run.X, ulY, run.Width, ulH));
                        FillPath(brush, ulPath);
                    }

                    if (run.Font.Strikeout && run.Width > 0f)
                    {
                        float soY = run.Baseline - (float)(metrics.StrikeoutPosition * emScale);
                        float soH = Math.Max(1f, (float)(metrics.StrikeoutSize * emScale));
                        using var soPath = new GraphicsPath();
                        soPath.AddRectangle(new RectangleF(run.X, soY, run.Width, soH));
                        FillPath(brush, soPath);
                    }
                }
            }
        }
    }
}
