using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;
using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
using MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes;
using MiniSoftware.Drawing.Metafile.Emf.Records.ObjectRecordTypes;
using MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes;
using MiniSoftware.Drawing.Pens;
using MiniPdfFont = MiniSoftware.Drawing.Text.Font;

namespace MiniSoftware.Drawing.Rendering
{
    /// <summary>
    /// Replays EMF/EMF+ records onto any <see cref="IDrawingContext"/> (raster
    /// <see cref="Graphics"/>, <see cref="RecordingGraphics"/>, or any other
    /// context). The legacy <see cref="Render"/> overload remains as a thin
    /// backward-compat wrapper that creates a raster <see cref="Graphics"/> and
    /// delegates to <see cref="Play"/>.
    /// </summary>
    internal static class EmfPlayer
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

        private sealed class FontState
        {
            public string FamilyName = "Arial";
            public float Size = 12f;
            public bool Bold;
            public bool Italic;
            public bool Underline;
            public bool Strikeout;
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
        /// Replays EMF records onto the specified drawing context.
        /// </summary>
        public static void Play(Metafile.MetafileDocument document, IDrawingContext ctx, Rectangle logicalBounds)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            var viewport = new Viewport(logicalBounds, ctx.Width, ctx.Height);
            var objects = new Dictionary<uint, object>();

            var currentPoint = new PointF(0f, 0f);
            var currentPen = new PenState();
            var currentBrush = new BrushState();
            var currentFont = new FontState();
            var textColor = Color.Black;

            GraphicsPath? pathBuilder = null;
            var pathCurrentPoint = new PointF(0f, 0f);

            ctx.Clear(Color.Transparent);

