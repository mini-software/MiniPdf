using System;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// Represents a metadata property item for an image.
    /// </summary>
    public sealed class PropertyItem
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public int    Id    { get; set; }

        /// <summary>
        /// Gets or sets the length of the property value in bytes.
        /// </summary>
        public int    Len   { get; set; }

        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        public short  Type  { get; set; }

        /// <summary>
        /// Gets or sets the property value as a byte array.
        /// </summary>
        public byte[] Value { get; set; } = System.Array.Empty<byte>();
    }
}
