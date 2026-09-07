using MiniSoftware.Drawing.Font.IO;
using System.Collections.Generic;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "cmap" table (character-to-glyph-index mapping).
    /// Reads all encoding records from the table directory and parses the
    /// unique subtables; exposes <see cref="FindUnicodeTable"/> to select the
    /// best subtable for Unicode text shaping.
    /// Ref: OpenType spec §5.6.
    /// </summary>
    public sealed class TtfCMapTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the cmap table.
        /// </summary>
        public const string TableTag = "cmap";

        private readonly IReadOnlyList<TtfCMapFormatBaseTable> _subtables;

        private TtfCMapTable(byte[] rawBytes, IReadOnlyList<TtfCMapFormatBaseTable> subtables)
            : base(TableTag, rawBytes)
        {
            _subtables = subtables;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "cmap" table from its raw bytes.</summary>
        public static TtfCMapTable Parse(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            reader.Skip(2);                     // version (USHORT, should be 0)
            ushort numTables = reader.ReadUInt16();

            // Read encoding records.
            var encodingRecords = new (ushort platformId, ushort encodingId, uint offset)[numTables];
            for (int i = 0; i < numTables; i++)
            {
                ushort platformId = reader.ReadUInt16();
                ushort encodingId = reader.ReadUInt16();
                uint   offset     = reader.ReadUInt32();
                encodingRecords[i] = (platformId, encodingId, offset);
            }

            // Parse unique subtables (multiple encoding records may share the same subtable).
            var seenOffsets = new HashSet<uint>();
            var subtables   = new List<TtfCMapFormatBaseTable>();

            foreach (var rec in encodingRecords)
            {
                if (!seenOffsets.Add(rec.offset)) continue;
                if (rec.offset + 2 > (uint)rawBytes.Length) continue;

                var subtable = ParseSubtable(rawBytes, (int)rec.offset, rec.platformId, rec.encodingId);
                if (subtable != null)
                    subtables.Add(subtable);
            }

            return new TtfCMapTable(rawBytes, subtables);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>All parsed CMap subtables.</summary>
        public IReadOnlyList<TtfCMapFormatBaseTable> Subtables => _subtables;

        /// <summary>
        /// Finds the best Unicode-capable subtable.
        /// Priority order:
        /// <list type="number">
        ///   <item>Format 12, platform 0 or 3 (full Unicode coverage)</item>
        ///   <item>Format 12, any platform</item>
        ///   <item>Format 4,  platform 3 (Windows BMP)</item>
        ///   <item>Format 4,  platform 0 (Unicode BMP)</item>
        ///   <item>Format 4,  any platform</item>
        /// </list>
        /// Returns <c>null</c> if no usable Unicode subtable is found.
        /// </summary>
        public TtfCMapFormatBaseTable? FindUnicodeTable()
        {
            TtfCMapFormatBaseTable? best = null;
            int bestPriority = -1;

            foreach (var subtable in _subtables)
            {
                int priority = GetPriority(subtable);
                if (priority > bestPriority)
                {
                    best = subtable;
                    bestPriority = priority;
                }
            }

            return best;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static int GetPriority(TtfCMapFormatBaseTable t)
        {
            if (t.Format == 12 && (t.PlatformId == 0 || t.PlatformId == 3)) return 100;
            if (t.Format == 12)                                               return  90;
            if (t.Format == 4  && t.PlatformId == 3)                         return  80;
            if (t.Format == 4  && t.PlatformId == 0)                         return  70;
            if (t.Format == 4)                                                return  60;
            return -1; // formats 0, 6 etc. are never preferred over 4/12
        }

        private static TtfCMapFormatBaseTable? ParseSubtable(
            byte[] cmapBytes, int subtableOffset,
            ushort platformId, ushort encodingId)
        {
            if (subtableOffset < 0 || subtableOffset + 2 > cmapBytes.Length)
                return null;

            // Read the format field (big-endian ushort) from the raw bytes.
            ushort format = (ushort)((cmapBytes[subtableOffset] << 8) | cmapBytes[subtableOffset + 1]);

            switch (format)
            {
                case 0:
                    return TtfCMapFormat0Table.Parse(cmapBytes, subtableOffset, platformId, encodingId);
                case 4:
                    return TtfCMapFormat4Table.Parse(cmapBytes, subtableOffset, platformId, encodingId);
                case 12:
                    return TtfCMapFormat12Table.Parse(cmapBytes, subtableOffset, platformId, encodingId);
                default:
                    return null; // unsupported format; not an error
            }
        }
    }
}
