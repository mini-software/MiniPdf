using System;

namespace MiniSoftware.Drawing.Geometry
{
    /// <summary>
    /// Represents the location and size of a rectangular region, using float coordinates.
    /// </summary>
    /// <remarks>
    /// A <see cref="RectangleF"/> defines a region with X, Y, Width, and Height.
    /// X and Y represent the top-left corner; Width and Height represent the dimensions.
    /// </remarks>
    public struct RectangleF : IEquatable<RectangleF>
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the top-left corner.
        /// </summary>
        public float X;

        /// <summary>
        /// Gets or sets the y-coordinate of the top-left corner.
        /// </summary>
        public float Y;

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public float Width;

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public float Height;

        /// <summary>
        /// Initializes a new <see cref="RectangleF"/> with the specified location and size.
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left corner.</param>
        /// <param name="y">The y-coordinate of the top-left corner.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public RectangleF(float x, float y, float width, float height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        /// <summary>
        /// Initializes a new <see cref="RectangleF"/> from a point and a size.
        /// </summary>
        /// <param name="location">The top-left corner.</param>
        /// <param name="size">The dimensions.</param>
        public RectangleF(PointF location, SizeF size)
        {
            X = location.X; Y = location.Y; Width = size.Width; Height = size.Height;
        }

        /// <summary>
        /// Gets an empty <see cref="RectangleF"/> (0, 0, 0, 0).
        /// </summary>
        public static readonly RectangleF Empty = default;

        // ── Derived edges ─────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the x-coordinate of the left edge.
        /// </summary>
        public float Left   => X;

        /// <summary>
        /// Gets the y-coordinate of the top edge.
        /// </summary>
        public float Top    => Y;

        /// <summary>
        /// Gets the x-coordinate of the right edge.
        /// </summary>
        public float Right  => X + Width;

        /// <summary>
        /// Gets the y-coordinate of the bottom edge.
        /// </summary>
        public float Bottom => Y + Height;

        /// <summary>
        /// Gets a value indicating whether this rectangle is empty (Width or Height less than or equal to 0).
        /// </summary>
        public bool IsEmpty => Width <= 0f || Height <= 0f;

