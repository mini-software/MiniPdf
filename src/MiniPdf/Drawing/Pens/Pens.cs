using MiniPdf.Drawing.Colors;

namespace MiniPdf.Drawing.Pens
{
    /// <summary>
    /// Provides a set of predefined <see cref="Pen"/> objects representing all <see cref="KnownColor"/> colors.
    /// </summary>
    /// <remarks>
    /// Each property returns a cached <see cref="Pen"/> with a width of 1.
    /// These pens are commonly used for drawing shapes and lines.
    /// </remarks>
    public static class Pens
    {
        private static readonly Pen?[] s_pens = new Pen?[141];

        private static Pen Get(KnownColor kc)
        {
            int idx = (int)kc - 27; // KnownColor.Transparent == 27
            return s_pens[idx] ??= new Pen(Color.FromKnownColor(kc), 1f);
        }

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Transparent"/> color.
        /// </summary>
        public static Pen Transparent         => Get(KnownColor.Transparent);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.AliceBlue"/> color.
        /// </summary>
        public static Pen AliceBlue           => Get(KnownColor.AliceBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.AntiqueWhite"/> color.
        /// </summary>
        public static Pen AntiqueWhite        => Get(KnownColor.AntiqueWhite);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Aqua"/> color.
        /// </summary>
        public static Pen Aqua                => Get(KnownColor.Aqua);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Aquamarine"/> color.
        /// </summary>
        public static Pen Aquamarine          => Get(KnownColor.Aquamarine);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Azure"/> color.
        /// </summary>
        public static Pen Azure               => Get(KnownColor.Azure);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Beige"/> color.
        /// </summary>
        public static Pen Beige               => Get(KnownColor.Beige);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Bisque"/> color.
        /// </summary>
        public static Pen Bisque              => Get(KnownColor.Bisque);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Black"/> color.
        /// </summary>
        public static Pen Black               => Get(KnownColor.Black);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.BlanchedAlmond"/> color.
        /// </summary>
        public static Pen BlanchedAlmond      => Get(KnownColor.BlanchedAlmond);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Blue"/> color.
        /// </summary>
        public static Pen Blue                => Get(KnownColor.Blue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.BlueViolet"/> color.
        /// </summary>
        public static Pen BlueViolet          => Get(KnownColor.BlueViolet);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Brown"/> color.
        /// </summary>
        public static Pen Brown               => Get(KnownColor.Brown);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.BurlyWood"/> color.
        /// </summary>
        public static Pen BurlyWood           => Get(KnownColor.BurlyWood);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.CadetBlue"/> color.
        /// </summary>
        public static Pen CadetBlue           => Get(KnownColor.CadetBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Chartreuse"/> color.
        /// </summary>
        public static Pen Chartreuse          => Get(KnownColor.Chartreuse);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Chocolate"/> color.
        /// </summary>
        public static Pen Chocolate           => Get(KnownColor.Chocolate);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Coral"/> color.
        /// </summary>
        public static Pen Coral               => Get(KnownColor.Coral);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.CornflowerBlue"/> color.
        /// </summary>
        public static Pen CornflowerBlue      => Get(KnownColor.CornflowerBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Cornsilk"/> color.
        /// </summary>
        public static Pen Cornsilk            => Get(KnownColor.Cornsilk);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Crimson"/> color.
        /// </summary>
        public static Pen Crimson             => Get(KnownColor.Crimson);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Cyan"/> color.
        /// </summary>
        public static Pen Cyan                => Get(KnownColor.Cyan);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkBlue"/> color.
        /// </summary>
        public static Pen DarkBlue            => Get(KnownColor.DarkBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkCyan"/> color.
        /// </summary>
        public static Pen DarkCyan            => Get(KnownColor.DarkCyan);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkGoldenrod"/> color.
        /// </summary>
        public static Pen DarkGoldenrod       => Get(KnownColor.DarkGoldenrod);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkGray"/> color.
        /// </summary>
        public static Pen DarkGray            => Get(KnownColor.DarkGray);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkGreen"/> color.
        /// </summary>
        public static Pen DarkGreen           => Get(KnownColor.DarkGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkKhaki"/> color.
        /// </summary>
        public static Pen DarkKhaki           => Get(KnownColor.DarkKhaki);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkMagenta"/> color.
        /// </summary>
        public static Pen DarkMagenta         => Get(KnownColor.DarkMagenta);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkOliveGreen"/> color.
        /// </summary>
        public static Pen DarkOliveGreen      => Get(KnownColor.DarkOliveGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkOrange"/> color.
        /// </summary>
        public static Pen DarkOrange          => Get(KnownColor.DarkOrange);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkOrchid"/> color.
        /// </summary>
        public static Pen DarkOrchid          => Get(KnownColor.DarkOrchid);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkRed"/> color.
        /// </summary>
        public static Pen DarkRed             => Get(KnownColor.DarkRed);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkSalmon"/> color.
        /// </summary>
        public static Pen DarkSalmon          => Get(KnownColor.DarkSalmon);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkSeaGreen"/> color.
        /// </summary>
        public static Pen DarkSeaGreen        => Get(KnownColor.DarkSeaGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkSlateBlue"/> color.
        /// </summary>
        public static Pen DarkSlateBlue       => Get(KnownColor.DarkSlateBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkSlateGray"/> color.
        /// </summary>
        public static Pen DarkSlateGray       => Get(KnownColor.DarkSlateGray);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkTurquoise"/> color.
        /// </summary>
        public static Pen DarkTurquoise       => Get(KnownColor.DarkTurquoise);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DarkViolet"/> color.
        /// </summary>
        public static Pen DarkViolet          => Get(KnownColor.DarkViolet);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DeepPink"/> color.
        /// </summary>
        public static Pen DeepPink            => Get(KnownColor.DeepPink);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DeepSkyBlue"/> color.
        /// </summary>
        public static Pen DeepSkyBlue         => Get(KnownColor.DeepSkyBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DimGray"/> color.
        /// </summary>
        public static Pen DimGray             => Get(KnownColor.DimGray);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.DodgerBlue"/> color.
        /// </summary>
        public static Pen DodgerBlue          => Get(KnownColor.DodgerBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Firebrick"/> color.
        /// </summary>
        public static Pen Firebrick           => Get(KnownColor.Firebrick);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.FloralWhite"/> color.
        /// </summary>
        public static Pen FloralWhite         => Get(KnownColor.FloralWhite);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.ForestGreen"/> color.
        /// </summary>
        public static Pen ForestGreen         => Get(KnownColor.ForestGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Fuchsia"/> color.
        /// </summary>
        public static Pen Fuchsia             => Get(KnownColor.Fuchsia);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Gainsboro"/> color.
        /// </summary>
        public static Pen Gainsboro           => Get(KnownColor.Gainsboro);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.GhostWhite"/> color.
        /// </summary>
        public static Pen GhostWhite          => Get(KnownColor.GhostWhite);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Gold"/> color.
        /// </summary>
        public static Pen Gold                => Get(KnownColor.Gold);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Goldenrod"/> color.
        /// </summary>
        public static Pen Goldenrod           => Get(KnownColor.Goldenrod);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Gray"/> color.
        /// </summary>
        public static Pen Gray                => Get(KnownColor.Gray);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Green"/> color.
        /// </summary>
        public static Pen Green               => Get(KnownColor.Green);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.GreenYellow"/> color.
        /// </summary>
        public static Pen GreenYellow         => Get(KnownColor.GreenYellow);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Honeydew"/> color.
        /// </summary>
        public static Pen Honeydew            => Get(KnownColor.Honeydew);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.HotPink"/> color.
        /// </summary>
        public static Pen HotPink             => Get(KnownColor.HotPink);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.IndianRed"/> color.
        /// </summary>
        public static Pen IndianRed           => Get(KnownColor.IndianRed);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Indigo"/> color.
        /// </summary>
        public static Pen Indigo              => Get(KnownColor.Indigo);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Ivory"/> color.
        /// </summary>
        public static Pen Ivory               => Get(KnownColor.Ivory);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Khaki"/> color.
        /// </summary>
        public static Pen Khaki               => Get(KnownColor.Khaki);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Lavender"/> color.
        /// </summary>
        public static Pen Lavender            => Get(KnownColor.Lavender);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LavenderBlush"/> color.
        /// </summary>
        public static Pen LavenderBlush       => Get(KnownColor.LavenderBlush);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LawnGreen"/> color.
        /// </summary>
        public static Pen LawnGreen           => Get(KnownColor.LawnGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LemonChiffon"/> color.
        /// </summary>
        public static Pen LemonChiffon        => Get(KnownColor.LemonChiffon);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightBlue"/> color.
        /// </summary>
        public static Pen LightBlue           => Get(KnownColor.LightBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightCoral"/> color.
        /// </summary>
        public static Pen LightCoral          => Get(KnownColor.LightCoral);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightCyan"/> color.
        /// </summary>
        public static Pen LightCyan           => Get(KnownColor.LightCyan);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightGoldenrodYellow"/> color.
        /// </summary>
        public static Pen LightGoldenrodYellow => Get(KnownColor.LightGoldenrodYellow);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightGray"/> color.
        /// </summary>
        public static Pen LightGray           => Get(KnownColor.LightGray);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightGreen"/> color.
        /// </summary>
        public static Pen LightGreen          => Get(KnownColor.LightGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightPink"/> color.
        /// </summary>
        public static Pen LightPink           => Get(KnownColor.LightPink);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightSalmon"/> color.
        /// </summary>
        public static Pen LightSalmon         => Get(KnownColor.LightSalmon);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightSeaGreen"/> color.
        /// </summary>
        public static Pen LightSeaGreen       => Get(KnownColor.LightSeaGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightSkyBlue"/> color.
        /// </summary>
        public static Pen LightSkyBlue        => Get(KnownColor.LightSkyBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightSlateGray"/> color.
        /// </summary>
        public static Pen LightSlateGray      => Get(KnownColor.LightSlateGray);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightSteelBlue"/> color.
        /// </summary>
        public static Pen LightSteelBlue      => Get(KnownColor.LightSteelBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LightYellow"/> color.
        /// </summary>
        public static Pen LightYellow         => Get(KnownColor.LightYellow);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Lime"/> color.
        /// </summary>
        public static Pen Lime                => Get(KnownColor.Lime);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.LimeGreen"/> color.
        /// </summary>
        public static Pen LimeGreen           => Get(KnownColor.LimeGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Linen"/> color.
        /// </summary>
        public static Pen Linen               => Get(KnownColor.Linen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Magenta"/> color.
        /// </summary>
        public static Pen Magenta             => Get(KnownColor.Magenta);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Maroon"/> color.
        /// </summary>
        public static Pen Maroon              => Get(KnownColor.Maroon);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumAquamarine"/> color.
        /// </summary>
        public static Pen MediumAquamarine    => Get(KnownColor.MediumAquamarine);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumBlue"/> color.
        /// </summary>
        public static Pen MediumBlue          => Get(KnownColor.MediumBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumOrchid"/> color.
        /// </summary>
        public static Pen MediumOrchid        => Get(KnownColor.MediumOrchid);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumPurple"/> color.
        /// </summary>
        public static Pen MediumPurple        => Get(KnownColor.MediumPurple);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumSeaGreen"/> color.
        /// </summary>
        public static Pen MediumSeaGreen      => Get(KnownColor.MediumSeaGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumSlateBlue"/> color.
        /// </summary>
        public static Pen MediumSlateBlue     => Get(KnownColor.MediumSlateBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumSpringGreen"/> color.
        /// </summary>
        public static Pen MediumSpringGreen   => Get(KnownColor.MediumSpringGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumTurquoise"/> color.
        /// </summary>
        public static Pen MediumTurquoise     => Get(KnownColor.MediumTurquoise);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MediumVioletRed"/> color.
        /// </summary>
        public static Pen MediumVioletRed     => Get(KnownColor.MediumVioletRed);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MidnightBlue"/> color.
        /// </summary>
        public static Pen MidnightBlue        => Get(KnownColor.MidnightBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MintCream"/> color.
        /// </summary>
        public static Pen MintCream           => Get(KnownColor.MintCream);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.MistyRose"/> color.
        /// </summary>
        public static Pen MistyRose           => Get(KnownColor.MistyRose);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Moccasin"/> color.
        /// </summary>
        public static Pen Moccasin            => Get(KnownColor.Moccasin);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.NavajoWhite"/> color.
        /// </summary>
        public static Pen NavajoWhite         => Get(KnownColor.NavajoWhite);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Navy"/> color.
        /// </summary>
        public static Pen Navy                => Get(KnownColor.Navy);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.OldLace"/> color.
        /// </summary>
        public static Pen OldLace             => Get(KnownColor.OldLace);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Olive"/> color.
        /// </summary>
        public static Pen Olive               => Get(KnownColor.Olive);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.OliveDrab"/> color.
        /// </summary>
        public static Pen OliveDrab           => Get(KnownColor.OliveDrab);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Orange"/> color.
        /// </summary>
        public static Pen Orange              => Get(KnownColor.Orange);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.OrangeRed"/> color.
        /// </summary>
        public static Pen OrangeRed           => Get(KnownColor.OrangeRed);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Orchid"/> color.
        /// </summary>
        public static Pen Orchid              => Get(KnownColor.Orchid);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.PaleGoldenrod"/> color.
        /// </summary>
        public static Pen PaleGoldenrod       => Get(KnownColor.PaleGoldenrod);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.PaleGreen"/> color.
        /// </summary>
        public static Pen PaleGreen           => Get(KnownColor.PaleGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.PaleTurquoise"/> color.
        /// </summary>
        public static Pen PaleTurquoise       => Get(KnownColor.PaleTurquoise);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.PaleVioletRed"/> color.
        /// </summary>
        public static Pen PaleVioletRed       => Get(KnownColor.PaleVioletRed);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.PapayaWhip"/> color.
        /// </summary>
        public static Pen PapayaWhip          => Get(KnownColor.PapayaWhip);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.PeachPuff"/> color.
        /// </summary>
        public static Pen PeachPuff           => Get(KnownColor.PeachPuff);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Peru"/> color.
        /// </summary>
        public static Pen Peru                => Get(KnownColor.Peru);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Pink"/> color.
        /// </summary>
        public static Pen Pink                => Get(KnownColor.Pink);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Plum"/> color.
        /// </summary>
        public static Pen Plum                => Get(KnownColor.Plum);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.PowderBlue"/> color.
        /// </summary>
        public static Pen PowderBlue          => Get(KnownColor.PowderBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Purple"/> color.
        /// </summary>
        public static Pen Purple              => Get(KnownColor.Purple);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Red"/> color.
        /// </summary>
        public static Pen Red                 => Get(KnownColor.Red);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.RosyBrown"/> color.
        /// </summary>
        public static Pen RosyBrown           => Get(KnownColor.RosyBrown);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.RoyalBlue"/> color.
        /// </summary>
        public static Pen RoyalBlue           => Get(KnownColor.RoyalBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SaddleBrown"/> color.
        /// </summary>
        public static Pen SaddleBrown         => Get(KnownColor.SaddleBrown);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Salmon"/> color.
        /// </summary>
        public static Pen Salmon              => Get(KnownColor.Salmon);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SandyBrown"/> color.
        /// </summary>
        public static Pen SandyBrown          => Get(KnownColor.SandyBrown);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SeaGreen"/> color.
        /// </summary>
        public static Pen SeaGreen            => Get(KnownColor.SeaGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SeaShell"/> color.
        /// </summary>
        public static Pen SeaShell            => Get(KnownColor.SeaShell);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Sienna"/> color.
        /// </summary>
        public static Pen Sienna              => Get(KnownColor.Sienna);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Silver"/> color.
        /// </summary>
        public static Pen Silver              => Get(KnownColor.Silver);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SkyBlue"/> color.
        /// </summary>
        public static Pen SkyBlue             => Get(KnownColor.SkyBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SlateBlue"/> color.
        /// </summary>
        public static Pen SlateBlue           => Get(KnownColor.SlateBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SlateGray"/> color.
        /// </summary>
        public static Pen SlateGray           => Get(KnownColor.SlateGray);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Snow"/> color.
        /// </summary>
        public static Pen Snow                => Get(KnownColor.Snow);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SpringGreen"/> color.
        /// </summary>
        public static Pen SpringGreen         => Get(KnownColor.SpringGreen);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.SteelBlue"/> color.
        /// </summary>
        public static Pen SteelBlue           => Get(KnownColor.SteelBlue);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Tan"/> color.
        /// </summary>
        public static Pen Tan                 => Get(KnownColor.Tan);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Teal"/> color.
        /// </summary>
        public static Pen Teal                => Get(KnownColor.Teal);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Thistle"/> color.
        /// </summary>
        public static Pen Thistle             => Get(KnownColor.Thistle);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Tomato"/> color.
        /// </summary>
        public static Pen Tomato              => Get(KnownColor.Tomato);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Turquoise"/> color.
        /// </summary>
        public static Pen Turquoise           => Get(KnownColor.Turquoise);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Violet"/> color.
        /// </summary>
        public static Pen Violet              => Get(KnownColor.Violet);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Wheat"/> color.
        /// </summary>
        public static Pen Wheat               => Get(KnownColor.Wheat);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.White"/> color.
        /// </summary>
        public static Pen White               => Get(KnownColor.White);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.WhiteSmoke"/> color.
        /// </summary>
        public static Pen WhiteSmoke          => Get(KnownColor.WhiteSmoke);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.Yellow"/> color.
        /// </summary>
        public static Pen Yellow              => Get(KnownColor.Yellow);

        /// <summary>
        /// Gets a <see cref="Pen"/> with <see cref="KnownColor.YellowGreen"/> color.
        /// </summary>
        public static Pen YellowGreen         => Get(KnownColor.YellowGreen);
    }
}
