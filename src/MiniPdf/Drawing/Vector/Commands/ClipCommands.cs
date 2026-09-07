using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Vector.Commands
{
    /// <summary>
    /// Sets the clipping region to the specified region. The region captures
    /// the resulting clip state; replay uses <see cref="CombineMode.Replace"/>.
    /// </summary>
    public sealed class SetClipCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the clip region.
        /// </summary>
        public Region Clip { get; }

        /// <summary>
        /// Gets the combine mode (always Replace for recorded resulting clips).
        /// </summary>
        public CombineMode Mode { get; }

        /// <summary>
        /// Initializes a new <see cref="SetClipCommand"/>, cloning the region.
        /// </summary>
        public SetClipCommand(Region clip, CombineMode mode)
        {
            Clip = clip.Clone();
            Mode = mode;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.SetClip(Clip, Mode);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new SetClipCommand(Clip, Mode);
    }

    /// <summary>
    /// Resets the clipping region to the entire drawing surface.
    /// </summary>
    public sealed class ResetClipCommand : DrawingCommand
    {
        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.ResetClip();

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new ResetClipCommand();
    }

    /// <summary>
    /// Translates the clipping region by the specified amount.
    /// </summary>
    public sealed class TranslateClipCommand : DrawingCommand
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
        /// Initializes a new <see cref="TranslateClipCommand"/>.
        /// </summary>
        public TranslateClipCommand(float dx, float dy)
        {
            Dx = dx;
            Dy = dy;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.TranslateClip(Dx, Dy);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new TranslateClipCommand(Dx, Dy);
    }
}