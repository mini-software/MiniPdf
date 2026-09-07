using MiniPdf.Drawing.Geometry;
using System;
using System.Collections.Generic;
using System.IO;

namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Describes the interior of a graphics shape composed of rectangles, paths,
    /// and set operations (union, intersection, exclusion, complement, xor).
    /// </summary>
    /// <remarks>
    /// A <see cref="Region"/> can be used to clip drawing operations to a specific area.
    /// Regions support boolean operations like Union, Intersect, Exclude, Complement, and Xor.
    /// </remarks>
    public sealed class Region : IDisposable
    {
        // The root node of the CSG tree.
        private RegionNode _root;

        // -----------------------------------------------------------------------
        // Constructors
        // -----------------------------------------------------------------------

        /// <summary>Creates an infinite region.</summary>
        public Region() { _root = InfiniteRegionNode.Instance; }

        /// <summary>Creates a region from a rectangle.</summary>
        public Region(RectangleF rect) { _root = new LeafRectNode(rect); }

        /// <summary>Creates a region from a rectangle.</summary>
        public Region(Rectangle rect)
            : this(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height)) { }

        /// <summary>Creates a region that has the same interior as a path.</summary>
        public Region(GraphicsPath path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            _root = new LeafPathNode(path.Clone());
        }

        /// <summary>Initialises a Region from serialized data.</summary>
        public Region(RegionData rgnData)
        {
            if (rgnData == null) throw new ArgumentNullException(nameof(rgnData));
            using (var ms = new MemoryStream(rgnData.Data))
            using (var br = new BinaryReader(ms))
                _root = RegionNodeSerializer.Read(br);
        }

        // -----------------------------------------------------------------------
        // Set operations
        // -----------------------------------------------------------------------

        /// <summary>
        /// Updates this region to the union of itself and the specified region.
        /// </summary>
        /// <param name="region">The region to union with.</param>
        public void Union(Region region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            _root = new OpRegionNode(_root, region._root.DeepClone(), SetOp.Union);
        }

        /// <summary>
        /// Updates this region to the union of itself and the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to union with.</param>
        public void Union(RectangleF rect) => Union(new Region(rect));

        /// <summary>
        /// Updates this region to the union of itself and the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to union with.</param>
        public void Union(Rectangle  rect) => Union(new Region(rect));

        /// <summary>
        /// Updates this region to the union of itself and the specified path.
        /// </summary>
        /// <param name="path">The path to union with.</param>
        public void Union(GraphicsPath path) => Union(new Region(path));

        /// <summary>
        /// Updates this region to the intersection of itself and the specified region.
        /// </summary>
        /// <param name="region">The region to intersect with.</param>
        public void Intersect(Region region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            _root = new OpRegionNode(_root, region._root.DeepClone(), SetOp.Intersect);
        }

        /// <summary>
        /// Updates this region to the intersection of itself and the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to intersect with.</param>
        public void Intersect(RectangleF rect) => Intersect(new Region(rect));

        /// <summary>
        /// Updates this region to the intersection of itself and the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to intersect with.</param>
        public void Intersect(Rectangle  rect) => Intersect(new Region(rect));

        /// <summary>
        /// Updates this region to the intersection of itself and the specified path.
        /// </summary>
        /// <param name="path">The path to intersect with.</param>
        public void Intersect(GraphicsPath path) => Intersect(new Region(path));

        /// <summary>
        /// Updates this region to the exclusion of the specified region.
        /// </summary>
        /// <param name="region">The region to exclude.</param>
        public void Exclude(Region region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            _root = new OpRegionNode(_root, region._root.DeepClone(), SetOp.Exclude);
        }

        /// <summary>
        /// Updates this region to the exclusion of the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to exclude.</param>
        public void Exclude(RectangleF rect) => Exclude(new Region(rect));

        /// <summary>
        /// Updates this region to the exclusion of the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to exclude.</param>
        public void Exclude(Rectangle  rect) => Exclude(new Region(rect));

        /// <summary>
        /// Updates this region to the exclusion of the specified path.
        /// </summary>
        /// <param name="path">The path to exclude.</param>
        public void Exclude(GraphicsPath path) => Exclude(new Region(path));

        /// <summary>
        /// Updates this region to the complement of itself relative to the specified region.
        /// </summary>
        /// <param name="region">The region to use for the complement operation.</param>
        public void Complement(Region region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            _root = new OpRegionNode(_root, region._root.DeepClone(), SetOp.Complement);
        }

        /// <summary>
        /// Updates this region to the complement of itself relative to the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to use for the complement operation.</param>
        public void Complement(RectangleF rect) => Complement(new Region(rect));

        /// <summary>
        /// Updates this region to the complement of itself relative to the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to use for the complement operation.</param>
        public void Complement(Rectangle  rect) => Complement(new Region(rect));

        /// <summary>
        /// Updates this region to the complement of itself relative to the specified path.
        /// </summary>
        /// <param name="path">The path to use for the complement operation.</param>
        public void Complement(GraphicsPath path) => Complement(new Region(path));

        /// <summary>
        /// Updates this region to the exclusive OR of itself and the specified region.
        /// </summary>
        /// <param name="region">The region to XOR with.</param>
        public void Xor(Region region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            _root = new OpRegionNode(_root, region._root.DeepClone(), SetOp.Xor);
        }

        /// <summary>
        /// Updates this region to the exclusive OR of itself and the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to XOR with.</param>
        public void Xor(RectangleF rect) => Xor(new Region(rect));

        /// <summary>
        /// Updates this region to the exclusive OR of itself and the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to XOR with.</param>
        public void Xor(Rectangle  rect) => Xor(new Region(rect));

        /// <summary>
        /// Updates this region to the exclusive OR of itself and the specified path.
        /// </summary>
        /// <param name="path">The path to XOR with.</param>
        public void Xor(GraphicsPath path) => Xor(new Region(path));

        // -----------------------------------------------------------------------
        // State mutators
        // -----------------------------------------------------------------------

        /// <summary>Makes this region empty (contains no points).</summary>
        public void MakeEmpty() => _root = EmptyRegionNode.Instance;

        /// <summary>Makes this region infinite (contains all points).</summary>
        public void MakeInfinite() => _root = InfiniteRegionNode.Instance;

        // -----------------------------------------------------------------------
        // Transformation
        // -----------------------------------------------------------------------

        /// <summary>
        /// Translates this region by the specified offsets.
        /// </summary>
        /// <param name="dx">The horizontal translation.</param>
        /// <param name="dy">The vertical translation.</param>
        public void Translate(float dx, float dy) => _root.Translate(dx, dy);

        /// <summary>
        /// Translates this region by the specified offsets.
        /// </summary>
        /// <param name="dx">The horizontal translation.</param>
        /// <param name="dy">The vertical translation.</param>
        public void Translate(int   dx, int   dy) => Translate((float)dx, (float)dy);

        /// <summary>
        /// Transforms this region by the specified matrix.
        /// </summary>
        /// <param name="matrix">The transformation matrix.</param>
        public void Transform(Matrix matrix)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            _root.ApplyTransform(matrix);
        }

        // -----------------------------------------------------------------------
        // Visibility tests
        // -----------------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this region is empty.
        /// </summary>
        /// <param name="g">The graphics context (optional).</param>
        /// <returns><c>true</c> if the region is empty; otherwise, <c>false</c>.</returns>
        public bool IsEmpty(Graphics? g = null)  => IsEmptyInternal();

        /// <summary>
        /// Gets a value indicating whether this region is infinite.
        /// </summary>
        /// <param name="g">The graphics context (optional).</param>
        /// <returns><c>true</c> if the region is infinite; otherwise, <c>false</c>.</returns>
        public bool IsInfinite(Graphics? g = null) => _root is InfiniteRegionNode;

        /// <summary>
        /// Determines whether the specified point is visible within this region.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <returns><c>true</c> if the point is visible; otherwise, <c>false</c>.</returns>
        public bool IsVisible(float x, float y) => _root.IsPointInside(x, y);

        /// <summary>
        /// Determines whether the specified point is visible within this region.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <param name="g">The graphics context (optional).</param>
        /// <returns><c>true</c> if the point is visible; otherwise, <c>false</c>.</returns>
        public bool IsVisible(float x, float y, Graphics? g) => _root.IsPointInside(x, y);

        /// <summary>
        /// Determines whether the specified point is visible within this region.
        /// </summary>
        /// <param name="pt">The point to test.</param>
        /// <returns><c>true</c> if the point is visible; otherwise, <c>false</c>.</returns>
        public bool IsVisible(PointF pt) => _root.IsPointInside(pt.X, pt.Y);

        /// <summary>
        /// Determines whether the specified point is visible within this region.
        /// </summary>
        /// <param name="pt">The point to test.</param>
        /// <param name="g">The graphics context (optional).</param>
        /// <returns><c>true</c> if the point is visible; otherwise, <c>false</c>.</returns>
        public bool IsVisible(PointF pt, Graphics? g) => _root.IsPointInside(pt.X, pt.Y);

        /// <summary>
        /// Determines whether the specified point is visible within this region.
        /// </summary>
        /// <param name="pt">The point to test.</param>
        /// <returns><c>true</c> if the point is visible; otherwise, <c>false</c>.</returns>
        public bool IsVisible(Point  pt) => _root.IsPointInside(pt.X, pt.Y);

        /// <summary>
        /// Determines whether the specified point is visible within this region.
        /// </summary>
        /// <param name="pt">The point to test.</param>
        /// <param name="g">The graphics context (optional).</param>
        /// <returns><c>true</c> if the point is visible; otherwise, <c>false</c>.</returns>
        public bool IsVisible(Point  pt, Graphics? g) => _root.IsPointInside(pt.X, pt.Y);

        /// <summary>
        /// Determines whether the specified point is visible within this region.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <param name="g">The graphics context (optional).</param>
        /// <returns><c>true</c> if the point is visible; otherwise, <c>false</c>.</returns>
        public bool IsVisible(int x, int y, Graphics? g) => _root.IsPointInside(x, y);

        /// <summary>
        /// Returns true if any part of <paramref name="rect"/> lies within this region.
        /// </summary>
        public bool IsVisible(RectangleF rect, Graphics? g = null)
        {
            if (rect.IsEmpty) return false;
            // Check 9 sample points: 4 corners, 4 edge midpoints, centre
            float mx = rect.X + rect.Width  * 0.5f;
            float my = rect.Y + rect.Height * 0.5f;
            return IsVisible(rect.X, rect.Y) || IsVisible(rect.Right, rect.Y)
                || IsVisible(rect.Right, rect.Bottom) || IsVisible(rect.X, rect.Bottom)
                || IsVisible(mx, rect.Y) || IsVisible(rect.Right, my)
                || IsVisible(mx, rect.Bottom) || IsVisible(rect.X, my)
                || IsVisible(mx, my);
        }

        /// <summary>
        /// Returns true if any part of <paramref name="rect"/> lies within this region.
        /// </summary>
        /// <param name="rect">The rectangle to test.</param>
        /// <param name="g">The graphics context (optional).</param>
        /// <returns><c>true</c> if any part of the rectangle is visible; otherwise, <c>false</c>.</returns>
        public bool IsVisible(Rectangle rect, Graphics? g = null)
            => IsVisible(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), g);

        // -----------------------------------------------------------------------
        // Geometry
        // -----------------------------------------------------------------------

        /// <summary>Gets the bounding rectangle of this region.</summary>
        public RectangleF GetBounds(Graphics? g = null) => _root.GetBounds();

        // -----------------------------------------------------------------------
        // Serialization
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns a <see cref="RegionData"/> object that represents the information
        /// stored in this <see cref="Region"/>.
        /// </summary>
        public RegionData GetRegionData()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                _root.WriteTo(bw);
                bw.Flush();
                return new RegionData(ms.ToArray());
            }
        }

        // -----------------------------------------------------------------------
        // GetRegionScans
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns an array of RectangleF that approximate the region as a set of
        /// horizontal scan bands. The returned rectangles are transformed by
        /// <paramref name="matrix"/> (pass <c>new Matrix()</c> for identity).
        /// </summary>
        public RectangleF[] GetRegionScans(Matrix matrix)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));

            // Fast path: single rect leaf
            if (_root is LeafRectNode rectLeaf)
            {
                var r = rectLeaf.Rect;
                if (r.IsEmpty) return Array.Empty<RectangleF>();
                return new[] { ApplyMatrix(matrix, r) };
            }

            if (_root is EmptyRegionNode)
                return Array.Empty<RectangleF>();

            if (_root is InfiniteRegionNode)
            {
                // Return a large bounding rect (max 1e6 × 1e6)
                var big = new RectangleF(-1e6f, -1e6f, 2e6f, 2e6f);
                return new[] { ApplyMatrix(matrix, big) };
            }

            // General case: scanline sampling
            var bounds = _root.GetBounds();
            if (bounds.IsEmpty) return Array.Empty<RectangleF>();

            // Clamp infinite/huge bounds
            const float maxCoord = 1e6f;
            float x0 = Math.Max(bounds.X,      -maxCoord);
            float y0 = Math.Max(bounds.Y,      -maxCoord);
            float x1 = Math.Min(bounds.Right,   maxCoord);
            float y1 = Math.Min(bounds.Bottom,  maxCoord);

            float scanH = 1.0f; // 1 unit scanline height
            int   xSamples = 512;
            float xStep = (x1 - x0) / xSamples;
            if (xStep <= 0f) return Array.Empty<RectangleF>();

            var scans = new List<RectangleF>();
            float yEnd = (float)Math.Ceiling((y1 - y0) / scanH) * scanH + y0;

            for (float y = y0; y < yEnd; y += scanH)
            {
                float yMid = y + scanH * 0.5f;
                bool  inSpan     = false;
                float spanStartX = 0f;

                for (int i = 0; i <= xSamples; i++)
                {
                    float x   = x0 + i * xStep;
                    bool  vis = _root.IsPointInside(x, yMid);

                    if (!inSpan && vis)
                    {
                        inSpan     = true;
                        spanStartX = x;
                    }
                    else if (inSpan && !vis)
                    {
                        scans.Add(new RectangleF(spanStartX, y, x - spanStartX, scanH));
                        inSpan = false;
                    }
                }
                if (inSpan)
                    scans.Add(new RectangleF(spanStartX, y, x1 - spanStartX, scanH));
            }

            // Apply matrix
            if (!matrix.IsIdentity)
            {
                for (int i = 0; i < scans.Count; i++)
                    scans[i] = ApplyMatrix(matrix, scans[i]);
            }

            return scans.ToArray();
        }

        // -----------------------------------------------------------------------
        // Clone / Equals
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates an exact copy of this <see cref="Region"/>.
        /// </summary>
        /// <returns>A new <see cref="Region"/> that is a copy of this instance.</returns>
        public Region Clone()
        {
            var c = new Region();
            c._root = _root.DeepClone();
            return c;
        }

        /// <summary>
        /// Tests whether this region has the same interior as <paramref name="region"/>.
        /// Uses point-sampling since full rasterization requires Graphics (Batch 7).
        /// </summary>
        public bool Equals(Region region, Graphics? g = null)
        {
            if (region == null) return false;
            if (ReferenceEquals(this, region)) return true;

            // Fast structural shortcut for trivial nodes
            if (_root is EmptyRegionNode   && region._root is EmptyRegionNode)   return true;
            if (_root is InfiniteRegionNode && region._root is InfiniteRegionNode) return true;

            // Sample a grid of points within the union of both bounds
            var b1 = GetBounds();
            var b2 = region.GetBounds();
            var combined = RectangleF.Union(b1, b2);
            if (combined.IsEmpty) return true;

            const int steps = 20;
            float wStep = combined.Width  / steps;
            float hStep = combined.Height / steps;
            if (wStep <= 0f) wStep = 1f;
            if (hStep <= 0f) hStep = 1f;

            for (int iy = 0; iy <= steps; iy++)
            for (int ix = 0; ix <= steps; ix++)
            {
                float x = combined.X + ix * wStep;
                float y = combined.Y + iy * hStep;
                if (_root.IsPointInside(x, y) != region._root.IsPointInside(x, y))
                    return false;
            }
            return true;
        }

        // -----------------------------------------------------------------------
        // IDisposable
        // -----------------------------------------------------------------------

        private bool _disposed;

        /// <summary>
        /// Releases all resources used by this <see cref="Region"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            // Walk the tree and dispose any path leaves
            DisposeNode(_root);
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private bool IsEmptyInternal()
        {
            if (_root is EmptyRegionNode) return true;
            if (_root is InfiniteRegionNode) return false;
            var b = _root.GetBounds();
            if (b.IsEmpty) return true;
            // Quick centre sample
            float cx = b.X + b.Width * 0.5f;
            float cy = b.Y + b.Height * 0.5f;
            return !_root.IsPointInside(cx, cy);
        }

        private static RectangleF ApplyMatrix(Matrix matrix, RectangleF r)
        {
            if (matrix.IsIdentity) return r;
            var pts = new PointF[]
            {
                new PointF(r.X, r.Y),
                new PointF(r.Right, r.Y),
                new PointF(r.Right, r.Bottom),
                new PointF(r.X, r.Bottom),
            };
            matrix.TransformPoints(pts);
            float minX = pts[0].X, maxX = pts[0].X;
            float minY = pts[0].Y, maxY = pts[0].Y;
            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].X < minX) minX = pts[i].X;
                if (pts[i].X > maxX) maxX = pts[i].X;
                if (pts[i].Y < minY) minY = pts[i].Y;
                if (pts[i].Y > maxY) maxY = pts[i].Y;
            }
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        private static void DisposeNode(RegionNode node)
        {
            if (node is LeafPathNode p) { p.Path.Dispose(); return; }
            if (node is OpRegionNode op)
            {
                DisposeNode(op.Left);
                DisposeNode(op.Right);
            }
        }
    }
}
