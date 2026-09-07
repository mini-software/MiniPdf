using MiniPdf.Drawing.Font.IO;
using System;
using System.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "gvar" (glyph variations) table header.
    /// Exposes per-glyph raw tuple variation data; full interpolation is deferred.
    /// Ref: OpenType spec §table-gvar.
    /// </summary>
    public sealed class TtfGvarTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the gvar table.
        /// </summary>
        public const string TableTag = "gvar";

        /// <summary>Major version of the gvar table (should be 1).</summary>
        public int MajorVersion { get; }

        /// <summary>Minor version of the gvar table (should be 0).</summary>
        public int MinorVersion { get; }

        /// <summary>Number of variation axes (matches fvar axis count).</summary>
        public int AxisCount { get; }

        /// <summary>Number of shared tuple records in the shared tuple table.</summary>
        public int SharedTupleCount { get; }

        /// <summary>Number of glyphs in the font (matches maxp.numGlyphs).</summary>
        public int GlyphCount { get; }

        // Raw per-glyph variation data blobs.
        private readonly byte[][] _glyphVariations;

        private TtfGvarTable(
            byte[] rawBytes,
            int majorVersion, int minorVersion,
            int axisCount, int sharedTupleCount, int glyphCount,
            byte[][] glyphVariations)
            : base(TableTag, rawBytes)
        {
            MajorVersion     = majorVersion;
            MinorVersion     = minorVersion;
            AxisCount        = axisCount;
            SharedTupleCount = sharedTupleCount;
            GlyphCount       = glyphCount;
            _glyphVariations = glyphVariations;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the raw tuple variation data blob for the given glyph, or an
        /// empty array if the glyph has no variation data.  Returns null if the
        /// index is out of range.
        /// </summary>
        public byte[]? GetGlyphVariationData(int glyphIndex)
        {
            if (glyphIndex < 0 || glyphIndex >= _glyphVariations.Length)
                return null;
            return _glyphVariations[glyphIndex];
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "gvar" table from its raw bytes.</summary>
        public static TtfGvarTable Parse(byte[] rawBytes)
        {
            // gvar header is 20 bytes minimum.
            if (rawBytes == null || rawBytes.Length < 20)
                return Empty(rawBytes ?? Array.Empty<byte>());

            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            // Header (20 bytes):
            //   majorVersion(2) + minorVersion(2) + axisCount(2) + sharedTupleCount(2)
            //   + offsetToSharedTuples(4) + glyphCount(2) + flags(2)
            //   + offsetToGlyphVariationData(4)
            int  majorVersion            = reader.ReadUInt16();
            int  minorVersion            = reader.ReadUInt16();
            int  axisCount               = reader.ReadUInt16();
            int  sharedTupleCount        = reader.ReadUInt16();
            uint offsetToSharedTuples    = reader.ReadUInt32();
            int  glyphCount              = reader.ReadUInt16();
            int  flags                   = reader.ReadUInt16();
            uint offsetToGlyphVariData   = reader.ReadUInt32();

            // flags bit 0: 0 = uint16 offsets (×2), 1 = uint32 offsets.
            bool longOffsets = (flags & 1) != 0;

            // glyphVariationDataOffsets[glyphCount + 1] immediately follows the header.
            int offsetCount   = glyphCount + 1;
            int bytesPerEntry = longOffsets ? 4 : 2;
            if (20L + (long)offsetCount * bytesPerEntry > rawBytes.Length)
                return Empty(rawBytes);

            uint[] glyphOffsets = new uint[offsetCount];

            if (longOffsets)
            {
                for (int i = 0; i < offsetCount; i++)
                    glyphOffsets[i] = reader.ReadUInt32();
            }
            else
            {
                for (int i = 0; i < offsetCount; i++)
                    glyphOffsets[i] = (uint)(reader.ReadUInt16() * 2);
            }

            // Extract per-glyph raw variation data.
            // offsets are relative to offsetToGlyphVariationData (from table start).
            var glyphVariations = new byte[glyphCount][];
            for (int i = 0; i < glyphCount; i++)
            {
                uint start = glyphOffsets[i];
                uint end   = glyphOffsets[i + 1];

                if (start == end)
                {
                    glyphVariations[i] = Array.Empty<byte>();
                    continue;
                }

                long absStart = (long)offsetToGlyphVariData + start;
                long length   = end - start;

                if (absStart < 0 || absStart + length > rawBytes.Length)
                {
                    glyphVariations[i] = Array.Empty<byte>();
                    continue;
                }

                byte[] data = new byte[length];
                Array.Copy(rawBytes, absStart, data, 0, length);
                glyphVariations[i] = data;
            }

            return new TtfGvarTable(rawBytes, majorVersion, minorVersion,
                axisCount, sharedTupleCount, glyphCount, glyphVariations);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static TtfGvarTable Empty(byte[] rawBytes) =>
            new TtfGvarTable(rawBytes, 0, 0, 0, 0, 0, Array.Empty<byte[]>());
    }
}
