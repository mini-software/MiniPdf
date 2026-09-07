using MiniPdf.Drawing.Geometry;
using System;
using System.IO;

namespace MiniPdf.Drawing.Drawing2D
{
    // -------------------------------------------------------------------------
    // Set operation codes (mirrors CombineMode for internal use)
    // -------------------------------------------------------------------------
    internal enum SetOp : byte
    {
        Union       = 0,
        Intersect   = 1,
        Exclude     = 2,
        Complement  = 3,
        Xor         = 4,
    }

    // -------------------------------------------------------------------------
    // Node type tags used in binary serialization
    // -------------------------------------------------------------------------
    internal enum NodeTag : byte
    {
        Empty     = 0,
        Infinite  = 1,
        Rect      = 2,
        Path      = 3,
        Op        = 4,
    }

    // -------------------------------------------------------------------------
    // Abstract base
    // -------------------------------------------------------------------------
    internal abstract class RegionNode
    {
        internal abstract bool        IsPointInside(float x, float y);
        internal abstract RectangleF  GetBounds();
        internal abstract RegionNode  DeepClone();
        internal abstract void        Translate(float dx, float dy);
        internal abstract void        ApplyTransform(Matrix matrix);
        internal abstract void        WriteTo(BinaryWriter w);
    }

    // -------------------------------------------------------------------------
    // Empty node – no pixels
    // -------------------------------------------------------------------------
    internal sealed class EmptyRegionNode : RegionNode
    {
        internal static readonly EmptyRegionNode Instance = new EmptyRegionNode();

        internal override bool       IsPointInside(float x, float y) => false;
        internal override RectangleF GetBounds()                      => RectangleF.Empty;
        internal override RegionNode DeepClone()                      => Instance;
        internal override void       Translate(float dx, float dy)    { }
        internal override void       ApplyTransform(Matrix matrix)    { }
        internal override void       WriteTo(BinaryWriter w)          => w.Write((byte)NodeTag.Empty);
    }

    // -------------------------------------------------------------------------
    // Infinite node – all pixels
    // -------------------------------------------------------------------------
    internal sealed class InfiniteRegionNode : RegionNode
    {
        internal static readonly InfiniteRegionNode Instance = new InfiniteRegionNode();

        // Represent infinity as a very large rectangle for bounds purposes
        private static readonly RectangleF InfiniteBounds =
            new RectangleF(-1e9f, -1e9f, 2e9f, 2e9f);

        internal override bool       IsPointInside(float x, float y) => true;
        internal override RectangleF GetBounds()                      => InfiniteBounds;
        internal override RegionNode DeepClone()                      => Instance;
        internal override void       Translate(float dx, float dy)    { }
        internal override void       ApplyTransform(Matrix matrix)    { }
        internal override void       WriteTo(BinaryWriter w)          => w.Write((byte)NodeTag.Infinite);
    }

    // -------------------------------------------------------------------------
    // Rectangle leaf
    // -------------------------------------------------------------------------
    internal sealed class LeafRectNode : RegionNode
    {
        internal RectangleF Rect;

        internal LeafRectNode(RectangleF rect) { Rect = rect; }

        internal override bool IsPointInside(float x, float y)
            => Rect.Contains(x, y);

        internal override RectangleF GetBounds()
            => Rect;

        internal override RegionNode DeepClone()
            => new LeafRectNode(Rect);

        internal override void Translate(float dx, float dy)
            => Rect = new RectangleF(Rect.X + dx, Rect.Y + dy, Rect.Width, Rect.Height);

