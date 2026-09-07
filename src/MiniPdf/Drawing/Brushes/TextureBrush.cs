using System;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Brushes
{
    /// <summary>
    /// Fills the interior of a shape with an image, optionally tiling/wrapping it.
    /// </summary>
    public sealed class TextureBrush : Brush
    {
        private Image    _image;
        private WrapMode _wrapMode;
        private Matrix   _transform;

        // ── Constructors ────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes a new <see cref="TextureBrush"/> with the specified image and default wrap mode.
        /// </summary>
        /// <param name="image">The image to fill the brush with.</param>
        public TextureBrush(Image image)
            : this(image, WrapMode.Tile) { }

        /// <summary>
        /// Initializes a new <see cref="TextureBrush"/> with the specified image and wrap mode.
        /// </summary>
        /// <param name="image">The image to fill the brush with.</param>
        /// <param name="wrapMode">The wrap mode specifying how the image is tiled.</param>
        public TextureBrush(Image image, WrapMode wrapMode)
        {
            _image     = image ?? throw new ArgumentNullException(nameof(image));
            _wrapMode  = wrapMode;
            _transform = new Matrix();
        }

        /// <summary>
        /// Initializes a new <see cref="TextureBrush"/> with the specified image and destination rectangle.
        /// </summary>
        /// <param name="image">The image to fill the brush with.</param>
        /// <param name="dstRect">The destination rectangle.</param>
        public TextureBrush(Image image, RectangleF dstRect)
            : this(image, WrapMode.Tile) { }

        /// <summary>
        /// Initializes a new <see cref="TextureBrush"/> with the specified image, destination rectangle, and image attributes.
        /// </summary>
        /// <param name="image">The image to fill the brush with.</param>
        /// <param name="dstRect">The destination rectangle.</param>
        /// <param name="imageAttrs">The image attributes.</param>
        public TextureBrush(Image image, RectangleF dstRect, Drawing2D.ImageAttributes? imageAttrs)
            : this(image, WrapMode.Tile) { }

        // ── Properties ──────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the image associated with this <see cref="TextureBrush"/>.
        /// </summary>
        public Image Image => _image;

        /// <summary>
        /// Gets or sets the wrap mode for tiling the image.
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

        // ── Transform helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Resets the transformation matrix to the identity matrix.
        /// </summary>
        public void ResetTransform() => _transform.Reset();

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

        // ── Clone / Dispose ──────────────────────────────────────────────────────

        /// <summary>
        /// Creates an exact copy of this <see cref="TextureBrush"/>.
        /// </summary>
        /// <returns>A new <see cref="TextureBrush"/> with the same properties.</returns>
        public override Brush Clone()
            => new TextureBrush(_image, _wrapMode)
               {
                   _transform = (Matrix)_transform.Clone(),
               };

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TextureBrush"/>.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) { }
    }
}
