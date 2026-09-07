using MiniPdf.Drawing.Font.IO;
using System.Collections.Generic;
using System.IO;
using SysEncoding = System.Text.Encoding;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "name" table.
    /// Exposes the most common font name strings, preferring Windows/Unicode
    /// platform records (platformID=3, encodingID=1, languageID=0x0409) over
    /// Unicode-BMP (platformID=0) and Macintosh (platformID=1) records.
    /// Ref: OpenType spec §5.2.9.
    /// </summary>
    public sealed class TtfNameTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the name table.
        /// </summary>
        public const string TableTag = "name";

        // ── Well-known name IDs ───────────────────────────────────────────────

        private const ushort NameIdFamily      = 1;
        private const ushort NameIdStyle       = 2;
        private const ushort NameIdUniqueId    = 3;
        private const ushort NameIdFullName    = 4;
        private const ushort NameIdVersion     = 5;
        private const ushort NameIdPostScript  = 6;

        // ── Exposed strings ───────────────────────────────────────────────────

        /// <summary>Font family name (nameID 1), e.g. "Montserrat".</summary>
        public string? FamilyName { get; }

        /// <summary>Font subfamily / style name (nameID 2), e.g. "Regular".</summary>
        public string? StyleName { get; }

        /// <summary>Unique font identifier string (nameID 3).</summary>
        public string? UniqueId { get; }

        /// <summary>Full human-readable font name (nameID 4), e.g. "Montserrat Regular".</summary>
        public string? FullName { get; }

        /// <summary>Font version string (nameID 5).</summary>
        public string? Version { get; }

        /// <summary>PostScript font name (nameID 6), e.g. "Montserrat-Regular".</summary>
        public string? PostScriptName { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfNameTable(
            byte[] rawBytes,
            string? familyName, string? styleName, string? uniqueId,
            string? fullName, string? version, string? postScriptName)
            : base(TableTag, rawBytes)
        {
            FamilyName     = familyName;
            StyleName      = styleName;
            UniqueId       = uniqueId;
            FullName       = fullName;
            Version        = version;
            PostScriptName = postScriptName;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "name" table from its raw bytes.</summary>
        public static TtfNameTable Parse(byte[] rawBytes)
        {
            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            ushort format       = reader.ReadUInt16();
            ushort count        = reader.ReadUInt16();
            ushort stringOffset = reader.ReadUInt16();

            // Read all name records (each 12 bytes).
            var records = new (ushort platformId, ushort encodingId, ushort languageId,
                               ushort nameId, ushort length, ushort offset)[count];

            for (int i = 0; i < count; i++)
            {
                ushort platformId = reader.ReadUInt16();
                ushort encodingId = reader.ReadUInt16();
                ushort languageId = reader.ReadUInt16();
                ushort nameId     = reader.ReadUInt16();
                ushort length     = reader.ReadUInt16();
                ushort offset     = reader.ReadUInt16();
                records[i] = (platformId, encodingId, languageId, nameId, length, offset);
            }

            // For each nameID keep the record with the highest priority encoding.
            var best = new Dictionary<ushort, (int priority, string str)>();

            foreach (var rec in records)
            {
                int priority = GetPriority(rec.platformId, rec.encodingId, rec.languageId);
                if (priority < 0) continue;

                string str = DecodeString(rawBytes, stringOffset + rec.offset,
                                          rec.length, rec.platformId, rec.encodingId);

                if (!best.TryGetValue(rec.nameId, out var existing) ||
                    priority > existing.priority)
                {
                    best[rec.nameId] = (priority, str);
                }
            }

            string? Get(ushort nameId) =>
                best.TryGetValue(nameId, out var v) ? v.str : null;

            return new TtfNameTable(
                rawBytes,
                Get(NameIdFamily),
                Get(NameIdStyle),
                Get(NameIdUniqueId),
                Get(NameIdFullName),
                Get(NameIdVersion),
                Get(NameIdPostScript));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a priority score for a name record.  Higher = preferred.
        /// Returns -1 for unsupported platform/encoding combinations.
        /// </summary>
        private static int GetPriority(ushort platformId, ushort encodingId, ushort languageId)
        {
            switch (platformId)
            {
                case 3: // Windows
                    if (encodingId == 1 && languageId == 0x0409) return 100; // en-US
                    if (encodingId == 1)                          return 90;  // other language
                    return 80;                                                 // other encoding

                case 0: // Unicode BMP / full-repertoire
                    return 70;

                case 1: // Macintosh
                    return 60;

                default:
                    return -1; // skip
            }
        }

        /// <summary>
        /// Decodes a name string from the raw table byte array.
        /// Platform 3 and platform 0 use UTF-16BE; platform 1 (Macintosh) is
        /// treated as Latin-1 (adequate for ASCII-range font names).
        /// </summary>
        private static string DecodeString(byte[] data, int offset, int length,
                                           ushort platformId, ushort encodingId)
        {
            if (length == 0 || offset + length > data.Length)
                return string.Empty;

            // Platforms 0 (Unicode) and 3 (Windows) → UTF-16 big-endian.
            if (platformId == 0 || platformId == 3)
                return SysEncoding.BigEndianUnicode.GetString(data, offset, length);

            // Platform 1 (Macintosh) → Latin-1 approximation (good for ASCII names).
            var chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = (char)data[offset + i];
            return new string(chars);
        }
    }
}
