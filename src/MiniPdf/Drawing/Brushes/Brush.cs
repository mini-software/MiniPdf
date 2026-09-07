using System;

namespace MiniPdf.Drawing.Brushes
{
    /// <summary>
    /// Defines objects used to fill the interior of shapes, such as rectangles, ellipses, and paths.
    /// This is the base class for all brush types including <see cref="SolidBrush"/>, <see cref="HatchBrush"/>,
    /// <see cref="LinearGradientBrush"/>, <see cref="PathGradientBrush"/>, and <see cref="TextureBrush"/>.
    /// </summary>
    /// <remarks>
    /// Brushes are used with <see cref="Graphics"/> objects to fill shapes.
    /// Derived classes provide different fill patterns such as solid colors, hatching, gradients, and textures.
    /// </remarks>
    public abstract class Brush : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Creates an exact copy of this brush.
        /// </summary>
        /// <returns>A new <see cref="Brush"/> object that is a copy of this instance.</returns>
        public abstract Brush Clone();

        /// <summary>
        /// Releases the unmanaged resources used by the brush and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        /// <summary>
        /// Releases all resources used by this brush.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if this brush has been disposed.
        /// </summary>
        protected void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
