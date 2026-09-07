using System.IO;
using MiniPdf.Drawing.Font.Glyphs;
using MiniPdf.Drawing.Font.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "head" table (font header).
    /// Ref: OpenType spec §5.2.1 / TrueType Reference §'head'.
    /// </summary>
    public sealed class TtfHeadTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the head table.
        /// </summary>
        public const string TableTag = "head";

        // ── Parsed fields ─────────────────────────────────────────────────────

        /// <summary>Units per EM square.  Typically 1000 or 2048.</summary>
        public double UnitsPerEm { get; }

        /// <summary>
        /// Bit flags describing font style: bit 0=Bold, 1=Italic, 2=Underline,
        /// 3=Outline, 4=Shadow, 5=Condensed, 6=Extended.
        /// </summary>
        public ushort MacStyle { get; }

        /// <summary>
        /// Determines the format of the "loca" table:
        /// 0 = short offsets (×2); 1 = long offsets.
        /// </summary>
        public short IndexToLocFormat { get; }

        /// <summary>Design-space bounding box that encloses all glyphs.</summary>
        public GlyphBoundingBox Bounds { get; }

        /// <summary>Creation timestamp (seconds since 1904-01-01 00:00:00 UTC).</summary>
        public long Created { get; }

        /// <summary>Last-modified timestamp (seconds since 1904-01-01 00:00:00 UTC).</summary>
        public long Modified { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfHeadTable(
            byte[] rawBytes,
            double unitsPerEm,
            ushort macStyle,
            short indexToLocFormat,
            GlyphBoundingBox bounds,
            long created,
            long modified)
            : base(TableTag, rawBytes)
        {
            UnitsPerEm       = unitsPerEm;
            MacStyle         = macStyle;
            IndexToLocFormat = indexToLocFormat;
            Bounds           = bounds;
            Created          = created;
            Modified         = modified;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "head" table from its raw bytes.</summary>
        public static TtfHeadTable Parse(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            reader.Skip(4);  // version         (Fixed 1.0)
            reader.Skip(4);  // fontRevision    (Fixed)
            reader.Skip(4);  // checkSumAdjustment (ULONG)
            reader.Skip(4);  // magicNumber     (ULONG, should be 0x5F0F3CF5)
            reader.Skip(2);  // flags           (USHORT)

            ushort unitsPerEm = reader.ReadUInt16();
            long   created    = reader.ReadInt64();
            long   modified   = reader.ReadInt64();

            short xMin = reader.ReadInt16();
            short yMin = reader.ReadInt16();
            short xMax = reader.ReadInt16();
            short yMax = reader.ReadInt16();

            ushort macStyle         = reader.ReadUInt16();
            reader.Skip(2);          // lowestRecPPEM  (USHORT)
            reader.Skip(2);          // fontDirectionHint (SHORT, deprecated)
            short indexToLocFormat  = reader.ReadInt16();
            // glyphDataFormat (SHORT) not required here

            return new TtfHeadTable(
                rawBytes,
                unitsPerEm,
                macStyle,
                indexToLocFormat,
                new GlyphBoundingBox(xMin, yMin, xMax, yMax),
                created,
                modified);
        }
    }
}
