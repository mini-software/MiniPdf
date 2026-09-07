namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Stores the blend definition for gradient brushes including position and factor arrays.
    /// </summary>
    public sealed class Blend
    {
        /// <summary>
        /// Initializes a new <see cref="Blend"/> with empty factor and position arrays.
        /// </summary>
        public Blend()
        {
            Factors   = System.Array.Empty<float>();
            Positions = System.Array.Empty<float>();
        }

        /// <summary>
        /// Initializes a new <see cref="Blend"/> with the specified number of elements.
        /// </summary>
        /// <param name="count">The number of blend positions and factors.</param>
        public Blend(int count)
        {
            Factors   = new float[count];
            Positions = new float[count];
        }

        /// <summary>
        /// Gets or sets the array of blend factors.
        /// </summary>
        public float[] Factors   { get; set; }

        /// <summary>
        /// Gets or sets the array of blend positions.
        /// </summary>
        public float[] Positions { get; set; }
    }
}
