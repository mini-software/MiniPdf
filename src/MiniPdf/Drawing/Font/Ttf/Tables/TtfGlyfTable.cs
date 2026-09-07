using System;
using System.Collections.Generic;
using System.IO;
using MiniPdf.Drawing.Font.Glyphs;
using MiniPdf.Drawing.Font.IO;
using MiniPdf.Drawing.Font.Ttf.Glyphs;

namespace MiniPdf.Drawing.Font.Ttf.Tables
{
    /// <summary>
    /// Parsed OpenType "glyf" table.
    /// Provides lazy on-demand parsing of individual glyph outlines using the
    /// "loca" table to locate each glyph's data within the raw byte array.
    /// Ref: OpenType spec §5.3.3.
    /// </summary>
    public sealed class TtfGlyfTable : TtfTableBase
    {
        /// <summary>
        /// The table tag for the glyf table.
        /// </summary>
        public const string TableTag = "glyf";

        private readonly TtfLocaTable _loca;

        // ── Simple-glyph flag bits ────────────────────────────────────────────
        private const byte FlagOnCurve          = 0x01;
        private const byte FlagXShortVector     = 0x02;
        private const byte FlagYShortVector     = 0x04;
        private const byte FlagRepeat           = 0x08;
        private const byte FlagXSameOrPositive  = 0x10;
        private const byte FlagYSameOrPositive  = 0x20;

        // ── Composite-glyph flag bits ─────────────────────────────────────────
        private const ushort CompArg1And2AreWords    = 0x0001;
        private const ushort CompArgsAreXYValues     = 0x0002;
        private const ushort CompWeHaveAScale        = 0x0008;
        private const ushort CompMoreComponents      = 0x0020;
        private const ushort CompWeHaveAnXAndYScale  = 0x0040;
        private const ushort CompWeHaveATwoByTwo     = 0x0080;
        private const ushort CompWeHaveInstructions  = 0x0100;

        // ── Constructor ───────────────────────────────────────────────────────

        private TtfGlyfTable(byte[] rawBytes, TtfLocaTable loca)
            : base(TableTag, rawBytes)
        {
            _loca = loca;
        }

        // ── Factory ───────────────────────────────────────────────────────────

