using MiniSoftware.Drawing.Font.IO;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "hhea" table (horizontal header).
    /// Contains metrics that describe the overall horizontal layout of the font.
    /// Ref: OpenType spec §5.2.3 / TrueType Reference §'hhea'.
    /// </summary>
    public sealed class TtfHheaTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the hhea table.
        /// </summary>
        public const string TableTag = "hhea";

        // ── Parsed fields ─────────────────────────────────────────────────────

        /// <summary>
        /// Distance from baseline to top of tallest ascender, in design units.
        /// Positive value.
        /// </summary>
        public short Ascender { get; }

        /// <summary>
        /// Distance from baseline to bottom of lowest descender, in design units.
        /// Negative value (below baseline).
        /// </summary>
        public short Descender { get; }

        /// <summary>
        /// Typographic line gap (additional line spacing beyond Ascender − Descender),
        /// in design units.
        /// </summary>
        public short LineGap { get; }

        /// <summary>Maximum advance width across all glyphs.</summary>
        public ushort AdvanceWidthMax { get; }

        /// <summary>Minimum left side-bearing across all glyphs.</summary>
        public short MinLsb { get; }

        /// <summary>Minimum right side-bearing across all glyphs.</summary>
        public short MinRsb { get; }

        /// <summary>Maximum x extent (xMin + advanceWidth) across all glyphs.</summary>
        public short XMaxExtent { get; }

        /// <summary>
        /// Number of hMetrics entries in the "hmtx" table.
        /// Glyphs with index ≥ this value share the last advance width.
        /// </summary>
        public ushort NumberOfHMetrics { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfHheaTable(
            byte[] rawBytes,
            short ascender, short descender, short lineGap,
            ushort advanceWidthMax,
            short minLsb, short minRsb, short xMaxExtent,
            ushort numberOfHMetrics)
            : base(TableTag, rawBytes)
        {
            Ascender          = ascender;
            Descender         = descender;
            LineGap           = lineGap;
            AdvanceWidthMax   = advanceWidthMax;
            MinLsb            = minLsb;
            MinRsb            = minRsb;
            XMaxExtent        = xMaxExtent;
            NumberOfHMetrics  = numberOfHMetrics;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "hhea" table from its raw bytes.</summary>
        public static TtfHheaTable Parse(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            reader.Skip(4);              // version (Fixed 1.0)
            short ascender        = reader.ReadInt16();
            short descender       = reader.ReadInt16();
            short lineGap         = reader.ReadInt16();
            ushort advanceWidthMax = reader.ReadUInt16();
            short minLsb          = reader.ReadInt16();
            short minRsb          = reader.ReadInt16();
            short xMaxExtent      = reader.ReadInt16();
            reader.Skip(2);              // caretSlopeRise  (SHORT)
            reader.Skip(2);              // caretSlopeRun   (SHORT)
            reader.Skip(2);              // caretOffset     (SHORT)
            reader.Skip(8);              // reserved[4]     (4 × SHORT)
            reader.Skip(2);              // metricDataFormat (SHORT)
            ushort numberOfHMetrics = reader.ReadUInt16();

            return new TtfHheaTable(
                rawBytes, ascender, descender, lineGap,
                advanceWidthMax, minLsb, minRsb, xMaxExtent,
                numberOfHMetrics);
        }
    }
}
