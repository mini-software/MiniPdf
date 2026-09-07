using MiniSoftware.Drawing.Colors;

namespace MiniSoftware.Drawing.Brushes
{
    /// <summary>
    /// Provides a set of predefined <see cref="SolidBrush"/> objects representing all <see cref="KnownColor"/> colors.
    /// </summary>
    /// <remarks>
    /// Each property returns a cached <see cref="SolidBrush"/> for efficient reuse.
    /// These brushes are commonly used for filling shapes and backgrounds.
    /// </remarks>
    public static class Brushes
    {
        private static readonly SolidBrush?[] s_brushes = new SolidBrush?[141];

        private static SolidBrush Get(KnownColor kc)
        {
            int idx = (int)kc - 27; // KnownColor.Transparent == 27
            return s_brushes[idx] ??= new SolidBrush(Color.FromKnownColor(kc));
        }

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Transparent"/> color.
        /// </summary>
        public static SolidBrush Transparent         => Get(KnownColor.Transparent);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.AliceBlue"/> color.
        /// </summary>
        public static SolidBrush AliceBlue           => Get(KnownColor.AliceBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.AntiqueWhite"/> color.
        /// </summary>
        public static SolidBrush AntiqueWhite        => Get(KnownColor.AntiqueWhite);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Aqua"/> color.
        /// </summary>
        public static SolidBrush Aqua                => Get(KnownColor.Aqua);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Aquamarine"/> color.
        /// </summary>
        public static SolidBrush Aquamarine          => Get(KnownColor.Aquamarine);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Azure"/> color.
        /// </summary>
        public static SolidBrush Azure               => Get(KnownColor.Azure);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Beige"/> color.
        /// </summary>
        public static SolidBrush Beige               => Get(KnownColor.Beige);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Bisque"/> color.
        /// </summary>
        public static SolidBrush Bisque              => Get(KnownColor.Bisque);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Black"/> color.
        /// </summary>
        public static SolidBrush Black               => Get(KnownColor.Black);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.BlanchedAlmond"/> color.
        /// </summary>
        public static SolidBrush BlanchedAlmond      => Get(KnownColor.BlanchedAlmond);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Blue"/> color.
        /// </summary>
        public static SolidBrush Blue                => Get(KnownColor.Blue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.BlueViolet"/> color.
        /// </summary>
        public static SolidBrush BlueViolet          => Get(KnownColor.BlueViolet);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Brown"/> color.
        /// </summary>
        public static SolidBrush Brown               => Get(KnownColor.Brown);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.BurlyWood"/> color.
        /// </summary>
        public static SolidBrush BurlyWood           => Get(KnownColor.BurlyWood);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.CadetBlue"/> color.
        /// </summary>
        public static SolidBrush CadetBlue           => Get(KnownColor.CadetBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Chartreuse"/> color.
        /// </summary>
        public static SolidBrush Chartreuse          => Get(KnownColor.Chartreuse);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Chocolate"/> color.
        /// </summary>
        public static SolidBrush Chocolate           => Get(KnownColor.Chocolate);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Coral"/> color.
        /// </summary>
        public static SolidBrush Coral               => Get(KnownColor.Coral);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.CornflowerBlue"/> color.
        /// </summary>
        public static SolidBrush CornflowerBlue      => Get(KnownColor.CornflowerBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Cornsilk"/> color.
        /// </summary>
        public static SolidBrush Cornsilk            => Get(KnownColor.Cornsilk);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Crimson"/> color.
        /// </summary>
        public static SolidBrush Crimson             => Get(KnownColor.Crimson);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Cyan"/> color.
        /// </summary>
        public static SolidBrush Cyan                => Get(KnownColor.Cyan);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkBlue"/> color.
        /// </summary>
        public static SolidBrush DarkBlue            => Get(KnownColor.DarkBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkCyan"/> color.
        /// </summary>
        public static SolidBrush DarkCyan            => Get(KnownColor.DarkCyan);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkGoldenrod"/> color.
        /// </summary>
        public static SolidBrush DarkGoldenrod       => Get(KnownColor.DarkGoldenrod);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkGray"/> color.
        /// </summary>
        public static SolidBrush DarkGray            => Get(KnownColor.DarkGray);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkGreen"/> color.
        /// </summary>
        public static SolidBrush DarkGreen           => Get(KnownColor.DarkGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkKhaki"/> color.
        /// </summary>
        public static SolidBrush DarkKhaki           => Get(KnownColor.DarkKhaki);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkMagenta"/> color.
        /// </summary>
        public static SolidBrush DarkMagenta         => Get(KnownColor.DarkMagenta);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkOliveGreen"/> color.
        /// </summary>
        public static SolidBrush DarkOliveGreen      => Get(KnownColor.DarkOliveGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkOrange"/> color.
        /// </summary>
        public static SolidBrush DarkOrange          => Get(KnownColor.DarkOrange);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkOrchid"/> color.
        /// </summary>
        public static SolidBrush DarkOrchid          => Get(KnownColor.DarkOrchid);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkRed"/> color.
        /// </summary>
        public static SolidBrush DarkRed             => Get(KnownColor.DarkRed);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkSalmon"/> color.
        /// </summary>
        public static SolidBrush DarkSalmon          => Get(KnownColor.DarkSalmon);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkSeaGreen"/> color.
        /// </summary>
        public static SolidBrush DarkSeaGreen        => Get(KnownColor.DarkSeaGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkSlateBlue"/> color.
        /// </summary>
        public static SolidBrush DarkSlateBlue       => Get(KnownColor.DarkSlateBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkSlateGray"/> color.
        /// </summary>
        public static SolidBrush DarkSlateGray       => Get(KnownColor.DarkSlateGray);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkTurquoise"/> color.
        /// </summary>
        public static SolidBrush DarkTurquoise       => Get(KnownColor.DarkTurquoise);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DarkViolet"/> color.
        /// </summary>
        public static SolidBrush DarkViolet          => Get(KnownColor.DarkViolet);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DeepPink"/> color.
        /// </summary>
        public static SolidBrush DeepPink            => Get(KnownColor.DeepPink);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DeepSkyBlue"/> color.
        /// </summary>
        public static SolidBrush DeepSkyBlue         => Get(KnownColor.DeepSkyBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DimGray"/> color.
        /// </summary>
        public static SolidBrush DimGray             => Get(KnownColor.DimGray);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.DodgerBlue"/> color.
        /// </summary>
        public static SolidBrush DodgerBlue          => Get(KnownColor.DodgerBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Firebrick"/> color.
        /// </summary>
        public static SolidBrush Firebrick           => Get(KnownColor.Firebrick);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.FloralWhite"/> color.
        /// </summary>
        public static SolidBrush FloralWhite         => Get(KnownColor.FloralWhite);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.ForestGreen"/> color.
        /// </summary>
        public static SolidBrush ForestGreen         => Get(KnownColor.ForestGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Fuchsia"/> color.
        /// </summary>
        public static SolidBrush Fuchsia             => Get(KnownColor.Fuchsia);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Gainsboro"/> color.
        /// </summary>
        public static SolidBrush Gainsboro           => Get(KnownColor.Gainsboro);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.GhostWhite"/> color.
        /// </summary>
        public static SolidBrush GhostWhite          => Get(KnownColor.GhostWhite);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Gold"/> color.
        /// </summary>
        public static SolidBrush Gold                => Get(KnownColor.Gold);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Goldenrod"/> color.
        /// </summary>
        public static SolidBrush Goldenrod           => Get(KnownColor.Goldenrod);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Gray"/> color.
        /// </summary>
        public static SolidBrush Gray                => Get(KnownColor.Gray);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Green"/> color.
        /// </summary>
        public static SolidBrush Green               => Get(KnownColor.Green);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.GreenYellow"/> color.
        /// </summary>
        public static SolidBrush GreenYellow         => Get(KnownColor.GreenYellow);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Honeydew"/> color.
        /// </summary>
        public static SolidBrush Honeydew            => Get(KnownColor.Honeydew);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.HotPink"/> color.
        /// </summary>
        public static SolidBrush HotPink             => Get(KnownColor.HotPink);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.IndianRed"/> color.
        /// </summary>
        public static SolidBrush IndianRed           => Get(KnownColor.IndianRed);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Indigo"/> color.
        /// </summary>
        public static SolidBrush Indigo              => Get(KnownColor.Indigo);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Ivory"/> color.
        /// </summary>
        public static SolidBrush Ivory               => Get(KnownColor.Ivory);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Khaki"/> color.
        /// </summary>
        public static SolidBrush Khaki               => Get(KnownColor.Khaki);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Lavender"/> color.
        /// </summary>
        public static SolidBrush Lavender            => Get(KnownColor.Lavender);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LavenderBlush"/> color.
        /// </summary>
        public static SolidBrush LavenderBlush       => Get(KnownColor.LavenderBlush);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LawnGreen"/> color.
        /// </summary>
        public static SolidBrush LawnGreen           => Get(KnownColor.LawnGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LemonChiffon"/> color.
        /// </summary>
        public static SolidBrush LemonChiffon        => Get(KnownColor.LemonChiffon);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightBlue"/> color.
        /// </summary>
        public static SolidBrush LightBlue           => Get(KnownColor.LightBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightCoral"/> color.
        /// </summary>
        public static SolidBrush LightCoral          => Get(KnownColor.LightCoral);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightCyan"/> color.
        /// </summary>
        public static SolidBrush LightCyan           => Get(KnownColor.LightCyan);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightGoldenrodYellow"/> color.
        /// </summary>
        public static SolidBrush LightGoldenrodYellow => Get(KnownColor.LightGoldenrodYellow);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightGray"/> color.
        /// </summary>
        public static SolidBrush LightGray           => Get(KnownColor.LightGray);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightGreen"/> color.
        /// </summary>
        public static SolidBrush LightGreen          => Get(KnownColor.LightGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightPink"/> color.
        /// </summary>
        public static SolidBrush LightPink           => Get(KnownColor.LightPink);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightSalmon"/> color.
        /// </summary>
        public static SolidBrush LightSalmon         => Get(KnownColor.LightSalmon);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightSeaGreen"/> color.
        /// </summary>
        public static SolidBrush LightSeaGreen       => Get(KnownColor.LightSeaGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightSkyBlue"/> color.
        /// </summary>
        public static SolidBrush LightSkyBlue        => Get(KnownColor.LightSkyBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightSlateGray"/> color.
        /// </summary>
        public static SolidBrush LightSlateGray      => Get(KnownColor.LightSlateGray);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightSteelBlue"/> color.
        /// </summary>
        public static SolidBrush LightSteelBlue      => Get(KnownColor.LightSteelBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LightYellow"/> color.
        /// </summary>
        public static SolidBrush LightYellow         => Get(KnownColor.LightYellow);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Lime"/> color.
        /// </summary>
        public static SolidBrush Lime                => Get(KnownColor.Lime);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.LimeGreen"/> color.
        /// </summary>
        public static SolidBrush LimeGreen           => Get(KnownColor.LimeGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Linen"/> color.
        /// </summary>
        public static SolidBrush Linen               => Get(KnownColor.Linen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Magenta"/> color.
        /// </summary>
        public static SolidBrush Magenta             => Get(KnownColor.Magenta);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Maroon"/> color.
        /// </summary>
        public static SolidBrush Maroon              => Get(KnownColor.Maroon);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumAquamarine"/> color.
        /// </summary>
        public static SolidBrush MediumAquamarine    => Get(KnownColor.MediumAquamarine);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumBlue"/> color.
        /// </summary>
        public static SolidBrush MediumBlue          => Get(KnownColor.MediumBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumOrchid"/> color.
        /// </summary>
        public static SolidBrush MediumOrchid        => Get(KnownColor.MediumOrchid);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumPurple"/> color.
        /// </summary>
        public static SolidBrush MediumPurple        => Get(KnownColor.MediumPurple);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumSeaGreen"/> color.
        /// </summary>
        public static SolidBrush MediumSeaGreen      => Get(KnownColor.MediumSeaGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumSlateBlue"/> color.
        /// </summary>
        public static SolidBrush MediumSlateBlue     => Get(KnownColor.MediumSlateBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumSpringGreen"/> color.
        /// </summary>
        public static SolidBrush MediumSpringGreen   => Get(KnownColor.MediumSpringGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumTurquoise"/> color.
        /// </summary>
        public static SolidBrush MediumTurquoise     => Get(KnownColor.MediumTurquoise);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MediumVioletRed"/> color.
        /// </summary>
        public static SolidBrush MediumVioletRed     => Get(KnownColor.MediumVioletRed);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MidnightBlue"/> color.
        /// </summary>
        public static SolidBrush MidnightBlue        => Get(KnownColor.MidnightBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MintCream"/> color.
        /// </summary>
        public static SolidBrush MintCream           => Get(KnownColor.MintCream);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.MistyRose"/> color.
        /// </summary>
        public static SolidBrush MistyRose           => Get(KnownColor.MistyRose);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Moccasin"/> color.
        /// </summary>
        public static SolidBrush Moccasin            => Get(KnownColor.Moccasin);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.NavajoWhite"/> color.
        /// </summary>
        public static SolidBrush NavajoWhite         => Get(KnownColor.NavajoWhite);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Navy"/> color.
        /// </summary>
        public static SolidBrush Navy                => Get(KnownColor.Navy);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.OldLace"/> color.
        /// </summary>
        public static SolidBrush OldLace             => Get(KnownColor.OldLace);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Olive"/> color.
        /// </summary>
        public static SolidBrush Olive               => Get(KnownColor.Olive);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.OliveDrab"/> color.
        /// </summary>
        public static SolidBrush OliveDrab           => Get(KnownColor.OliveDrab);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Orange"/> color.
        /// </summary>
        public static SolidBrush Orange              => Get(KnownColor.Orange);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.OrangeRed"/> color.
        /// </summary>
        public static SolidBrush OrangeRed           => Get(KnownColor.OrangeRed);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Orchid"/> color.
        /// </summary>
        public static SolidBrush Orchid              => Get(KnownColor.Orchid);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.PaleGoldenrod"/> color.
        /// </summary>
        public static SolidBrush PaleGoldenrod       => Get(KnownColor.PaleGoldenrod);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.PaleGreen"/> color.
        /// </summary>
        public static SolidBrush PaleGreen           => Get(KnownColor.PaleGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.PaleTurquoise"/> color.
        /// </summary>
        public static SolidBrush PaleTurquoise       => Get(KnownColor.PaleTurquoise);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.PaleVioletRed"/> color.
        /// </summary>
        public static SolidBrush PaleVioletRed       => Get(KnownColor.PaleVioletRed);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.PapayaWhip"/> color.
        /// </summary>
        public static SolidBrush PapayaWhip          => Get(KnownColor.PapayaWhip);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.PeachPuff"/> color.
        /// </summary>
        public static SolidBrush PeachPuff           => Get(KnownColor.PeachPuff);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Peru"/> color.
        /// </summary>
        public static SolidBrush Peru                => Get(KnownColor.Peru);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Pink"/> color.
        /// </summary>
        public static SolidBrush Pink                => Get(KnownColor.Pink);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Plum"/> color.
        /// </summary>
        public static SolidBrush Plum                => Get(KnownColor.Plum);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.PowderBlue"/> color.
        /// </summary>
        public static SolidBrush PowderBlue          => Get(KnownColor.PowderBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Purple"/> color.
        /// </summary>
        public static SolidBrush Purple              => Get(KnownColor.Purple);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Red"/> color.
        /// </summary>
        public static SolidBrush Red                 => Get(KnownColor.Red);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.RosyBrown"/> color.
        /// </summary>
        public static SolidBrush RosyBrown           => Get(KnownColor.RosyBrown);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.RoyalBlue"/> color.
        /// </summary>
        public static SolidBrush RoyalBlue           => Get(KnownColor.RoyalBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SaddleBrown"/> color.
        /// </summary>
        public static SolidBrush SaddleBrown         => Get(KnownColor.SaddleBrown);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Salmon"/> color.
        /// </summary>
        public static SolidBrush Salmon              => Get(KnownColor.Salmon);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SandyBrown"/> color.
        /// </summary>
        public static SolidBrush SandyBrown          => Get(KnownColor.SandyBrown);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SeaGreen"/> color.
        /// </summary>
        public static SolidBrush SeaGreen            => Get(KnownColor.SeaGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SeaShell"/> color.
        /// </summary>
        public static SolidBrush SeaShell            => Get(KnownColor.SeaShell);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Sienna"/> color.
        /// </summary>
        public static SolidBrush Sienna              => Get(KnownColor.Sienna);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Silver"/> color.
        /// </summary>
        public static SolidBrush Silver              => Get(KnownColor.Silver);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SkyBlue"/> color.
        /// </summary>
        public static SolidBrush SkyBlue             => Get(KnownColor.SkyBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SlateBlue"/> color.
        /// </summary>
        public static SolidBrush SlateBlue           => Get(KnownColor.SlateBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SlateGray"/> color.
        /// </summary>
        public static SolidBrush SlateGray           => Get(KnownColor.SlateGray);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Snow"/> color.
        /// </summary>
        public static SolidBrush Snow                => Get(KnownColor.Snow);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SpringGreen"/> color.
        /// </summary>
        public static SolidBrush SpringGreen         => Get(KnownColor.SpringGreen);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.SteelBlue"/> color.
        /// </summary>
        public static SolidBrush SteelBlue           => Get(KnownColor.SteelBlue);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Tan"/> color.
        /// </summary>
        public static SolidBrush Tan                 => Get(KnownColor.Tan);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Teal"/> color.
        /// </summary>
        public static SolidBrush Teal                => Get(KnownColor.Teal);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Thistle"/> color.
        /// </summary>
        public static SolidBrush Thistle             => Get(KnownColor.Thistle);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Tomato"/> color.
        /// </summary>
        public static SolidBrush Tomato              => Get(KnownColor.Tomato);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Turquoise"/> color.
        /// </summary>
        public static SolidBrush Turquoise           => Get(KnownColor.Turquoise);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Violet"/> color.
        /// </summary>
        public static SolidBrush Violet              => Get(KnownColor.Violet);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Wheat"/> color.
        /// </summary>
        public static SolidBrush Wheat               => Get(KnownColor.Wheat);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.White"/> color.
        /// </summary>
        public static SolidBrush White               => Get(KnownColor.White);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.WhiteSmoke"/> color.
        /// </summary>
        public static SolidBrush WhiteSmoke          => Get(KnownColor.WhiteSmoke);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.Yellow"/> color.
        /// </summary>
        public static SolidBrush Yellow              => Get(KnownColor.Yellow);

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> with <see cref="KnownColor.YellowGreen"/> color.
        /// </summary>
        public static SolidBrush YellowGreen         => Get(KnownColor.YellowGreen);
    }
}
