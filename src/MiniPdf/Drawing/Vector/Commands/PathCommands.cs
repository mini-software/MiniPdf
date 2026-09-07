using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Pens;

namespace MiniSoftware.Drawing.Vector.Commands
{
    /// <summary>
    /// Draws (strokes) a graphics path with the specified pen.
    /// </summary>
    public sealed class DrawPathCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the pen used to stroke the path.
        /// </summary>
        public Pen Pen { get; }

        /// <summary>
        /// Gets the path to draw.
        /// </summary>
        public GraphicsPath Path { get; }

        /// <summary>
        /// Initializes a new <see cref="DrawPathCommand"/>, cloning the pen and path.
        /// </summary>
        /// <param name="pen">The pen to stroke with.</param>
        /// <param name="path">The path to draw.</param>
        public DrawPathCommand(Pen pen, GraphicsPath path)
        {
            Pen  = pen.Clone();
            Path = path.Clone();
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.DrawPath(Pen, Path);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new DrawPathCommand(Pen, Path);
    }

    /// <summary>
    /// Fills the interior of a graphics path with the specified brush.
    /// </summary>
    public sealed class FillPathCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the brush used to fill the path.
        /// </summary>
        public Brush Brush { get; }

        /// <summary>
        /// Gets the path to fill.
        /// </summary>
        public GraphicsPath Path { get; }

        /// <summary>
        /// Gets the fill mode.
        /// </summary>
        public FillMode FillMode { get; }

        /// <summary>
        /// Initializes a new <see cref="FillPathCommand"/>, cloning the brush and path.
        /// </summary>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="path">The path to fill.</param>
        /// <param name="fillMode">The fill mode.</param>
        public FillPathCommand(Brush brush, GraphicsPath path, FillMode fillMode)
        {
            Brush   = brush.Clone();
            Path    = path.Clone();
            FillMode = fillMode;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.FillPath(Brush, Path, FillMode);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new FillPathCommand(Brush, Path, FillMode);
    }
}