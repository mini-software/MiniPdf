namespace MiniSoftware.Drawing.Vector
{
    /// <summary>
    /// Base class for one recorded drawing operation. Each subclass carries
    /// immutable, cloned state and a <see cref="Replay"/> that re-issues the
    /// operation onto any <see cref="IDrawingContext"/>.
    /// </summary>
    public abstract class DrawingCommand
    {
/// <summary>
    /// Replays this command onto the specified drawing context.
    /// </summary>
    /// <param name="ctx">The target drawing context.</param>
    internal abstract void Replay(IDrawingContext ctx);

        /// <summary>
        /// Creates a deep copy of this command.
        /// </summary>
        /// <returns>A new <see cref="DrawingCommand"/> with the same state.</returns>
        public abstract DrawingCommand Clone();
    }
}