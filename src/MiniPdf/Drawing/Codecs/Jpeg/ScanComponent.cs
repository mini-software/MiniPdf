namespace MiniPdf.Drawing.Codecs.Jpeg
{
    /// <summary>
    /// Per-component selector inside a JPEG SOS (Start-of-Scan) header.
    /// </summary>
    internal struct ScanComponent
    {
        /// <summary>Component identifier (matches a <see cref="FrameComponent.Id"/>).</summary>
        public byte ComponentId;

        /// <summary>DC Huffman table id (0–3).</summary>
        public byte DcTableId;

        /// <summary>AC Huffman table id (0–3).</summary>
        public byte AcTableId;
    }
}
