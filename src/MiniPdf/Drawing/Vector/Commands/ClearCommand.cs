using MiniPdf.Drawing.Colors;

namespace MiniPdf.Drawing.Vector.Commands
{
    /// <summary>
    /// Clears the entire drawing surface with the specified color.
    /// </summary>
    public sealed class ClearCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the color to clear with.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Initializes a new <see cref="ClearCommand"/>.
        /// </summary>
        /// <param name="color">The color to clear with.</param>
        public ClearCommand(Color color)
        {
            Color = color;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.Clear(Color);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new ClearCommand(Color);
    }
}