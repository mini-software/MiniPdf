namespace MiniSoftware.Drawing.Rendering
{
    /// <summary>
    /// Represents a graphics container that stores state information for a <see cref="Graphics"/> object.
    /// </summary>
    public sealed class GraphicsContainer
    {
        internal GraphicsContainer(GraphicsState state)
        {
            State = state;
        }

        /// <summary>
        /// Gets the graphics state stored in this container.
        /// </summary>
        internal GraphicsState State { get; }
    }
}
