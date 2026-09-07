using System;
using System.Collections.Generic;
using System.IO;
using MiniSoftware.Drawing.Font.Core;
using MiniSoftware.Drawing.Font.IO;

namespace MiniSoftware.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "fvar" (font variations) table.
    /// Exposes the variation axes and named instances of a variable font.
    /// Ref: OpenType spec §table-fvar.
    /// </summary>
    public sealed class TtfFvarTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the fvar table.
        /// </summary>
        public const string TableTag = "fvar";

        /// <summary>Design-variation axes defined in the font.</summary>
        public IReadOnlyList<FontVariationAxis> Axes { get; }

        /// <summary>Named design instances (e.g. "Light", "Bold Condensed").</summary>
        public IReadOnlyList<FontVariationInstance> Instances { get; }

        private TtfFvarTable(
            byte[] rawBytes,
            List<FontVariationAxis> axes,
            List<FontVariationInstance> instances)
            : base(TableTag, rawBytes)
        {
            Axes      = axes;
            Instances = instances;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>Parses the "fvar" table from its raw bytes.</summary>
        public static TtfFvarTable Parse(byte[] rawBytes)
        {
            if (rawBytes == null || rawBytes.Length < 16)
                return Empty(rawBytes ?? Array.Empty<byte>());

            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            // fvar header (16 bytes)
            ushort majorVersion    = reader.ReadUInt16(); // = 1
            ushort minorVersion    = reader.ReadUInt16(); // = 0
            ushort axesArrayOffset = reader.ReadUInt16(); // offset from table start to axes array
            ushort reserved        = reader.ReadUInt16(); // = 2, ignored
            ushort axisCount       = reader.ReadUInt16();
            ushort axisSize        = reader.ReadUInt16(); // should be 20
            ushort instanceCount   = reader.ReadUInt16();
            ushort instanceSize    = reader.ReadUInt16();

            if (axisSize < 20 || axesArrayOffset + (long)axisCount * axisSize > rawBytes.Length)
                return Empty(rawBytes);

            // ── Parse axes ────────────────────────────────────────────────────
            reader.Seek(axesArrayOffset);
            var axes = new List<FontVariationAxis>(axisCount);
            for (int i = 0; i < axisCount; i++)
            {
                long axisStart = reader.Position;

                string tag         = reader.ReadTag().TrimEnd();  // 4 bytes
                double minValue    = reader.ReadFixed();           // 4 bytes, 16.16 fixed
                double defaultValue = reader.ReadFixed();          // 4 bytes
                double maxValue    = reader.ReadFixed();           // 4 bytes
                ushort axisFlags   = reader.ReadUInt16();          // 2 bytes
                ushort axisNameId  = reader.ReadUInt16();          // 2 bytes → name table ID

                string axisName = GetWellKnownAxisName(tag);
                axes.Add(new FontVariationAxis(tag, minValue, defaultValue, maxValue, axisName));

                // Skip any extra bytes if axisSize > 20.
                int consumed = (int)(reader.Position - axisStart);
                if (consumed < axisSize)
                    reader.Skip(axisSize - consumed);
            }

            // ── Parse named instances ─────────────────────────────────────────
            // Each instance record: subfamilyNameID(2) + flags(2) + coordinates[axisCount](4 each)
            //   optionally: + postScriptNameID(2) when instanceSize == axisCount*4 + 6
            var instances = new List<FontVariationInstance>(instanceCount);
            for (int i = 0; i < instanceCount; i++)
            {
                long instStart = reader.Position;

                ushort subfamilyNameId = reader.ReadUInt16();
                ushort instFlags       = reader.ReadUInt16();

                var coords = new Dictionary<string, double>(axisCount);
                for (int a = 0; a < axisCount; a++)
                {
                    double coord = reader.ReadFixed();
                    if (a < axes.Count)
                        coords[axes[a].Tag] = coord;
                }

                // Skip optional postScriptNameID or any extra bytes.
                int consumed = (int)(reader.Position - instStart);
                if (consumed < instanceSize)
                    reader.Skip(instanceSize - consumed);

                // Instance name would require resolving subfamilyNameId via the name table.
                // Use a placeholder; callers may resolve via TtfFont.Tables.Name.
                instances.Add(new FontVariationInstance($"Instance {i + 1}", coords));
            }

            return new TtfFvarTable(rawBytes, axes, instances);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static TtfFvarTable Empty(byte[] rawBytes) =>
            new TtfFvarTable(rawBytes,
                new List<FontVariationAxis>(),
                new List<FontVariationInstance>());

        /// <summary>Returns a human-readable name for well-known registered axis tags.</summary>
        private static string GetWellKnownAxisName(string tag)
        {
            switch (tag)
            {
                case "wght": return "Weight";
                case "wdth": return "Width";
                case "ital": return "Italic";
                case "slnt": return "Slant";
                case "opsz": return "Optical Size";
                case "GRAD": return "Grade";
                case "MONO": return "Monospace";
                case "CASL": return "Casual";
                case "CRSV": return "Cursive";
                case "FILL": return "Fill";
                default:     return tag;
            }
        }
    }
}
