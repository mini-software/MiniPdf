using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Geometry;
using System;

namespace MiniSoftware.Drawing.Drawing2D
{
    /// <summary>
    /// Defines a brush that blends two colors along a linear gradient.
    /// </summary>
    /// <remarks>
    /// A <see cref="LinearGradientBrush"/> fills shapes with a gradient that transitions
    /// linearly from a start point to an end point. The gradient can be customized with
    /// <see cref="Blend"/> and <see cref="ColorBlend"/> for non-linear interpolation.
    /// </remarks>
    public sealed class LinearGradientBrush : Brush
    {
        private Color[]    _colors;        // always length 2
        private PointF     _pt1;
        private PointF     _pt2;
        private RectangleF _rectangle;
        private WrapMode   _wrapMode;
        private bool       _gammaCorrection;
        private Blend?     _blend;
        private ColorBlend? _interpolationColors;
        private Matrix     _transform;

        // -----------------------------------------------------------------------
        // Constructors
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="LinearGradientBrush"/> with the specified start and end points and colors.
        /// </summary>
        /// <param name="pt1">The starting point of the gradient.</param>
        /// <param name="pt2">The ending point of the gradient.</param>
        /// <param name="color1">The starting color.</param>
        /// <param name="color2">The ending color.</param>
        public LinearGradientBrush(PointF pt1, PointF pt2, Color color1, Color color2)
        {
            _pt1 = pt1;
            _pt2 = pt2;
            _colors    = new[] { color1, color2 };
            _rectangle = new RectangleF(
                Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y),
                Math.Abs(pt2.X - pt1.X), Math.Abs(pt2.Y - pt1.Y));
            _wrapMode  = WrapMode.Tile;
            _transform = new Matrix();
        }

        /// <summary>
        /// Initializes a new <see cref="LinearGradientBrush"/> with the specified start and end points and colors.
        /// </summary>
        /// <param name="pt1">The starting point of the gradient.</param>
        /// <param name="pt2">The ending point of the gradient.</param>
        /// <param name="color1">The starting color.</param>
        /// <param name="color2">The ending color.</param>
        public LinearGradientBrush(Point pt1, Point pt2, Color color1, Color color2)
            : this(new PointF(pt1.X, pt1.Y), new PointF(pt2.X, pt2.Y), color1, color2) { }

        /// <summary>
        /// Initializes a new <see cref="LinearGradientBrush"/> with the specified rectangle and colors, using a gradient mode.
        /// </summary>
        /// <param name="rect">The rectangle that defines the start and end points of the gradient.</param>
        /// <param name="color1">The starting color.</param>
        /// <param name="color2">The ending color.</param>
        /// <param name="mode">The linear gradient mode specifying the direction.</param>
        public LinearGradientBrush(RectangleF rect, Color color1, Color color2, LinearGradientMode mode)
        {
            _colors    = new[] { color1, color2 };
            _rectangle = rect;
            _wrapMode  = WrapMode.Tile;
            _transform = new Matrix();
            ComputePointsFromMode(rect, mode);
        }

