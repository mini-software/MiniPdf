using System;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "CBDT" (Color Bitmap Data) table.
    /// Exposes colour bitmap images (typically PNG) for individual glyphs.
    /// Requires the companion "CBLC" table to be parsed first; pass a
    /// <see cref="TtfCblcTable"/> to <see cref="Parse"/>.
    /// Ref: OpenType spec §table-CBDT.
    /// </summary>
    public sealed class TtfCbdtTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the CBDT table.
        /// </summary>
        public const string TableTag = "CBDT";

        private readonly TtfCblcTable _cblc;

        /// <summary>
        /// Raw bytes of the companion "CBLC" table, needed when re-serialising the font.
        /// </summary>
        internal byte[] CblcRawBytes { get; }

        private TtfCbdtTable(byte[] rawBytes, byte[] cblcRawBytes, TtfCblcTable cblc)
            : base(TableTag, rawBytes)
        {
            CblcRawBytes = cblcRawBytes ?? throw new ArgumentNullException(nameof(cblcRawBytes));
            _cblc = cblc;
        }

        // ── Properties ────────────────────────────────────────────────────────

        /// <summary>Number of bitmap strikes exposed by the CBLC companion table.</summary>
        public int StrikeCount => _cblc.StrikeCount;

        /// <summary>Pixels-per-em (X axis) for the given strike, or 0 when out of range.</summary>
        public int GetStrikePpemX(int strikeIndex) => _cblc.GetStrikePpemX(strikeIndex);

        /// <summary>Pixels-per-em (Y axis) for the given strike, or 0 when out of range.</summary>
        public int GetStrikePpemY(int strikeIndex) => _cblc.GetStrikePpemY(strikeIndex);

        // ── Lookup ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the raw image bytes (PNG) for <paramref name="glyphIndex"/> in
        /// <paramref name="strikeIndex"/>, or <see langword="null"/> when the glyph
        /// is absent from that strike.
        /// </summary>
        /// <remarks>
        /// The metrics header prepended by CBDT image formats 17/18/19 is stripped;
        /// the returned bytes always begin with the raw PNG signature when the
        /// underlying image is encoded as PNG.
        /// </remarks>
        public byte[]? GetGlyphBitmap(int glyphIndex, int strikeIndex = 0)
        {
            var loc = _cblc.GetGlyphLocation(glyphIndex, strikeIndex);
            if (loc == null) return null;

            var location = loc.Value;
            if (location.Length == 0) return null;

            int blobStart = (int)location.CbdtOffset;
            int blobLen   = (int)location.Length;

            if (blobStart < 0 || (long)blobStart + blobLen > RawBytes.Length) return null;

            // Strip the per-format metrics/length header so callers always receive
            // the raw image data (PNG signature at byte 0).
            //
            //  Format 17: SmallGlyphMetrics (5) + dataLen (4) = 9 bytes
            //  Format 18: BigGlyphMetrics   (8) + dataLen (4) = 12 bytes
            //  Format 19: dataLen (4)                         =  4 bytes
            int skip = location.ImageFormat switch
            {
                17 => 9,
                18 => 12,
                19 => 4,
                _  => 0
            };

            if (skip >= blobLen) return null;

            int dataLen = blobLen - skip;
            var result  = new byte[dataLen];
            Array.Copy(RawBytes, blobStart + skip, result, 0, dataLen);
            return result;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Parses the "CBDT" table together with a pre-parsed
        /// <paramref name="cblcTable"/>.
        /// </summary>
        /// <param name="rawBytes">Raw bytes of the CBDT table.</param>
        /// <param name="cblcRawBytes">Raw bytes of the companion CBLC table (stored for serialisation).</param>
        /// <param name="cblcTable">Pre-parsed CBLC companion table.</param>
        internal static TtfCbdtTable Parse(byte[] rawBytes, byte[] cblcRawBytes, TtfCblcTable cblcTable)
        {
            if (rawBytes      == null) rawBytes      = Array.Empty<byte>();
            if (cblcRawBytes  == null) cblcRawBytes  = Array.Empty<byte>();
            if (cblcTable     == null) throw new ArgumentNullException(nameof(cblcTable));
            return new TtfCbdtTable(rawBytes, cblcRawBytes, cblcTable);
        }
    }
}
