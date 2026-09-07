using System;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;

namespace MiniPdf.Drawing.Pens
{
    /// <summary>
    /// Defines a pen used to draw lines, curves, and shapes.
    /// </summary>
    /// <remarks>
    /// A <see cref="Pen"/> can be initialized with a color or brush, and customized
    /// with line caps, joins, dash patterns, and alignment. Pens are used with
    /// <see cref="Graphics"/> objects to draw outlines of shapes.
    /// </remarks>
    public sealed class Pen : IDisposable
    {
        private bool         _disposed;
        private Color        _color;
        private float        _width;
        private Brush?       _brush;
        private LineCap      _startCap     = LineCap.Flat;
        private LineCap      _endCap       = LineCap.Flat;
        private LineJoin     _lineJoin     = LineJoin.Miter;
        private float        _miterLimit   = 10f;
        private DashStyle    _dashStyle    = DashStyle.Solid;
        private float        _dashOffset;
        private float[]?     _dashPattern;
        private DashCap      _dashCap      = DashCap.Flat;
        private PenAlignment _alignment    = PenAlignment.Center;
        private float[]?     _compoundArray;
        private CustomLineCap? _customStartCap;
        private CustomLineCap? _customEndCap;
        private Matrix       _transform;

        // -----------------------------------------------------------------------
        // Constructors
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="Pen"/> of the specified color and width of 1.
        /// </summary>
        /// <param name="color">The color of the pen.</param>
        public Pen(Color color) : this(color, 1f) { }

        /// <summary>
        /// Initializes a new <see cref="Pen"/> of the specified color and width.
        /// </summary>
        /// <param name="color">The color of the pen.</param>
        /// <param name="width">The width of the pen.</param>
        public Pen(Color color, float width)
        {
            _color     = color;
            _width     = width;
            _transform = new Matrix();
        }

        /// <summary>
        /// Initializes a new <see cref="Pen"/> of the specified brush and width of 1.
        /// </summary>
        /// <param name="brush">The brush that determines the fill properties of the pen.</param>
        public Pen(Brush brush) : this(brush, 1f) { }

        /// <summary>
        /// Initializes a new <see cref="Pen"/> of the specified brush and width.
        /// </summary>
        /// <param name="brush">The brush that determines the fill properties of the pen.</param>
        /// <param name="width">The width of the pen.</param>
        public Pen(Brush brush, float width)
        {
            if (brush == null) throw new ArgumentNullException(nameof(brush));
            _brush     = brush;
            _color     = brush is SolidBrush sb ? sb.Color : Color.Black;
            _width     = width;
            _transform = new Matrix();
        }

        // -----------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the color of this pen.
        /// </summary>
        public Color Color
        {
            get
            {
                CheckDisposed();
                return _brush is SolidBrush sb ? sb.Color : _color;
            }
            set
            {
                CheckDisposed();
                _color = value;
                _brush = null;
            }
        }

        /// <summary>
        /// Gets or sets the width of this pen.
        /// </summary>
        public float Width
        {
            get { CheckDisposed(); return _width; }
            set { CheckDisposed(); _width = value; }
        }

        /// <summary>
        /// Gets or sets the brush that determines the fill properties of this pen.
        /// </summary>
        public Brush Brush
        {
            get
            {
                CheckDisposed();
                return _brush ?? new SolidBrush(_color);
            }
            set
            {
                CheckDisposed();
                if (value == null) throw new ArgumentNullException(nameof(value));
                _brush = value;
                if (value is SolidBrush sb)
                    _color = sb.Color;
            }
        }

        /// <summary>
        /// Gets the type of this pen (SolidColor, HatchFill, or LinearGradient).
        /// </summary>
        public PenType PenType
        {
            get
            {
                CheckDisposed();
                return _brush switch
                {
                    null                 => PenType.SolidColor,
                    SolidBrush           => PenType.SolidColor,
                    HatchBrush           => PenType.HatchFill,
                    LinearGradientBrush  => PenType.LinearGradient,
                    _                    => PenType.SolidColor,
                };
            }
        }

        /// <summary>
        /// Gets or sets the cap style at the start of the line.
        /// </summary>
        public LineCap StartCap
        {
            get { CheckDisposed(); return _startCap; }
            set { CheckDisposed(); _startCap = value; }
        }

        /// <summary>
        /// Gets or sets the cap style at the end of the line.
        /// </summary>
        public LineCap EndCap
        {
            get { CheckDisposed(); return _endCap; }
            set { CheckDisposed(); _endCap = value; }
        }

        /// <summary>
        /// Gets or sets the join style for connected line segments.
        /// </summary>
        public LineJoin LineJoin
        {
            get { CheckDisposed(); return _lineJoin; }
            set { CheckDisposed(); _lineJoin = value; }
        }

        /// <summary>
        /// Gets or sets the miter limit for sharp join angles.
        /// </summary>
        public float MiterLimit
        {
            get { CheckDisposed(); return _miterLimit; }
            set { CheckDisposed(); _miterLimit = value; }
        }

