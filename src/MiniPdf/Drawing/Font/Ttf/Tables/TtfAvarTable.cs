using MiniSoftware.Drawing.Font.IO;
using System;
using System.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "avar" (axis variations) table.
    /// Stores piecewise-linear segment maps that remap default-normalized axis
    /// values to final normalized values.
    /// Ref: OpenType spec §table-avar.
    /// </summary>
    public sealed class TtfAvarTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the avar table.
        /// </summary>
        public const string TableTag = "avar";

        // ── Internal structures ───────────────────────────────────────────────

        private readonly struct AxisValueMap
        {
            public readonly short From; // F2Dot14: signed, /16384.0 → [-2, ~2)
            public readonly short To;
            public AxisValueMap(short from, short to) { From = from; To = to; }
        }

        /// <summary>Per-axis segment maps (indexed in fvar axis order).</summary>
        private readonly AxisValueMap[]?[] _segments;

        private TtfAvarTable(byte[] rawBytes, AxisValueMap[]?[] segments)
            : base(TableTag, rawBytes)
        {
            _segments = segments;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Number of segment maps (one per variation axis, in fvar order).</summary>
        public int SegmentCount => _segments.Length;

        /// <summary>
        /// Applies the avar piecewise-linear segment map for the given axis index to
        /// <paramref name="normalizedValue"/> (a default-normalized value in [-1, 1]).
        /// Returns <paramref name="normalizedValue"/> unchanged if the axis index is
        /// out of range or no segment map is defined for it.
        /// </summary>
        public double ApplySegmentMap(int axisIndex, double normalizedValue)
        {
            if (axisIndex < 0 || axisIndex >= _segments.Length)
                return normalizedValue;

            AxisValueMap[]? maps = _segments[axisIndex];
            if (maps == null || maps.Length == 0)
                return normalizedValue;

            return Interpolate(maps, normalizedValue);
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "avar" table from its raw bytes.</summary>
        public static TtfAvarTable Parse(byte[] rawBytes)
        {
            // avar v1 header: majorVersion(2) + minorVersion(2) + reserved(2) + segmentCount(2) = 8 bytes
            if (rawBytes == null || rawBytes.Length < 8)
                return Empty(rawBytes ?? Array.Empty<byte>());

            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            ushort majorVersion  = reader.ReadUInt16();
            ushort minorVersion  = reader.ReadUInt16();
            ushort reserved      = reader.ReadUInt16();
            ushort segmentCount  = reader.ReadUInt16();

            // Only v1 is handled; v2 adds a second lookup table after the v1 data.
            // For v2, we still parse the v1 segment maps.
            if (majorVersion != 1 && majorVersion != 2)
                return Empty(rawBytes);

            var segments = new AxisValueMap[segmentCount][];
            for (int s = 0; s < segmentCount; s++)
            {
                if (reader.Position + 2 > reader.Length)
                    break;

                ushort pairCount = reader.ReadUInt16();
                if (reader.Position + (long)pairCount * 4 > reader.Length)
                    break;

                var maps = new AxisValueMap[pairCount];
                for (int p = 0; p < pairCount; p++)
                {
                    short from = reader.ReadInt16();
                    short to   = reader.ReadInt16();
                    maps[p] = new AxisValueMap(from, to);
                }
                segments[s] = maps;
            }

            // Fill any segments not yet parsed (corrupt table) with empty arrays.
            for (int s = 0; s < segmentCount; s++)
                if (segments[s] == null)
                    segments[s] = Array.Empty<AxisValueMap>();

            return new TtfAvarTable(rawBytes, (AxisValueMap[]?[])segments);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static TtfAvarTable Empty(byte[] rawBytes) =>
            new TtfAvarTable(rawBytes, Array.Empty<AxisValueMap[]?>());

        /// <summary>
        /// Applies piecewise linear interpolation using F2Dot14-encoded segment maps.
        /// The input and output are floating-point values in [-1.0, 1.0].
        /// </summary>
        private static double Interpolate(AxisValueMap[] maps, double value)
        {
            int n = maps.Length;
            if (n == 0) return value;

            double fromFirst = maps[0].From / 16384.0;
            double fromLast  = maps[n - 1].From / 16384.0;

            // Clamp to the mapped range.
            if (value <= fromFirst) return maps[0].To / 16384.0;
            if (value >= fromLast)  return maps[n - 1].To / 16384.0;

            // Binary search for the segment containing value.
            int lo = 0, hi = n - 2;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (maps[mid].From / 16384.0 <= value)
                    lo = mid;
                else
                    hi = mid - 1;
            }

            double f0 = maps[lo].From     / 16384.0;
            double f1 = maps[lo + 1].From / 16384.0;
            double t0 = maps[lo].To       / 16384.0;
            double t1 = maps[lo + 1].To   / 16384.0;

            // Linear interpolation within the segment.
            double frac = (value - f0) / (f1 - f0);
            return t0 + frac * (t1 - t0);
        }
    }
}
