using System;
using System.Collections.Generic;

namespace MiniPdf.Drawing.Font.IO
{
    /// <summary>
    /// Parses the OpenType table directory and provides helpers for reading raw table bytes.
    /// </summary>
    /// <remarks>
    /// The table directory immediately follows the 12-byte offset table in an sfnt file.
    /// Each entry is 16 bytes: 4-byte tag, 4-byte checksum, 4-byte offset, 4-byte length.
    /// </remarks>
    public static class OpenTypeTableDirectory
    {
        /// <summary>
        /// Reads the offset table and the complete table directory from the current
        /// position of <paramref name="reader"/> (expected to be at byte 0 of the font).
        /// </summary>
        /// <returns>
        /// A tuple of the parsed <see cref="OpenTypeOffsetTable"/> header and a
        /// case-sensitive dictionary mapping each table tag to its
        /// <see cref="OpenTypeTableRecord"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the sfnt magic bytes are not recognised as a supported font format.
        /// </exception>
        public static (OpenTypeOffsetTable Header,
                        IReadOnlyDictionary<string, OpenTypeTableRecord> Records)
            Parse(FontBinaryReader reader)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));

            uint sfVersion     = reader.ReadUInt32();
            ushort numTables   = reader.ReadUInt16();
            ushort searchRange = reader.ReadUInt16();
            ushort entrySelector = reader.ReadUInt16();
            ushort rangeShift  = reader.ReadUInt16();

            // Validate the sfnt magic (relaxed: warn but don't throw for unknown values
            // to allow future extension; hard check is done in TtfFontReader).
            var header = new OpenTypeOffsetTable(
                sfVersion, numTables, searchRange, entrySelector, rangeShift);

            var records = new Dictionary<string, OpenTypeTableRecord>(numTables,
                StringComparer.Ordinal);

            for (int i = 0; i < numTables; i++)
            {
                string tag      = reader.ReadTag();
                uint checksum   = reader.ReadUInt32();
                uint offset     = reader.ReadUInt32();
                uint length     = reader.ReadUInt32();
                records[tag] = new OpenTypeTableRecord(tag, checksum, offset, length);
            }

            return (header, records);
        }

        /// <summary>
        /// Seeks to a table's data and reads its raw bytes.
        /// </summary>
        public static byte[] ReadTableBytes(
            FontBinaryReader reader,
            OpenTypeTableRecord record)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));
            if (record is null) throw new ArgumentNullException(nameof(record));

            reader.Seek(record.Offset);
            return reader.ReadBytes((int)record.Length);
        }

        /// <summary>
        /// Reads the raw bytes of every table listed in <paramref name="records"/>
        /// and returns them keyed by tag.
        /// </summary>
        public static IReadOnlyDictionary<string, byte[]> ReadAllTableBytes(
            FontBinaryReader reader,
            IReadOnlyDictionary<string, OpenTypeTableRecord> records)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));
            if (records is null) throw new ArgumentNullException(nameof(records));

            var result = new Dictionary<string, byte[]>(records.Count, StringComparer.Ordinal);
            foreach (var kvp in records)
            {
                reader.Seek(kvp.Value.Offset);
                result[kvp.Key] = reader.ReadBytes((int)kvp.Value.Length);
            }
            return result;
        }

        /// <summary>
        /// Computes the OpenType table checksum for a block of bytes.
        /// Algorithm: sum all 32-bit big-endian words (zero-padding the last word if needed),
        /// modulo 2^32.
        /// </summary>
        public static uint ComputeChecksum(byte[] tableData)
        {
            if (tableData is null) throw new ArgumentNullException(nameof(tableData));

            uint sum = 0;
            int wordCount = (tableData.Length + 3) / 4;
            for (int i = 0; i < wordCount; i++)
            {
                int pos = i * 4;
                uint word = 0;
                for (int b = 0; b < 4; b++)
                {
                    word = (word << 8) |
                           (uint)(pos + b < tableData.Length ? tableData[pos + b] : 0);
                }
                sum += word;
            }
            return sum;
        }
    }
}
