using System;

namespace MiniSoftware.Drawing.Colors
{
    /// <summary>
    /// Represents an ARGB color, mirroring the System.Drawing.Color API.
    /// </summary>
    public readonly struct Color : IEquatable<Color>
    {
        // _state == 0 → Empty (default-constructed)
        // _state == 1 → raw ARGB value
        // _state == 2 → KnownColor
        private readonly uint _argb;
        private readonly short _knownColor; // cast of KnownColor; 0 when _state != 2
        private readonly short _state;

        // ── Constructors ────────────────────────────────────────────────────────

        private Color(uint argb)
        {
            _argb       = argb;
            _knownColor = 0;
            _state      = 1;
        }

        private Color(KnownColor kc)
        {
            _argb       = KnownColorTable.ArgbFromKnownColor(kc);
            _knownColor = (short)kc;
            _state      = 2;
        }

        // ── Well-known singleton ─────────────────────────────────────────────────

        /// <summary>
        /// Gets a <see cref="Color"/> structure that has ARGB values of (0, 0, 0, 0).
        /// </summary>
        public static readonly Color Empty = default;

        // ── Channel properties ───────────────────────────────────────────────────

        /// <summary>
        /// Gets the alpha channel component of this <see cref="Color"/> structure.
        /// </summary>
        public byte A => (byte)((_argb >> 24) & 0xFF);

        /// <summary>
        /// Gets the red channel component of this <see cref="Color"/> structure.
        /// </summary>
        public byte R => (byte)((_argb >> 16) & 0xFF);

        /// <summary>
        /// Gets the green channel component of this <see cref="Color"/> structure.
        /// </summary>
        public byte G => (byte)((_argb >>  8) & 0xFF);

        /// <summary>
        /// Gets the blue channel component of this <see cref="Color"/> structure.
        /// </summary>
        public byte B => (byte)( _argb        & 0xFF);

        // ── State properties ─────────────────────────────────────────────────────

        /// <summary>
        /// Gets a value indicating whether this <see cref="Color"/> is empty.
        /// </summary>
        public bool IsEmpty        => _state == 0;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Color"/> is a known color.
        /// </summary>
        public bool IsKnownColor   => _state == 2;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Color"/> is a named color.
        /// </summary>
        public bool IsNamedColor   => _state == 2;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Color"/> is a system color.
        /// </summary>
        public bool IsSystemColor  => _state == 2 && KnownColorTable.IsSystemColor((KnownColor)_knownColor);

        /// <summary>
        /// Gets the name of this <see cref="Color"/>.
        /// </summary>
        public string Name
        {
            get
            {
                if (_state == 2) return ((KnownColor)_knownColor).ToString();
                if (_state == 0) return "0";
                return _argb.ToString("x8");
            }
        }

        // ── Factory methods ──────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="Color"/> from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A 32-bit ARGB value.</param>
        /// <returns>A <see cref="Color"/> structure.</returns>
        public static Color FromArgb(int argb)
            => new Color((uint)argb);

        /// <summary>
        /// Creates a <see cref="Color"/> from the specified ARGB component values.
        /// </summary>
        /// <param name="alpha">The alpha channel value (0-255).</param>
        /// <param name="red">The red channel value (0-255).</param>
        /// <param name="green">The green channel value (0-255).</param>
        /// <param name="blue">The blue channel value (0-255).</param>
        /// <returns>A <see cref="Color"/> structure.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any component is outside 0-255.</exception>
        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            if ((uint)alpha > 255) throw new ArgumentOutOfRangeException(nameof(alpha), "Value must be 0-255.");
            if ((uint)red   > 255) throw new ArgumentOutOfRangeException(nameof(red),   "Value must be 0-255.");
            if ((uint)green > 255) throw new ArgumentOutOfRangeException(nameof(green), "Value must be 0-255.");
            if ((uint)blue  > 255) throw new ArgumentOutOfRangeException(nameof(blue),  "Value must be 0-255.");
            return new Color(((uint)alpha << 24) | ((uint)red << 16) | ((uint)green << 8) | (uint)blue);
        }

