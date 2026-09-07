using MiniSoftware.Drawing.Font.IO;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "maxp" table (maximum profile).
    /// The version 0.5 variant contains only <see cref="NumGlyphs"/>;
    /// version 1.0 adds glyph-program-stack and component limits used by
    /// the TrueType hinting interpreter.
    /// Ref: OpenType spec §5.2.6.
    /// </summary>
    public sealed class TtfMaxpTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the maxp table.
        /// </summary>
        public const string TableTag = "maxp";

        // ── Version magic ─────────────────────────────────────────────────────

        /// <summary>Raw version field value for version 0.5 (CFF/OTF fonts).</summary>
        public const uint Version05 = 0x00005000u;

        /// <summary>Raw version field value for version 1.0 (TrueType/glyf fonts).</summary>
        public const uint Version10 = 0x00010000u;

        // ── Common fields ─────────────────────────────────────────────────────

        /// <summary>Raw version field (see <see cref="Version05"/> / <see cref="Version10"/>).</summary>
        public uint Version { get; }

        /// <summary>Total number of glyphs in the font.</summary>
        public ushort NumGlyphs { get; }

        // ── Version 1.0 optional fields ───────────────────────────────────────

        /// <summary>Maximum number of points in a non-composite glyph (v1.0 only).</summary>
        public ushort? MaxPoints { get; }

        /// <summary>Maximum number of contours in a non-composite glyph (v1.0 only).</summary>
        public ushort? MaxContours { get; }

        /// <summary>Maximum number of points in a composite glyph (v1.0 only).</summary>
        public ushort? MaxComponentPoints { get; }

        /// <summary>Maximum number of contours in a composite glyph (v1.0 only).</summary>
        public ushort? MaxComponentContours { get; }

        /// <summary>Maximum number of hinting zones; 1 or 2 (v1.0 only).</summary>
        public ushort? MaxZones { get; }

        /// <summary>Maximum number of twilight zone points (v1.0 only).</summary>
        public ushort? MaxTwilightPoints { get; }

        /// <summary>Maximum number of storage area locations (v1.0 only).</summary>
        public ushort? MaxStorage { get; }

        /// <summary>Maximum number of function definitions (v1.0 only).</summary>
        public ushort? MaxFunctionDefs { get; }

        /// <summary>Maximum number of instruction definitions (v1.0 only).</summary>
        public ushort? MaxInstructionDefs { get; }

        /// <summary>Maximum stack depth for hinting instructions (v1.0 only).</summary>
        public ushort? MaxStackElements { get; }

        /// <summary>Maximum size of hinting instruction stream per glyph (v1.0 only).</summary>
        public ushort? MaxSizeOfInstructions { get; }

        /// <summary>Maximum number of components in a composite glyph (v1.0 only).</summary>
        public ushort? MaxComponentElements { get; }

        /// <summary>Maximum nesting depth of composite glyphs (v1.0 only).</summary>
        public ushort? MaxComponentDepth { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfMaxpTable(
            byte[] rawBytes, uint version, ushort numGlyphs,
            ushort? maxPoints, ushort? maxContours,
            ushort? maxComponentPoints, ushort? maxComponentContours,
            ushort? maxZones, ushort? maxTwilightPoints,
            ushort? maxStorage, ushort? maxFunctionDefs, ushort? maxInstructionDefs,
            ushort? maxStackElements, ushort? maxSizeOfInstructions,
            ushort? maxComponentElements, ushort? maxComponentDepth)
            : base(TableTag, rawBytes)
        {
            Version                = version;
            NumGlyphs              = numGlyphs;
            MaxPoints              = maxPoints;
            MaxContours            = maxContours;
            MaxComponentPoints     = maxComponentPoints;
            MaxComponentContours   = maxComponentContours;
            MaxZones               = maxZones;
            MaxTwilightPoints      = maxTwilightPoints;
            MaxStorage             = maxStorage;
            MaxFunctionDefs        = maxFunctionDefs;
            MaxInstructionDefs     = maxInstructionDefs;
            MaxStackElements       = maxStackElements;
            MaxSizeOfInstructions  = maxSizeOfInstructions;
            MaxComponentElements   = maxComponentElements;
            MaxComponentDepth      = maxComponentDepth;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "maxp" table from its raw bytes.</summary>
        public static TtfMaxpTable Parse(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            uint   version   = reader.ReadUInt32();
            ushort numGlyphs = reader.ReadUInt16();

            // Version 0.5 only has numGlyphs; version 1.0 has additional fields.
            if (version != Version10 || rawBytes.Length < 32)
            {
                return new TtfMaxpTable(
                    rawBytes, version, numGlyphs,
                    null, null, null, null, null, null,
                    null, null, null, null, null, null, null);
            }

            ushort maxPoints             = reader.ReadUInt16();
            ushort maxContours           = reader.ReadUInt16();
            ushort maxComponentPoints    = reader.ReadUInt16();
            ushort maxComponentContours  = reader.ReadUInt16();
            ushort maxZones              = reader.ReadUInt16();
            ushort maxTwilightPoints     = reader.ReadUInt16();
            ushort maxStorage            = reader.ReadUInt16();
            ushort maxFunctionDefs       = reader.ReadUInt16();
            ushort maxInstructionDefs    = reader.ReadUInt16();
            ushort maxStackElements      = reader.ReadUInt16();
            ushort maxSizeOfInstructions = reader.ReadUInt16();
            ushort maxComponentElements  = reader.ReadUInt16();
            ushort maxComponentDepth     = reader.ReadUInt16();

            return new TtfMaxpTable(
                rawBytes, version, numGlyphs,
                maxPoints, maxContours,
                maxComponentPoints, maxComponentContours,
                maxZones, maxTwilightPoints,
                maxStorage, maxFunctionDefs, maxInstructionDefs,
                maxStackElements, maxSizeOfInstructions,
                maxComponentElements, maxComponentDepth);
        }
    }
}
