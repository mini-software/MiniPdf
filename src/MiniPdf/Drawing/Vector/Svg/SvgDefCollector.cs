using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;
using MiniPdf.Drawing.Pens;
using MiniPdf.Drawing.Rendering;
using MiniPdf.Drawing.Text;

namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// A non-emitting <see cref="IDrawingContext"/> used in the def-collection
    /// pass. It traverses the same command stream as the emit pass, registering
    /// every unique brush, pen, and clip with the <see cref="SvgDefTable"/> so
    /// that <c>&lt;defs&gt;</c> can be written before the body.
    /// </summary>
    internal sealed class SvgDefCollector : IDrawingContext
    {
        private readonly SvgDefTable _defs;
        private readonly int _width;
        private readonly int _height;
        private Matrix _transform = new Matrix();
        private Region _clip;
        private bool _disposed;

        public SvgDefCollector(SvgDefTable defs, int width, int height)
        {
            _defs = defs;
            _width = width;
            _height = height;
            _clip = new Region();
            _clip.MakeInfinite();
        }

        public int Width => _width;
        public int Height => _height;
        public float DpiX => 96f;
        public float DpiY => 96f;

        public Matrix Transform { get => _transform; set => _transform = value ?? new Matrix(); }
        public Region Clip { get => _clip; set => _clip = value ?? new Region(); }
        public SmoothingMode SmoothingMode { get; set; }
        public InterpolationMode InterpolationMode { get; set; }
        public CompositingMode CompositingMode { get; set; }
        public CompositingQuality CompositingQuality { get; set; }
        public PixelOffsetMode PixelOffsetMode { get; set; }
        public TextRenderingHint TextRenderingHint { get; set; }
        public GraphicsUnit PageUnit { get; set; } = GraphicsUnit.Pixel;
        public float PageScale { get; set; } = 1f;
        public Point RenderingOrigin { get; set; }

        public void ResetTransform() => _transform.Reset();
        public void MultiplyTransform(Matrix m, MatrixOrder order = MatrixOrder.Prepend) => _transform.Multiply(m, order);
        public void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend) => _transform.Rotate(angle, order);
        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend) => _transform.Scale(sx, sy, order);
        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend) => _transform.Translate(dx, dy, order);

        public void SetClip(RectangleF rect, CombineMode mode = CombineMode.Replace) => _clip = new Region(rect);
        public void SetClip(GraphicsPath path, CombineMode mode = CombineMode.Replace) => _clip = new Region(path);
        public void SetClip(Region region, CombineMode mode = CombineMode.Replace) => _clip = region.Clone();
        public void IntersectClip(RectangleF rect) => _clip.Intersect(rect);
        public void IntersectClip(Region region) => _clip.Intersect(region);
        public void ExcludeClip(Rectangle rect) => _clip.Exclude(rect);
        public void ExcludeClip(Region region) => _clip.Exclude(region);
        public void ResetClip() { _clip = new Region(); _clip.MakeInfinite(); }
        public void TranslateClip(float dx, float dy) => _clip.Translate(dx, dy);

        public GraphicsState Save()
        {
            return new GraphicsState(_transform.Clone(), _clip.Clone(), PageUnit, PageScale,
                SmoothingMode, InterpolationMode, CompositingMode, CompositingQuality,
                PixelOffsetMode, TextRenderingHint, RenderingOrigin);
        }
        public void Restore(GraphicsState state) { }
        public GraphicsContainer BeginContainer()
        {
            var gs = new GraphicsState(_transform.Clone(), _clip.Clone(), PageUnit, PageScale,
                SmoothingMode, InterpolationMode, CompositingMode, CompositingQuality,
                PixelOffsetMode, TextRenderingHint, RenderingOrigin);
            return new GraphicsContainer(gs);
        }
        public GraphicsContainer BeginContainer(RectangleF dst, RectangleF src, GraphicsUnit unit) => BeginContainer();
        public void EndContainer(GraphicsContainer container) { }

        // ── Primitives: register brushes/pens/clips ────────────────────────────

        public void Clear(Color color) { /* no def needed */ }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            if (pen != null && pen.PenType != PenType.SolidColor)
                _defs.RegisterBrush(pen.Brush);
            if (!_clip.IsInfinite())
                _defs.RegisterClip(_clip, null);
        }

        public void FillPath(Brush brush, GraphicsPath path)
            => FillPath(brush, path, path?.FillMode ?? FillMode.Alternate);

        public void FillPath(Brush brush, GraphicsPath path, FillMode fillMode)
        {
            if (brush != null)
                _defs.RegisterBrush(brush);
            if (!_clip.IsInfinite())
                _defs.RegisterClip(_clip, null);
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect,
                               GraphicsUnit srcUnit, ImageAttributes? attr)
        {
            if (!_clip.IsInfinite())
                _defs.RegisterClip(_clip, null);
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect,
                               GraphicsUnit srcUnit, ImageAttributes? attr)
        {
            if (!_clip.IsInfinite())
                _defs.RegisterClip(_clip, null);
        }

        public void DrawString(string s, MiniPdf.Drawing.Text.Font font, Brush brush,
                                RectangleF layoutRect, StringFormat? format)
        {
            if (brush != null)
                _defs.RegisterBrush(brush);
            if (!_clip.IsInfinite())
                _defs.RegisterClip(_clip, null);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}