        /// <summary>
        /// Creates a <see cref="Color"/> from the specified alpha and base color.
        /// </summary>
        /// <param name="alpha">The alpha channel value (0-255).</param>
        /// <param name="baseColor">The base color to use for RGB components.</param>
        /// <returns>A <see cref="Color"/> structure.</returns>
        public static Color FromArgb(int alpha, Color baseColor)
            => FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);

        /// <summary>
        /// Creates a <see cref="Color"/> from the specified RGB component values with full opacity.
        /// </summary>
        /// <param name="red">The red channel value (0-255).</param>
        /// <param name="green">The green channel value (0-255).</param>
        /// <param name="blue">The blue channel value (0-255).</param>
        /// <returns>A <see cref="Color"/> structure.</returns>
        public static Color FromArgb(int red, int green, int blue)
            => FromArgb(255, red, green, blue);

        /// <summary>
        /// Creates a <see cref="Color"/> from the specified known color.
        /// </summary>
        /// <param name="color">The known color to create.</param>
        /// <returns>A <see cref="Color"/> structure.</returns>
        public static Color FromKnownColor(KnownColor color)
            => new Color(color);

        /// <summary>
        /// Creates a <see cref="Color"/> from the specified color name.
        /// </summary>
        /// <param name="name">The name of the color.</param>
        /// <returns>A <see cref="Color"/> structure.</returns>
        public static Color FromName(string name)
        {
            if (Enum.TryParse<KnownColor>(name, ignoreCase: true, out KnownColor kc))
                return FromKnownColor(kc);
            // Unknown name: return an ARGB-zero color (not Empty, but unnamed)
            return new Color(0u);
        }

        // ── Conversion ──────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the 32-bit ARGB value of this <see cref="Color"/>.
        /// </summary>
        /// <returns>A 32-bit ARGB value.</returns>
        public int ToArgb() => (int)_argb;

        /// <summary>
        /// Gets the <see cref="KnownColor"/> value of this <see cref="Color"/>.
        /// </summary>
        /// <returns>The known color value.</returns>
        public KnownColor ToKnownColor() => (KnownColor)_knownColor;

        // ── HSB ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the hue component of this <see cref="Color"/>, in degrees (0-360).
        /// </summary>
        /// <returns>The hue value in degrees.</returns>
        public float GetHue()
        {
            if (R == G && G == B) return 0f;
            float r = R / 255f, g = G / 255f, b = B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;
            float hue;
            if      (max == r) hue = ((g - b) / delta) % 6f * 60f;
            else if (max == g) hue = ((b - r) / delta + 2f) * 60f;
            else               hue = ((r - g) / delta + 4f) * 60f;
            return hue < 0f ? hue + 360f : hue;
        }

        /// <summary>
        /// Gets the saturation component of this <see cref="Color"/> (0.0-1.0).
        /// </summary>
        /// <returns>The saturation value.</returns>
        public float GetSaturation()
        {
            float r = R / 255f, g = G / 255f, b = B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            if (max == min) return 0f;
            float l = (max + min) / 2f;
            float delta = max - min;
            return l <= 0.5f ? delta / (max + min) : delta / (2f - max - min);
        }

        /// <summary>
        /// Gets the brightness (lightness) component of this <see cref="Color"/> (0.0-1.0).
        /// </summary>
        /// <returns>The brightness value.</returns>
        public float GetBrightness()
        {
            float r = R / 255f, g = G / 255f, b = B / 255f;
            return (Math.Max(r, Math.Max(g, b)) + Math.Min(r, Math.Min(g, b))) / 2f;
        }

        // ── Equality ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Indicates whether this <see cref="Color"/> is equal to another <see cref="Color"/>.
        /// </summary>
        /// <param name="other">The color to compare with.</param>
        /// <returns>True if the colors are equal; otherwise, false.</returns>
        public bool Equals(Color other)
            => _argb == other._argb && _knownColor == other._knownColor && _state == other._state;

        /// <summary>
        /// Indicates whether this <see cref="Color"/> is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the colors are equal; otherwise, false.</returns>
        public override bool Equals(object? obj) => obj is Color c && Equals(c);

        /// <summary>
        /// Gets the hash code for this <see cref="Color"/>.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int h = (int)_argb;
                h = h * 397 ^ _knownColor;
                h = h * 397 ^ _state;
                return h;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="Color"/> structures are equal.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns>True if the colors are equal; otherwise, false.</returns>
        public static bool operator ==(Color left, Color right) => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="Color"/> structures are not equal.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns>True if the colors are not equal; otherwise, false.</returns>
        public static bool operator !=(Color left, Color right) => !left.Equals(right);

        /// <summary>
        /// Converts this <see cref="Color"/> to its string representation.
        /// </summary>
        /// <returns>A string representation of the color.</returns>
        public override string ToString() => $"Color [A={A}, R={R}, G={G}, B={B}]";

        // ── Named color static properties ────────────────────────────────────────

        /// <summary>
        /// Gets a <see cref="Color"/> representing transparent (ARGB = 0x00FFFFFF).
        /// </summary>
        public static Color Transparent          => FromKnownColor(KnownColor.Transparent);

        /// <summary>
        /// Gets a <see cref="Color"/> representing AliceBlue.
        /// </summary>
        public static Color AliceBlue            => FromKnownColor(KnownColor.AliceBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing AntiqueWhite.
        /// </summary>
        public static Color AntiqueWhite         => FromKnownColor(KnownColor.AntiqueWhite);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Aqua.
        /// </summary>
        public static Color Aqua                 => FromKnownColor(KnownColor.Aqua);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Aquamarine.
        /// </summary>
        public static Color Aquamarine           => FromKnownColor(KnownColor.Aquamarine);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Azure.
        /// </summary>
        public static Color Azure                => FromKnownColor(KnownColor.Azure);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Beige.
        /// </summary>
        public static Color Beige                => FromKnownColor(KnownColor.Beige);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Bisque.
        /// </summary>
        public static Color Bisque               => FromKnownColor(KnownColor.Bisque);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Black.
        /// </summary>
        public static Color Black                => FromKnownColor(KnownColor.Black);

        /// <summary>
        /// Gets a <see cref="Color"/> representing BlanchedAlmond.
        /// </summary>
        public static Color BlanchedAlmond       => FromKnownColor(KnownColor.BlanchedAlmond);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Blue.
        /// </summary>
        public static Color Blue                 => FromKnownColor(KnownColor.Blue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing BlueViolet.
        /// </summary>
        public static Color BlueViolet           => FromKnownColor(KnownColor.BlueViolet);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Brown.
        /// </summary>
        public static Color Brown                => FromKnownColor(KnownColor.Brown);

        /// <summary>
        /// Gets a <see cref="Color"/> representing BurlyWood.
        /// </summary>
        public static Color BurlyWood            => FromKnownColor(KnownColor.BurlyWood);

        /// <summary>
        /// Gets a <see cref="Color"/> representing CadetBlue.
        /// </summary>
        public static Color CadetBlue            => FromKnownColor(KnownColor.CadetBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Chartreuse.
        /// </summary>
        public static Color Chartreuse           => FromKnownColor(KnownColor.Chartreuse);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Chocolate.
        /// </summary>
        public static Color Chocolate            => FromKnownColor(KnownColor.Chocolate);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Coral.
        /// </summary>
        public static Color Coral                => FromKnownColor(KnownColor.Coral);

        /// <summary>
        /// Gets a <see cref="Color"/> representing CornflowerBlue.
        /// </summary>
        public static Color CornflowerBlue       => FromKnownColor(KnownColor.CornflowerBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Cornsilk.
        /// </summary>
        public static Color Cornsilk             => FromKnownColor(KnownColor.Cornsilk);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Crimson.
        /// </summary>
        public static Color Crimson              => FromKnownColor(KnownColor.Crimson);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Cyan.
        /// </summary>
        public static Color Cyan                 => FromKnownColor(KnownColor.Cyan);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkBlue.
        /// </summary>
        public static Color DarkBlue             => FromKnownColor(KnownColor.DarkBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkCyan.
        /// </summary>
        public static Color DarkCyan             => FromKnownColor(KnownColor.DarkCyan);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkGoldenrod.
        /// </summary>
        public static Color DarkGoldenrod        => FromKnownColor(KnownColor.DarkGoldenrod);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkGray.
        /// </summary>
        public static Color DarkGray             => FromKnownColor(KnownColor.DarkGray);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkGreen.
        /// </summary>
        public static Color DarkGreen            => FromKnownColor(KnownColor.DarkGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkKhaki.
        /// </summary>
        public static Color DarkKhaki            => FromKnownColor(KnownColor.DarkKhaki);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkMagenta.
        /// </summary>
        public static Color DarkMagenta          => FromKnownColor(KnownColor.DarkMagenta);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkOliveGreen.
        /// </summary>
        public static Color DarkOliveGreen       => FromKnownColor(KnownColor.DarkOliveGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkOrange.
        /// </summary>
        public static Color DarkOrange           => FromKnownColor(KnownColor.DarkOrange);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkOrchid.
        /// </summary>
        public static Color DarkOrchid           => FromKnownColor(KnownColor.DarkOrchid);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkRed.
        /// </summary>
        public static Color DarkRed              => FromKnownColor(KnownColor.DarkRed);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkSalmon.
        /// </summary>
        public static Color DarkSalmon           => FromKnownColor(KnownColor.DarkSalmon);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkSeaGreen.
        /// </summary>
        public static Color DarkSeaGreen         => FromKnownColor(KnownColor.DarkSeaGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkSlateBlue.
        /// </summary>
        public static Color DarkSlateBlue        => FromKnownColor(KnownColor.DarkSlateBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkSlateGray.
        /// </summary>
        public static Color DarkSlateGray        => FromKnownColor(KnownColor.DarkSlateGray);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkTurquoise.
        /// </summary>
        public static Color DarkTurquoise        => FromKnownColor(KnownColor.DarkTurquoise);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DarkViolet.
        /// </summary>
        public static Color DarkViolet           => FromKnownColor(KnownColor.DarkViolet);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DeepPink.
        /// </summary>
        public static Color DeepPink             => FromKnownColor(KnownColor.DeepPink);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DeepSkyBlue.
        /// </summary>
        public static Color DeepSkyBlue          => FromKnownColor(KnownColor.DeepSkyBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DimGray.
        /// </summary>
        public static Color DimGray              => FromKnownColor(KnownColor.DimGray);

        /// <summary>
        /// Gets a <see cref="Color"/> representing DodgerBlue.
        /// </summary>
        public static Color DodgerBlue           => FromKnownColor(KnownColor.DodgerBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Firebrick.
        /// </summary>
        public static Color Firebrick            => FromKnownColor(KnownColor.Firebrick);

        /// <summary>
        /// Gets a <see cref="Color"/> representing FloralWhite.
        /// </summary>
        public static Color FloralWhite          => FromKnownColor(KnownColor.FloralWhite);

        /// <summary>
        /// Gets a <see cref="Color"/> representing ForestGreen.
        /// </summary>
        public static Color ForestGreen          => FromKnownColor(KnownColor.ForestGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Fuchsia.
        /// </summary>
        public static Color Fuchsia              => FromKnownColor(KnownColor.Fuchsia);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Gainsboro.
        /// </summary>
        public static Color Gainsboro            => FromKnownColor(KnownColor.Gainsboro);

        /// <summary>
        /// Gets a <see cref="Color"/> representing GhostWhite.
        /// </summary>
        public static Color GhostWhite           => FromKnownColor(KnownColor.GhostWhite);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Gold.
        /// </summary>
        public static Color Gold                 => FromKnownColor(KnownColor.Gold);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Goldenrod.
        /// </summary>
        public static Color Goldenrod            => FromKnownColor(KnownColor.Goldenrod);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Gray.
        /// </summary>
        public static Color Gray                 => FromKnownColor(KnownColor.Gray);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Green.
        /// </summary>
        public static Color Green                => FromKnownColor(KnownColor.Green);

        /// <summary>
        /// Gets a <see cref="Color"/> representing GreenYellow.
        /// </summary>
        public static Color GreenYellow          => FromKnownColor(KnownColor.GreenYellow);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Honeydew.
        /// </summary>
        public static Color Honeydew             => FromKnownColor(KnownColor.Honeydew);

        /// <summary>
        /// Gets a <see cref="Color"/> representing HotPink.
        /// </summary>
        public static Color HotPink              => FromKnownColor(KnownColor.HotPink);

        /// <summary>
        /// Gets a <see cref="Color"/> representing IndianRed.
        /// </summary>
        public static Color IndianRed            => FromKnownColor(KnownColor.IndianRed);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Indigo.
        /// </summary>
        public static Color Indigo               => FromKnownColor(KnownColor.Indigo);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Ivory.
        /// </summary>
        public static Color Ivory                => FromKnownColor(KnownColor.Ivory);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Khaki.
        /// </summary>
        public static Color Khaki                => FromKnownColor(KnownColor.Khaki);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Lavender.
        /// </summary>
        public static Color Lavender             => FromKnownColor(KnownColor.Lavender);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LavenderBlush.
        /// </summary>
        public static Color LavenderBlush        => FromKnownColor(KnownColor.LavenderBlush);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LawnGreen.
        /// </summary>
        public static Color LawnGreen            => FromKnownColor(KnownColor.LawnGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LemonChiffon.
        /// </summary>
        public static Color LemonChiffon         => FromKnownColor(KnownColor.LemonChiffon);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightBlue.
        /// </summary>
        public static Color LightBlue            => FromKnownColor(KnownColor.LightBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightCoral.
        /// </summary>
        public static Color LightCoral           => FromKnownColor(KnownColor.LightCoral);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightCyan.
        /// </summary>
        public static Color LightCyan            => FromKnownColor(KnownColor.LightCyan);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightGoldenrodYellow.
        /// </summary>
        public static Color LightGoldenrodYellow => FromKnownColor(KnownColor.LightGoldenrodYellow);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightGray.
        /// </summary>
        public static Color LightGray            => FromKnownColor(KnownColor.LightGray);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightGreen.
        /// </summary>
        public static Color LightGreen           => FromKnownColor(KnownColor.LightGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightPink.
        /// </summary>
        public static Color LightPink            => FromKnownColor(KnownColor.LightPink);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightSalmon.
        /// </summary>
        public static Color LightSalmon          => FromKnownColor(KnownColor.LightSalmon);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightSeaGreen.
        /// </summary>
        public static Color LightSeaGreen        => FromKnownColor(KnownColor.LightSeaGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightSkyBlue.
        /// </summary>
        public static Color LightSkyBlue         => FromKnownColor(KnownColor.LightSkyBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightSlateGray.
        /// </summary>
        public static Color LightSlateGray       => FromKnownColor(KnownColor.LightSlateGray);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightSteelBlue.
        /// </summary>
        public static Color LightSteelBlue       => FromKnownColor(KnownColor.LightSteelBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LightYellow.
        /// </summary>
        public static Color LightYellow          => FromKnownColor(KnownColor.LightYellow);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Lime.
        /// </summary>
        public static Color Lime                 => FromKnownColor(KnownColor.Lime);

        /// <summary>
        /// Gets a <see cref="Color"/> representing LimeGreen.
        /// </summary>
        public static Color LimeGreen            => FromKnownColor(KnownColor.LimeGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Linen.
        /// </summary>
        public static Color Linen                => FromKnownColor(KnownColor.Linen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Magenta.
        /// </summary>
        public static Color Magenta              => FromKnownColor(KnownColor.Magenta);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Maroon.
        /// </summary>
        public static Color Maroon               => FromKnownColor(KnownColor.Maroon);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumAquamarine.
        /// </summary>
        public static Color MediumAquamarine     => FromKnownColor(KnownColor.MediumAquamarine);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumBlue.
        /// </summary>
        public static Color MediumBlue           => FromKnownColor(KnownColor.MediumBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumOrchid.
        /// </summary>
        public static Color MediumOrchid         => FromKnownColor(KnownColor.MediumOrchid);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumPurple.
        /// </summary>
        public static Color MediumPurple         => FromKnownColor(KnownColor.MediumPurple);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumSeaGreen.
        /// </summary>
        public static Color MediumSeaGreen       => FromKnownColor(KnownColor.MediumSeaGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumSlateBlue.
        /// </summary>
        public static Color MediumSlateBlue      => FromKnownColor(KnownColor.MediumSlateBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumSpringGreen.
        /// </summary>
        public static Color MediumSpringGreen    => FromKnownColor(KnownColor.MediumSpringGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumTurquoise.
        /// </summary>
        public static Color MediumTurquoise      => FromKnownColor(KnownColor.MediumTurquoise);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MediumVioletRed.
        /// </summary>
        public static Color MediumVioletRed      => FromKnownColor(KnownColor.MediumVioletRed);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MidnightBlue.
        /// </summary>
        public static Color MidnightBlue         => FromKnownColor(KnownColor.MidnightBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MintCream.
        /// </summary>
        public static Color MintCream            => FromKnownColor(KnownColor.MintCream);

        /// <summary>
        /// Gets a <see cref="Color"/> representing MistyRose.
        /// </summary>
        public static Color MistyRose            => FromKnownColor(KnownColor.MistyRose);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Moccasin.
        /// </summary>
        public static Color Moccasin             => FromKnownColor(KnownColor.Moccasin);

        /// <summary>
        /// Gets a <see cref="Color"/> representing NavajoWhite.
        /// </summary>
        public static Color NavajoWhite          => FromKnownColor(KnownColor.NavajoWhite);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Navy.
        /// </summary>
        public static Color Navy                 => FromKnownColor(KnownColor.Navy);

        /// <summary>
        /// Gets a <see cref="Color"/> representing OldLace.
        /// </summary>
        public static Color OldLace              => FromKnownColor(KnownColor.OldLace);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Olive.
        /// </summary>
        public static Color Olive                => FromKnownColor(KnownColor.Olive);

        /// <summary>
        /// Gets a <see cref="Color"/> representing OliveDrab.
        /// </summary>
        public static Color OliveDrab            => FromKnownColor(KnownColor.OliveDrab);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Orange.
        /// </summary>
        public static Color Orange               => FromKnownColor(KnownColor.Orange);

        /// <summary>
        /// Gets a <see cref="Color"/> representing OrangeRed.
        /// </summary>
        public static Color OrangeRed            => FromKnownColor(KnownColor.OrangeRed);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Orchid.
        /// </summary>
        public static Color Orchid               => FromKnownColor(KnownColor.Orchid);

        /// <summary>
        /// Gets a <see cref="Color"/> representing PaleGoldenrod.
        /// </summary>
        public static Color PaleGoldenrod        => FromKnownColor(KnownColor.PaleGoldenrod);

        /// <summary>
        /// Gets a <see cref="Color"/> representing PaleGreen.
        /// </summary>
        public static Color PaleGreen            => FromKnownColor(KnownColor.PaleGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing PaleTurquoise.
        /// </summary>
        public static Color PaleTurquoise        => FromKnownColor(KnownColor.PaleTurquoise);

        /// <summary>
        /// Gets a <see cref="Color"/> representing PaleVioletRed.
        /// </summary>
        public static Color PaleVioletRed        => FromKnownColor(KnownColor.PaleVioletRed);

        /// <summary>
        /// Gets a <see cref="Color"/> representing PapayaWhip.
        /// </summary>
        public static Color PapayaWhip           => FromKnownColor(KnownColor.PapayaWhip);

        /// <summary>
        /// Gets a <see cref="Color"/> representing PeachPuff.
        /// </summary>
        public static Color PeachPuff            => FromKnownColor(KnownColor.PeachPuff);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Peru.
        /// </summary>
        public static Color Peru                 => FromKnownColor(KnownColor.Peru);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Pink.
        /// </summary>
        public static Color Pink                 => FromKnownColor(KnownColor.Pink);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Plum.
        /// </summary>
        public static Color Plum                 => FromKnownColor(KnownColor.Plum);

        /// <summary>
        /// Gets a <see cref="Color"/> representing PowderBlue.
        /// </summary>
        public static Color PowderBlue           => FromKnownColor(KnownColor.PowderBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Purple.
        /// </summary>
        public static Color Purple               => FromKnownColor(KnownColor.Purple);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Red.
        /// </summary>
        public static Color Red                  => FromKnownColor(KnownColor.Red);

        /// <summary>
        /// Gets a <see cref="Color"/> representing RosyBrown.
        /// </summary>
        public static Color RosyBrown            => FromKnownColor(KnownColor.RosyBrown);

        /// <summary>
        /// Gets a <see cref="Color"/> representing RoyalBlue.
        /// </summary>
        public static Color RoyalBlue            => FromKnownColor(KnownColor.RoyalBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SaddleBrown.
        /// </summary>
        public static Color SaddleBrown          => FromKnownColor(KnownColor.SaddleBrown);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Salmon.
        /// </summary>
        public static Color Salmon               => FromKnownColor(KnownColor.Salmon);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SandyBrown.
        /// </summary>
        public static Color SandyBrown           => FromKnownColor(KnownColor.SandyBrown);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SeaGreen.
        /// </summary>
        public static Color SeaGreen             => FromKnownColor(KnownColor.SeaGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SeaShell.
        /// </summary>
        public static Color SeaShell             => FromKnownColor(KnownColor.SeaShell);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Sienna.
        /// </summary>
        public static Color Sienna               => FromKnownColor(KnownColor.Sienna);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Silver.
        /// </summary>
        public static Color Silver               => FromKnownColor(KnownColor.Silver);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SkyBlue.
        /// </summary>
        public static Color SkyBlue              => FromKnownColor(KnownColor.SkyBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SlateBlue.
        /// </summary>
        public static Color SlateBlue            => FromKnownColor(KnownColor.SlateBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SlateGray.
        /// </summary>
        public static Color SlateGray            => FromKnownColor(KnownColor.SlateGray);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Snow.
        /// </summary>
        public static Color Snow                 => FromKnownColor(KnownColor.Snow);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SpringGreen.
        /// </summary>
        public static Color SpringGreen          => FromKnownColor(KnownColor.SpringGreen);

        /// <summary>
        /// Gets a <see cref="Color"/> representing SteelBlue.
        /// </summary>
        public static Color SteelBlue            => FromKnownColor(KnownColor.SteelBlue);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Tan.
        /// </summary>
        public static Color Tan                  => FromKnownColor(KnownColor.Tan);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Teal.
        /// </summary>
        public static Color Teal                 => FromKnownColor(KnownColor.Teal);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Thistle.
        /// </summary>
        public static Color Thistle              => FromKnownColor(KnownColor.Thistle);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Tomato.
        /// </summary>
        public static Color Tomato               => FromKnownColor(KnownColor.Tomato);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Turquoise.
        /// </summary>
        public static Color Turquoise            => FromKnownColor(KnownColor.Turquoise);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Violet.
        /// </summary>
        public static Color Violet               => FromKnownColor(KnownColor.Violet);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Wheat.
        /// </summary>
        public static Color Wheat                => FromKnownColor(KnownColor.Wheat);

        /// <summary>
        /// Gets a <see cref="Color"/> representing White.
        /// </summary>
        public static Color White                => FromKnownColor(KnownColor.White);

        /// <summary>
        /// Gets a <see cref="Color"/> representing WhiteSmoke.
        /// </summary>
        public static Color WhiteSmoke           => FromKnownColor(KnownColor.WhiteSmoke);

        /// <summary>
        /// Gets a <see cref="Color"/> representing Yellow.
        /// </summary>
        public static Color Yellow               => FromKnownColor(KnownColor.Yellow);

        /// <summary>
        /// Gets a <see cref="Color"/> representing YellowGreen.
        /// </summary>
        public static Color YellowGreen          => FromKnownColor(KnownColor.YellowGreen);
    }
}
