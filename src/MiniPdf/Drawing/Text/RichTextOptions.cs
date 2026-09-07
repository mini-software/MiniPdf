using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Enums;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Rich-text layout options for <see cref="Graphics.DrawRichText"/> and
    /// <see cref="Graphics.MeasureRichText"/>. Carries a list of
    /// <see cref="RichTextRun"/> segments and default font properties.
    /// </summary>
    public sealed class RichTextOptions
    {
        /// <summary>
        /// Initializes a new <see cref="RichTextOptions"/> with the specified
        /// default font family, size, and style.
        /// </summary>
        public RichTextOptions(FontFamily family, float emSize, FontStyle style = FontStyle.Regular)
        {
            FontFamily = family ?? throw new ArgumentNullException(nameof(family));
            EmSize = emSize;
            FontStyle = style;
            Unit = GraphicsUnit.Point;
        }

        /// <summary>Default font family for runs without a <see cref="RichTextRun.FontFamily"/> override.</summary>
        public FontFamily FontFamily { get; }

        /// <summary>Default em size for runs without a <see cref="RichTextRun.EmSize"/> override.</summary>
        public float EmSize { get; }

        /// <summary>Default font style for runs without a <see cref="RichTextRun.FontStyle"/> override.</summary>
        public FontStyle FontStyle { get; }

        /// <summary>Unit for <see cref="EmSize"/> and <see cref="RichTextRun.EmSize"/>. Default <see cref="GraphicsUnit.Point"/>.</summary>
        public GraphicsUnit Unit { get; set; } = GraphicsUnit.Point;

        /// <summary>
        /// Rich text runs. Characters not covered by any run use the default
        /// font/size/style. Runs may overlap; later runs win for overlapping
        /// ranges (matches ImageSharp <c>RichTextOptions.Runs</c> semantics).
        /// </summary>
        public IList<RichTextRun> Runs { get; } = new List<RichTextRun>();

        /// <summary>
        /// Fallback font chain override for this rich-text operation, or null
        /// to use <see cref="Graphics.FontFallback"/> (or the system default).
        /// </summary>
        public FontFallbackChain? Fallback { get; set; }
    }
}