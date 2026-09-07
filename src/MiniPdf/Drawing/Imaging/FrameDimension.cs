using System;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Provides properties that represent dimensions of an image, such as page number or time.
    /// </summary>
    public sealed class FrameDimension
    {
        /// <summary>
        /// Gets the GUID that identifies this frame dimension.
        /// </summary>
        public Guid Guid { get; }

        private FrameDimension(string guid) { Guid = new Guid(guid); }

        /// <summary>
        /// Gets the time dimension for multi-frame images.
        /// </summary>
        public static readonly FrameDimension Time       = new FrameDimension("6aedbd6d-3fb5-418a-83a6-7f45229dc872");

        /// <summary>
        /// Gets the resolution dimension for multi-frame images.
        /// </summary>
        public static readonly FrameDimension Resolution = new FrameDimension("84236f7b-3bd3-428f-8dab-4ea1439ca315");

        /// <summary>
        /// Gets the page dimension for multi-frame images.
        /// </summary>
        public static readonly FrameDimension Page       = new FrameDimension("7462dc86-6180-4c7e-8e3f-ee7333a7a483");

        /// <summary>
        /// Determines whether the specified object is equal to this <see cref="FrameDimension"/>.
        /// </summary>
        public override bool Equals(object? obj) => obj is FrameDimension fd && fd.Guid == Guid;

        /// <summary>
        /// Serves as a hash function for this <see cref="FrameDimension"/>.
        /// </summary>
        public override int  GetHashCode() => Guid.GetHashCode();

        /// <summary>
        /// Returns a string representation of this <see cref="FrameDimension"/>.
        /// </summary>
        public override string ToString()  => Guid.ToString();
    }
}
