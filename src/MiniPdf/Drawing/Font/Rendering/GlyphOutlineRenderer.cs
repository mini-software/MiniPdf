using System;
using MiniSoftware.Drawing.Font.Cff;
using MiniSoftware.Drawing.Font.Core;
using MiniSoftware.Drawing.Font.Glyphs;
using MiniSoftware.Drawing.Font.Ttf;
using MiniSoftware.Drawing.Font.Type1;

namespace MiniSoftware.Drawing.Font.Rendering
{
    /// <summary>
    /// High-level entry point for glyph outline rendering.
    /// Wraps an <see cref="IFont"/> and dispatches each
    /// <see cref="RenderGlyph"/> call to the appropriate low-level renderer:
    /// <list type="bullet">
    ///   <item><see cref="TtfFont"/> → <see cref="TtfOutlineRenderer"/></item>
    ///   <item>CffFont → CffOutlineRenderer (added in Batch 9)</item>
    /// </list>
    /// </summary>
    public sealed class GlyphOutlineRenderer
    {
        private readonly IFont _font;

        /// <param name="font">The font whose glyphs will be rendered.</param>
        public GlyphOutlineRenderer(IFont font)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
        }

        /// <summary>
        /// Decomposes the outline of the glyph identified by <paramref name="gid"/>
        /// and delivers drawing commands to <paramref name="painter"/>.
        /// </summary>
        /// <param name="gid">Glyph identifier, typically from <see cref="IFontEncoding.DecodeToGid"/>.</param>
        /// <param name="painter">Receives the MoveTo / LineTo / CurveTo / ClosePath calls.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gid"/> or <paramref name="painter"/> is null.</exception>
        /// <exception cref="NotSupportedException">
        /// The font type does not yet have an outline renderer implementation.
        /// </exception>
        public void RenderGlyph(GlyphId gid, IGlyphOutlinePainter painter)
        {
            if (gid     is null) throw new ArgumentNullException(nameof(gid));
            if (painter is null) throw new ArgumentNullException(nameof(painter));

            if (_font is TtfFont ttfFont)
            {
                RenderTtfGlyph(ttfFont, gid, painter);
                return;
            }

            if (_font is CffFont cffFont)
            {
                RenderCffGlyph(cffFont, gid, painter);
                return;
            }

            if (_font is Type1Font t1Font)
            {
                RenderType1Glyph(t1Font, gid, painter);
                return;
            }

            throw new NotSupportedException(
                $"GlyphOutlineRenderer does not yet support font type '{_font.GetType().Name}'.");
        }

        // ── TrueType / OTF/TT ────────────────────────────────────────────────

        private static void RenderTtfGlyph(
            TtfFont font, GlyphId gid, IGlyphOutlinePainter painter)
        {
            if (!(gid is GlyphUInt32Id uid)) return;

            var glyfTable = font.TtfTables.GlyfTable;
            if (glyfTable == null) return; // CFF-flavored OTF has no glyf table

            var outline = glyfTable.GetGlyphOutline((int)uid.Value);
            if (outline != null)
                TtfOutlineRenderer.Render(outline, painter, glyfTable);
        }

        // ── CFF / OTF+CFF ─────────────────────────────────────────────────────

        private static void RenderCffGlyph(
            CffFont font, GlyphId gid, IGlyphOutlinePainter painter)
        {
            if (!(gid is GlyphUInt32Id uid)) return;

            var data = font.CffData;
            int idx  = (int)uid.Value;
            if (idx < 0 || idx >= data.CharStrings.Length) return;

            CffPrivateDict priv = GetCffPrivateDict(data, idx);
            var interp = new Type2Interpreter();
            interp.Interpret(data.CharStrings[idx], painter, priv, data.GlobalSubrs);
        }

        private static CffPrivateDict GetCffPrivateDict(CffData data, int gid)
        {
            if (data.IsCIDFont && data.GlyphFDIndex != null && data.CIDFontDicts != null)
            {
                int fdIdx = gid < data.GlyphFDIndex.Length ? data.GlyphFDIndex[gid] : 0;
                if (fdIdx >= 0 && fdIdx < data.CIDFontDicts.Length)
                    return data.CIDFontDicts[fdIdx].PrivateDict;
            }
            return data.PrivateDict;
        }

        // ── Type 1 ────────────────────────────────────────────────────────────

        private static void RenderType1Glyph(
            Type1Font font, GlyphId gid, IGlyphOutlinePainter painter)
        {
            if (!(gid is GlyphUInt32Id uid)) return;

            var data = font.Data;
            int idx  = (int)uid.Value;
            if (idx < 0 || idx >= data.GlyphOrder.Length) return;

            string name = data.GlyphOrder[idx];
            if (!data.CharStrings.TryGetValue(name, out byte[]? cs)) return;

            var interp = new Type1CharStringInterpreter();
            interp.Interpret(cs, painter, data.Subrs, data.CharStrings);
        }
    }
}
