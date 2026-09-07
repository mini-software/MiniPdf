using MiniPdf.Drawing.Colors;
using System;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Defines an array of colors that make up a color palette.
    /// </summary>
    public sealed class ColorPalette
    {
        /// <summary>
        /// Gets or sets the array of colors in the palette.
        /// </summary>
        public Color[] Entries { get; set; }

        /// <summary>
        /// Gets or sets the flags for the palette.
        /// </summary>
        public int     Flags   { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ColorPalette"/> with an empty entries array.
        /// </summary>
        public ColorPalette()
        {
            Entries = Array.Empty<Color>();
        }

        /// <summary>
        /// Initializes a new <see cref="ColorPalette"/> with the specified flags and entries.
        /// </summary>
        /// <param name="flags">The palette flags.</param>
        /// <param name="entries">The color entries for the palette.</param>
        public ColorPalette(int flags, Color[] entries)
        {
            Flags   = flags;
            Entries = entries ?? Array.Empty<Color>();
        }
    }
}
