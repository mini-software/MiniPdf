namespace MiniPdf.Drawing.Font.IO
{
    /// <summary>
    /// One entry in the OpenType table directory: identifies a table by its four-character
    /// tag and records its checksum, absolute byte offset, and byte length within the font file.
    /// </summary>
    public sealed class OpenTypeTableRecord
    {
        /// <summary>
        /// Four-character ASCII identifier, e.g. "head", "cmap", "glyf", "CFF ".
        /// </summary>
        public string Tag { get; }

        /// <summary>
        /// Checksum of the table data as defined in the OpenType specification.
        /// (Sum of all uint32 values in the table, modulo 2^32.)
        /// </summary>
        public uint Checksum { get; }

        /// <summary>Absolute byte offset of the table data from the beginning of the font file.</summary>
        public uint Offset { get; }

        /// <summary>Length of the table data in bytes (before any 4-byte padding).</summary>
        public uint Length { get; }

        /// <summary>
        /// Initializes a new <see cref="OpenTypeTableRecord"/> with the specified values.
        /// </summary>
        /// <param name="tag">Four-character ASCII identifier.</param>
        /// <param name="checksum">Checksum of the table data.</param>
        /// <param name="offset">Absolute byte offset of the table data.</param>
        /// <param name="length">Length of the table data in bytes.</param>
        public OpenTypeTableRecord(string tag, uint checksum, uint offset, uint length)
        {
            Tag      = tag;
            Checksum = checksum;
            Offset   = offset;
            Length   = length;
        }

        /// <summary>
        /// Returns a string representation of this table record.
        /// </summary>
        public override string ToString() =>
            $"[{Tag}] offset=0x{Offset:X6} length={Length} checksum=0x{Checksum:X8}";
    }
}
