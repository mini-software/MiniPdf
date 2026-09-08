using System;

namespace MiniSoftware.Drawing.Geometry
{
    /// <summary>Represents the size of a rectangular region with an ordered pair of integers.</summary>
    /// <remarks>
    /// A <see cref="Size"/> represents the width and height of a rectangle.
    /// Width and Height are public fields for performance reasons, matching the System.Drawing API.
    /// </remarks>
    public struct Size : IEquatable<Size>
    {
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height;

        /// <summary>
        /// Initializes a new <see cref="Size"/> with the specified width and height.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Size(int width, int height) { Width = width; Height = height; }

        /// <summary>
        /// Initializes a new <see cref="Size"/> from a <see cref="Point"/>.
        /// </summary>
        /// <param name="pt">The point to convert to a size.</param>
        public Size(Point pt)              { Width = pt.X;  Height = pt.Y; }

        /// <summary>
        /// Gets an empty <see cref="Size"/> (0, 0).
        /// </summary>
        public static readonly Size Empty = default;

        /// <summary>
        /// Gets a value indicating whether this size is empty (0, 0).
        /// </summary>
        public bool IsEmpty => Width == 0 && Height == 0;

        /// <summary>
        /// Converts this <see cref="Size"/> to a <see cref="SizeF"/>.
        /// </summary>
        /// <returns>A new <see cref="SizeF"/> with the same dimensions.</returns>
        public SizeF ToSizeF() => new SizeF(Width, Height);

        /// <summary>
        /// Adds two sizes.
        /// </summary>
        /// <param name="sz1">The first size.</param>
        /// <param name="sz2">The second size.</param>
        /// <returns>The resulting size.</returns>
        public static Size Add(Size sz1, Size sz2)      => new Size(sz1.Width + sz2.Width, sz1.Height + sz2.Height);

        /// <summary>
        /// Subtracts the second size from the first.
        /// </summary>
        /// <param name="sz1">The size to subtract from.</param>
        /// <param name="sz2">The size to subtract.</param>
        /// <returns>The resulting size.</returns>
        public static Size Subtract(Size sz1, Size sz2) => new Size(sz1.Width - sz2.Width, sz1.Height - sz2.Height);

        /// <summary>
        /// Adds two sizes.
        /// </summary>
        public static Size operator +(Size sz1, Size sz2) => Add(sz1, sz2);

        /// <summary>
        /// Subtracts two sizes.
        /// </summary>
        public static Size operator -(Size sz1, Size sz2) => Subtract(sz1, sz2);

        /// <summary>
        /// Explicitly converts a <see cref="Size"/> to a <see cref="Point"/>.
        /// </summary>
        public static explicit operator Point(Size sz) => new Point(sz.Width, sz.Height);

        /// <summary>
        /// Implicitly converts a <see cref="Size"/> to a <see cref="SizeF"/>.
        /// </summary>
        public static implicit operator SizeF(Size sz) => new SizeF(sz.Width, sz.Height);

        /// <summary>
        /// Compares two sizes for equality.
        /// </summary>
        public static bool operator ==(Size left, Size right) => left.Width == right.Width && left.Height == right.Height;

        /// <summary>
        /// Compares two sizes for inequality.
        /// </summary>
        public static bool operator !=(Size left, Size right) => !(left == right);

        /// <summary>
        /// Indicates whether this size is equal to another size.
        /// </summary>
        /// <param name="other">The size to compare.</param>
        /// <returns><c>true</c> if the sizes are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(Size other) => this == other;

        /// <summary>
        /// Indicates whether this size is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is Size s && this == s;

        /// <summary>
        /// Returns a hash code for this size.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked { return Width * 397 ^ Height; }
        }

        /// <summary>
        /// Returns a string representation of this size.
        /// </summary>
        /// <returns>A string in the format "{Width=value, Height=value}".</returns>
        public override string ToString() => $"{{Width={Width}, Height={Height}}}";
    }
}
