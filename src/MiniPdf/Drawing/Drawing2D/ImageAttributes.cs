using System;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Specifies colour-adjustment information applied to pixels during image rendering.
    /// Supports colour matrices, gamma correction, colour-key transparency and colour remapping.
    /// </summary>
    public sealed class ImageAttributes : IDisposable
    {
        // Per-type slots: Bitmap (type=1) takes priority over Default (type=0)
        private ColorMatrix?  _bitmapColorMatrix;
        private ColorMatrix?  _defaultColorMatrix;
        private float         _bitmapGamma  = 1f;
        private float         _defaultGamma = 1f;
        private bool          _hasColorKey;
        private Color         _colorKeyLow;
        private Color         _colorKeyHigh;
        private ColorMap[]?   _remapTable;
        private bool          _noOp;

        // ── Color matrix ─────────────────────────────────────────────────────

        /// <summary>
        /// Sets the colour matrix for the default colour adjustment category.
        /// </summary>
        /// <param name="colorMatrix">The colour matrix to apply.</param>
        public void SetColorMatrix(ColorMatrix colorMatrix)
            => SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);

        /// <summary>
        /// Sets the colour matrix for the specified colour adjustment category.
        /// </summary>
        /// <param name="colorMatrix">The colour matrix to apply.</param>
        /// <param name="flags">The colour matrix flags that specify how the colour matrix is applied.</param>
        public void SetColorMatrix(ColorMatrix colorMatrix, ColorMatrixFlag flags)
            => SetColorMatrix(colorMatrix, flags, ColorAdjustType.Default);

        /// <summary>
        /// Sets the colour matrix for the specified colour adjustment category.
        /// </summary>
        /// <param name="colorMatrix">The colour matrix to apply.</param>
        /// <param name="flags">The colour matrix flags that specify how the colour matrix is applied.</param>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void SetColorMatrix(ColorMatrix colorMatrix, ColorMatrixFlag flags, ColorAdjustType type)
        {
            if (type == ColorAdjustType.Bitmap)
                _bitmapColorMatrix = colorMatrix;
            else
                _defaultColorMatrix = colorMatrix;
        }

        /// <summary>
        /// Clears the colour matrix for the default colour adjustment category.
        /// </summary>
        public void ClearColorMatrix()
            => ClearColorMatrix(ColorAdjustType.Default);

        /// <summary>
        /// Clears the colour matrix for the specified colour adjustment category.
        /// </summary>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void ClearColorMatrix(ColorAdjustType type)
        {
            if (type == ColorAdjustType.Bitmap)
                _bitmapColorMatrix = null;
            else
                _defaultColorMatrix = null;
        }

        // ── Gamma correction ──────────────────────────────────────────────────

        /// <summary>
        /// Sets the gamma value for the default colour adjustment category.
        /// </summary>
        /// <param name="gamma">The gamma value.</param>
        public void SetGamma(float gamma)
            => SetGamma(gamma, ColorAdjustType.Default);

        /// <summary>
        /// Sets the gamma value for the specified colour adjustment category.
        /// </summary>
        /// <param name="gamma">The gamma value.</param>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void SetGamma(float gamma, ColorAdjustType type)
        {
            if (type == ColorAdjustType.Bitmap)
                _bitmapGamma = gamma;
            else
                _defaultGamma = gamma;
        }

        /// <summary>
        /// Clears the gamma value for the default colour adjustment category.
        /// </summary>
        public void ClearGamma()
            => ClearGamma(ColorAdjustType.Default);

        /// <summary>
        /// Clears the gamma value for the specified colour adjustment category.
        /// </summary>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void ClearGamma(ColorAdjustType type)
        {
            if (type == ColorAdjustType.Bitmap) _bitmapGamma  = 1f;
            else                                _defaultGamma = 1f;
        }

        // ── Color key (transparency range) ────────────────────────────────────

        /// <summary>
        /// Sets the colour key for the default colour adjustment category.
        /// </summary>
        /// <param name="colorLow">The low colour key value.</param>
        /// <param name="colorHigh">The high colour key value.</param>
        public void SetColorKey(Color colorLow, Color colorHigh)
            => SetColorKey(colorLow, colorHigh, ColorAdjustType.Default);

        /// <summary>
        /// Sets the colour key for the specified colour adjustment category.
        /// </summary>
        /// <param name="colorLow">The low colour key value.</param>
        /// <param name="colorHigh">The high colour key value.</param>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void SetColorKey(Color colorLow, Color colorHigh, ColorAdjustType type)
        {
            _hasColorKey  = true;
            _colorKeyLow  = colorLow;
            _colorKeyHigh = colorHigh;
        }

        /// <summary>
        /// Clears the colour key for the default colour adjustment category.
        /// </summary>
        public void ClearColorKey()
            => ClearColorKey(ColorAdjustType.Default);

        /// <summary>
        /// Clears the colour key for the specified colour adjustment category.
        /// </summary>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void ClearColorKey(ColorAdjustType type)
            => _hasColorKey = false;

        // ── Remap table ───────────────────────────────────────────────────────

        /// <summary>
        /// Sets the colour remap table for the default colour adjustment category.
        /// </summary>
        /// <param name="map">The array of colour map entries that define the remap table.</param>
        public void SetRemapTable(ColorMap[] map)
            => SetRemapTable(map, ColorAdjustType.Default);

        /// <summary>
        /// Sets the colour remap table for the specified colour adjustment category.
        /// </summary>
        /// <param name="map">The array of colour map entries that define the remap table.</param>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void SetRemapTable(ColorMap[] map, ColorAdjustType type)
            => _remapTable = map == null ? null : (ColorMap[])map.Clone();

        /// <summary>
        /// Clears the colour remap table for the default colour adjustment category.
        /// </summary>
        public void ClearRemapTable()
            => ClearRemapTable(ColorAdjustType.Default);

        /// <summary>
        /// Clears the colour remap table for the specified colour adjustment category.
        /// </summary>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void ClearRemapTable(ColorAdjustType type)
            => _remapTable = null;

        // ── No-op ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Sets the no-op flag for the default colour adjustment category.
        /// </summary>
        public void SetNoOp()                           => _noOp = true;

        /// <summary>
        /// Sets the no-op flag for the specified colour adjustment category.
        /// </summary>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void SetNoOp(ColorAdjustType type)       => _noOp = true;

        /// <summary>
        /// Clears the no-op flag for the specified colour adjustment category.
        /// </summary>
        /// <param name="type">The colour adjustment type that specifies the adjustment category.</param>
        public void ClearNoOp(ColorAdjustType type)     => _noOp = false;

        // ── Clone / Dispose ───────────────────────────────────────────────────

        /// <summary>
        /// Creates an exact copy of this <see cref="ImageAttributes"/> object.
        /// </summary>
        /// <returns>A new <see cref="ImageAttributes"/> object that is a copy of this instance.</returns>
        public ImageAttributes Clone() => new ImageAttributes
        {
            _bitmapColorMatrix  = _bitmapColorMatrix,
            _defaultColorMatrix = _defaultColorMatrix,
            _bitmapGamma        = _bitmapGamma,
            _defaultGamma       = _defaultGamma,
            _hasColorKey        = _hasColorKey,
            _colorKeyLow        = _colorKeyLow,
            _colorKeyHigh       = _colorKeyHigh,
            _remapTable         = _remapTable == null ? null : (ColorMap[])_remapTable.Clone(),
            _noOp               = _noOp,
        };

        /// <summary>
        /// Releases all resources used by this <see cref="ImageAttributes"/>.
        /// </summary>
        public void Dispose() { }

        // ── Internal per-pixel colour adjustment ──────────────────────────────

        internal Color ApplyToColor(Color c)
        {
            if (_noOp) return c;

            // 1. Remap table
            if (_remapTable != null)
            {
                int argb = c.ToArgb();
                foreach (var entry in _remapTable)
                    if (entry.OldColor.ToArgb() == argb) { c = entry.NewColor; break; }
            }

            // 2. Color key → fully transparent
            if (_hasColorKey &&
                c.R >= _colorKeyLow.R && c.R <= _colorKeyHigh.R &&
                c.G >= _colorKeyLow.G && c.G <= _colorKeyHigh.G &&
                c.B >= _colorKeyLow.B && c.B <= _colorKeyHigh.B)
                return Color.FromArgb(0, c.R, c.G, c.B);

            // 3. Color matrix  (bitmap-specific slot wins over default)
            ColorMatrix? matrix = _bitmapColorMatrix ?? _defaultColorMatrix;
            if (matrix != null)
            {
                float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f, a = c.A / 255f;
                float nr = r * matrix[0, 0] + g * matrix[1, 0] + b * matrix[2, 0] + a * matrix[3, 0] + matrix[4, 0];
                float ng = r * matrix[0, 1] + g * matrix[1, 1] + b * matrix[2, 1] + a * matrix[3, 1] + matrix[4, 1];
                float nb = r * matrix[0, 2] + g * matrix[1, 2] + b * matrix[2, 2] + a * matrix[3, 2] + matrix[4, 2];
                float na = r * matrix[0, 3] + g * matrix[1, 3] + b * matrix[2, 3] + a * matrix[3, 3] + matrix[4, 3];
                c = Color.FromArgb(ClampByte(na * 255f), ClampByte(nr * 255f), ClampByte(ng * 255f), ClampByte(nb * 255f));
            }

            // 4. Gamma correction (apply inverse-gamma power)
            float gamma = (_bitmapGamma != 1f) ? _bitmapGamma : _defaultGamma;
            if (gamma != 1f && gamma > 0f)
            {
                float inv = 1f / gamma;
                c = Color.FromArgb(
                    c.A,
                    (byte)(255f * (float)Math.Pow(c.R / 255.0, inv)),
                    (byte)(255f * (float)Math.Pow(c.G / 255.0, inv)),
                    (byte)(255f * (float)Math.Pow(c.B / 255.0, inv)));
            }

            return c;
        }

        private static byte ClampByte(float v)
        {
            if (v <= 0f) return 0;
            if (v >= 255f) return 255;
            return (byte)v;
        }
    }
}

