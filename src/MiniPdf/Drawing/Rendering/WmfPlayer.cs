using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;
using MiniPdf.Drawing.Metafile.Wmf.Records;
using MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes;
using MiniPdf.Drawing.Pens;

namespace MiniPdf.Drawing.Rendering
{
    /// <summary>
    /// Replays WMF records onto any <see cref="IDrawingContext"/> (raster
    /// <see cref="Graphics"/>, <see cref="RecordingGraphics"/>, or any other
    /// context). The legacy <see cref="Render"/> overload remains as a thin
    /// backward-compat wrapper that creates a raster <see cref="Graphics"/> and
    /// delegates to <see cref="Play"/>.
    /// </summary>
    internal static class WmfPlayer
    {
        private sealed class PenState
        {
            public Color Color = Color.Black;
            public float Width = 1f;
            public bool IsNull;
        }

        private sealed class BrushState
        {
            public Color Color = Color.Transparent;
            public bool IsNull = true;
        }

        private readonly struct Viewport
        {
            private readonly Rectangle _logical;
            private readonly float _scaleX;
            private readonly float _scaleY;

            public Viewport(Rectangle logicalBounds, int pixelWidth, int pixelHeight)
            {
                _logical = logicalBounds;
                int logicalWidth = Math.Max(1, logicalBounds.Width);
                int logicalHeight = Math.Max(1, logicalBounds.Height);
                _scaleX = pixelWidth / (float)logicalWidth;
                _scaleY = pixelHeight / (float)logicalHeight;
            }

            public PointF MapPoint(float x, float y)
            {
                return new PointF(
                    (x - _logical.X) * _scaleX,
                    (y - _logical.Y) * _scaleY);
            }

            public RectangleF MapRect(float left, float top, float right, float bottom)
            {
                float x1 = (left - _logical.X) * _scaleX;
                float y1 = (top - _logical.Y) * _scaleY;
                float x2 = (right - _logical.X) * _scaleX;
                float y2 = (bottom - _logical.Y) * _scaleY;

                float minX = Math.Min(x1, x2);
                float minY = Math.Min(y1, y2);
                float width = Math.Abs(x2 - x1);
                float height = Math.Abs(y2 - y1);
                return new RectangleF(minX, minY, width, height);
            }
        }

        /// <summary>
        /// Replays WMF records onto the specified drawing context. This is the
        /// generalized entry point that targets any surface (raster, recording,
        /// SVG, EMF). The context's <see cref="IDrawingContext.Width"/> and
        /// <see cref="IDrawingContext.Height"/> define the output pixel size.
        /// </summary>
        /// <param name="document">The metafile document containing WMF records.</param>
        /// <param name="ctx">The target drawing context.</param>
        /// <param name="logicalBounds">The logical bounds of the metafile.</param>
        public static void Play(Metafile.MetafileDocument document, IDrawingContext ctx, Rectangle logicalBounds)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            var viewport = new Viewport(logicalBounds, ctx.Width, ctx.Height);
            var objects = new List<object?>();

            var currentPoint = new PointF(0f, 0f);
            var currentPen = new PenState();
            var currentBrush = new BrushState();

            ctx.Clear(Color.Transparent);

            foreach (var record in document.WmfRecords)
            {
                switch (record)
                {
                    case META_SETPIXEL setPixel:
                    {
                        int x = ClampToPixel(ctx.Width, viewport.MapPoint(setPixel.X, setPixel.Y).X);
                        int y = ClampToPixel(ctx.Height, viewport.MapPoint(setPixel.X, setPixel.Y).Y);
                        SetPixel(ctx, x, y, ToColor(setPixel.ColorRef));
                        break;
                    }
                    case Metafile.Wmf.Records.StateRecordTypes.META_MOVETO moveTo:
                        currentPoint = viewport.MapPoint(moveTo.X, moveTo.Y);
                        break;

                    case META_LINETO lineTo:
                    {
                        var next = viewport.MapPoint(lineTo.X, lineTo.Y);
                        DrawLine(ctx, currentPen, currentPoint, next);
                        currentPoint = next;
                        break;
                    }
                    case META_POLYLINE polyline when polyline.aPoints.Length >= 2:
                    {
                        var points = new PointF[polyline.aPoints.Length];
                        for (int i = 0; i < polyline.aPoints.Length; i++)
                        {
                            points[i] = viewport.MapPoint(polyline.aPoints[i].x, polyline.aPoints[i].y);
                        }

                        DrawPolyline(ctx, currentPen, points);
                        currentPoint = points[points.Length - 1];
                        break;
                    }
                    case META_POLYGON polygon when polygon.Points.Count >= 2:
                    {
                        var points = new PointF[polygon.Points.Count];
                        for (int i = 0; i < polygon.Points.Count; i++)
                        {
                            Point p = polygon.Points[i];
                            points[i] = viewport.MapPoint(p.X, p.Y);
                        }

                        FillPolygon(ctx, currentBrush, points);
                        DrawPolygon(ctx, currentPen, points);
                        break;
                    }
                    case META_RECTANGLE rectangle:
                    {
                        RectangleF rect = viewport.MapRect(
                            rectangle.LeftRect,
                            rectangle.TopRect,
                            rectangle.RightRect,
                            rectangle.BottomRect);

                        FillRectangle(ctx, currentBrush, rect);
                        DrawRectangle(ctx, currentPen, rect);
                        break;
                    }
                    case META_ELLIPSE ellipse:
                    {
                        RectangleF rect = viewport.MapRect(
                            ellipse.LeftRect,
                            ellipse.TopRect,
                            ellipse.RightRect,
                            ellipse.BottomRect);

                        FillEllipse(ctx, currentBrush, rect);
                        DrawEllipse(ctx, currentPen, rect);
                        break;
                    }
                    case MiniPdf.Drawing.Metafile.Wmf.Records.ObjectRecordTypes.META_CREATEPENINDIRECT createPen:
                        AllocateObject(objects, CreatePenState(createPen.Pen));
                        break;

                    case MetaCreatebrushindirect createBrush:
                        AllocateObject(objects, CreateBrushState(createBrush.LogBrush));
                        break;

                    case META_SELECTOBJECT selectObject:
                    {
                        if (selectObject.ObjectIndex >= objects.Count)
                            break;

                        object? selected = objects[selectObject.ObjectIndex];
                        if (selected is PenState penState)
                            currentPen = penState;
                        else if (selected is BrushState brushState)
                            currentBrush = brushState;
                        break;
                    }
                    case META_DELETEOBJECT deleteObject:
                    {
                        if (deleteObject.ObjectIndex < objects.Count)
                            objects[deleteObject.ObjectIndex] = null;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Renders a WMF document to a raster bitmap. This is the backward-compat
        /// wrapper that creates a <see cref="Graphics"/> from the bitmap and
        /// delegates to <see cref="Play"/>.
        /// </summary>
        public static void Render(Metafile.MetafileDocument document, Bitmap target, Rectangle logicalBounds)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            using var graphics = Graphics.FromImage(target);
            Play(document, graphics, logicalBounds);
        }

        private static void AllocateObject(List<object?> objects, object value)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] == null)
                {
                    objects[i] = value;
                    return;
                }
            }

