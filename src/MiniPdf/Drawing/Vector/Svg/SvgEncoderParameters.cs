namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// Specifies how text is emitted in SVG output.
    /// </summary>
    public enum SvgTextMode
    {
        /// <summary>
        /// Vectorize glyphs as <c>&lt;path&gt;</c> elements (default).
        /// Pixel-identical fidelity; no font dependency on viewer.
        /// </summary>
        Path = 0,

        /// <summary>
        /// Emit <c>&lt;text&gt;</c>/<c>&lt;tspan&gt;</c> elements with font
        /// attributes. Editable/smaller but best-effort fidelity.
        /// </summary>
        Text = 1,
    }

    /// <summary>
    /// Encoder parameters controlling SVG output.
    /// </summary>
    public sealed class SvgEncoderParameters
    {
        /// <summary>
        /// Gets or sets the text emission mode (default: Path).
        /// </summary>
        public SvgTextMode TextMode { get; set; } = SvgTextMode.Path;

        /// <summary>
        /// Gets or sets whether to pretty-print with indentation (default: true).
        /// </summary>
        public bool Indent { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to embed images as inline base64 data URIs
        /// (default: true). If false, images are referenced externally.
        /// </summary>
        public bool EmbedImages { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of decimal places for coordinate output
        /// (default: 4).
        /// </summary>
        public int CoordinatePrecision { get; set; } = 4;

        /// <summary>
        /// Gets the default encoder parameters.
        /// </summary>
        public static SvgEncoderParameters Default => new SvgEncoderParameters();
    }
}