using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Rendering;

namespace MiniPdf.Drawing.Vector.Commands
{
    /// <summary>
    /// Sets a rendering hint property (SmoothingMode, InterpolationMode, etc.).
    /// The value is boxed and dispatched by type on replay.
    /// </summary>
    public sealed class SetHintCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the hint value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new <see cref="SetHintCommand"/>.
        /// </summary>
        /// <param name="value">The hint value (a rendering-hint enum).</param>
        public SetHintCommand(object value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
        {
            switch (Value)
            {
                case SmoothingMode v:      ctx.SmoothingMode      = v; break;
                case InterpolationMode v:  ctx.InterpolationMode  = v; break;
                case CompositingMode v:    ctx.CompositingMode    = v; break;
                case CompositingQuality v: ctx.CompositingQuality = v; break;
                case PixelOffsetMode v:    ctx.PixelOffsetMode    = v; break;
                case Text.TextRenderingHint v: ctx.TextRenderingHint = v; break;
                case GraphicsUnit v:       ctx.PageUnit           = v; break;
                case float v:              ctx.PageScale          = v; break;
                case Point v:              ctx.RenderingOrigin    = v; break;
            }
        }

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new SetHintCommand(Value);
    }

    /// <summary>
    /// Sets the page unit.
    /// </summary>
    public sealed class SetPageUnitCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the page unit.
        /// </summary>
        public GraphicsUnit PageUnit { get; }

        /// <summary>
        /// Initializes a new <see cref="SetPageUnitCommand"/>.
        /// </summary>
        public SetPageUnitCommand(GraphicsUnit pageUnit)
        {
            PageUnit = pageUnit;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.PageUnit = PageUnit;

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new SetPageUnitCommand(PageUnit);
    }

    /// <summary>
    /// Sets the page scale.
    /// </summary>
    public sealed class SetPageScaleCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the page scale.
        /// </summary>
        public float PageScale { get; }

        /// <summary>
        /// Initializes a new <see cref="SetPageScaleCommand"/>.
        /// </summary>
        public SetPageScaleCommand(float pageScale)
        {
            PageScale = pageScale;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.PageScale = PageScale;

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new SetPageScaleCommand(PageScale);
    }

    /// <summary>
    /// Sets the rendering origin.
    /// </summary>
    public sealed class SetRenderingOriginCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the rendering origin.
        /// </summary>
        public Point Origin { get; }

        /// <summary>
        /// Initializes a new <see cref="SetRenderingOriginCommand"/>.
        /// </summary>
        public SetRenderingOriginCommand(Point origin)
        {
            Origin = origin;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.RenderingOrigin = Origin;

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new SetRenderingOriginCommand(Origin);
    }

    /// <summary>
    /// Saves the current graphics state. The <see cref="GraphicsState"/> token
    /// is produced by the target at replay time and stored in
    /// <see cref="LastToken"/> so the paired <see cref="RestoreStateCommand"/>
    /// can use it. This supports sequential multi-replay.
    /// </summary>
    public sealed class SaveStateCommand : DrawingCommand
    {
        /// <summary>
        /// Gets or sets the state token produced during the most recent replay.
        /// </summary>
        internal GraphicsState? LastToken { get; set; }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => LastToken = ctx.Save();

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new SaveStateCommand();
    }

    /// <summary>
    /// Restores the graphics state to the state saved by the paired
    /// <see cref="SaveStateCommand"/>.
    /// </summary>
    public sealed class RestoreStateCommand : DrawingCommand
    {
        private readonly SaveStateCommand _save;

        /// <summary>
        /// Gets the save command this restore pairs with.
        /// </summary>
        public SaveStateCommand Save => _save;

        /// <summary>
        /// Initializes a new <see cref="RestoreStateCommand"/>.
        /// </summary>
        /// <param name="save">The paired save command.</param>
        public RestoreStateCommand(SaveStateCommand save)
        {
            _save = save;
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.Restore(_save.LastToken!);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new RestoreStateCommand(_save);
    }
}