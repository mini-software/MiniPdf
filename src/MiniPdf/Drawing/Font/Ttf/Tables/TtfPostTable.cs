using MiniPdf.Drawing.Font.IO;
using System.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "post" table (PostScript compatibility).
    /// Contains italic angle and underline metrics used for PDF export and
    /// display rendering.
    /// Ref: OpenType spec §5.2.11.
    /// </summary>
    public sealed class TtfPostTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the post table.
        /// </summary>
        public const string TableTag = "post";

        // ── Parsed fields ─────────────────────────────────────────────────────

        /// <summary>
        /// Italic angle in counter-clockwise degrees from the vertical, in design units.
        /// Zero for upright fonts; negative for forward-slanting italic fonts.
        /// </summary>
        public double ItalicAngle { get; }

        /// <summary>
        /// Offset of the top of the underline stroke from the baseline, in design units.
        /// Negative values place the underline below the baseline.
        /// </summary>
        public short UnderlinePosition { get; }

        /// <summary>Thickness of the underline stroke, in design units.</summary>
        public short UnderlineThickness { get; }

        /// <summary>
        /// True if the font is monospaced (all glyphs have the same advance width).
        /// Zero in the table means proportional; any non-zero value means monospaced.
        /// </summary>
        public bool IsFixedPitch { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfPostTable(
            byte[] rawBytes,
            double italicAngle,
            short underlinePosition,
            short underlineThickness,
            bool isFixedPitch)
            : base(TableTag, rawBytes)
        {
            ItalicAngle        = italicAngle;
            UnderlinePosition  = underlinePosition;
            UnderlineThickness = underlineThickness;
            IsFixedPitch       = isFixedPitch;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "post" table from its raw bytes.</summary>
        public static TtfPostTable Parse(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            // Version is a 16.16 Fixed value; we read it as a Fixed double but
            // we don't branch on it — all versions share this prefix layout.
            reader.Skip(4);  // version (Fixed)

            double italicAngle       = reader.ReadFixed();  // 16.16 fixed-point
            short  underlinePosition  = reader.ReadInt16();
            short  underlineThickness = reader.ReadInt16();
            uint   isFixedPitchRaw    = reader.ReadUInt32(); // 0=proportional, nonzero=fixed

            return new TtfPostTable(
                rawBytes,
                italicAngle,
                underlinePosition,
                underlineThickness,
                isFixedPitchRaw != 0);
        }
    }
}
