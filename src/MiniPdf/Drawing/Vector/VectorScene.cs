using System.Collections.Generic;

namespace MiniPdf.Drawing.Vector
{
    /// <summary>
    /// Format-agnostic recording of a sequence of drawing operations.
    /// Producers (RecordingGraphics, EmfPlayer, SvgReader) append commands;
    /// consumers (VectorScenePlayer, SvgWriter, EmfDrawingContext) read them.
    /// </summary>
    public sealed class VectorScene
    {
        /// <summary>
        /// Gets or sets the canvas width in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the canvas height in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the horizontal resolution in dots per inch.
        /// </summary>
        public float DpiX { get; set; } = 96f;

        /// <summary>
        /// Gets or sets the vertical resolution in dots per inch.
        /// </summary>
        public float DpiY { get; set; } = 96f;

        /// <summary>
        /// Gets the ordered list of recorded drawing commands.
        /// </summary>
        public List<DrawingCommand> Commands { get; } = new List<DrawingCommand>();

        /// <summary>
        /// Appends a drawing command to the scene.
        /// </summary>
        /// <param name="cmd">The command to append.</param>
        internal void Add(DrawingCommand cmd)
        {
            if (cmd != null)
                Commands.Add(cmd);
        }

        /// <summary>
        /// Creates a deep copy of this scene, including cloned commands.
        /// </summary>
        /// <returns>A new <see cref="VectorScene"/> with the same content.</returns>
        public VectorScene Clone()
        {
            var clone = new VectorScene
            {
                Width  = Width,
                Height = Height,
                DpiX   = DpiX,
                DpiY   = DpiY,
            };
            foreach (var cmd in Commands)
                clone.Commands.Add(cmd.Clone());
            return clone;
        }
    }
}