        /// <summary>
        /// Gets or sets the dash style for the line.
        /// </summary>
        public DashStyle DashStyle
        {
            get { CheckDisposed(); return _dashStyle; }
            set
            {
                CheckDisposed();
                _dashStyle = value;
                if (value != DashStyle.Custom)
                    _dashPattern = null;
            }
        }

        /// <summary>
        /// Gets or sets the offset of the dash pattern.
        /// </summary>
        public float DashOffset
        {
            get { CheckDisposed(); return _dashOffset; }
            set { CheckDisposed(); _dashOffset = value; }
        }

        /// <summary>
        /// Gets or sets the dash pattern for custom line styles.
        /// </summary>
        public float[] DashPattern
        {
            get
            {
                CheckDisposed();
                return _dashPattern != null ? (float[])_dashPattern.Clone() : Array.Empty<float>();
            }
            set
            {
                CheckDisposed();
                _dashPattern = value != null ? (float[])value.Clone() : null;
                _dashStyle   = DashStyle.Custom;
            }
        }

        /// <summary>
        /// Gets or sets the dash cap style for the ends of dashes.
        /// </summary>
        public DashCap DashCap
        {
            get { CheckDisposed(); return _dashCap; }
            set { CheckDisposed(); _dashCap = value; }
        }

        /// <summary>
        /// Gets or sets the alignment of the pen relative to the stroke outline.
        /// </summary>
        public PenAlignment Alignment
        {
            get { CheckDisposed(); return _alignment; }
            set { CheckDisposed(); _alignment = value; }
        }

        /// <summary>
        /// Gets or sets the compound array for compound pen strokes.
        /// </summary>
        public float[]? CompoundArray
        {
            get { CheckDisposed(); return _compoundArray != null ? (float[])_compoundArray.Clone() : null; }
            set { CheckDisposed(); _compoundArray = value != null ? (float[])value.Clone() : null; }
        }

        /// <summary>
        /// Gets or sets the custom start cap for the line.
        /// </summary>
        public CustomLineCap? CustomStartCap
        {
            get { CheckDisposed(); return _customStartCap; }
            set { CheckDisposed(); _customStartCap = value; }
        }

        /// <summary>
        /// Gets or sets the custom end cap for the line.
        /// </summary>
        public CustomLineCap? CustomEndCap
        {
            get { CheckDisposed(); return _customEndCap; }
            set { CheckDisposed(); _customEndCap = value; }
        }

        /// <summary>
        /// Gets or sets the transformation matrix for the pen.
        /// </summary>
        public Matrix Transform
        {
            get { CheckDisposed(); return _transform; }
            set { CheckDisposed(); _transform = value ?? new Matrix(); }
        }

        // -----------------------------------------------------------------------
        // Transform methods
        // -----------------------------------------------------------------------

        /// <summary>
        /// Resets the transformation matrix to the identity matrix.
        /// </summary>
        public void ResetTransform()
        {
            CheckDisposed();
            _transform.Reset();
        }

        /// <summary>
        /// Multiplies the transformation matrix by the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix to multiply.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void MultiplyTransform(Matrix matrix, MatrixOrder order = MatrixOrder.Prepend)
        {
            CheckDisposed();
            _transform.Multiply(matrix, order);
        }

        /// <summary>
        /// Rotates the transformation matrix by the specified angle.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend)
        {
            CheckDisposed();
            _transform.Rotate(angle, order);
        }

        /// <summary>
        /// Scales the transformation matrix by the specified factors.
        /// </summary>
        /// <param name="sx">The horizontal scale factor.</param>
        /// <param name="sy">The vertical scale factor.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)
        {
            CheckDisposed();
            _transform.Scale(sx, sy, order);
        }

        /// <summary>
        /// Translates the transformation matrix by the specified offsets.
        /// </summary>
        /// <param name="dx">The horizontal translation.</param>
        /// <param name="dy">The vertical translation.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend)
        {
            CheckDisposed();
            _transform.Translate(dx, dy, order);
        }

        // -----------------------------------------------------------------------
        // Clone / Dispose
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates an exact copy of this <see cref="Pen"/>.
        /// </summary>
        /// <returns>A new <see cref="Pen"/> with the same properties.</returns>
        public Pen Clone()
        {
            CheckDisposed();
            var clone = new Pen(_color, _width)
            {
                _brush         = _brush?.Clone(),
                _startCap      = _startCap,
                _endCap        = _endCap,
                _lineJoin      = _lineJoin,
                _miterLimit    = _miterLimit,
                _dashStyle     = _dashStyle,
                _dashOffset    = _dashOffset,
                _dashPattern   = _dashPattern != null ? (float[])_dashPattern.Clone() : null,
                _dashCap       = _dashCap,
                _alignment     = _alignment,
                _compoundArray = _compoundArray != null ? (float[])_compoundArray.Clone() : null,
            };
            clone._transform = _transform.Clone();
            return clone;
        }

        /// <summary>
        /// Releases all resources used by this pen.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            _transform.Dispose();
            GC.SuppressFinalize(this);
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if this pen has been disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Pen));
        }
    }
}
