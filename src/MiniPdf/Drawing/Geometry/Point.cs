using System;

namespace MiniSoftware.Drawing.Geometry
{
    /// <summary>Represents an ordered pair of integer x- and y-coordinates.</summary>
    /// <remarks>
    /// A <see cref="Point"/> represents a location in a two-dimensional coordinate system.
    /// X and Y are public fields for performance reasons, matching the System.Drawing API.
    /// </remarks>
    public struct Point : IEquatable<Point>
    {
        /// <summary>
        /// Gets or sets the x-coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// Gets or sets the y-coordinate.
        /// </summary>
        public int Y;

        /// <summary>
        /// Initializes a new <see cref="Point"/> with the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public Point(int x, int y) { X = x; Y = y; }

        /// <summary>
        /// Initializes a new <see cref="Point"/> from a <see cref="Size"/>.
        /// </summary>
        /// <param name="sz">The size to convert to a point.</param>
        public Point(Size sz)      { X = sz.Width; Y = sz.Height; }

        /// <summary>
        /// Gets an empty <see cref="Point"/> (0, 0).
        /// </summary>
        public static readonly Point Empty = default;

        /// <summary>
        /// Gets a value indicating whether this point is empty (0, 0).
        /// </summary>
        public bool IsEmpty => X == 0 && Y == 0;

        /// <summary>
        /// Translates the point by the specified offsets.
        /// </summary>
        /// <param name="dx">The horizontal offset.</param>
        /// <param name="dy">The vertical offset.</param>
        public void Offset(int dx, int dy) { X += dx; Y += dy; }

        /// <summary>
        /// Translates the point by the offsets of another point.
        /// </summary>
        /// <param name="p">The point containing the offsets.</param>
        public void Offset(Point p)        { X += p.X; Y += p.Y; }

        /// <summary>
        /// Adds the specified point and size.
        /// </summary>
        /// <param name="pt">The point to add to.</param>
        /// <param name="sz">The size to add.</param>
        /// <returns>The resulting point.</returns>
        public static Point Add(Point pt, Size sz)      => new Point(pt.X + sz.Width,  pt.Y + sz.Height);

        /// <summary>
        /// Subtracts the specified size from the point.
        /// </summary>
        /// <param name="pt">The point to subtract from.</param>
        /// <param name="sz">The size to subtract.</param>
        /// <returns>The resulting point.</returns>
        public static Point Subtract(Point pt, Size sz) => new Point(pt.X - sz.Width,  pt.Y - sz.Height);

        /// <summary>
        /// Adds a point and a size.
        /// </summary>
        public static Point operator +(Point pt, Size sz) => Add(pt, sz);

        /// <summary>
        /// Subtracts a size from a point.
        /// </summary>
        public static Point operator -(Point pt, Size sz) => Subtract(pt, sz);

        /// <summary>
        /// Implicitly converts a <see cref="Point"/> to a <see cref="PointF"/>.
        /// </summary>
        public static implicit operator PointF(Point p) => new PointF(p.X, p.Y);

        /// <summary>
        /// Explicitly converts a <see cref="Point"/> to a <see cref="Size"/>.
        /// </summary>
        public static explicit operator Size(Point p)   => new Size(p.X, p.Y);

        /// <summary>
        /// Compares two points for equality.
        /// </summary>
        public static bool operator ==(Point left, Point right) => left.X == right.X && left.Y == right.Y;

        /// <summary>
        /// Compares two points for inequality.
        /// </summary>
        public static bool operator !=(Point left, Point right) => !(left == right);

        /// <summary>
        /// Indicates whether this point is equal to another point.
        /// </summary>
        /// <param name="other">The point to compare.</param>
        /// <returns><c>true</c> if the points are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(Point other) => this == other;

        /// <summary>
        /// Indicates whether this point is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is Point p && this == p;

        /// <summary>
        /// Returns a hash code for this point.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked { return X * 397 ^ Y; }
        }

        /// <summary>
        /// Returns a string representation of this point.
        /// </summary>
        /// <returns>A string in the format "{X=value, Y=value}".</returns>
        public override string ToString() => $"{{X={X}, Y={Y}}}";
    }
}
