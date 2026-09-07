namespace MiniSoftware.Drawing.Font.Core
{
    /// <summary>
    /// Describes a single design-variation axis in a variable font (OpenType "fvar" table).
    /// </summary>
    public sealed class FontVariationAxis
    {
        /// <summary>Four-character axis tag, e.g. "wght", "wdth", "ital".</summary>
        public string Tag { get; }

        /// <summary>Minimum user-space value along this axis.</summary>
        public double MinValue { get; }

        /// <summary>Default user-space value along this axis.</summary>
        public double DefaultValue { get; }

        /// <summary>Maximum user-space value along this axis.</summary>
        public double MaxValue { get; }

        /// <summary>Human-readable name of the axis (resolved from well-known tags or the name table).</summary>
        public string AxisName { get; }

        /// <summary>
        /// Initializes a new <see cref="FontVariationAxis"/> with the specified parameters.
        /// </summary>
        /// <param name="tag">Four-character axis tag, e.g. "wght", "wdth", "ital".</param>
        /// <param name="minValue">Minimum user-space value along this axis.</param>
        /// <param name="defaultValue">Default user-space value along this axis.</param>
        /// <param name="maxValue">Maximum user-space value along this axis.</param>
        /// <param name="axisName">Human-readable name of the axis.</param>
        public FontVariationAxis(string tag, double minValue, double defaultValue, double maxValue, string axisName)
        {
            Tag          = tag;
            MinValue     = minValue;
            DefaultValue = defaultValue;
            MaxValue     = maxValue;
            AxisName     = axisName;
        }
    }
}