        internal override void ApplyTransform(Matrix matrix)
        {
            // Transform the four corners, then compute the axis-aligned bounding box.
            var pts = new PointF[]
            {
                new PointF(Rect.X, Rect.Y),
                new PointF(Rect.Right, Rect.Y),
                new PointF(Rect.Right, Rect.Bottom),
                new PointF(Rect.X, Rect.Bottom),
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
            Rect = new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        internal override void WriteTo(BinaryWriter w)
        {
            w.Write((byte)NodeTag.Rect);
            w.Write(Rect.X);
            w.Write(Rect.Y);
            w.Write(Rect.Width);
            w.Write(Rect.Height);
        }
    }

    // -------------------------------------------------------------------------
    // Path leaf
    // -------------------------------------------------------------------------
    internal sealed class LeafPathNode : RegionNode
    {
        internal GraphicsPath Path;

        internal LeafPathNode(GraphicsPath path) { Path = path; }

        internal override bool IsPointInside(float x, float y)
            => Path.IsVisible(x, y);

        internal override RectangleF GetBounds()
            => Path.GetBounds();

        internal override RegionNode DeepClone()
            => new LeafPathNode(Path.Clone());

        internal override void Translate(float dx, float dy)
        {
            using (var m = new Matrix(1, 0, 0, 1, dx, dy))
                Path.Transform(m);
        }

        internal override void ApplyTransform(Matrix matrix)
            => Path.Transform(matrix);

        internal override void WriteTo(BinaryWriter w)
        {
            w.Write((byte)NodeTag.Path);
            var pts   = Path.PathPoints;
            var types = Path.PathTypes;
            int n = pts.Length;
            w.Write(n);
            for (int i = 0; i < n; i++) { w.Write(pts[i].X); w.Write(pts[i].Y); }
            for (int i = 0; i < n; i++) w.Write(types[i]);
        }
    }

    // -------------------------------------------------------------------------
    // CSG operation node
    // -------------------------------------------------------------------------
    internal sealed class OpRegionNode : RegionNode
    {
        internal RegionNode Left, Right;
        internal SetOp      Op;

        internal OpRegionNode(RegionNode left, RegionNode right, SetOp op)
        {
            Left  = left;
            Right = right;
            Op    = op;
        }

        internal override bool IsPointInside(float x, float y)
        {
            bool l = Left.IsPointInside(x, y);
            bool r = Right.IsPointInside(x, y);
            switch (Op)
            {
                case SetOp.Union:       return l | r;
                case SetOp.Intersect:   return l & r;
                case SetOp.Exclude:     return l & !r;
                case SetOp.Complement:  return !l & r;
                case SetOp.Xor:         return l ^ r;
                default:                return false;
            }
        }

        internal override RectangleF GetBounds()
        {
            var lb = Left.GetBounds();
            var rb = Right.GetBounds();
            switch (Op)
            {
                case SetOp.Intersect:
                    // Conservative: use intersection of bounds.
                    return RectangleF.Intersect(lb, rb);
                case SetOp.Exclude:
                    return lb;
                case SetOp.Complement:
                    return rb;
                default: // Union, Xor
                    return RectangleF.Union(lb, rb);
            }
        }

        internal override RegionNode DeepClone()
            => new OpRegionNode(Left.DeepClone(), Right.DeepClone(), Op);

        internal override void Translate(float dx, float dy)
        {
            Left.Translate(dx, dy);
            Right.Translate(dx, dy);
        }

        internal override void ApplyTransform(Matrix matrix)
        {
            Left.ApplyTransform(matrix);
            Right.ApplyTransform(matrix);
        }

        internal override void WriteTo(BinaryWriter w)
        {
            w.Write((byte)NodeTag.Op);
            w.Write((byte)Op);
            Left.WriteTo(w);
            Right.WriteTo(w);
        }
    }

    // -------------------------------------------------------------------------
    // Factory / deserializer
    // -------------------------------------------------------------------------
    internal static class RegionNodeSerializer
    {
        internal static RegionNode Read(BinaryReader r)
        {
            var tag = (NodeTag)r.ReadByte();
            switch (tag)
            {
                case NodeTag.Empty:
                    return EmptyRegionNode.Instance;

                case NodeTag.Infinite:
                    return InfiniteRegionNode.Instance;

                case NodeTag.Rect:
                {
                    float x = r.ReadSingle(), y = r.ReadSingle(),
                          w = r.ReadSingle(), h = r.ReadSingle();
                    return new LeafRectNode(new RectangleF(x, y, w, h));
                }

                case NodeTag.Path:
                {
                    int n = r.ReadInt32();
                    var pts   = new PointF[n];
                    var types = new byte[n];
                    for (int i = 0; i < n; i++)
                    {
                        pts[i] = new PointF(r.ReadSingle(), r.ReadSingle());
                    }
                    for (int i = 0; i < n; i++) types[i] = r.ReadByte();
                    return new LeafPathNode(new GraphicsPath(pts, types));
                }

                case NodeTag.Op:
                {
                    var op    = (SetOp)r.ReadByte();
                    var left  = Read(r);
                    var right = Read(r);
                    return new OpRegionNode(left, right, op);
                }

                default:
                    throw new InvalidOperationException($"Unknown region node tag {tag}.");
            }
        }
    }
}