        /// <summary>
        /// Wraps the raw "glyf" table bytes together with the parsed "loca" table.
        /// No parsing is performed at construction time; glyphs are parsed on demand.
        /// </summary>
        public static TtfGlyfTable Create(byte[] rawBytes, TtfLocaTable loca)
        {
            if (rawBytes == null) throw new ArgumentNullException(nameof(rawBytes));
            if (loca     == null) throw new ArgumentNullException(nameof(loca));
            return new TtfGlyfTable(rawBytes, loca);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Parses and returns the outline data for the glyph at
        /// <paramref name="glyphIndex"/>.
        /// Returns <see cref="GlyphOutlineData.Empty"/> for glyphs with no outlines
        /// (e.g. the space character) and <c>null</c> for out-of-range indices or
        /// corrupt data.
        /// </summary>
        public GlyphOutlineData? GetGlyphOutline(int glyphIndex)
        {
            if (glyphIndex < 0) return null;

            uint offset = _loca.GetGlyphOffset(glyphIndex);
            uint length = _loca.GetGlyphLength(glyphIndex);

            // A zero-length entry means no outline data (e.g. space).
            if (length == 0) return GlyphOutlineData.Empty;

            if (offset + length > (uint)RawBytes.Length) return null;

            try
            {
                // Create a sub-stream view over this glyph's bytes only.
                using var ms     = new MemoryStream(RawBytes, (int)offset, (int)length, writable: false);
                using var reader = new FontBinaryReader(ms);

                short numberOfContours = reader.ReadInt16();
                short xMin = reader.ReadInt16();
                short yMin = reader.ReadInt16();
                short xMax = reader.ReadInt16();
                short yMax = reader.ReadInt16();
                var   bbox = new GlyphBoundingBox(xMin, yMin, xMax, yMax);

                if (numberOfContours >= 0)
                {
                    var simple = numberOfContours == 0
                        ? TtfSimpleGlyph.Empty
                        : ParseSimpleGlyph(reader, numberOfContours);
                    return new GlyphOutlineData(false, simple, null, bbox);
                }
                else // numberOfContours == -1
                {
                    var composite = ParseCompositeGlyph(reader);
                    return new GlyphOutlineData(true, null, composite, bbox);
                }
            }
            catch
            {
                return null; // corrupt glyph data
            }
        }

        // ── Simple glyph parsing ──────────────────────────────────────────────

        private static TtfSimpleGlyph ParseSimpleGlyph(FontBinaryReader reader, short numberOfContours)
        {
            // 1. End-point indices (one per contour).
            var endPts = new ushort[numberOfContours];
            for (int i = 0; i < numberOfContours; i++)
                endPts[i] = reader.ReadUInt16();

            int numPoints = endPts[numberOfContours - 1] + 1;

            // 2. Instructions.
            ushort instrLen  = reader.ReadUInt16();
            byte[] instrs    = instrLen > 0 ? reader.ReadBytes(instrLen) : Array.Empty<byte>();

            // 3. Flags (run-length encoded with the REPEAT bit).
            byte[] flags = new byte[numPoints];
            for (int i = 0; i < numPoints; )
            {
                byte flag = reader.ReadUInt8();
                flags[i++] = flag;
                if ((flag & FlagRepeat) != 0)
                {
                    byte count = reader.ReadUInt8();
                    for (int r = 0; r < count; r++)
                        flags[i++] = flag;
                }
            }

            // 4. X-coordinate deltas → absolute coordinates.
            short[] xCoords = ReadCoordinates(reader, flags, numPoints,
                shortBit: FlagXShortVector, sameOrPosBit: FlagXSameOrPositive);

            // 5. Y-coordinate deltas → absolute coordinates.
            short[] yCoords = ReadCoordinates(reader, flags, numPoints,
                shortBit: FlagYShortVector, sameOrPosBit: FlagYSameOrPositive);

            // 6. Build TtfPoint array.
            // Determine which indices are contour endpoints.
            var endPtSet = new HashSet<int>();
            foreach (ushort ep in endPts) endPtSet.Add(ep);

            var points = new TtfPoint[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                bool onCurve   = (flags[i] & FlagOnCurve) != 0;
                bool isEndPt   = endPtSet.Contains(i);
                points[i]      = new TtfPoint(xCoords[i], yCoords[i], onCurve, isEndPt);
            }

            return new TtfSimpleGlyph(numberOfContours, points, endPts, instrs);
        }

        /// <summary>
        /// Reads delta-encoded signed coordinates and accumulates them into
        /// absolute values.
        /// </summary>
        private static short[] ReadCoordinates(
            FontBinaryReader reader, byte[] flags, int count,
            byte shortBit, byte sameOrPosBit)
        {
            var coords = new short[count];
            short current = 0;

            for (int i = 0; i < count; i++)
            {
                byte flag = flags[i];
                bool isShort    = (flag & shortBit)    != 0;
                bool sameOrPos  = (flag & sameOrPosBit) != 0;

                if (isShort)
                {
                    // 1-byte unsigned magnitude; direction from sameOrPos.
                    byte delta = reader.ReadUInt8();
                    current += (short)(sameOrPos ? delta : -delta);
                }
                else if (!sameOrPos)
                {
                    // 2-byte signed delta.
                    current += reader.ReadInt16();
                }
                // else: sameOrPos && !isShort → delta = 0 (same coordinate).

                coords[i] = current;
            }

            return coords;
        }

        // ── Composite glyph parsing ───────────────────────────────────────────

        private static TtfCompositeGlyph ParseCompositeGlyph(FontBinaryReader reader)
        {
            var components = new List<TtfCompositeComponent>();
            bool moreComponents;
            bool hasInstructions = false;

            do
            {
                ushort flags      = reader.ReadUInt16();
                ushort glyphIndex = reader.ReadUInt16();

                moreComponents  = (flags & CompMoreComponents)     != 0;
                hasInstructions = hasInstructions ||
                                  (flags & CompWeHaveInstructions) != 0;

                bool argsAreWords    = (flags & CompArg1And2AreWords)   != 0;
                bool argsAreXY       = (flags & CompArgsAreXYValues)    != 0;

                short arg1, arg2;
                if (argsAreWords)
                {
                    arg1 = reader.ReadInt16();
                    arg2 = reader.ReadInt16();
                }
                else
                {
                    arg1 = (sbyte)reader.ReadUInt8(); // signed byte
                    arg2 = (sbyte)reader.ReadUInt8();
                }

                // Build the 2×3 affine transform: | a  b  dx |
                //                                  | c  d  dy |
                float a = 1f, b = 0f, c = 0f, d = 1f;

                if ((flags & CompWeHaveATwoByTwo) != 0)
                {
                    a = (float)reader.ReadF2Dot14();
                    b = (float)reader.ReadF2Dot14();
                    c = (float)reader.ReadF2Dot14();
                    d = (float)reader.ReadF2Dot14();
                }
                else if ((flags & CompWeHaveAnXAndYScale) != 0)
                {
                    a = (float)reader.ReadF2Dot14();
                    d = (float)reader.ReadF2Dot14();
                }
                else if ((flags & CompWeHaveAScale) != 0)
                {
                    a = d = (float)reader.ReadF2Dot14();
                }

                float dx = argsAreXY ? arg1 : 0f;
                float dy = argsAreXY ? arg2 : 0f;

                var transform = new float[2, 3]
                {
                    { a, b, dx },
                    { c, d, dy }
                };

                components.Add(new TtfCompositeComponent(glyphIndex, transform, argsAreXY, arg1, arg2));

            } while (moreComponents);

            // Optional trailing instructions.
            byte[] instrs = Array.Empty<byte>();
            if (hasInstructions)
            {
                ushort instrLen = reader.ReadUInt16();
                instrs = instrLen > 0 ? reader.ReadBytes(instrLen) : Array.Empty<byte>();
            }

            return new TtfCompositeGlyph(components.ToArray(), instrs);
        }
    }
}
