using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Rendering
{
    internal readonly struct EdgeSeed
    {
        public EdgeSeed(int yStart, ActiveEdge edge)
        {
            YStart = yStart;
            Edge = edge;
        }

        public int YStart { get; }
        public ActiveEdge Edge { get; }
    }

    internal static class EdgeBuilder
    {
        public static List<EdgeSeed> Build(GraphicsPath path, float flatness = 0.25f)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            GraphicsPath flat = path.Clone();
            flat.Flatten(null, flatness);

            PointF[] points = flat.PathPoints;
            byte[] types = flat.PathTypes;
            var edges = new List<EdgeSeed>(points.Length);

            int subpathStart = -1;
            for (int i = 0; i < points.Length; i++)
            {
                byte rawType = (byte)(types[i] & (byte)PathPointType.PathTypeMask);
                if (rawType == (byte)PathPointType.Start)
                {
                    subpathStart = i;
                    continue;
                }

                if (i > 0)
                    AddEdge(points[i - 1], points[i], edges);

                bool closes = (types[i] & (byte)PathPointType.CloseSubpath) != 0;
                if (closes && subpathStart >= 0 && subpathStart != i)
                    AddEdge(points[i], points[subpathStart], edges);
            }

            return edges;
        }

        private static void AddEdge(PointF a, PointF b, List<EdgeSeed> edges)
        {
            float x0 = a.X;
            float y0 = a.Y;
            float x1 = b.X;
            float y1 = b.Y;

            if (Math.Abs(y1 - y0) < 1e-6f)
                return;

            int winding;
            if (y1 > y0)
            {
                winding = 1;
            }
            else
            {
                float tx = x0; x0 = x1; x1 = tx;
                float ty = y0; y0 = y1; y1 = ty;
                winding = -1;
            }

            float dxPerY = (x1 - x0) / (y1 - y0);

            int yStart = (int)Math.Ceiling(y0 - 0.5f);
            int yEndExclusive = (int)Math.Ceiling(y1 - 0.5f);
            if (yStart >= yEndExclusive)
                return;

            float sampleY = yStart + 0.5f;
            float xAtStart = x0 + (sampleY - y0) * dxPerY;

            var edge = new ActiveEdge
            {
                YMaxExclusive = yEndExclusive,
                CurrentX = xAtStart,
                DxPerY = dxPerY,
                Winding = winding,
            };

            edges.Add(new EdgeSeed(yStart, edge));
        }
    }
}
