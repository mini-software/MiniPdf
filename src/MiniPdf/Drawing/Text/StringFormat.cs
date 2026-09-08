using System;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Encapsulates text layout information such as alignment, format flags, and tab stops.
    /// </summary>
    /// <remarks>
    /// A <see cref="StringFormat"/> is used with <see cref="Graphics.DrawString"/> to control
    /// text alignment, line spacing, trimming, and other formatting options.
    /// </remarks>
    public sealed class StringFormat : IDisposable
    {
        /// <summary>
        /// Gets or sets the horizontal text alignment.
        /// </summary>
        public StringAlignment Alignment         { get; set; } = StringAlignment.Near;

        /// <summary>
        /// Gets or sets the vertical text alignment.
        /// </summary>
        public StringAlignment LineAlignment     { get; set; } = StringAlignment.Near;

        /// <summary>
        /// Gets or sets the trimming behavior for text that overflows the layout rectangle.
        /// </summary>
        public StringTrimming Trimming           { get; set; } = StringTrimming.None;

        /// <summary>
        /// Gets or sets the format flags that control text layout.
        /// </summary>
        public StringFormatFlags FormatFlags     { get; set; }

        /// <summary>
        /// Gets or sets the hotkey prefix handling for text.
        /// </summary>
        public HotkeyPrefix HotkeyPrefix         { get; set; } = HotkeyPrefix.None;

        private float _firstTabOffset;
        private float[] _tabStops = Array.Empty<float>();
        private CharacterRange[] _measurableCharacterRanges = Array.Empty<CharacterRange>();

        /// <summary>
        /// Initializes a new <see cref="StringFormat"/> with default settings.
        /// </summary>
        public StringFormat() { }

        /// <summary>
        /// Initializes a new <see cref="StringFormat"/> with the specified format flags.
        /// </summary>
        /// <param name="options">The format flags to apply.</param>
        public StringFormat(StringFormatFlags options) { FormatFlags = options; }

        /// <summary>
        /// Initializes a new <see cref="StringFormat"/> as a copy of an existing format.
        /// </summary>
        /// <param name="format">The format to copy.</param>
        public StringFormat(StringFormat format)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));

            Alignment = format.Alignment;
            LineAlignment = format.LineAlignment;
            Trimming = format.Trimming;
            FormatFlags = format.FormatFlags;
            HotkeyPrefix = format.HotkeyPrefix;
            _firstTabOffset = format._firstTabOffset;
            _tabStops = (float[])format._tabStops.Clone();
            _measurableCharacterRanges = (CharacterRange[])format._measurableCharacterRanges.Clone();
        }

        /// <summary>
        /// Gets a <see cref="StringFormat"/> with default settings.
        /// </summary>
        public static StringFormat GenericDefault => new StringFormat();

        /// <summary>
        /// Gets a <see cref="StringFormat"/> with typographic settings.
        /// </summary>
        public static StringFormat GenericTypographic => new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip);

        /// <summary>
        /// Sets the tab stops for this format.
        /// </summary>
        /// <param name="firstTabOffset">The offset of the first tab stop.</param>
        /// <param name="tabStops">The array of tab stop positions.</param>
        public void SetTabStops(float firstTabOffset, float[] tabStops)
        {
            if (tabStops == null) throw new ArgumentNullException(nameof(tabStops));
            _firstTabOffset = firstTabOffset;
            _tabStops = (float[])tabStops.Clone();
        }

        /// <summary>
        /// Gets the tab stops for this format.
        /// </summary>
        /// <param name="firstTabOffset">The offset of the first tab stop.</param>
        /// <returns>An array of tab stop positions.</returns>
        public float[] GetTabStops(out float firstTabOffset)
        {
            firstTabOffset = _firstTabOffset;
            return (float[])_tabStops.Clone();
        }

        /// <summary>
        /// Sets the character ranges that can be measured.
        /// </summary>
        /// <param name="ranges">The array of character ranges.</param>
        public void SetMeasurableCharacterRanges(CharacterRange[] ranges)
        {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));
            _measurableCharacterRanges = (CharacterRange[])ranges.Clone();
        }

        /// <summary>
        /// Gets the character ranges that can be measured.
        /// </summary>
        /// <returns>An array of character ranges.</returns>
        public CharacterRange[] GetMeasurableCharacterRanges()
            => (CharacterRange[])_measurableCharacterRanges.Clone();

        /// <summary>
        /// Creates an exact copy of this <see cref="StringFormat"/>.
        /// </summary>
        /// <returns>A new <see cref="StringFormat"/> with the same settings.</returns>
        public StringFormat Clone() => new StringFormat(this);

        /// <summary>
        /// Releases all resources used by this format.
        /// </summary>
        public void Dispose() { }
    }
}
