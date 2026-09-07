using MiniSoftware.Drawing.Font.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "GSUB" table header and feature tag list.
    /// Exposes <see cref="FeatureTags"/> (e.g. "liga", "calt", "dlig") so callers
    /// can detect which substitution features are available.
    /// Full substitution lookup application is deferred to a future batch.
    /// </summary>
    /// <remarks>
    /// Ref: https://learn.microsoft.com/en-us/typography/opentype/spec/gsub
    /// </remarks>
    public sealed class TtfGsubTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the GSUB table.
        /// </summary>
        public const string TableTag = "GSUB";

        private readonly IReadOnlyList<string> _featureTags;

        private TtfGsubTable(byte[] rawBytes, IReadOnlyList<string> featureTags)
            : base(TableTag, rawBytes)
        {
            _featureTags = featureTags;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Distinct, lexicographically sorted OpenType feature tags present in the
        /// GSUB table's FeatureList (e.g. "calt", "dlig", "liga", "rlig").
        /// May be empty for fonts with a malformed or absent FeatureList.
        /// </summary>
        public IReadOnlyList<string> FeatureTags => _featureTags;

        // ── Parser ────────────────────────────────────────────────────────────

        /// <summary>Parses the "GSUB" table from its raw bytes.</summary>
        public static TtfGsubTable Parse(byte[] rawBytes)
        {
            if (rawBytes is null) throw new ArgumentNullException(nameof(rawBytes));
            return new TtfGsubTable(rawBytes, ParseFeatureTags(rawBytes));
        }

        // ── Internal helper ───────────────────────────────────────────────────

        // Reads the FeatureList from a GSUB or GPOS table and returns the
        // distinct, sorted feature tags.
        internal static List<string> ParseFeatureTags(byte[] rawBytes)
        {
            if (rawBytes is null || rawBytes.Length < 10)
                return new List<string>();

            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            /* majorVersion */          reader.ReadUInt16(); // 0
            /* minorVersion */          reader.ReadUInt16(); // 2
            /* scriptListOffset */      reader.ReadUInt16(); // 4
            ushort featureListOffset  = reader.ReadUInt16(); // 6 — from table start

            if (featureListOffset == 0 || featureListOffset >= rawBytes.Length)
                return new List<string>();

            reader.Seek(featureListOffset);

            if (reader.Position + 2 > rawBytes.Length) return new List<string>();

            ushort featureCount = reader.ReadUInt16();

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < featureCount; i++)
            {
                if (reader.Position + 6 > rawBytes.Length) break;

                // FeatureRecord: featureTag(4 bytes) + featureOffset(2 bytes)
                string tag = reader.ReadTag().TrimEnd();
                reader.ReadUInt16(); // featureOffset — not needed

                if (!string.IsNullOrWhiteSpace(tag))
                    seen.Add(tag);
            }

            var list = new List<string>(seen);
            list.Sort(StringComparer.Ordinal);
            return list;
        }
    }
}
