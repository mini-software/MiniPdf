using MiniPdf.Drawing.Font.IO;
using System;
using System.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "CBLC" (Color Bitmap Location) table.
    /// Stores per-glyph offset information that is used together with "CBDT"
    /// to retrieve colour bitmap images.
    /// This type is an internal helper; callers use <see cref="TtfCbdtTable"/>.
    /// Ref: OpenType spec §table-CBLC.
    /// </summary>
    internal sealed class TtfCblcTable
    {
        internal const string TableTag = "CBLC";

        // ── Internal location result ──────────────────────────────────────────

        internal struct GlyphBitmapLocation
        {
            /// <summary>Byte offset from the start of the CBDT byte array.</summary>
            internal uint CbdtOffset;
            /// <summary>Number of bytes in the CBDT blob for this glyph.</summary>
            internal uint Length;
            /// <summary>CBDT image format (17 = SmallMetrics+PNG, 18 = BigMetrics+PNG, 19 = raw PNG).</summary>
            internal int ImageFormat;
        }

        // ── Internal data structures ──────────────────────────────────────────

        private struct SubTableEntry
        {
            internal int    FirstGlyphIndex;
            internal int    LastGlyphIndex;
            internal int    IndexFormat;   // 1, 2, or 3
            internal int    ImageFormat;
            internal uint   ImageDataOffset; // offset in CBDT
            // Formats 1 & 3: offset[glyphCount + 1] (relative to ImageDataOffset)
            internal uint[]? GlyphOffsets;
            // Format 2: constant image size
            internal uint   ImageSize;
        }

        private struct StrikeInfo
        {
            internal int StartGlyphIndex;
            internal int EndGlyphIndex;
            internal int PpemX;
            internal int PpemY;
            internal SubTableEntry[] SubTables;
        }

        private readonly StrikeInfo[] _strikes;

        private TtfCblcTable(StrikeInfo[] strikes) { _strikes = strikes; }

        // ── Properties ────────────────────────────────────────────────────────

        /// <summary>Number of bitmap strikes (one per ppem size) in this font.</summary>
        internal int StrikeCount => _strikes.Length;

        internal int GetStrikePpemX(int strikeIndex) =>
            strikeIndex >= 0 && strikeIndex < _strikes.Length ? _strikes[strikeIndex].PpemX : 0;

        internal int GetStrikePpemY(int strikeIndex) =>
            strikeIndex >= 0 && strikeIndex < _strikes.Length ? _strikes[strikeIndex].PpemY : 0;

        // ── Lookup ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the CBDT location for the given glyph in the given strike, or
        /// <see langword="null"/> if the glyph is not present in that strike.
        /// </summary>
        internal GlyphBitmapLocation? GetGlyphLocation(int glyphIndex, int strikeIndex)
        {
            if (strikeIndex < 0 || strikeIndex >= _strikes.Length) return null;

            var strike = _strikes[strikeIndex];
            if (glyphIndex < strike.StartGlyphIndex || glyphIndex > strike.EndGlyphIndex)
                return null;

            foreach (var sub in strike.SubTables)
            {
                if (glyphIndex < sub.FirstGlyphIndex || glyphIndex > sub.LastGlyphIndex)
                    continue;

                int idx = glyphIndex - sub.FirstGlyphIndex;

                if (sub.IndexFormat == 2)
                {
                    // All glyphs in this subtable have the same size.
                    uint start = sub.ImageDataOffset + (uint)idx * sub.ImageSize;
                    return new GlyphBitmapLocation
                    {
                        CbdtOffset  = start,
                        Length      = sub.ImageSize,
                        ImageFormat = sub.ImageFormat
                    };
                }

                if (sub.GlyphOffsets == null || idx + 1 >= sub.GlyphOffsets.Length)
                    return null;

                uint gStart = sub.GlyphOffsets[idx];
                uint gEnd   = sub.GlyphOffsets[idx + 1];
                if (gEnd <= gStart) return null;

                return new GlyphBitmapLocation
                {
                    CbdtOffset  = sub.ImageDataOffset + gStart,
                    Length      = gEnd - gStart,
                    ImageFormat = sub.ImageFormat
                };
            }
            return null;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        private static readonly TtfCblcTable _empty = new TtfCblcTable(Array.Empty<StrikeInfo>());

        internal static TtfCblcTable Parse(byte[] rawBytes)
        {
            if (rawBytes == null || rawBytes.Length < 8)
                return _empty;

            try
            {
                return ParseCore(rawBytes);
            }
            catch
            {
                return _empty;
            }
        }

        private static TtfCblcTable ParseCore(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            // CBLC header
            ushort majorVersion = reader.ReadUInt16();
            ushort minorVersion = reader.ReadUInt16();
            uint   numSizes     = reader.ReadUInt32();

            if (numSizes == 0 || numSizes > 256) return _empty;

            var strikes = new StrikeInfo[numSizes];

            // BitmapSize records start at offset 8 (immediately after the header).
            for (uint s = 0; s < numSizes; s++)
            {
                // Each BitmapSize record is 48 bytes.
                long bsBase = 8L + s * 48L;
                if (bsBase + 48 > rawBytes.Length) break;

                reader.Seek(bsBase);

                uint isaOffset            = reader.ReadUInt32(); // offset from CBLC start to IndexSubTableArray
                uint indexTablesSize      = reader.ReadUInt32();
                uint numberOfSubTables    = reader.ReadUInt32();
                uint colorRef             = reader.ReadUInt32(); // reserved

                // SbitLineMetrics hori (12 bytes) + vert (12 bytes) — skip
                reader.Skip(24);

                ushort startGlyph = reader.ReadUInt16();
                ushort endGlyph   = reader.ReadUInt16();
                byte   ppemX      = reader.ReadUInt8();
                byte   ppemY      = reader.ReadUInt8();
                byte   bitDepth   = reader.ReadUInt8();
                byte   flags      = reader.ReadUInt8();

                if (numberOfSubTables == 0 || numberOfSubTables > 4096) continue;

                var subTables = new SubTableEntry[numberOfSubTables];

                // IndexSubTableArray starts at isaOffset from the start of CBLC.
                long isaBase = isaOffset;
                if (isaBase + numberOfSubTables * 8L > rawBytes.Length) continue;

                for (uint st = 0; st < numberOfSubTables; st++)
                {
                    // Each IndexSubTableArray entry: firstGlyphIndex(2) + lastGlyphIndex(2) + additionalOffset(4)
                    long entryOff = isaBase + st * 8L;
                    reader.Seek(entryOff);

                    ushort firstG      = reader.ReadUInt16();
                    ushort lastG       = reader.ReadUInt16();
                    uint   addOffset   = reader.ReadUInt32();

                    // additionalOffset is relative to the start of the IndexSubTableArray.
                    long istBase = isaBase + addOffset;
                    if (istBase + 8 > rawBytes.Length) continue;

                    reader.Seek(istBase);

                    ushort indexFormat    = reader.ReadUInt16();
                    ushort imageFormat    = reader.ReadUInt16();
                    uint   imageDataOffset = reader.ReadUInt32();

                    int glyphCount = lastG - firstG + 1;
                    if (glyphCount <= 0 || glyphCount > 65536) continue;

                    var entry = new SubTableEntry
                    {
                        FirstGlyphIndex  = firstG,
                        LastGlyphIndex   = lastG,
                        IndexFormat      = indexFormat,
                        ImageFormat      = imageFormat,
                        ImageDataOffset  = imageDataOffset
                    };

                    if (indexFormat == 1)
                    {
                        // Format 1: variable-width images, uint32 offsets array
                        int offsetCount = glyphCount + 1;
                        long arrEnd = istBase + 8L + offsetCount * 4L;
                        if (arrEnd > rawBytes.Length) { subTables[st] = entry; continue; }

                        entry.GlyphOffsets = new uint[offsetCount];
                        for (int oi = 0; oi < offsetCount; oi++)
                            entry.GlyphOffsets[oi] = reader.ReadUInt32();
                    }
                    else if (indexFormat == 2)
                    {
                        // Format 2: constant-size images
                        if (istBase + 8 + 12 > rawBytes.Length) { subTables[st] = entry; continue; }
                        entry.ImageSize = reader.ReadUInt32();
                        // BigGlyphMetrics (8 bytes) — skip
                        reader.Skip(8);
                    }
                    else if (indexFormat == 3)
                    {
                        // Format 3: variable-width images, uint16 offsets array
                        int offsetCount = glyphCount + 1;
                        long arrEnd = istBase + 8L + offsetCount * 2L;
                        if (arrEnd > rawBytes.Length) { subTables[st] = entry; continue; }

                        entry.GlyphOffsets = new uint[offsetCount];
                        for (int oi = 0; oi < offsetCount; oi++)
                            entry.GlyphOffsets[oi] = reader.ReadUInt16();
                    }
                    // Formats 4/5 are not common; leave GlyphOffsets null (returns null on lookup).

                    subTables[st] = entry;
                }

                strikes[s] = new StrikeInfo
                {
                    StartGlyphIndex = startGlyph,
                    EndGlyphIndex   = endGlyph,
                    PpemX           = ppemX,
                    PpemY           = ppemY,
                    SubTables       = subTables
                };
            }

            return new TtfCblcTable(strikes);
        }
    }
}
