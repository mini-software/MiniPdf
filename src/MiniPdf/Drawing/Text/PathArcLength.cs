using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Text
{
    /// <summary>
    /// Arc-length parameterization of a flattened <see cref="GraphicsPath"/>.
    /// Builds a walkable map of cumulative segment lengths per subpath so that
    /// a caller can ask "what point and tangent angle is at distance D along
    /// the path?".
    /// </summary>
    internal sealed class PathArcLength
    {
        private readonly Subpath[] _subpaths;
        private readonly float _totalLength;

        private PathArcLength(Subpath[] subpaths, float totalLength)
        {
            _subpaths = subpaths;
            _totalLength = totalLength;
        }

        /// <summary>Total arc length of the path, in path-local units.</summary>
        public float TotalLength => _totalLength;

        /// <summary>Number of subpaths (figures) in the path.</summary>
        public int SubpathCount => _subpaths.Length;

        // ── Construction ──────────────────────────────────────────────────────

        /// <summary>
        /// Flattens <paramref name="baseline"/> (de Casteljau) and builds the
        /// arc-length map. The input path is not modified (a flattened clone is
        /// used internally).
        /// </summary>
        public static PathArcLength Build(GraphicsPath baseline)
        {
            if (baseline == null) throw new ArgumentNullException(nameof(baseline));
            if (baseline.PointCount == 0)
                return new PathArcLength(Array.Empty<Subpath>(), 0f);

            GraphicsPath flat = baseline.Clone();
            flat.Flatten();

            PointF[] pts = flat.PathPoints;
            byte[] types = flat.PathTypes;

            var subpaths = new List<Subpath>();
            float totalLength = 0f;

            int i = 0;
            while (i < pts.Length)
            {
                int start = i;
                i++;
                while (i < pts.Length && (types[i] & 0x07) != (byte)PathPointType.Start)
                    i++;
                int end = i - 1; // inclusive

                int count = end - start + 1;
                if (count < 2)
                    continue; // degenerate single-point figure

                bool closed = (types[end] & (byte)PathPointType.CloseSubpath) != 0;

                int segCount = count - 1 + (closed ? 1 : 0);
                var segs = new Segment[segCount];
                var cum = new float[segCount + 1];
                cum[0] = 0f;
                float length = 0f;

                int s = 0;
                for (int j = 0; j < count - 1; j++)
                {
                    PointF a = pts[start + j];
                    PointF b = pts[start + j + 1];
                    float d = Dist(a, b);
                    segs[s] = new Segment(a, b, d);
                    length += d;
                    cum[s + 1] = length;
                    s++;
                }
                if (closed)
                {
                    PointF a = pts[start + count - 1];
                    PointF b = pts[start];
                    float d = Dist(a, b);
                    segs[s] = new Segment(a, b, d);
                    length += d;
                    cum[s + 1] = length;
                }

                subpaths.Add(new Subpath(segs, cum, length, closed, totalLength));
                totalLength += length;
            }

            return new PathArcLength(subpaths.ToArray(), totalLength);
        }

        // ── Query ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the point and tangent angle (degrees, screen space) at the
        /// given arc-length <paramref name="distance"/>. Distances outside
        /// <c>[0, TotalLength]</c> are clamped. For a closed subpath, the local
        /// distance wraps modulo the subpath length.
        /// </summary>
        /// <returns><c>false</c> if the path is empty.</returns>
        public bool GetPointAt(float distance, out PointF point, out float angleDegrees)
        {
            point = new PointF(float.NaN, float.NaN);
            angleDegrees = 0f;

            if (_totalLength <= 0f || _subpaths.Length == 0)
                return false;

            if (distance < 0f) distance = 0f;
            if (distance > _totalLength) distance = _totalLength;

            // Find the subpath containing this distance.
            Subpath sub = _subpaths[_subpaths.Length - 1];
            for (int k = 0; k < _subpaths.Length; k++)
            {
                if (distance <= _subpaths[k].StartOffset + _subpaths[k].Length)
                {
                    sub = _subpaths[k];
                    break;
                }
            }

            float localDist = distance - sub.StartOffset;
            if (sub.Closed && sub.Length > 0f)
            {
                localDist = localDist % sub.Length;
                if (localDist < 0f) localDist += sub.Length;
            }

            Segment[] segs = sub.Segments;
            float[] cum = sub.Cumulative;
            if (segs.Length == 0)
                return false;

            // Binary search: largest index where cum[index] <= localDist.
            int lo = 0;
            int hi = segs.Length - 1;
            if (localDist >= cum[segs.Length])
            {
                lo = segs.Length - 1;
            }
            else
            {
                while (lo < hi)
                {
                    int mid = (lo + hi + 1) / 2;
                    if (cum[mid] <= localDist)
                        lo = mid;
                    else
                        hi = mid - 1;
                }
            }

            Segment seg = segs[lo];
            float segLen = seg.Length;
            float t = segLen > 0f ? (localDist - cum[lo]) / segLen : 0f;
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;

            point = new PointF(
                seg.Start.X + (seg.End.X - seg.Start.X) * t,
                seg.Start.Y + (seg.End.Y - seg.Start.Y) * t);
            angleDegrees = (float)(Math.Atan2(
                seg.End.Y - seg.Start.Y,
                seg.End.X - seg.Start.X) * 180.0 / Math.PI);
            return true;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static float Dist(PointF a, PointF b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private readonly struct Segment
        {
            public readonly PointF Start;
            public readonly PointF End;
            public readonly float Length;

            public Segment(PointF start, PointF end, float length)
            {
                Start = start;
                End = end;
                Length = length;
            }
        }

        private readonly struct Subpath
        {
            public readonly Segment[] Segments;
            public readonly float[] Cumulative;
            public readonly float Length;
            public readonly bool Closed;
            public readonly float StartOffset;

            public Subpath(Segment[] segments, float[] cumulative,
                           float length, bool closed, float startOffset)
            {
                Segments = segments;
                Cumulative = cumulative;
                Length = length;
                Closed = closed;
                StartOffset = startOffset;
            }
        }
    }
}