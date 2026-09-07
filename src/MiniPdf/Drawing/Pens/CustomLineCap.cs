using MiniPdf.Drawing.Drawing2D;
using System;

namespace MiniPdf.Drawing.Pens
{
    /// <summary>
    /// Represents a custom line cap used to draw the ends of lines.
    /// </summary>
    /// <remarks>
    /// A <see cref="CustomLineCap"/> can be used to create custom start and end caps
    /// for lines drawn with a <see cref="Pen"/>. The cap can be filled with a path
    /// and outlined with another path.
    /// </remarks>
    public class CustomLineCap : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="CustomLineCap"/> with the specified fill and stroke paths.
        /// </summary>
        /// <param name="fillPath">The path used to fill the cap.</param>
        /// <param name="strokePath">The path used to outline the cap.</param>
        /// <param name="baseCap">The base cap style.</param>
        /// <param name="baseInset">The inset from the end of the line.</param>
        public CustomLineCap(GraphicsPath? fillPath, GraphicsPath? strokePath,
                             LineCap baseCap = LineCap.Flat, float baseInset = 0f)
        {
            FillPath   = fillPath;
            StrokePath = strokePath;
            BaseCap    = baseCap;
            BaseInset  = baseInset;
        }

        /// <summary>
        /// Gets or sets the path used to fill the cap.
        /// </summary>
        public GraphicsPath? FillPath   { get; set; }

        /// <summary>
        /// Gets or sets the path used to outline the cap.
        /// </summary>
        public GraphicsPath? StrokePath { get; set; }

        /// <summary>
        /// Gets or sets the base cap style.
        /// </summary>
        public LineCap   BaseCap    { get; set; }

        /// <summary>
        /// Gets or sets the stroke join style.
        /// </summary>
        public LineJoin  StrokeJoin { get; set; } = LineJoin.Round;

        /// <summary>
        /// Gets or sets the inset from the end of the line.
        /// </summary>
        public float     BaseInset  { get; set; }

        /// <summary>
        /// Gets or sets the width scale for the cap.
        /// </summary>
        public float     WidthScale { get; set; } = 1f;

        /// <summary>
        /// Creates an exact copy of this <see cref="CustomLineCap"/>.
        /// </summary>
        /// <returns>A new <see cref="CustomLineCap"/> with the same properties.</returns>
        public virtual CustomLineCap Clone()
        {
            CheckDisposed();
            return new CustomLineCap(
                (GraphicsPath?)FillPath?.Clone(),
                (GraphicsPath?)StrokePath?.Clone(),
                BaseCap, BaseInset)
            {
                StrokeJoin = StrokeJoin,
                WidthScale = WidthScale,
            };
        }

        /// <summary>
        /// Releases all resources used by this custom line cap.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the custom line cap and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                FillPath?.Dispose();
                StrokePath?.Dispose();
            }
            _disposed = true;
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if this custom line cap has been disposed.
        /// </summary>
        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
        }
    }
}
