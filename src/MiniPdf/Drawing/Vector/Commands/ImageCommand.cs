using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Vector.Commands
{
    /// <summary>
    /// Draws an image scaled to fit a destination rectangle.
    /// </summary>
    public sealed class DrawImageCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the image to draw.
        /// </summary>
        public Image Image { get; }

        /// <summary>
        /// Gets the destination rectangle.
        /// </summary>
        public RectangleF DestRect { get; }

        /// <summary>
        /// Gets the source rectangle.
        /// </summary>
        public RectangleF SrcRect { get; }

        /// <summary>
        /// Gets the source unit.
        /// </summary>
        public GraphicsUnit SrcUnit { get; }

        /// <summary>
        /// Gets the image attributes, or null.
        /// </summary>
        public ImageAttributes? Attr { get; }

        /// <summary>
        /// Initializes a new <see cref="DrawImageCommand"/>, cloning the image and attributes.
        /// </summary>
        public DrawImageCommand(Image image, RectangleF destRect, RectangleF srcRect,
                                GraphicsUnit srcUnit, ImageAttributes? attr)
        {
            Image    = image.Clone();
            DestRect = destRect;
            SrcRect  = srcRect;
            SrcUnit  = srcUnit;
            Attr     = attr?.Clone();
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.DrawImage(Image, DestRect, SrcRect, SrcUnit, Attr);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new DrawImageCommand(Image, DestRect, SrcRect, SrcUnit, Attr);
    }

    /// <summary>
    /// Draws an image transformed to fit a parallelogram defined by destination points.
    /// </summary>
    public sealed class DrawImageAffineCommand : DrawingCommand
    {
        /// <summary>
        /// Gets the image to draw.
        /// </summary>
        public Image Image { get; }

        /// <summary>
        /// Gets the destination points defining a parallelogram.
        /// </summary>
        public PointF[] DestPoints { get; }

        /// <summary>
        /// Gets the source rectangle.
        /// </summary>
        public RectangleF SrcRect { get; }

        /// <summary>
        /// Gets the source unit.
        /// </summary>
        public GraphicsUnit SrcUnit { get; }

        /// <summary>
        /// Gets the image attributes, or null.
        /// </summary>
        public ImageAttributes? Attr { get; }

        /// <summary>
        /// Initializes a new <see cref="DrawImageAffineCommand"/>, cloning the image and attributes.
        /// </summary>
        public DrawImageAffineCommand(Image image, PointF[] destPoints, RectangleF srcRect,
                                      GraphicsUnit srcUnit, ImageAttributes? attr)
        {
            Image      = image.Clone();
            DestPoints = (PointF[])destPoints.Clone();
            SrcRect    = srcRect;
            SrcUnit    = srcUnit;
            Attr       = attr?.Clone();
        }

        /// <inheritdoc/>
        internal override void Replay(IDrawingContext ctx)
            => ctx.DrawImage(Image, DestPoints, SrcRect, SrcUnit, Attr);

        /// <inheritdoc/>
        public override DrawingCommand Clone()
            => new DrawImageAffineCommand(Image, DestPoints, SrcRect, SrcUnit, Attr);
    }
}