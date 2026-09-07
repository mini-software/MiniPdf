using System;
using MiniSoftware.Drawing.Font.Glyphs;
using MiniSoftware.Drawing.Font.Rendering;

namespace MiniSoftware.Drawing.Font.Cff
{
    /// <summary>
    /// Provides glyph metrics and names for a CFF font by executing
    /// Type 2 charstrings via <see cref="Type2Interpreter"/>.
    /// </summary>
    public sealed class CffGlyphAccessor : IGlyphAccessor
    {
        private readonly CffData _data;

        internal CffGlyphAccessor(CffData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <inheritdoc/>
        public Glyph? GetGlyphById(GlyphId gid)
        {
            if (!(gid is GlyphUInt32Id uid)) return null;

            int idx = (int)uid.Value;
            if (idx < 0 || idx >= _data.CharStrings.Length) return null;

            var priv = GetPrivateDict(idx);
            var recording = new BBoxRecordingPainter();
            var interp = new Type2Interpreter();
            double width = interp.Interpret(_data.CharStrings[idx], recording, priv, _data.GlobalSubrs);

            return new Glyph(gid, recording.GetBBox(), width);
        }

        /// <inheritdoc/>
        public Glyph? GetGlyphByIndex(uint index)
            => GetGlyphById(new GlyphUInt32Id(index));

        // ── Helpers ───────────────────────────────────────────────────────────

        private CffPrivateDict GetPrivateDict(int gid)
        {
            if (_data.IsCIDFont && _data.GlyphFDIndex != null && _data.CIDFontDicts != null)
            {
                int fdIdx = gid < _data.GlyphFDIndex.Length ? _data.GlyphFDIndex[gid] : 0;
                if (fdIdx >= 0 && fdIdx < _data.CIDFontDicts.Length)
                    return _data.CIDFontDicts[fdIdx].PrivateDict;
            }
            return _data.PrivateDict;
        }

        // ── BBox recording painter ────────────────────────────────────────────

        private sealed class BBoxRecordingPainter : IGlyphOutlinePainter
        {
            private double _minX = double.MaxValue, _minY = double.MaxValue;
            private double _maxX = double.MinValue, _maxY = double.MinValue;
            private bool   _hasPoints;

            private void Track(double x, double y)
            {
                if (x < _minX) _minX = x;
                if (x > _maxX) _maxX = x;
                if (y < _minY) _minY = y;
                if (y > _maxY) _maxY = y;
                _hasPoints = true;
            }

            public void MoveTo(MoveTo cmd)  => Track(cmd.X, cmd.Y);
            public void LineTo(LineTo cmd)  => Track(cmd.X, cmd.Y);
            public void CurveTo(CurveTo cmd)
            {
                Track(cmd.X1, cmd.Y1);
                Track(cmd.X2, cmd.Y2);
                Track(cmd.X3, cmd.Y3);
            }
            public void ClosePath() { }

            public GlyphBoundingBox GetBBox()
                => _hasPoints
                    ? new GlyphBoundingBox(_minX, _minY, _maxX, _maxY)
                    : new GlyphBoundingBox(0, 0, 0, 0);
        }
    }
}
