using System;

namespace MiniPdf.Drawing.Geometry
{
    /// <summary>
    /// Represents the location and size of a rectangular region, using integer coordinates.
    /// </summary>
    /// <remarks>
    /// A <see cref="Rectangle"/> defines a region with X, Y, Width, and Height.
    /// X and Y represent the top-left corner; Width and Height represent the dimensions.
    /// </remarks>
    public struct Rectangle : IEquatable<Rectangle>
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the top-left corner.
        /// </summary>
        public int X;

        /// <summary>
        /// Gets or sets the y-coordinate of the top-left corner.
        /// </summary>
        public int Y;

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height;

        /// <summary>
        /// Initializes a new <see cref="Rectangle"/> with the specified location and size.
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left corner.</param>
        /// <param name="y">The y-coordinate of the top-left corner.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Rectangle(int x, int y, int width, int height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        /// <summary>
        /// Initializes a new <see cref="Rectangle"/> from a point and a size.
        /// </summary>
        /// <param name="location">The top-left corner.</param>
        /// <param name="size">The dimensions.</param>
        public Rectangle(Point location, Size size)
        {
            X = location.X; Y = location.Y; Width = size.Width; Height = size.Height;
        }

        /// <summary>
        /// Gets an empty <see cref="Rectangle"/> (0, 0, 0, 0).
        /// </summary>
        public static readonly Rectangle Empty = default;

        // ── Derived edges ────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the x-coordinate of the left edge.
        /// </summary>
        public int Left   => X;

        /// <summary>
        /// Gets the y-coordinate of the top edge.
        /// </summary>
        public int Top    => Y;

        /// <summary>
        /// Gets the x-coordinate of the right edge.
        /// </summary>
        public int Right  => X + Width;

        /// <summary>
        /// Gets the y-coordinate of the bottom edge.
        /// </summary>
        public int Bottom => Y + Height;

        /// <summary>
        /// Gets a value indicating whether this rectangle is empty (Width or Height less than or equal to 0).
        /// </summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;

