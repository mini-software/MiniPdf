namespace MiniSoftware.Drawing.Font.IO
{
    /// <summary>
    /// The OpenType offset table (the first 12 bytes of every sfnt-based font file).
    /// It describes the font's table directory and is used to locate each font table.
    /// </summary>
    public sealed class OpenTypeOffsetTable
    {
        // ── Well-known sfVersion magic values ────────────────────────────────

        /// <summary>0x00010000 — Standard TrueType / OpenType with TrueType outlines.</summary>
        public const uint SfVersionTrueType = 0x00010000u;

        /// <summary>0x4F54544F — "OTTO" — OpenType font with CFF/Type 2 outlines.</summary>
        public const uint SfVersionCff = 0x4F54544Fu;

        /// <summary>0x74727565 — "true" — Apple TrueType.</summary>
        public const uint SfVersionAppleTrue = 0x74727565u;

        /// <summary>0x74797031 — "typ1" — Legacy Apple Type 1 wrapper (rare).</summary>
        public const uint SfVersionAppleTyp1 = 0x74797031u;

        // ── Fields ───────────────────────────────────────────────────────────

        /// <summary>
        /// The sfnt version, also called the 'sfVersion' or 'sfntVersion' field.
        /// Compare against the <c>SfVersion*</c> constants above.
        /// </summary>
        public uint SfVersion { get; }

        /// <summary>Number of tables in this font.</summary>
        public ushort NumTables { get; }

        /// <summary>
        /// (Maximum power of 2 ≤ NumTables) × 16.
        /// Pre-computed binary search value; informational only.
        /// </summary>
        public ushort SearchRange { get; }

        /// <summary>
        /// Log₂ of the maximum power of 2 ≤ NumTables.
        /// Pre-computed binary search value; informational only.
        /// </summary>
        public ushort EntrySelector { get; }

        /// <summary>
        /// NumTables × 16 − SearchRange.
        /// Pre-computed binary search value; informational only.
        /// </summary>
        public ushort RangeShift { get; }

        // ── Derived helpers ──────────────────────────────────────────────────

        /// <summary>True when the sfnt container holds TrueType outlines (glyf table).</summary>
        public bool IsTrueType =>
            SfVersion == SfVersionTrueType || SfVersion == SfVersionAppleTrue;

        /// <summary>True when the sfnt container holds CFF (Type 2 charstring) outlines.</summary>
        public bool IsCff => SfVersion == SfVersionCff;

        /// <summary>
        /// Initializes a new <see cref="OpenTypeOffsetTable"/> with the specified values.
        /// </summary>
        /// <param name="sfVersion">The sfnt version.</param>
        /// <param name="numTables">Number of tables in this font.</param>
        /// <param name="searchRange">Pre-computed binary search value.</param>
        /// <param name="entrySelector">Log₂ of the maximum power of 2 ≤ NumTables.</param>
        /// <param name="rangeShift">NumTables × 16 − SearchRange.</param>
        public OpenTypeOffsetTable(
            uint sfVersion,
            ushort numTables,
            ushort searchRange,
            ushort entrySelector,
            ushort rangeShift)
        {
            SfVersion    = sfVersion;
            NumTables    = numTables;
            SearchRange  = searchRange;
            EntrySelector = entrySelector;
            RangeShift   = rangeShift;
        }

        /// <summary>
        /// Returns a string representation of this offset table.
        /// </summary>
        public override string ToString() =>
            $"OffsetTable(sfVersion=0x{SfVersion:X8}, numTables={NumTables}, isCff={IsCff})";
    }
}
