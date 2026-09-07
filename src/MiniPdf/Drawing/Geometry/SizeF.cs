using System;

namespace MiniSoftware.Drawing.Geometry
{
    /// <summary>Represents the size of a rectangular region with an ordered pair of floats.</summary>
    /// <remarks>
    /// A <see cref="SizeF"/> represents the width and height of a rectangle.
    /// Width and Height are public fields for performance reasons, matching the System.Drawing API.
    /// </remarks>
    public struct SizeF : IEquatable<SizeF>
    {
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public float Width;

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public float Height;

        /// <summary>
        /// Initializes a new <see cref="SizeF"/> with the specified width and height.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public SizeF(float width, float height) { Width = width; Height = height; }

        /// <summary>
        /// Initializes a new <see cref="SizeF"/> from a <see cref="PointF"/>.
        /// </summary>
        /// <param name="pt">The point to convert to a size.</param>
        public SizeF(PointF pt)                 { Width = pt.X; Height = pt.Y; }

        /// <summary>
        /// Initializes a new <see cref="SizeF"/> as a copy of another size.
        /// </summary>
        /// <param name="sz">The size to copy.</param>
        public SizeF(SizeF sz)                  { Width = sz.Width; Height = sz.Height; }

        /// <summary>
        /// Gets an empty <see cref="SizeF"/> (0, 0).
        /// </summary>
        public static readonly SizeF Empty = default;

        /// <summary>
        /// Gets a value indicating whether this size is empty (0, 0).
        /// </summary>
        public bool IsEmpty => Width == 0f && Height == 0f;

        /// <summary>
        /// Truncates each dimension to the nearest integer toward zero.
        /// </summary>
        /// <returns>A new <see cref="Size"/> with truncated dimensions.</returns>
        public Size ToSize() => new Size((int)Width, (int)Height);

        /// <summary>
        /// Converts this <see cref="SizeF"/> to a <see cref="PointF"/>.
        /// </summary>
        /// <returns>A new <see cref="PointF"/> with the same dimensions.</returns>
        public PointF ToPointF() => new PointF(Width, Height);

        /// <summary>
        /// Adds two sizes.
        /// </summary>
        /// <param name="sz1">The first size.</param>
        /// <param name="sz2">The second size.</param>
        /// <returns>The resulting size.</returns>
        public static SizeF Add(SizeF sz1, SizeF sz2)      => new SizeF(sz1.Width + sz2.Width, sz1.Height + sz2.Height);

        /// <summary>
        /// Subtracts the second size from the first.
        /// </summary>
        /// <param name="sz1">The size to subtract from.</param>
        /// <param name="sz2">The size to subtract.</param>
        /// <returns>The resulting size.</returns>
        public static SizeF Subtract(SizeF sz1, SizeF sz2) => new SizeF(sz1.Width - sz2.Width, sz1.Height - sz2.Height);

        /// <summary>
        /// Adds two sizes.
        /// </summary>
        public static SizeF operator +(SizeF sz1, SizeF sz2) => Add(sz1, sz2);

        /// <summary>
        /// Subtracts two sizes.
        /// </summary>
        public static SizeF operator -(SizeF sz1, SizeF sz2) => Subtract(sz1, sz2);

        /// <summary>
        /// Compares two sizes for equality.
        /// </summary>
        public static bool operator ==(SizeF left, SizeF right) => left.Width == right.Width && left.Height == right.Height;

        /// <summary>
        /// Compares two sizes for inequality.
        /// </summary>
        public static bool operator !=(SizeF left, SizeF right) => !(left == right);

        /// <summary>
        /// Indicates whether this size is equal to another size.
        /// </summary>
        /// <param name="other">The size to compare.</param>
        /// <returns><c>true</c> if the sizes are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(SizeF other) => this == other;

        /// <summary>
        /// Indicates whether this size is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is SizeF s && this == s;

        /// <summary>
        /// Returns a hash code for this size.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked { return Width.GetHashCode() * 397 ^ Height.GetHashCode(); }
        }

        /// <summary>
        /// Returns a string representation of this size.
        /// </summary>
        /// <returns>A string in the format "{Width=value, Height=value}".</returns>
        public override string ToString() => $"{{Width={Width}, Height={Height}}}";
    }
}
