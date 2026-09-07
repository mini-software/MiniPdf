using System;
using System.Collections.Generic;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "GPOS" table header and feature tag list.
    /// Exposes <see cref="FeatureTags"/> (e.g. "kern", "mark", "mkmk") so callers
    /// can detect which positioning features are available.
    /// Full positioning lookup application is deferred to a future batch.
    /// </summary>
    /// <remarks>
    /// Ref: https://learn.microsoft.com/en-us/typography/opentype/spec/gpos
    /// </remarks>
    public sealed class TtfGposTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the GPOS table.
        /// </summary>
        public const string TableTag = "GPOS";

        private readonly IReadOnlyList<string> _featureTags;

        private TtfGposTable(byte[] rawBytes, IReadOnlyList<string> featureTags)
            : base(TableTag, rawBytes)
        {
            _featureTags = featureTags;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Distinct, lexicographically sorted OpenType feature tags present in the
        /// GPOS table's FeatureList (e.g. "cpsp", "kern", "mark", "mkmk").
        /// May be empty for fonts with a malformed or absent FeatureList.
        /// </summary>
        public IReadOnlyList<string> FeatureTags => _featureTags;

        // ── Parser ────────────────────────────────────────────────────────────

        /// <summary>Parses the "GPOS" table from its raw bytes.</summary>
        public static TtfGposTable Parse(byte[] rawBytes)
        {
            if (rawBytes is null) throw new ArgumentNullException(nameof(rawBytes));
            return new TtfGposTable(rawBytes, TtfGsubTable.ParseFeatureTags(rawBytes));
        }
    }
}
