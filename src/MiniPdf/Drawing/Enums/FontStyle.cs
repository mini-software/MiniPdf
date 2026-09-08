using System;

namespace MiniSoftware.Drawing.Enums
{
    /// <summary>
    /// Specifies style information applied to text.
    /// </summary>
    [Flags]
    public enum FontStyle
    {
        /// <summary>
        /// Normal text.
        /// </summary>
        Regular   = 0,

        /// <summary>
        /// Bold text.
        /// </summary>
        Bold      = 1,

        /// <summary>
        /// Italic text.
        /// </summary>
        Italic    = 2,

        /// <summary>
        /// Underlined text.
        /// </summary>
        Underline = 4,

        /// <summary>
        /// Strikethrough text.
        /// </summary>
        Strikeout = 8,
    }
}
