using MiniPdf.Drawing.Font.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using SysEncoding = System.Text.Encoding;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "SVG " (Scalable Vector Graphics) table.
    /// Exposes per-glyph SVG document strings.
    /// Ref: OpenType spec §table-SVG.
    /// </summary>
    public sealed class TtfSvgTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the SVG table.
        /// </summary>
        public const string TableTag = "SVG ";

        // ── Internal record ───────────────────────────────────────────────────

        private struct SvgRecord
        {
            internal int  StartGlyphId;
            internal int  EndGlyphId;
            /// <summary>Absolute offset into <see cref="TtfTableBase.RawBytes"/> of the SVG document data.</summary>
            internal int  DataOffset;
            internal int  DataLength;
        }

        private readonly SvgRecord[] _records;

        private TtfSvgTable(byte[] rawBytes, SvgRecord[] records)
            : base(TableTag, rawBytes)
        {
            _records = records;
        }

        // ── Properties ────────────────────────────────────────────────────────

        /// <summary>Number of SVG document records in the table.</summary>
        public int DocumentCount => _records.Length;

        // ── Lookup ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the SVG document string for <paramref name="glyphIndex"/>, or
        /// <see langword="null"/> if no document covers that glyph.
        /// </summary>
        /// <remarks>
        /// Documents may cover a range of glyph IDs that share the same SVG.
        /// GZip-compressed documents (magic bytes 0x1F 0x8B) are decompressed
        /// automatically before being returned as a UTF-8 string.
        /// </remarks>
        public string? GetSvgDocument(int glyphIndex)
        {
            foreach (var rec in _records)
            {
                if (glyphIndex < rec.StartGlyphId || glyphIndex > rec.EndGlyphId)
                    continue;

                if (rec.DataOffset < 0 || rec.DataLength <= 0)
                    return null;
                if (rec.DataOffset + rec.DataLength > RawBytes.Length)
                    return null;

                byte[] data = new byte[rec.DataLength];
                Array.Copy(RawBytes, rec.DataOffset, data, 0, rec.DataLength);

                // GZip-compressed data starts with 0x1F 0x8B.
                if (data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B)
                {
                    data = Decompress(data);
                    if (data == null) return null;
                }

                return SysEncoding.UTF8.GetString(data);
            }
            return null;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        private static readonly TtfSvgTable _empty =
            new TtfSvgTable(Array.Empty<byte>(), Array.Empty<SvgRecord>());

        /// <summary>Parses the "SVG " table from its raw bytes.</summary>
        public static TtfSvgTable Parse(byte[] rawBytes)
        {
            if (rawBytes == null || rawBytes.Length < 10)
                return new TtfSvgTable(rawBytes ?? Array.Empty<byte>(), Array.Empty<SvgRecord>());

            try
            {
                return ParseCore(rawBytes);
            }
            catch
            {
                return new TtfSvgTable(rawBytes, Array.Empty<SvgRecord>());
            }
        }

        private static TtfSvgTable ParseCore(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            // SVG table header (10 bytes):
            //   version(2) + offsetToSVGDocumentList(4) + reserved(4)
            ushort version            = reader.ReadUInt16();
            uint   docListOffset      = reader.ReadUInt32();
            uint   reserved           = reader.ReadUInt32();

            if (docListOffset + 2 > (uint)rawBytes.Length)
                return new TtfSvgTable(rawBytes, Array.Empty<SvgRecord>());

            // SVGDocumentList starts at docListOffset from the table start.
            reader.Seek(docListOffset);
            ushort numEntries = reader.ReadUInt16();

            if (numEntries == 0)
                return new TtfSvgTable(rawBytes, Array.Empty<SvgRecord>());

            // Each SVGDocumentRecord is 12 bytes:
            //   startGlyphID(2) + endGlyphID(2) + svgDocOffset(4) + svgDocLength(4)
            // svgDocOffset is relative to the start of the SVGDocumentList.
            long recordsBase   = docListOffset + 2L;
            long docListBase   = docListOffset;

            var records = new List<SvgRecord>(numEntries);
            for (int i = 0; i < numEntries; i++)
            {
                long recPos = recordsBase + i * 12L;
                if (recPos + 12 > rawBytes.Length) break;

                reader.Seek(recPos);
                ushort startGlyphId = reader.ReadUInt16();
                ushort endGlyphId   = reader.ReadUInt16();
                uint   svgDocOff    = reader.ReadUInt32();
                uint   svgDocLen    = reader.ReadUInt32();

                if (svgDocLen == 0) continue;

                // svgDocOff is relative to the start of SVGDocumentList.
                long absDocOffset = docListBase + svgDocOff;
                if (absDocOffset + svgDocLen > rawBytes.Length) continue;

                records.Add(new SvgRecord
                {
                    StartGlyphId = startGlyphId,
                    EndGlyphId   = endGlyphId,
                    DataOffset   = (int)absDocOffset,
                    DataLength   = (int)svgDocLen
                });
            }

            return new TtfSvgTable(rawBytes, records.ToArray());
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static byte[]? Decompress(byte[] compressed)
        {
            try
            {
                using var input  = new MemoryStream(compressed);
                using var gz     = new GZipStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                gz.CopyTo(output);
                return output.ToArray();
            }
            catch
            {
                return null;
            }
        }
    }
}
