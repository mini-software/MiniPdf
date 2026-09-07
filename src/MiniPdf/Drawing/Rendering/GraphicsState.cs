using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Text;

namespace MiniSoftware.Drawing.Rendering
{
    /// <summary>
    /// Represents the state of a <see cref="Graphics"/> object including transform, clipping, and rendering settings.
    /// </summary>
    public sealed class GraphicsState
    {
        internal GraphicsState(
            Matrix transform,
            Region clip,
            GraphicsUnit pageUnit,
            float pageScale,
            SmoothingMode smoothingMode,
            InterpolationMode interpolationMode,
            CompositingMode compositingMode,
            CompositingQuality compositingQuality,
            PixelOffsetMode pixelOffsetMode,
            TextRenderingHint textRenderingHint,
            Point renderingOrigin)
        {
            Transform = transform;
            Clip = clip;
            PageUnit = pageUnit;
            PageScale = pageScale;
            SmoothingMode = smoothingMode;
            InterpolationMode = interpolationMode;
            CompositingMode = compositingMode;
            CompositingQuality = compositingQuality;
            PixelOffsetMode = pixelOffsetMode;
            TextRenderingHint = textRenderingHint;
            RenderingOrigin = renderingOrigin;
        }

        /// <summary>
        /// Gets the transformation matrix.
        /// </summary>
        internal Matrix Transform { get; }

        /// <summary>
        /// Gets the clipping region.
        /// </summary>
        internal Region Clip { get; }

        /// <summary>
        /// Gets the page unit of measure.
        /// </summary>
        internal GraphicsUnit PageUnit { get; }

        /// <summary>
        /// Gets the page scale factor.
        /// </summary>
        internal float PageScale { get; }

        /// <summary>
        /// Gets the smoothing mode.
        /// </summary>
        internal SmoothingMode SmoothingMode { get; }

        /// <summary>
        /// Gets the interpolation mode.
        /// </summary>
        internal InterpolationMode InterpolationMode { get; }

        /// <summary>
        /// Gets the compositing mode.
        /// </summary>
        internal CompositingMode CompositingMode { get; }

        /// <summary>
        /// Gets the compositing quality.
        /// </summary>
        internal CompositingQuality CompositingQuality { get; }

        /// <summary>
        /// Gets the pixel offset mode.
        /// </summary>
        internal PixelOffsetMode PixelOffsetMode { get; }

        /// <summary>
        /// Gets the text rendering hint.
        /// </summary>
        internal TextRenderingHint TextRenderingHint { get; }

        /// <summary>
        /// Gets the rendering origin point.
        /// </summary>
        internal Point RenderingOrigin { get; }
    }
}
