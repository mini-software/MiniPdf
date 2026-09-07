using System.Collections.Generic;

namespace MiniPdf.Drawing.Font.Core
{
    /// <summary>
    /// Represents a named instance (a specific set of axis coordinates) in a variable font.
    /// </summary>
    public sealed class FontVariationInstance
    {
        /// <summary>Human-readable name for this instance (e.g. "Bold", "Light Condensed").</summary>
        public string Name { get; }

        /// <summary>
        /// Per-axis coordinate values for this instance, keyed by axis tag (e.g. "wght" → 700.0).
        /// </summary>
        public IReadOnlyDictionary<string, double> Coordinates { get; }

        /// <summary>
        /// Initializes a new <see cref="FontVariationInstance"/> with the specified name and coordinates.
        /// </summary>
        /// <param name="name">Human-readable name for this instance.</param>
        /// <param name="coordinates">Per-axis coordinate values for this instance.</param>
        public FontVariationInstance(string name, IReadOnlyDictionary<string, double> coordinates)
        {
            Name        = name;
            Coordinates = coordinates;
        }
    }
}