        /// <summary>
        /// Gets or sets the top-left corner of this rectangle.
        /// </summary>
        public PointF Location
        {
            get => new PointF(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        /// <summary>
        /// Gets or sets the dimensions of this rectangle.
        /// </summary>
        public SizeF Size
        {
            get => new SizeF(Width, Height);
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
        /// <returns>A new <see cref="RectangleF"/>.</returns>
        public static RectangleF FromLTRB(float left, float top, float right, float bottom)
            => new RectangleF(left, top, right - left, bottom - top);

        // ── Containment / intersection ────────────────────────────────────────────

        /// <summary>
        /// Determines whether the specified point is contained within this rectangle.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns><c>true</c> if the point is within the rectangle; otherwise, <c>false</c>.</returns>
        public bool Contains(float x, float y)
            => x >= X && x < X + Width && y >= Y && y < Y + Height;

        /// <summary>
        /// Determines whether the specified point is contained within this rectangle.
        /// </summary>
        /// <param name="pt">The point to check.</param>
        /// <returns><c>true</c> if the point is within the rectangle; otherwise, <c>false</c>.</returns>
        public bool Contains(PointF pt)
            => Contains(pt.X, pt.Y);

        /// <summary>
        /// Determines whether the specified rectangle is fully contained within this rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns><c>true</c> if the rectangle is fully contained; otherwise, <c>false</c>.</returns>
        public bool Contains(RectangleF rect)
            => rect.X >= X && rect.Y >= Y && rect.Right <= Right && rect.Bottom <= Bottom;

        /// <summary>
        /// Determines whether this rectangle intersects with another rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns><c>true</c> if the rectangles intersect; otherwise, <c>false</c>.</returns>
        public bool IntersectsWith(RectangleF rect)
            => rect.X < Right && X < rect.Right && rect.Y < Bottom && Y < rect.Bottom;

        // ── Set operations (instance — mutate this) ───────────────────────────────

        /// <summary>
        /// Intersects this rectangle with another, modifying this rectangle to the result.
        /// </summary>
        /// <param name="rect">The rectangle to intersect with.</param>
        public void Intersect(RectangleF rect)
        {
            RectangleF result = Intersect(this, rect);
            X = result.X; Y = result.Y; Width = result.Width; Height = result.Height;
        }

        /// <summary>
        /// Inflates this rectangle by the specified width and height.
        /// </summary>
        /// <param name="width">The horizontal inflation amount.</param>
        /// <param name="height">The vertical inflation amount.</param>
        public void Inflate(float width, float height)
        {
            X      -= width;
            Y      -= height;
            Width  += 2f * width;
            Height += 2f * height;
        }

        /// <summary>
        /// Inflates this rectangle by the specified size.
        /// </summary>
        /// <param name="size">The size to inflate by.</param>
        public void Inflate(SizeF size) => Inflate(size.Width, size.Height);

        /// <summary>
        /// Offsets this rectangle by the specified horizontal and vertical amounts.
        /// </summary>
        /// <param name="dx">The horizontal offset.</param>
        /// <param name="dy">The vertical offset.</param>
        public void Offset(float dx, float dy) { X += dx; Y += dy; }

        /// <summary>
        /// Offsets this rectangle by the specified point.
        /// </summary>
        /// <param name="pos">The point containing the offsets.</param>
        public void Offset(PointF pos)          { X += pos.X; Y += pos.Y; }

        // ── Set operations (static — return new RectangleF) ──────────────────────

        /// <summary>
        /// Returns the intersection of two rectangles.
        /// </summary>
        /// <param name="a">The first rectangle.</param>
        /// <param name="b">The second rectangle.</param>
        /// <returns>The intersection rectangle, or <see cref="Empty"/> if they don't intersect.</returns>
        public static RectangleF Intersect(RectangleF a, RectangleF b)
        {
            float x      = Math.Max(a.X,      b.X);
            float y      = Math.Max(a.Y,      b.Y);
            float right  = Math.Min(a.Right,  b.Right);
            float bottom = Math.Min(a.Bottom, b.Bottom);
            return right > x && bottom > y
                ? new RectangleF(x, y, right - x, bottom - y)
                : Empty;
        }

        /// <summary>
        /// Returns the union of two rectangles.
        /// </summary>
        /// <param name="a">The first rectangle.</param>
        /// <param name="b">The second rectangle.</param>
        /// <returns>The smallest rectangle containing both input rectangles.</returns>
        public static RectangleF Union(RectangleF a, RectangleF b)
        {
            float x      = Math.Min(a.X,      b.X);
            float y      = Math.Min(a.Y,      b.Y);
            float right  = Math.Max(a.Right,  b.Right);
            float bottom = Math.Max(a.Bottom, b.Bottom);
            return new RectangleF(x, y, right - x, bottom - y);
        }

        /// <summary>
        /// Returns an inflated copy of the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to inflate.</param>
        /// <param name="x">The horizontal inflation amount.</param>
        /// <param name="y">The vertical inflation amount.</param>
        /// <returns>The inflated rectangle.</returns>
        public static RectangleF Inflate(RectangleF rect, float x, float y)
        {
            RectangleF r = rect;
            r.Inflate(x, y);
            return r;
        }

        // ── Conversion ────────────────────────────────────────────────────────────

        /// <summary>
        /// Truncates each component toward zero and converts to a <see cref="Rectangle"/>.
        /// </summary>
        /// <returns>A new <see cref="Rectangle"/> with truncated dimensions.</returns>
        public Rectangle ToRectangle()
            => new Rectangle((int)X, (int)Y, (int)Width, (int)Height);

        /// <summary>
        /// Rounds each component to the nearest integer and converts to a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="r">The rectangle to round.</param>
        /// <returns>A new <see cref="Rectangle"/> with rounded dimensions.</returns>
        public static Rectangle Round(RectangleF r)
            => new Rectangle(
                (int)Math.Round(r.X),      (int)Math.Round(r.Y),
                (int)Math.Round(r.Width),  (int)Math.Round(r.Height));

        /// <summary>
        /// Truncates each component toward negative infinity and converts to a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="r">The rectangle to truncate.</param>
        /// <returns>A new <see cref="Rectangle"/> with truncated dimensions.</returns>
        public static Rectangle Truncate(RectangleF r)
            => new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);

        /// <summary>
        /// Applies ceiling to each component and converts to a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="r">The rectangle to apply ceiling to.</param>
        /// <returns>A new <see cref="Rectangle"/> with ceiled dimensions.</returns>
        public static Rectangle Ceiling(RectangleF r)
            => new Rectangle(
                (int)Math.Ceiling(r.X),     (int)Math.Ceiling(r.Y),
                (int)Math.Ceiling(r.Width), (int)Math.Ceiling(r.Height));

        /// <summary>
        /// Implicitly converts a <see cref="Rectangle"/> to a <see cref="RectangleF"/>.
        /// </summary>
        public static implicit operator RectangleF(Rectangle r)
            => new RectangleF(r.X, r.Y, r.Width, r.Height);

        // ── Equality ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Indicates whether this rectangle is equal to another rectangle.
        /// </summary>
        /// <param name="other">The rectangle to compare.</param>
        /// <returns><c>true</c> if the rectangles are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(RectangleF other)
            => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        /// <summary>
        /// Indicates whether this rectangle is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is RectangleF r && Equals(r);

        /// <summary>
        /// Returns a hash code for this rectangle.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int h = X.GetHashCode();
                h = h * 397 ^ Y.GetHashCode();
                h = h * 397 ^ Width.GetHashCode();
                h = h * 397 ^ Height.GetHashCode();
                return h;
            }
        }

        /// <summary>
        /// Compares two rectangles for equality.
        /// </summary>
        public static bool operator ==(RectangleF left, RectangleF right) => left.Equals(right);

        /// <summary>
        /// Compares two rectangles for inequality.
        /// </summary>
        public static bool operator !=(RectangleF left, RectangleF right) => !left.Equals(right);

        /// <summary>
        /// Returns a string representation of this rectangle.
        /// </summary>
        /// <returns>A string in the format "{X=value, Y=value, Width=value, Height=value}".</returns>
        public override string ToString()
            => $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }
}
