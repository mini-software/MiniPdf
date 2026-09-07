using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Contains attributes and dimensions of a metafile.
    /// </summary>
    public sealed class MetafileHeader
    {
        /// <summary>
        /// Gets the bounding rectangle of the metafile.
        /// </summary>
        public Rectangle Bounds { get; }

        /// <summary>
        /// Gets the horizontal DPI of the metafile.
        /// </summary>
        public float DpiX { get; }

        /// <summary>
        /// Gets the vertical DPI of the metafile.
        /// </summary>
        public float DpiY { get; }

        /// <summary>
        /// Gets the frame unit of the metafile.
        /// </summary>
        public MetafileFrameUnit FrameUnit { get; }

        /// <summary>
        /// Gets the type of the metafile.
        /// </summary>
        public EmfType Type { get; }

        /// <summary>
        /// Gets a value indicating whether the metafile is a WMF (Windows Metafile).
        /// </summary>
        public bool IsWmf { get; }

        /// <summary>
        /// Gets a value indicating whether the metafile is an EMF (Enhanced Metafile).
        /// </summary>
        public bool IsEmf { get; }

        /// <summary>
        /// Gets a value indicating whether the metafile is an EMF+ (Enhanced Metafile Plus).
        /// </summary>
        public bool IsEmfPlus { get; }

        /// <summary>
        /// Initializes a new <see cref="MetafileHeader"/> with the specified values.
        /// </summary>
        /// <param name="bounds">The bounding rectangle.</param>
        /// <param name="dpiX">The horizontal DPI.</param>
        /// <param name="dpiY">The vertical DPI.</param>
        /// <param name="frameUnit">The frame unit.</param>
        /// <param name="type">The metafile type.</param>
        /// <param name="isWmf">Whether this is a WMF.</param>
        /// <param name="isEmf">Whether this is an EMF.</param>
        /// <param name="isEmfPlus">Whether this is an EMF+.</param>
        internal MetafileHeader(
            Rectangle bounds,
            float dpiX,
            float dpiY,
            MetafileFrameUnit frameUnit,
            EmfType type,
            bool isWmf,
            bool isEmf,
            bool isEmfPlus)
        {
            Bounds = bounds;
            DpiX = dpiX;
            DpiY = dpiY;
            FrameUnit = frameUnit;
            Type = type;
            IsWmf = isWmf;
            IsEmf = isEmf;
            IsEmfPlus = isEmfPlus;
        }
    }
}
