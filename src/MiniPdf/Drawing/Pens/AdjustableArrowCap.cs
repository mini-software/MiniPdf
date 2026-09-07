using MiniSoftware.Drawing.Drawing2D;

namespace MiniSoftware.Drawing.Pens
{
    /// <summary>
    /// Represents an adjustable arrow-shaped line cap that can be added to any <see cref="Pen"/>.
    /// </summary>
    /// <remarks>
    /// An <see cref="AdjustableArrowCap"/> can be used to draw arrowheads at the ends of lines.
    /// The width and height can be adjusted independently, and the arrow can be filled or outlined.
    /// </remarks>
    public sealed class AdjustableArrowCap : CustomLineCap
    {
        /// <summary>
        /// Initializes a new <see cref="AdjustableArrowCap"/> with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the arrow cap.</param>
        /// <param name="height">The height of the arrow cap.</param>
        /// <param name="isFilled">If <c>true</c>, the arrow cap is filled; otherwise, it is outlined.</param>
        public AdjustableArrowCap(float width, float height, bool isFilled = true)
            : base(null, null, LineCap.Flat, 0f)
        {
            Width  = width;
            Height = height;
            Filled = isFilled;
        }

        /// <summary>
        /// Gets or sets the width of the arrow cap.
        /// </summary>
        public float Width        { get; set; }

        /// <summary>
        /// Gets or sets the height of the arrow cap.
        /// </summary>
        public float Height       { get; set; }

        /// <summary>
        /// Gets or sets the middle inset of the arrow cap.
        /// </summary>
        public float MiddleInset  { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the arrow cap is filled.
        /// </summary>
        public bool  Filled       { get; set; }

        /// <summary>
        /// Creates an exact copy of this <see cref="AdjustableArrowCap"/>.
        /// </summary>
        /// <returns>A new <see cref="AdjustableArrowCap"/> with the same properties.</returns>
        public override CustomLineCap Clone()
            => new AdjustableArrowCap(Width, Height, Filled)
            {
                MiddleInset = MiddleInset,
                BaseCap     = BaseCap,
                BaseInset   = BaseInset,
                StrokeJoin  = StrokeJoin,
                WidthScale  = WidthScale,
            };
    }
}
