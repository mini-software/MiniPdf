using MiniSoftware.Drawing.Drawing2D;

namespace MiniSoftware.Drawing.Vector.Commands
{
    /// <summary>
    /// Sets the world transform to the specified matrix.
    /// </summary>
    public sealed class SetTransformCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the transform matrix.
        /// </summary>
        public Matrix Transform { get; }

        /// <summary>
        /// Initializes a new <see cref="SetTransformCommand"/>, cloning the matrix.
        /// </summary>
        public SetTransformCommand(Matrix transform)
        {
            Transform = transform.Clone();
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.Transform = Transform;

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new SetTransformCommand(Transform);
    }

    /// <summary>
    /// Resets the world transform to the identity matrix.
    /// </summary>
    public sealed class ResetTransformCommand : DrawingCommand
    {
        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.ResetTransform();

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new ResetTransformCommand();
    }

    /// <summary>
    /// Multiplies the world transform by the specified matrix.
    /// </summary>
    public sealed class MultiplyTransformCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the matrix to multiply by.
        /// </summary>
        public Matrix Matrix { get; }

        /// <summary>
        /// Gets the multiplication order.
        /// </summary>
        public MatrixOrder Order { get; }

        /// <summary>
        /// Initializes a new <see cref="MultiplyTransformCommand"/>, cloning the matrix.
        /// </summary>
        public MultiplyTransformCommand(Matrix matrix, MatrixOrder order)
        {
            Matrix = matrix.Clone();
            Order  = order;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.MultiplyTransform(Matrix, Order);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new MultiplyTransformCommand(Matrix, Order);
    }

    /// <summary>
    /// Rotates the world transform by the specified angle.
    /// </summary>
    public sealed class RotateTransformCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the rotation angle in degrees.
        /// </summary>
        public float Angle { get; }

        /// <summary>
        /// Gets the multiplication order.
        /// </summary>
        public MatrixOrder Order { get; }

        /// <summary>
        /// Initializes a new <see cref="RotateTransformCommand"/>.
        /// </summary>
        public RotateTransformCommand(float angle, MatrixOrder order)
        {
            Angle = angle;
            Order = order;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.RotateTransform(Angle, Order);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new RotateTransformCommand(Angle, Order);
    }

    /// <summary>
    /// Scales the world transform by the specified factors.
    /// </summary>
    public sealed class ScaleTransformCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the horizontal scale factor.
        /// </summary>
        public float Sx { get; }

        /// <summary>
        /// Gets the vertical scale factor.
        /// </summary>
        public float Sy { get; }

        /// <summary>
        /// Gets the multiplication order.
        /// </summary>
        public MatrixOrder Order { get; }

        /// <summary>
        /// Initializes a new <see cref="ScaleTransformCommand"/>.
        /// </summary>
        public ScaleTransformCommand(float sx, float sy, MatrixOrder order)
        {
            Sx   = sx;
            Sy   = sy;
            Order = order;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.ScaleTransform(Sx, Sy, Order);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new ScaleTransformCommand(Sx, Sy, Order);
    }

    /// <summary>
    /// Translates the world transform by the specified offsets.
    /// </summary>
    public sealed class TranslateTransformCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the horizontal translation.
        /// </summary>
        public float Dx { get; }

        /// <summary>
        /// Gets the vertical translation.
        /// </summary>
        public float Dy { get; }

        /// <summary>
        /// Gets the multiplication order.
        /// </summary>
        public MatrixOrder Order { get; }

        /// <summary>
        /// Initializes a new <see cref="TranslateTransformCommand"/>.
        /// </summary>
        public TranslateTransformCommand(float dx, float dy, MatrixOrder order)
        {
            Dx   = dx;
            Dy   = dy;
            Order = order;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.TranslateTransform(Dx, Dy, Order);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new TranslateTransformCommand(Dx, Dy, Order);
    }
}