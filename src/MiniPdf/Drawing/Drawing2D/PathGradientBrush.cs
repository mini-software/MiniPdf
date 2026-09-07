using System;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Geometry;

namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Encapsulates a Brush object that creates a gradient using the center point and
    /// the vertices of the path defining the gradient shape.
    /// </summary>
    public sealed class PathGradientBrush : Brush
    {
        private PointF[]     _pathPoints;
        private PointF       _centerPoint;
        private Color        _centerColor;
        private Color[]      _surroundColors;
        private PointF       _focusScales;
        private Blend?       _blend;
        private ColorBlend?  _interpolationColors;
        private Matrix       _transform;
        private WrapMode     _wrapMode;

        // -----------------------------------------------------------------------
        // Constructors
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="PathGradientBrush"/> with the specified points.
        /// </summary>
        /// <param name="points">The array of points that define the gradient path.</param>
        public PathGradientBrush(PointF[] points)
            : this(points, WrapMode.Clamp) { }

        /// <summary>
        /// Initializes a new <see cref="PathGradientBrush"/> with the specified points and wrap mode.
        /// </summary>
        /// <param name="points">The array of points that define the gradient path.</param>
        /// <param name="wrapMode">The wrap mode that determines how the gradient is tiled.</param>
        public PathGradientBrush(PointF[] points, WrapMode wrapMode)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Length < 2) throw new ArgumentException("At least 2 points required.", nameof(points));

            _pathPoints     = (PointF[])points.Clone();
            _wrapMode       = wrapMode;
            _centerColor    = Color.White;
            _surroundColors = new[] { Color.Black };
            _focusScales    = new PointF(0f, 0f);
            _transform      = new Matrix();
            _centerPoint    = ComputeCentroid(points);
        }

        /// <summary>
        /// Initializes a new <see cref="PathGradientBrush"/> from the specified path.
        /// </summary>
        /// <param name="path">The path that defines the gradient shape.</param>
        public PathGradientBrush(GraphicsPath path)
            : this(path?.PathPoints ?? throw new ArgumentNullException(nameof(path))) { }

        /// <summary>
        /// Initializes a new <see cref="PathGradientBrush"/> from the specified path and wrap mode.
        /// </summary>
        /// <param name="path">The path that defines the gradient shape.</param>
        /// <param name="wrapMode">The wrap mode that determines how the gradient is tiled.</param>
        public PathGradientBrush(GraphicsPath path, WrapMode wrapMode)
            : this(path?.PathPoints ?? throw new ArgumentNullException(nameof(path)), wrapMode) { }

        // -----------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the center color of the gradient.
        /// </summary>
        public Color CenterColor
        {
            get => _centerColor;
            set => _centerColor = value;
        }

        /// <summary>
        /// Gets or sets the array of surround colors.
        /// </summary>
        public Color[] SurroundColors
        {
            get => (Color[])_surroundColors.Clone();
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _surroundColors = (Color[])value.Clone();
            }
        }

        /// <summary>
        /// Gets or sets the center point of the gradient.
        /// </summary>
        public PointF CenterPoint
        {
            get => _centerPoint;
            set => _centerPoint = value;
        }

        /// <summary>
        /// Gets or sets the focus scales for the gradient.
        /// </summary>
        public PointF FocusScales
        {
            get => _focusScales;
            set => _focusScales = value;
        }

        /// <summary>
        /// Gets the bounding rectangle of the gradient path.
        /// </summary>
        public RectangleF Rectangle
        {
            get
            {
                float minX = _pathPoints[0].X, maxX = _pathPoints[0].X;
                float minY = _pathPoints[0].Y, maxY = _pathPoints[0].Y;
                for (int i = 1; i < _pathPoints.Length; i++)
                {
                    if (_pathPoints[i].X < minX) minX = _pathPoints[i].X;
                    if (_pathPoints[i].X > maxX) maxX = _pathPoints[i].X;
                    if (_pathPoints[i].Y < minY) minY = _pathPoints[i].Y;
                    if (_pathPoints[i].Y > maxY) maxY = _pathPoints[i].Y;
                }
                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }
        }

        /// <summary>
        /// Gets or sets the wrap mode for filling areas outside the gradient.
        /// </summary>
        public WrapMode WrapMode
        {
            get => _wrapMode;
            set => _wrapMode = value;
        }

        /// <summary>
        /// Gets or sets the transformation matrix for the brush.
        /// </summary>
        public Matrix Transform
        {
            get => _transform;
            set => _transform = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the blend factors for custom gradient blending.
        /// </summary>
        public Blend? Blend
        {
            get => _blend;
            set => _blend = value;
        }

        /// <summary>
        /// Gets or sets the color blend for custom gradient interpolation.
        /// </summary>
        public ColorBlend? InterpolationColors
        {
            get => _interpolationColors;
            set => _interpolationColors = value;
        }

        // -----------------------------------------------------------------------
        // Transform helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Resets the transformation matrix to the identity matrix.
        /// </summary>
        public void ResetTransform()  => _transform.Reset();

        /// <summary>
        /// Multiplies the transformation matrix by the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix to multiply.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void MultiplyTransform(Matrix matrix, MatrixOrder order = MatrixOrder.Prepend)
            => _transform.Multiply(matrix, order);

        /// <summary>
        /// Rotates the transformation matrix by the specified angle.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend)
            => _transform.Rotate(angle, order);

        /// <summary>
        /// Scales the transformation matrix by the specified factors.
        /// </summary>
        /// <param name="sx">The horizontal scale factor.</param>
        /// <param name="sy">The vertical scale factor.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)
            => _transform.Scale(sx, sy, order);

        /// <summary>
        /// Translates the transformation matrix by the specified offsets.
        /// </summary>
        /// <param name="dx">The horizontal translation.</param>
        /// <param name="dy">The vertical translation.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend)
            => _transform.Translate(dx, dy, order);

        // -----------------------------------------------------------------------
        // Blend shape helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates a triangular blend that emphasizes colors at the specified focus.
        /// </summary>
        /// <param name="focus">The focus position (0.0 to 1.0).</param>
        /// <param name="scale">The scale factor for the blend.</param>
        public void SetBlendTriangularShape(float focus, float scale = 1.0f)
        {
            if (focus < 0f || focus > 1f) throw new ArgumentOutOfRangeException(nameof(focus));
            if (scale < 0f || scale > 1f) throw new ArgumentOutOfRangeException(nameof(scale));
            var b = new Blend();
            b.Positions = new[] { 0f, focus, 1f };
            b.Factors   = new[] { 0f, scale, 0f };
            _blend = b;
        }

        /// <summary>
        /// Creates a bell-shaped blend centered at the specified focus.
        /// </summary>
        /// <param name="focus">The focus position (0.0 to 1.0).</param>
        /// <param name="scale">The scale factor for the blend.</param>
        public void SetSigmaBellShape(float focus, float scale = 1.0f)
        {
            if (focus < 0f || focus > 1f) throw new ArgumentOutOfRangeException(nameof(focus));
            if (scale < 0f || scale > 1f) throw new ArgumentOutOfRangeException(nameof(scale));
            const int steps = 64;
            var pos     = new float[steps + 1];
            var factors = new float[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float t  = (float)i / steps;
                pos[i]     = t;
                double u   = (t - focus) * 6.0;
                factors[i] = (float)(scale * (1.0 / (1.0 + Math.Exp(-u))));
            }
            _blend = new Blend { Positions = pos, Factors = factors };
        }

        // -----------------------------------------------------------------------
        // Clone / Dispose
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates an exact copy of this <see cref="PathGradientBrush"/>.
        /// </summary>
        /// <returns>A new <see cref="PathGradientBrush"/> with the same properties.</returns>
        public override Brush Clone()
        {
            var c = new PathGradientBrush((PointF[])_pathPoints.Clone(), _wrapMode)
            {
                CenterColor           = _centerColor,
                CenterPoint           = _centerPoint,
                FocusScales           = _focusScales,
                _transform            = (Matrix)_transform.Clone(),
            };
            c._surroundColors = (Color[])_surroundColors.Clone();
            c._blend                  = _blend;
            c._interpolationColors    = _interpolationColors;
            return c;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PathGradientBrush"/>.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) { }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static PointF ComputeCentroid(PointF[] pts)
        {
            float cx = 0f, cy = 0f;
            foreach (var p in pts) { cx += p.X; cy += p.Y; }
            return new PointF(cx / pts.Length, cy / pts.Length);
        }
    }
}