        /// <summary>
        /// Initializes a new <see cref="LinearGradientBrush"/> with the specified rectangle and colors, using a gradient mode.
        /// </summary>
        /// <param name="rect">The rectangle that defines the start and end points of the gradient.</param>
        /// <param name="color1">The starting color.</param>
        /// <param name="color2">The ending color.</param>
        /// <param name="mode">The linear gradient mode specifying the direction.</param>
        public LinearGradientBrush(Rectangle rect, Color color1, Color color2, LinearGradientMode mode)
            : this(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), color1, color2, mode) { }

        /// <summary>
        /// Initializes a new <see cref="LinearGradientBrush"/> with the specified rectangle, colors, and angle.
        /// </summary>
        /// <param name="rect">The rectangle that defines the center of the gradient.</param>
        /// <param name="color1">The starting color.</param>
        /// <param name="color2">The ending color.</param>
        /// <param name="angle">The angle of the gradient in degrees.</param>
        public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle)
            : this(rect, color1, color2, angle, false) { }

        /// <summary>
        /// Initializes a new <see cref="LinearGradientBrush"/> with the specified rectangle, colors, angle, and scaleability.
        /// </summary>
        /// <param name="rect">The rectangle that defines the center of the gradient.</param>
        /// <param name="color1">The starting color.</param>
        /// <param name="color2">The ending color.</param>
        /// <param name="angle">The angle of the gradient in degrees.</param>
        /// <param name="isAngleScaleable">If <c>true</c>, the angle scales with the shape.</param>
        public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable)
        {
            _colors    = new[] { color1, color2 };
            _rectangle = rect;
            _wrapMode  = WrapMode.Tile;
            _transform = new Matrix();
            ComputePointsFromAngle(rect, angle, isAngleScaleable);
        }

        /// <summary>
        /// Initializes a new <see cref="LinearGradientBrush"/> with the specified rectangle, colors, angle, and scaleability.
        /// </summary>
        /// <param name="rect">The rectangle that defines the center of the gradient.</param>
        /// <param name="color1">The starting color.</param>
        /// <param name="color2">The ending color.</param>
        /// <param name="angle">The angle of the gradient in degrees.</param>
        /// <param name="isAngleScaleable">If <c>true</c>, the angle scales with the shape.</param>
        public LinearGradientBrush(Rectangle rect, Color color1, Color color2, float angle, bool isAngleScaleable)
            : this(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), color1, color2, angle, isAngleScaleable) { }

        // -----------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the starting and ending colors of the gradient.
        /// </summary>
        public Color[] LinearColors
        {
            get => new[] { _colors[0], _colors[1] };
            set
            {
                if (value == null || value.Length < 2)
                    throw new ArgumentException("LinearColors requires at least 2 colours.", nameof(value));
                _colors[0] = value[0];
                _colors[1] = value[1];
            }
        }

        /// <summary>
        /// Gets the rectangle that defines the bounds of this gradient brush.
        /// </summary>
        public RectangleF Rectangle => _rectangle;

        /// <summary>
        /// Gets or sets the transformation matrix for this brush.
        /// </summary>
        public Matrix Transform
        {
            get => _transform;
            set => _transform = value ?? new Matrix();
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
        /// Gets or sets whether gamma correction is enabled for the gradient.
        /// </summary>
        public bool GammaCorrection
        {
            get => _gammaCorrection;
            set => _gammaCorrection = value;
        }

        /// <summary>
        /// Gets or sets the blend factors for custom gradient blending.
        /// </summary>
        public Blend? Blend
        {
            get => _blend;
            set { _blend = value; if (value != null) _interpolationColors = null; }
        }

        /// <summary>
        /// Gets or sets the color blend for custom gradient interpolation.
        /// </summary>
        public ColorBlend InterpolationColors
        {
            get => _interpolationColors ?? DefaultColorBlend();
            set { _interpolationColors = value; _blend = null; }
        }

        /// <summary>
        /// Gets the starting point of the gradient.
        /// </summary>
        public PointF Point1 => _pt1;

        /// <summary>
        /// Gets the ending point of the gradient.
        /// </summary>
        public PointF Point2 => _pt2;

        // -----------------------------------------------------------------------
        // Transform helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Resets the transformation matrix to the identity matrix.
        /// </summary>
        public void ResetTransform()                                                                => _transform.Reset();

        /// <summary>
        /// Multiplies the transformation matrix by the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix to multiply.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void MultiplyTransform(Matrix matrix, MatrixOrder order = MatrixOrder.Prepend)       => _transform.Multiply(matrix, order);

        /// <summary>
        /// Rotates the transformation matrix by the specified angle.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend)           => _transform.Rotate(angle, order);

        /// <summary>
        /// Scales the transformation matrix by the specified factors.
        /// </summary>
        /// <param name="sx">The horizontal scale factor.</param>
        /// <param name="sy">The vertical scale factor.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)     => _transform.Scale(sx, sy, order);

        /// <summary>
        /// Translates the transformation matrix by the specified offsets.
        /// </summary>
        /// <param name="dx">The horizontal translation.</param>
        /// <param name="dy">The vertical translation.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend) => _transform.Translate(dx, dy, order);

        // -----------------------------------------------------------------------
        // Blend shape helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates a triangular blend that emphasizes colors at the specified focus.
        /// </summary>
        /// <param name="focus">The focus position (0.0 to 1.0).</param>
        /// <param name="scale">The scale factor for the blend.</param>
        public void SetBlendTriangularShape(float focus, float scale = 1f)
        {
            // Creates a triangular blend: 0 → scale at focus → 0
            float mid = Math.Max(0f, Math.Min(1f, focus));
            _blend = new Blend(3)
            {
                Positions = new[] { 0f, mid, 1f },
                Factors   = new[] { 0f, scale, 0f },
            };
            _interpolationColors = null;
        }

        /// <summary>
        /// Creates a bell-shaped blend that emphasizes colors at the specified focus.
        /// </summary>
        /// <param name="focus">The focus position (0.0 to 1.0).</param>
        /// <param name="scale">The scale factor for the blend.</param>
        public void SetSigmaBellShape(float focus, float scale = 1f)
        {
            // Creates a bell-curve blend approximated with 7 sample points
            const int pts = 7;
            float mid = Math.Max(0f, Math.Min(1f, focus));
            var positions = new float[pts];
            var factors   = new float[pts];

            for (int i = 0; i < pts; i++)
            {
                float t = i / (float)(pts - 1);
                positions[i] = t;
                // Gaussian-like curve centred at mid
                float dist = (t - mid) * 4f; // scale so bell fits [0,1]
                factors[i] = scale * (float)Math.Exp(-dist * dist);
            }
            _blend = new Blend(pts) { Positions = positions, Factors = factors };
            _interpolationColors = null;
        }

        /// <summary>
        /// Creates an exact copy of this <see cref="LinearGradientBrush"/>.
        /// </summary>
        /// <returns>A new <see cref="LinearGradientBrush"/> with the same properties.</returns>
        public override Brush Clone()
        {
            var clone = new LinearGradientBrush(_pt1, _pt2, _colors[0], _colors[1])
            {
                _rectangle        = _rectangle,
                _wrapMode         = _wrapMode,
                _gammaCorrection  = _gammaCorrection,
            };
            clone._transform = _transform.Clone();
            if (_blend != null)
            {
                clone._blend = new Blend(_blend.Positions.Length)
                {
                    Positions = (float[])_blend.Positions.Clone(),
                    Factors   = (float[])_blend.Factors.Clone(),
                };
            }
            if (_interpolationColors != null)
            {
                clone._interpolationColors = new ColorBlend(_interpolationColors.Positions.Length)
                {
                    Positions = (float[])_interpolationColors.Positions.Clone(),
                    Colors    = (Color[])_interpolationColors.Colors.Clone(),
                };
            }
            return clone;
        }

        /// <summary>
        /// Releases the unmanaged and optionally the managed resources used by this <see cref="LinearGradientBrush"/>.
        /// </summary>
        /// <param name="disposing">If <c>true</c>, releases both managed and unmanaged resources; if <c>false</c>, releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _transform.Dispose();
            base.Dispose(disposing);
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private void ComputePointsFromMode(RectangleF rect, LinearGradientMode mode)
        {
            switch (mode)
            {
                case LinearGradientMode.Horizontal:
                    _pt1 = new PointF(rect.Left, rect.Top);
                    _pt2 = new PointF(rect.Right, rect.Top);
                    break;
                case LinearGradientMode.Vertical:
                    _pt1 = new PointF(rect.Left, rect.Top);
                    _pt2 = new PointF(rect.Left, rect.Bottom);
                    break;
                case LinearGradientMode.ForwardDiagonal:
                    _pt1 = new PointF(rect.Left, rect.Top);
                    _pt2 = new PointF(rect.Right, rect.Bottom);
                    break;
                default: // BackwardDiagonal
                    _pt1 = new PointF(rect.Right, rect.Top);
                    _pt2 = new PointF(rect.Left, rect.Bottom);
                    break;
            }
        }

        private void ComputePointsFromAngle(RectangleF rect, float angle, bool scaleAngle)
        {
            float effectiveAngle = angle;
            if (scaleAngle && rect.Width != 0f && rect.Height != 0f)
            {
                double radians = angle * (Math.PI / 180.0);
                effectiveAngle = (float)(Math.Atan2(
                    Math.Tan(radians) * rect.Height,
                    rect.Width) * (180.0 / Math.PI));
            }

            float cx  = rect.X + rect.Width  * 0.5f;
            float cy  = rect.Y + rect.Height * 0.5f;
            double rad = effectiveAngle * (Math.PI / 180.0);
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            float halfLen = (Math.Abs(rect.Width * cos) + Math.Abs(rect.Height * sin)) * 0.5f;

            _pt1 = new PointF(cx - cos * halfLen, cy - sin * halfLen);
            _pt2 = new PointF(cx + cos * halfLen, cy + sin * halfLen);
        }

        private ColorBlend DefaultColorBlend() => new ColorBlend(2)
        {
            Colors    = new[] { _colors[0], _colors[1] },
            Positions = new[] { 0f, 1f },
        };
    }
}
