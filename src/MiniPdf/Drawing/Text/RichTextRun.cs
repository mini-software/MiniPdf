using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Enums;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Describes a contiguous range of characters within a rich-text string that
    /// share font, style, size, and/or foreground brush overrides. Used with
    /// <see cref="RichTextOptions"/>.
    /// </summary>
    public sealed class RichTextRun
    {
        /// <summary>
        /// Initializes a new <see cref="RichTextRun"/> for the specified range.
        /// </summary>
        /// <param name="start">Zero-based start index within the text.</param>
        /// <param name="length">Number of characters in the run.</param>
        public RichTextRun(int start, int length)
        {
            if (start < 0) throw new System.ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new System.ArgumentOutOfRangeException(nameof(length));
            Start = start;
            Length = length;
        }

        /// <summary>Zero-based start index within the text.</summary>
        public int Start { get; }

        /// <summary>Number of characters in the run.</summary>
        public int Length { get; }

        /// <summary>
        /// Font family override for this run, or null to use the default
        /// family from <see cref="RichTextOptions"/>.
        /// </summary>
        public FontFamily? FontFamily { get; set; }

        /// <summary>
        /// Font style override for this run, or null to use the default style.
        /// </summary>
        public FontStyle? FontStyle { get; set; }

        /// <summary>
        /// Em size override for this run (in the unit specified by
        /// <see cref="RichTextOptions.Unit"/>), or null to use the default size.
        /// </summary>
        public float? EmSize { get; set; }

        /// <summary>
        /// Foreground brush override for this run, or null to use the
        /// <c>defaultBrush</c> passed to <see cref="Graphics.DrawRichText"/>.
        /// </summary>
        public Brush? Brush { get; set; }
    }
}