            foreach (var record in document.EmfRecords)
            {
                switch (record)
                {
                    // ── Pixel ──
                    case EMR_SETPIXELV setPixel:
                    {
                        var mapped = viewport.MapPoint(setPixel.Position.x, setPixel.Position.y);
                        int x = ClampToPixel(ctx.Width, mapped.X);
                        int y = ClampToPixel(ctx.Height, mapped.Y);
                        SetPixel(ctx, x, y, ToColor(setPixel.Color));
                        break;
                    }

                    // ── Move/Line (path-aware) ──
                    case EMR_MOVETOEX moveTo:
                    {
                        var mapped = viewport.MapPoint(moveTo.Offset.x, moveTo.Offset.y);
                        if (pathBuilder != null)
                        {
                            pathBuilder.StartFigure();
                            pathCurrentPoint = mapped;
                        }
                        currentPoint = mapped;
                        break;
                    }

                    case EMR_LINETO lineTo:
                    {
                        var next = viewport.MapPoint(lineTo.Point.x, lineTo.Point.y);
                        if (pathBuilder != null)
                        {
                            pathBuilder.AddLine(pathCurrentPoint, next);
                            pathCurrentPoint = next;
                        }
                        else
                        {
                            DrawLine(ctx, currentPen, currentPoint, next);
                        }
                        currentPoint = next;
                        break;
                    }

                    // ── Polylines (outside path) ──
                    case EMR_POLYLINE polyline when polyline.Points.Length >= 2:
                    {
                        var points = MapPoints(viewport, polyline.Points);
                        DrawPolyline(ctx, currentPen, points);
                        currentPoint = points[points.Length - 1];
                        break;
                    }
                    case EMR_POLYLINE16 polyline16 when polyline16.Points.Length >= 2:
                    {
                        var points = MapPointsS(viewport, polyline16.Points);
                        DrawPolyline(ctx, currentPen, points);
                        currentPoint = points[points.Length - 1];
                        break;
                    }

                    // ── PolylineTo (path-aware) ──
                    case EMR_POLYLINETO polyLineTo when polyLineTo.Points.Length >= 1:
                    {
                        var points = MapPoints(viewport, polyLineTo.Points);
                        if (pathBuilder != null)
                        {
                            for (int i = 0; i < points.Length; i++)
                            {
                                pathBuilder.AddLine(pathCurrentPoint, points[i]);
                                pathCurrentPoint = points[i];
                            }
                        }
                        currentPoint = points[points.Length - 1];
                        break;
                    }
                    case EMR_POLYLINETO16 polyLineTo16 when polyLineTo16.Points.Length >= 1:
                    {
                        var points = MapPointsS(viewport, polyLineTo16.Points);
                        if (pathBuilder != null)
                        {
                            for (int i = 0; i < points.Length; i++)
                            {
                                pathBuilder.AddLine(pathCurrentPoint, points[i]);
                                pathCurrentPoint = points[i];
                            }
                        }
                        currentPoint = points[points.Length - 1];
                        break;
                    }

                    // ── PolyBezierTo (path-aware) ──
                    case EMR_POLYBEZIERTO polyBezierTo when polyBezierTo.Points.Length >= 3:
                    {
                        var points = MapPoints(viewport, polyBezierTo.Points);
                        if (pathBuilder != null)
                        {
                            for (int i = 0; i + 2 < points.Length; i += 3)
                            {
                                pathBuilder.AddBezier(pathCurrentPoint, points[i], points[i + 1], points[i + 2]);
                                pathCurrentPoint = points[i + 2];
                            }
                        }
                        currentPoint = points[points.Length - 1];
                        break;
                    }
                    case EMR_POLYBEZIERTO16 polyBezierTo16 when polyBezierTo16.Points.Length >= 3:
                    {
                        var points = MapPointsS(viewport, polyBezierTo16.Points);
                        if (pathBuilder != null)
                        {
                            for (int i = 0; i + 2 < points.Length; i += 3)
                            {
                                pathBuilder.AddBezier(pathCurrentPoint, points[i], points[i + 1], points[i + 2]);
                                pathCurrentPoint = points[i + 2];
                            }
                        }
                        currentPoint = points[points.Length - 1];
                        break;
                    }

                    // ── Polygons ──
                    case EMR_POLYGON polygon when polygon.Points.Length >= 2:
                    {
                        var points = MapPoints(viewport, polygon.Points);
                        FillPolygon(ctx, currentBrush, points);
                        DrawPolygon(ctx, currentPen, points);
                        break;
                    }
                    case EMR_POLYGON16 polygon16 when polygon16.Points.Length >= 2:
                    {
                        var points = MapPointsS(viewport, polygon16.Points);
                        FillPolygon(ctx, currentBrush, points);
                        DrawPolygon(ctx, currentPen, points);
                        break;
                    }

                    // ── Primitives ──
                    case EMR_RECTANGLE rectangle:
                    {
                        RectangleF rect = viewport.MapRect(
                            rectangle.Box.Left, rectangle.Box.Top,
                            rectangle.Box.Right, rectangle.Box.Bottom);
                        FillRectangle(ctx, currentBrush, rect);
                        DrawRectangle(ctx, currentPen, rect);
                        break;
                    }
                    case EMR_ELLIPSE ellipse:
                    {
                        RectangleF rect = viewport.MapRect(
                            ellipse.Box.Left, ellipse.Box.Top,
                            ellipse.Box.Right, ellipse.Box.Bottom);
                        FillEllipse(ctx, currentBrush, rect);
                        DrawEllipse(ctx, currentPen, rect);
                        break;
                    }

                    // ── Path bracket ──
                    case EMR_BEGINPATH _:
                        pathBuilder = new GraphicsPath();
                        pathCurrentPoint = currentPoint;
                        break;

                    case EMR_ENDPATH _:
                        // Path is finalized; pathBuilder stays for STROKEPATH/FILLPATH
                        break;

                    case EMR_CLOSEFIGURE _:
                        pathBuilder?.CloseFigure();
                        break;

                    case EMR_ABORTPATH _:
                        pathBuilder = null;
                        break;

                    case EMR_FLATTENPATH _:
                        // No-op: our path model is already flat-compatible
                        break;

                    case EMR_STROKEPATH _:
                    {
                        if (pathBuilder != null && pathBuilder.PointCount > 0)
                            DrawPathDirect(ctx, currentPen, pathBuilder);
                        break;
                    }
                    case EMR_FILLPATH _:
                    {
                        if (pathBuilder != null && pathBuilder.PointCount > 0)
                            FillPathDirect(ctx, currentBrush, pathBuilder);
                        break;
                    }
                    case EMR_STROKEANDFILLPATH _:
                    {
                        if (pathBuilder != null && pathBuilder.PointCount > 0)
                        {
                            FillPathDirect(ctx, currentBrush, pathBuilder);
                            DrawPathDirect(ctx, currentPen, pathBuilder);
                        }
                        break;
                    }

                    // ── Object table ──
                    case EMR_CREATEPEN createPen:
                        objects[createPen.IhPen] = CreatePenState(createPen.Pen);
                        break;

                    case EMR_CREATEBRUSHINDIRECT createBrush:
                        objects[createBrush.IhBrush] = CreateBrushState(createBrush.LogBrush);
                        break;

                    case EMR_EXTCREATEFONTINDIRECTW createFont:
                        objects[createFont.IhFont] = CreateFontState(createFont.Font);
                        break;

                    case EMR_SELECTOBJECT selectObject:
                    {
                        if (!objects.TryGetValue(selectObject.ObjectIndex, out object? selected))
                            break;
                        if (selected is PenState penState)
                            currentPen = penState;
                        else if (selected is BrushState brushState)
                            currentBrush = brushState;
                        else if (selected is FontState fontState)
                            currentFont = fontState;
                        break;
                    }
                    case EMR_DELETEOBJECT deleteObject:
                        objects.Remove(deleteObject.ObjectIndex);
                        break;

                    // ── Transforms ──
                    case EMR_SETWORLDTRANSFORM wt:
                    {
                        var m = new Matrix(wt.M11, wt.M12, wt.M21, wt.M22, wt.Dx, wt.Dy);
                        ctx.Transform = m;
                        break;
                    }
                    case EMR_MODIFYWORLDTRANSFORM mwt:
                    {
                        var m = new Matrix(mwt.M11, mwt.M12, mwt.M21, mwt.M22, mwt.Dx, mwt.Dy);
                        switch (mwt.ModifyWorldTransformMode)
                        {
                            case ModifyWorldTransformMode.MWT_IDENTITY:
                                ctx.ResetTransform();
                                break;
                            case ModifyWorldTransformMode.MWT_LEFTMULTIPLY:
                                ctx.MultiplyTransform(m, MatrixOrder.Prepend);
                                break;
                            case ModifyWorldTransformMode.MWT_RIGHTMULTIPLY:
                                ctx.MultiplyTransform(m, MatrixOrder.Append);
                                break;
                            case ModifyWorldTransformMode.MWT_SET:
                                ctx.Transform = m;
                                break;
                        }
                        break;
                    }

                    // ── State stack ──
                    case EMR_SAVEDC _:
                        ctx.Save();
                        break;
                    case EMR_RESTOREDC rdc:
                        // SavedDC is negative in well-formed EMF (relative restore)
                        // We do a single Restore for each RESTOREDC
                        ctx.Restore(ctx.Save());
                        break;

                    // ── Window/MapMode (accepted but viewport handles scaling) ──
                    case EMR_SETWINDOWEXTEX _:
                    case EMR_SETWINDOWORGEX _:
                    case EMR_SETVIEWPORTEXTEX _:
                    case EMR_SETVIEWPORTORGEX _:
                    case EMR_SETMAPMODE _:
                    case EMR_SETPOLYFILLMODE _:
                    case EMR_SETBKMODE _:
                    case EMR_SETBKCOLOR _:
                    case EMR_SETROP2 _:
                    case EMR_SETSTRETCHBLTMODE _:
                    case EMR_SETTEXTALIGN _:
                    case EMR_SETMITERLIMIT _:
                    case EMR_SETARCDIRECTION _:
                    case EMR_SETLAYOUT _:
                        break;

                    case EMR_SETTEXTCOLOR stc:
                        textColor = ToColor(stc.Color);
                        break;

                    // ── Clip ──
                    case EMR_INTERSECTCLIPRECT icr:
                    {
                        RectangleF rect = viewport.MapRect(
                            icr.Clip.Left, icr.Clip.Top,
                            icr.Clip.Right, icr.Clip.Bottom);
                        ctx.IntersectClip(rect);
                        break;
                    }
                    case EMR_EXCLUDECLIPRECT ecr:
                    {
                        var rf = viewport.MapRect(
                            ecr.Clip.Left, ecr.Clip.Top,
                            ecr.Clip.Right, ecr.Clip.Bottom);
                        var rect = new Rectangle(
                            (int)Math.Round(rf.X), (int)Math.Round(rf.Y),
                            (int)Math.Round(rf.Width), (int)Math.Round(rf.Height));
                        ctx.ExcludeClip(rect);
                        break;
                    }
                    case EMR_EXTSELECTCLIPRGN _:
                        // Region data parsing is complex; skip for now
                        break;
                    case EMR_SETMETARGN _:
                        break;

                    // ── Text ──
                    case EMR_EXTTEXTOUTW textOut:
                    {
                        if (textOut.EmrText?.OutputString?.Length > 0)
                        {
                            var refPoint = viewport.MapPoint(
                                textOut.EmrText.Reference.x,
                                textOut.EmrText.Reference.y);
                            string text = System.Text.Encoding.Unicode.GetString(textOut.EmrText.OutputString);
                            var style = FontStyle.Regular;
                            if (currentFont.Bold) style |= FontStyle.Bold;
                            if (currentFont.Italic) style |= FontStyle.Italic;
                            if (currentFont.Underline) style |= FontStyle.Underline;
                            if (currentFont.Strikeout) style |= FontStyle.Strikeout;
                            using var font = new MiniPdfFont(currentFont.FamilyName, currentFont.Size, style, GraphicsUnit.Pixel);
                            using var brush = new SolidBrush(textColor);
                            var layoutRect = new RectangleF(refPoint.X, refPoint.Y - font.Height, 1000, font.Height);
                            ctx.DrawString(text, font, brush, layoutRect, null);
                        }
                        break;
                    }

                    // ── Image ──
                    case EMR_STRETCHDIBITS dibits:
                    {
                        var destRect = viewport.MapRect(
                            dibits.Bounds.Left, dibits.Bounds.Top,
                            dibits.Bounds.Right, dibits.Bounds.Bottom);
                        var bmp = DibToBitmap(dibits.BmiSrc, dibits.BitsSrc);
                        if (bmp != null)
                        {
                            ctx.DrawImage(bmp, destRect,
                                new RectangleF(0, 0, bmp.Width, bmp.Height),
                                GraphicsUnit.Pixel, null);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Renders an EMF document to a raster bitmap (backward-compat wrapper).
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

        // ── Helpers ──────────────────────────────────────────────────────────

        private static PointF[] MapPoints(Viewport vp, Metafile.Wmf.Objects.PointL[] pts)
        {
            var result = new PointF[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                result[i] = vp.MapPoint(pts[i].x, pts[i].y);
            return result;
        }

        private static PointF[] MapPointsS(Viewport vp, Metafile.Wmf.Objects.PointS[] pts)
        {
            var result = new PointF[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                result[i] = vp.MapPoint(pts[i].x, pts[i].y);
            return result;
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

        private static FontState CreateFontState(Metafile.Wmf.Objects.Font font)
        {
            return new FontState
            {
                FamilyName = string.IsNullOrEmpty(font.Facename) ? "Arial" : font.Facename,
                Size = Math.Abs(font.Height) > 0 ? Math.Abs(font.Height) : 12f,
                Bold = font.Weight >= 700,
                Italic = font.Italic != 0,
                Underline = font.Underline != 0,
                Strikeout = font.StrikeOut != 0,
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

        private static void DrawPathDirect(IDrawingContext ctx, PenState penState, GraphicsPath path)
        {
            if (penState.IsNull) return;
            using var pen = new Pen(penState.Color, penState.Width);
            ctx.DrawPath(pen, path);
        }

        private static void FillPathDirect(IDrawingContext ctx, BrushState brushState, GraphicsPath path)
        {
            if (brushState.IsNull) return;
            using var brush = new SolidBrush(brushState.Color);
            ctx.FillPath(brush, path);
        }

        private static void DrawLine(IDrawingContext ctx, PenState penState, PointF p1, PointF p2)
        {
            if (penState.IsNull) return;
            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddLine(p1, p2);
            ctx.DrawPath(pen, path);
        }

        private static void DrawPolyline(IDrawingContext ctx, PenState penState, PointF[] points)
        {
            if (penState.IsNull) return;
            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddLines(points);
            ctx.DrawPath(pen, path);
        }

        private static void DrawPolygon(IDrawingContext ctx, PenState penState, PointF[] points)
        {
            if (penState.IsNull) return;
            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddPolygon(points);
            ctx.DrawPath(pen, path);
        }

        private static void FillPolygon(IDrawingContext ctx, BrushState brushState, PointF[] points)
        {
            if (brushState.IsNull) return;
            using var brush = new SolidBrush(brushState.Color);
            var path = new GraphicsPath();
            path.AddPolygon(points);
            ctx.FillPath(brush, path);
        }

        private static void DrawRectangle(IDrawingContext ctx, PenState penState, RectangleF rect)
        {
            if (penState.IsNull) return;
            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddRectangle(rect);
            ctx.DrawPath(pen, path);
        }

        private static void FillRectangle(IDrawingContext ctx, BrushState brushState, RectangleF rect)
        {
            if (brushState.IsNull) return;
            using var brush = new SolidBrush(brushState.Color);
            var path = new GraphicsPath();
            path.AddRectangle(rect);
            ctx.FillPath(brush, path);
        }

        private static void DrawEllipse(IDrawingContext ctx, PenState penState, RectangleF rect)
        {
            if (penState.IsNull) return;
            using var pen = new Pen(penState.Color, penState.Width);
            var path = new GraphicsPath();
            path.AddEllipse(rect);
            ctx.DrawPath(pen, path);
        }

        private static void FillEllipse(IDrawingContext ctx, BrushState brushState, RectangleF rect)
        {
            if (brushState.IsNull) return;
            using var brush = new SolidBrush(brushState.Color);
            var path = new GraphicsPath();
            path.AddEllipse(rect);
            ctx.FillPath(brush, path);
        }

        private static Bitmap? DibToBitmap(byte[] bmi, byte[] bits)
        {
            if (bmi == null || bmi.Length < 40 || bits == null || bits.Length == 0)
                return null;

            try
            {
                int width = BitConverter.ToInt32(bmi, 4);
                int height = BitConverter.ToInt32(bmi, 8);
                ushort bpp = BitConverter.ToUInt16(bmi, 14);
                if (width <= 0 || height == 0 || bpp == 0)
                    return null;

                bool topDown = height < 0;
                height = Math.Abs(height);

                var format = bpp switch
                {
                    32 => PixelFormat.Format32bppArgb,
                    24 => PixelFormat.Format24bppRgb,
                    _ => PixelFormat.Format32bppArgb,
                };

                var bmp = new Bitmap(width, height, format);
                int stride = Bitmap.CalcStride(width, format);
                int copyLen = Math.Min(bits.Length, stride * height);
                Buffer.BlockCopy(bits, 0, bmp._pixels, 0, copyLen);

                if (topDown)
                {
                    // Already top-down
                }
                else
                {
                    // Flip vertically (bottom-up → top-down)
                    FlipVertical(bmp._pixels, stride, height);
                }

                return bmp;
            }
            catch
            {
                return null;
            }
        }

        private static void FlipVertical(byte[] pixels, int stride, int height)
        {
            int half = height / 2;
            var temp = new byte[stride];
            for (int y = 0; y < half; y++)
            {
                int top = y * stride;
                int bot = (height - 1 - y) * stride;
                Buffer.BlockCopy(pixels, top, temp, 0, stride);
                Buffer.BlockCopy(pixels, bot, pixels, top, stride);
                Buffer.BlockCopy(temp, 0, pixels, bot, stride);
            }
        }
    }
}