using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Rendering
{
    internal static class Rasterizer
    {
        public static void FillPath(
            Bitmap target,
            GraphicsPath path,
            Brush brush,
            Matrix? transform,
            FillMode fillMode,
            Rectangle? clip,
            SmoothingMode smoothingMode,
            CompositingMode compositingMode,
            InterpolationMode interpolationMode)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            bool aa = smoothingMode == SmoothingMode.AntiAlias || smoothingMode == SmoothingMode.HighQuality;
            if (aa)
            {
                FillPathAntialiased(target, path, brush, transform, fillMode, clip, compositingMode, interpolationMode);
                return;
            }

            GraphicsPath effectivePath = path.Clone();
            if (transform != null)
                effectivePath.Transform(transform);

            FillPathCore(target, effectivePath, brush, fillMode, clip, compositingMode, interpolationMode);
        }

        public static void DrawPath(
            Bitmap target,
            GraphicsPath path,
            Pens.Pen pen,
            Matrix? transform,
            Rectangle? clip,
            SmoothingMode smoothingMode,
            CompositingMode compositingMode,
            InterpolationMode interpolationMode)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (pen == null) throw new ArgumentNullException(nameof(pen));

            GraphicsPath widened = StrokeExpander.Expand(path, pen, transform, 0.25f);
            FillPath(target, widened, pen.Brush, null, FillMode.Winding, clip, smoothingMode, compositingMode, interpolationMode);
        }

        private static void FillPathAntialiased(
            Bitmap target,
            GraphicsPath path,
            Brush brush,
            Matrix? transform,
            FillMode fillMode,
            Rectangle? clip,
            CompositingMode compositingMode,
            InterpolationMode interpolationMode)
        {
            int w2 = target.Width * 2;
            int h2 = target.Height * 2;

            using var hi = new Bitmap(w2, h2, PixelFormat.Format32bppArgb);
            GraphicsPath hiPath = path.Clone();

            var aaTransform = new Matrix(2f, 0f, 0f, 2f, 0f, 0f);
            if (transform != null)
                aaTransform.Multiply(transform, MatrixOrder.Append);
            hiPath.Transform(aaTransform);

            Rectangle? hiClip = null;
            if (clip.HasValue)
                hiClip = new Rectangle(clip.Value.X * 2, clip.Value.Y * 2, clip.Value.Width * 2, clip.Value.Height * 2);

            FillPathCore(hi, hiPath, brush, fillMode, hiClip, CompositingMode.SourceOver, interpolationMode);

            Rectangle clipRect = ClipToBounds(clip, target.Width, target.Height);
            for (int y = clipRect.Y; y < clipRect.Bottom; y++)
            {
                for (int x = clipRect.X; x < clipRect.Right; x++)
                {
                    Color c00 = hi.GetPixel(x * 2, y * 2);
                    Color c10 = hi.GetPixel(x * 2 + 1, y * 2);
                    Color c01 = hi.GetPixel(x * 2, y * 2 + 1);
                    Color c11 = hi.GetPixel(x * 2 + 1, y * 2 + 1);

                    Color avg = Color.FromArgb(
                        (c00.A + c10.A + c01.A + c11.A + 2) / 4,
                        (c00.R + c10.R + c01.R + c11.R + 2) / 4,
                        (c00.G + c10.G + c01.G + c11.G + 2) / 4,
                        (c00.B + c10.B + c01.B + c11.B + 2) / 4);

                    PixelCompositor.BlendPixel(target, x, y, avg, compositingMode);
                }
            }
        }

        private static void FillPathCore(
            Bitmap target,
            GraphicsPath path,
            Brush brush,
            FillMode fillMode,
            Rectangle? clip,
            CompositingMode compositingMode,
            InterpolationMode interpolationMode)
        {
            List<EdgeSeed> seeds = EdgeBuilder.Build(path, 0.25f);
            if (seeds.Count == 0)
                return;

            int minY = int.MaxValue;
            int maxYEx = int.MinValue;
            var buckets = new Dictionary<int, List<ActiveEdge>>();

            for (int i = 0; i < seeds.Count; i++)
            {
                EdgeSeed seed = seeds[i];
                if (!buckets.TryGetValue(seed.YStart, out List<ActiveEdge>? list))
                {
                    list = new List<ActiveEdge>();
                    buckets[seed.YStart] = list;
                }

                list.Add(seed.Edge);
                if (seed.YStart < minY) minY = seed.YStart;
                if (seed.Edge.YMaxExclusive > maxYEx) maxYEx = seed.Edge.YMaxExclusive;
            }

            Rectangle clipRect = ClipToBounds(clip, target.Width, target.Height);
            minY = Math.Max(minY, clipRect.Y);
            maxYEx = Math.Min(maxYEx, clipRect.Bottom);

            var active = new List<ActiveEdge>();
            var spans = new ScanlineBuffer();

            for (int y = minY; y < maxYEx; y++)
            {
                if (buckets.TryGetValue(y, out List<ActiveEdge>? startEdges))
                    active.AddRange(startEdges);

                for (int i = active.Count - 1; i >= 0; i--)
                {
                    if (active[i].YMaxExclusive <= y)
                        active.RemoveAt(i);
                }

                if (active.Count == 0)
                    continue;

                active.Sort((a, b) => a.CurrentX.CompareTo(b.CurrentX));
                spans.Clear();

                if (fillMode == FillMode.Alternate)
                {
                    for (int i = 0; i + 1 < active.Count; i += 2)
                        AddSpan(spans, active[i].CurrentX, active[i + 1].CurrentX, clipRect);
                }
                else
                {
                    int winding = 0;
                    float startX = 0f;
                    for (int i = 0; i < active.Count; i++)
                    {
                        int prev = winding;
                        winding += active[i].Winding;

                        if (prev == 0 && winding != 0)
                            startX = active[i].CurrentX;
                        else if (prev != 0 && winding == 0)
                            AddSpan(spans, startX, active[i].CurrentX, clipRect);
                    }
                }

                DrawSpans(target, y, spans, brush, compositingMode, interpolationMode);

                for (int i = 0; i < active.Count; i++)
                {
                    ActiveEdge edge = active[i];
                    edge.CurrentX += edge.DxPerY;
                    active[i] = edge;
                }
            }
        }

        private static void AddSpan(ScanlineBuffer spans, float x0, float x1, Rectangle clipRect)
        {
            if (x1 <= x0)
                return;

            int xs = (int)Math.Ceiling(x0);
            int xe = (int)Math.Floor(x1 - 1e-6f);
            if (xe < xs)
                return;

            if (xs < clipRect.X) xs = clipRect.X;
            if (xe >= clipRect.Right) xe = clipRect.Right - 1;
            if (xe < xs)
                return;

            spans.AddSpan(xs, xe);
        }

        private static void DrawSpans(
            Bitmap target,
            int y,
            ScanlineBuffer spans,
            Brush brush,
            CompositingMode compositingMode,
            InterpolationMode interpolationMode)
        {
            var list = spans.Spans;
            for (int i = 0; i < list.Count; i++)
            {
                int x0 = list[i].X0;
                int x1 = list[i].X1;
                for (int x = x0; x <= x1; x++)
                {
                    Color src = BrushSampler.Sample(brush, x + 0.5f, y + 0.5f, interpolationMode);
                    PixelCompositor.BlendPixel(target, x, y, src, compositingMode);
                }
            }
        }

        private static Rectangle ClipToBounds(Rectangle? clip, int width, int height)
        {
            Rectangle bounds = new Rectangle(0, 0, width, height);
            if (!clip.HasValue)
                return bounds;

            Rectangle c = clip.Value;
            int x = Math.Max(bounds.X, c.X);
            int y = Math.Max(bounds.Y, c.Y);
            int r = Math.Min(bounds.Right, c.Right);
            int b = Math.Min(bounds.Bottom, c.Bottom);

            if (r <= x || b <= y)
                return Rectangle.Empty;

            return Rectangle.FromLTRB(x, y, r, b);
        }
    }
}