        /// <summary>
        /// Gets or sets the top-left corner of this rectangle.
        /// </summary>
        public Point Location
        {
            get => new Point(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        /// <summary>
        /// Gets or sets the dimensions of this rectangle.
        /// </summary>
        public Size Size
        {
            get => new Size(Width, Height);
            set { Width = value.Width; Height = value.Height; }
        }

        // ── Factory ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a rectangle from left, top, right, and bottom coordinates.
        /// </summary>
        /// <param name="left">The x-coordinate of the left edge.</param>
        /// <param name="top">The y-coordinate of the top edge.</param>
        /// <param name="right">The x-coordinate of the right edge.</param>
        /// <param name="bottom">The y-coordinate of the bottom edge.</param>
        /// <returns>A new <see cref="Rectangle"/>.</returns>
        public static Rectangle FromLTRB(int left, int top, int right, int bottom)
            => new Rectangle(left, top, right - left, bottom - top);

        // ── Containment / intersection ────────────────────────────────────────────

        /// <summary>
        /// Determines whether the specified point is contained within this rectangle.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns><c>true</c> if the point is within the rectangle; otherwise, <c>false</c>.</returns>
        public bool Contains(int x, int y)
            => x >= X && x < X + Width && y >= Y && y < Y + Height;

        /// <summary>
        /// Determines whether the specified point is contained within this rectangle.
        /// </summary>
        /// <param name="pt">The point to check.</param>
        /// <returns><c>true</c> if the point is within the rectangle; otherwise, <c>false</c>.</returns>
        public bool Contains(Point pt)
            => Contains(pt.X, pt.Y);

        /// <summary>
        /// Determines whether the specified rectangle is fully contained within this rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns><c>true</c> if the rectangle is fully contained; otherwise, <c>false</c>.</returns>
        public bool Contains(Rectangle rect)
            => rect.X >= X && rect.Y >= Y && rect.Right <= Right && rect.Bottom <= Bottom;

        /// <summary>
        /// Determines whether this rectangle intersects with another rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns><c>true</c> if the rectangles intersect; otherwise, <c>false</c>.</returns>
        public bool IntersectsWith(Rectangle rect)
            => rect.X < Right && X < rect.Right && rect.Y < Bottom && Y < rect.Bottom;

        // ── Set operations (instance — mutate this) ───────────────────────────────

        /// <summary>
        /// Intersects this rectangle with another, modifying this rectangle to the result.
        /// </summary>
        /// <param name="rect">The rectangle to intersect with.</param>
        public void Intersect(Rectangle rect)
        {
            Rectangle result = Intersect(this, rect);
            X = result.X; Y = result.Y; Width = result.Width; Height = result.Height;
        }

        /// <summary>
        /// Inflates this rectangle by the specified width and height.
        /// </summary>
        /// <param name="width">The horizontal inflation amount.</param>
        /// <param name="height">The vertical inflation amount.</param>
        public void Inflate(int width, int height)
        {
            X      -= width;
            Y      -= height;
            Width  += 2 * width;
            Height += 2 * height;
        }

        /// <summary>
        /// Inflates this rectangle by the specified size.
        /// </summary>
        /// <param name="size">The size to inflate by.</param>
        public void Inflate(Size size) => Inflate(size.Width, size.Height);

        /// <summary>
        /// Offsets this rectangle by the specified horizontal and vertical amounts.
        /// </summary>
        /// <param name="dx">The horizontal offset.</param>
        /// <param name="dy">The vertical offset.</param>
        public void Offset(int dx, int dy) { X += dx; Y += dy; }

        /// <summary>
        /// Offsets this rectangle by the specified point.
        /// </summary>
        /// <param name="pos">The point containing the offsets.</param>
        public void Offset(Point pos)      { X += pos.X; Y += pos.Y; }

        // ── Set operations (static — return new Rectangle) ────────────────────────

        /// <summary>
        /// Returns the intersection of two rectangles.
        /// </summary>
        /// <param name="a">The first rectangle.</param>
        /// <param name="b">The second rectangle.</param>
        /// <returns>The intersection rectangle, or <see cref="Empty"/> if they don't intersect.</returns>
        public static Rectangle Intersect(Rectangle a, Rectangle b)
        {
            int x      = Math.Max(a.X,      b.X);
            int y      = Math.Max(a.Y,      b.Y);
            int right  = Math.Min(a.Right,  b.Right);
            int bottom = Math.Min(a.Bottom, b.Bottom);
            return right > x && bottom > y
                ? new Rectangle(x, y, right - x, bottom - y)
                : Empty;
        }

        /// <summary>
        /// Returns the union of two rectangles.
        /// </summary>
        /// <param name="a">The first rectangle.</param>
        /// <param name="b">The second rectangle.</param>
        /// <returns>The smallest rectangle containing both input rectangles.</returns>
        public static Rectangle Union(Rectangle a, Rectangle b)
        {
            int x      = Math.Min(a.X,      b.X);
            int y      = Math.Min(a.Y,      b.Y);
            int right  = Math.Max(a.Right,  b.Right);
            int bottom = Math.Max(a.Bottom, b.Bottom);
            return new Rectangle(x, y, right - x, bottom - y);
        }

        /// <summary>
        /// Returns an inflated copy of the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to inflate.</param>
        /// <param name="x">The horizontal inflation amount.</param>
        /// <param name="y">The vertical inflation amount.</param>
        /// <returns>The inflated rectangle.</returns>
        public static Rectangle Inflate(Rectangle rect, int x, int y)
        {
            Rectangle r = rect;
            r.Inflate(x, y);
            return r;
        }

        // ── Conversion ────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts this <see cref="Rectangle"/> to a <see cref="RectangleF"/>.
        /// </summary>
        /// <returns>A new <see cref="RectangleF"/> with the same dimensions.</returns>
        public RectangleF ToRectangleF() => new RectangleF(X, Y, Width, Height);

        // ── Equality ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Indicates whether this rectangle is equal to another rectangle.
        /// </summary>
        /// <param name="other">The rectangle to compare.</param>
        /// <returns><c>true</c> if the rectangles are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(Rectangle other)
            => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        /// <summary>
        /// Indicates whether this rectangle is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is Rectangle r && Equals(r);

        /// <summary>
        /// Returns a hash code for this rectangle.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int h = X;
                h = h * 397 ^ Y;
                h = h * 397 ^ Width;
                h = h * 397 ^ Height;
                return h;
            }
        }

        /// <summary>
        /// Compares two rectangles for equality.
        /// </summary>
        public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);

        /// <summary>
        /// Compares two rectangles for inequality.
        /// </summary>
        public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);

        /// <summary>
        /// Returns a string representation of this rectangle.
        /// </summary>
        /// <returns>A string in the format "{X=value, Y=value, Width=value, Height=value}".</returns>
        public override string ToString()
            => $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }
}
