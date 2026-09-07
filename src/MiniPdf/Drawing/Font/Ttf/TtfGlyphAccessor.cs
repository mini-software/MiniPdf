using System;
using MiniPdf.Drawing.Font.Glyphs;
using MiniPdf.Drawing.Font.Ttf.Glyphs;

namespace MiniPdf.Drawing.Font.Ttf
{
    /// <summary>
    /// Implements <see cref="IGlyphAccessor"/> for TrueType/OTF fonts.
    /// Retrieves glyph metrics from "hmtx" and outline bounds from "glyf".
    /// </summary>
    public sealed class TtfGlyphAccessor : IGlyphAccessor
    {
        private readonly TtfTables _tables;

        internal TtfGlyphAccessor(TtfTables tables)
        {
            _tables = tables ?? throw new ArgumentNullException(nameof(tables));
        }

        /// <inheritdoc/>
        public Glyph? GetGlyphById(GlyphId glyphId)
        {
            if (glyphId is null) return null;

            if (glyphId is GlyphUInt32Id uid)
                return GetGlyphByIndex(uid.Value);

            return null;
        }

        /// <inheritdoc/>
        public Glyph? GetGlyphByIndex(uint index)
        {
            int i = (int)index;

            if (i < 0 || i >= _tables.Maxp.NumGlyphs)
                return null;

            // Advance width from hmtx (design units).
            double advanceWidth = _tables.Hmtx.GetAdvanceWidth(i);

            // Bounding box from glyf (if present), otherwise empty.
            GlyphBoundingBox bbox = GlyphBoundingBox.Empty;
            if (_tables.GlyfTable != null)
            {
                GlyphOutlineData? outline = _tables.GlyfTable.GetGlyphOutline(i);
                if (outline != null)
                    bbox = outline.BBox;
            }

            return new Glyph(new GlyphUInt32Id(index), bbox, advanceWidth);
        }
    }
}