            objects.Add(value);
        }

        private static PenState CreatePenState(Metafile.Wmf.Objects.Pen pen)
        {
            return new PenState
            {
                Color = ToColor(pen.ColorRef),
                Width = Math.Max(1f, Math.Abs(pen.Width.x)),
                IsNull = pen.PenStyle == Metafile.Wmf.Enumerations.PenStyle.PS_NULL,
            };
        }

        private static BrushState CreateBrushState(Metafile.Wmf.Objects.LogBrush brush)
        {
            return new BrushState
            {
                Color = ToColor(brush.ColorRef),
                IsNull = brush.BrushStyle == Metafile.Wmf.Enumerations.BrushStyle.BS_NULL,
            };
        }

        private static int ClampToPixel(int size, float value)
        {
            int v = (int)Math.Round(value);
            if (v < 0) return 0;
            if (v >= size) return size - 1;
            return v;
        }

        private static Color ToColor(Metafile.Wmf.Objects.ColorRef color)
            => Color.FromArgb(255, color.Red, color.Green, color.Blue);

        private static void SetPixel(IDrawingContext ctx, int x, int y, Color color)
        {
            using var brush = new SolidBrush(color);
            var path = new GraphicsPath();
            path.AddRectangle(new RectangleF(x, y, 1, 1));
            ctx.FillPath(brush, path);
        }

        private static void DrawLine(IDrawingContext ctx, PenState penState, PointF p1, PointF p2)
        {
            if (penState.IsNull)
                return;

            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddLine(p1, p2);
            ctx.DrawPath(pen, path);
        }

        private static void DrawPolyline(IDrawingContext ctx, PenState penState, PointF[] points)
        {
            if (penState.IsNull)
                return;

            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddLines(points);
            ctx.DrawPath(pen, path);
        }

        private static void DrawPolygon(IDrawingContext ctx, PenState penState, PointF[] points)
        {
            if (penState.IsNull)
                return;

            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddPolygon(points);
            ctx.DrawPath(pen, path);
        }

        private static void FillPolygon(IDrawingContext ctx, BrushState brushState, PointF[] points)
        {
            if (brushState.IsNull)
                return;

            using var brush = new SolidBrush(brushState.Color);
            var path = new GraphicsPath();
            path.AddPolygon(points);
            ctx.FillPath(brush, path);
        }

        private static void DrawRectangle(IDrawingContext ctx, PenState penState, RectangleF rect)
        {
            if (penState.IsNull)
                return;

            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddRectangle(rect);
            ctx.DrawPath(pen, path);
        }

        private static void FillRectangle(IDrawingContext ctx, BrushState brushState, RectangleF rect)
        {
            if (brushState.IsNull)
                return;

            using var brush = new SolidBrush(brushState.Color);
            var path = new GraphicsPath();
            path.AddRectangle(rect);
            ctx.FillPath(brush, path);
        }

        private static void DrawEllipse(IDrawingContext ctx, PenState penState, RectangleF rect)
        {
            if (penState.IsNull)
                return;

            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddEllipse(rect);
            ctx.DrawPath(pen, path);
        }

        private static void FillEllipse(IDrawingContext ctx, BrushState brushState, RectangleF rect)
        {
            if (brushState.IsNull)
                return;

            using var brush = new SolidBrush(brushState.Color);
            var path = new GraphicsPath();
            path.AddEllipse(rect);
            ctx.FillPath(brush, path);
        }
    }
}