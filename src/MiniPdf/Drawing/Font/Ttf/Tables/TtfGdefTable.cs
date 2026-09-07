using System;
using System.IO;
using MiniPdf.Drawing.Font.Core;
using MiniPdf.Drawing.Font.IO;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "GDEF" table — Glyph Definition table.
    /// Exposes per-glyph classification (Base, Ligature, Mark, Component) via
    /// <see cref="GetClass"/>.
    /// </summary>
    /// <remarks>
    /// Supports GDEF versions 1.0, 1.2, and 1.3.
    /// Only the GlyphClassDef sub-table is parsed; AttachList, LigCaretList,
    /// MarkAttachClassDef, MarkGlyphSets and ItemVarStore are skipped.
    ///
    /// ClassDef format 1 (contiguous range): stored as a flat array.
    /// ClassDef format 2 (range records): stored as a sorted array and binary-searched.
    ///
    /// Ref: https://learn.microsoft.com/en-us/typography/opentype/spec/gdef
    /// </remarks>
    public sealed class TtfGdefTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the GDEF table.
        /// </summary>
        public const string TableTag = "GDEF";

        // ── Storage for ClassDef ──────────────────────────────────────────────

        // Format 1: flat class array starting at _f1Start.
        private readonly ushort _f1Start;
        private readonly GlyphClass[]? _f1Classes;   // non-null ↔ format 1

        // Format 2: sorted range records.
        private readonly ClassRange[]? _f2Ranges;    // non-null ↔ format 2

        private readonly struct ClassRange
        {
            public readonly ushort Start;
            public readonly ushort End;
            public readonly GlyphClass Class;

            public ClassRange(ushort start, ushort end, GlyphClass cls)
            {
                Start = start;
                End   = end;
                Class = cls;
            }
        }

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfGdefTable(
            byte[] rawBytes,
            ushort f1Start,
            GlyphClass[]? f1Classes,
            ClassRange[]?  f2Ranges)
            : base(TableTag, rawBytes)
        {
            _f1Start   = f1Start;
            _f1Classes = f1Classes;
            _f2Ranges  = f2Ranges;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the <see cref="GlyphClass"/> of <paramref name="glyphId"/>.
        /// Returns <see cref="GlyphClass.Unclassified"/> when the glyph is not listed.
        /// </summary>
        public GlyphClass GetClass(ushort glyphId)
        {
            if (_f1Classes != null)
            {
                int idx = glyphId - _f1Start;
                return (idx >= 0 && idx < _f1Classes.Length)
                    ? _f1Classes[idx]
                    : GlyphClass.Unclassified;
            }

            if (_f2Ranges != null)
            {
                // Binary search over sorted range array.
                int lo = 0, hi = _f2Ranges.Length - 1;
                while (lo <= hi)
                {
                    int mid = (lo + hi) >> 1;
                    var r = _f2Ranges[mid];
                    if      (glyphId < r.Start) hi = mid - 1;
                    else if (glyphId > r.End)   lo = mid + 1;
                    else                        return r.Class;
                }
            }

            return GlyphClass.Unclassified;
        }

        // ── Parser ────────────────────────────────────────────────────────────

        /// <summary>Parses the "GDEF" table from its raw bytes.</summary>
        public static TtfGdefTable Parse(byte[] rawBytes)
        {
            if (rawBytes is null) throw new ArgumentNullException(nameof(rawBytes));

            if (rawBytes.Length < 12)
                return Empty(rawBytes);

            using var reader = new FontBinaryReader(new MemoryStream(rawBytes));

            /* majorVersion */                  reader.ReadUInt16(); // 0
            /* minorVersion */                  reader.ReadUInt16(); // 2
            ushort glyphClassDefOffset        = reader.ReadUInt16(); // 4 — from table start; 0=absent
            /* attachListOffset */              reader.ReadUInt16(); // 6
            /* ligCaretListOffset */            reader.ReadUInt16(); // 8
            /* markAttachClassDefOffset */      reader.ReadUInt16(); // 10

            if (glyphClassDefOffset == 0 || glyphClassDefOffset >= rawBytes.Length)
                return Empty(rawBytes);

            // ── ClassDef sub-table ────────────────────────────────────────────
            reader.Seek(glyphClassDefOffset);

            if (reader.Position + 2 > rawBytes.Length) return Empty(rawBytes);

            ushort classFormat = reader.ReadUInt16();

            if (classFormat == 1)
            {
                // Format 1: contiguous glyph range.
                if (reader.Position + 4 > rawBytes.Length) return Empty(rawBytes);

                ushort startGlyphID = reader.ReadUInt16();
                ushort glyphCount   = reader.ReadUInt16();

                var classes = new GlyphClass[glyphCount];
                for (int i = 0; i < glyphCount; i++)
                {
                    if (reader.Position + 2 > rawBytes.Length) break;
                    classes[i] = (GlyphClass)reader.ReadUInt16();
                }
                return new TtfGdefTable(rawBytes, startGlyphID, classes, null);
            }
            else if (classFormat == 2)
            {
                // Format 2: range records (startGlyphID, endGlyphID, class).
                if (reader.Position + 2 > rawBytes.Length) return Empty(rawBytes);

                ushort rangeCount = reader.ReadUInt16();
                var ranges = new ClassRange[rangeCount];

                for (int i = 0; i < rangeCount; i++)
                {
                    if (reader.Position + 6 > rawBytes.Length) break;
                    ushort start = reader.ReadUInt16();
                    ushort end   = reader.ReadUInt16();
                    ushort cls   = reader.ReadUInt16();
                    ranges[i] = new ClassRange(start, end, (GlyphClass)cls);
                }
                return new TtfGdefTable(rawBytes, 0, null, ranges);
            }

            return Empty(rawBytes);
        }

        private static TtfGdefTable Empty(byte[] rawBytes) =>
            new TtfGdefTable(rawBytes, 0, null, null);
    }
}
