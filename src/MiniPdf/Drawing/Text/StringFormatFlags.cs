using System;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Specifies the display and layout information for text strings.
    /// </summary>
    [Flags]
    public enum StringFormatFlags
    {
        /// <summary>
        /// Text is displayed from right to left.
        /// </summary>
        DirectionRightToLeft = 1,

        /// <summary>
        /// Text is displayed vertically.
        /// </summary>
        DirectionVertical = 2,

        /// <summary>
        /// Text fits into the bounding box.
        /// </summary>
        FitBlackBox = 4,

        /// <summary>
        /// Display format control characters.
        /// </summary>
        DisplayFormatControl = 0x20,

        /// <summary>
        /// Disable font fallback.
        /// </summary>
        NoFontFallback = 0x400,

        /// <summary>
        /// Include trailing spaces in measurement.
        /// </summary>
        MeasureTrailingSpaces = 0x800,

        /// <summary>
        /// Disable text wrapping.
        /// </summary>
        NoWrap = 0x1000,

        /// <summary>
        /// Limit text to a single line.
        /// </summary>
        LineLimit = 0x2000,

        /// <summary>
        /// Disable text clipping.
        /// </summary>
        NoClip = 0x4000
    }
}