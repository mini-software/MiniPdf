using System;
using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;
using MiniSoftware.Drawing.Pens;
using MiniSoftware.Drawing.Rendering;
using MiniSoftware.Drawing.Text;

namespace MiniSoftware.Drawing
{
    /// <summary>
    /// Primitive drawing-surface contract shared by raster, recording,
    /// SVG, and EMF surfaces. Convenience overloads (DrawLine, DrawEllipse, …)
    /// are NOT on this interface; they reduce to these primitives.
    /// </summary>
    internal interface IDrawingContext : IDisposable
    {
        // ── Dimensions / DPI ──
        int   Width  { get; }
        int   Height { get; }
        float DpiX   { get; }
        float DpiY   { get; }

        // ── Rendering state (get/set) ──
        Matrix             Transform          { get; set; }
        Region             Clip               { get; set; }
        SmoothingMode      SmoothingMode      { get; set; }
        InterpolationMode  InterpolationMode  { get; set; }
        CompositingMode    CompositingMode    { get; set; }
        CompositingQuality CompositingQuality { get; set; }
        PixelOffsetMode    PixelOffsetMode    { get; set; }
        TextRenderingHint  TextRenderingHint  { get; set; }
        GraphicsUnit       PageUnit           { get; set; }
        float              PageScale          { get; set; }
        Point              RenderingOrigin    { get; set; }

        // ── Transform mutators ──
        void ResetTransform();
        void MultiplyTransform(Matrix m, MatrixOrder order = MatrixOrder.Prepend);
        void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend);
        void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend);
        void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend);

        // ── Clip mutators ──
        void SetClip(RectangleF rect, CombineMode mode = CombineMode.Replace);
        void SetClip(GraphicsPath path, CombineMode mode = CombineMode.Replace);
        void SetClip(Region region, CombineMode mode = CombineMode.Replace);
        void IntersectClip(RectangleF rect);
        void IntersectClip(Region region);
        void ExcludeClip(Rectangle rect);
        void ExcludeClip(Region region);
        void ResetClip();
        void TranslateClip(float dx, float dy);

        // ── State stack ──
        GraphicsState      Save();
        void               Restore(GraphicsState state);
        GraphicsContainer  BeginContainer();
        GraphicsContainer  BeginContainer(RectangleF dst, RectangleF src, GraphicsUnit unit);
        void               EndContainer(GraphicsContainer container);

        // ── Primitives (everything else reduces to these) ──
        void Clear(Color color);
        void DrawPath(Pen pen, GraphicsPath path);
        void FillPath(Brush brush, GraphicsPath path);
        void FillPath(Brush brush, GraphicsPath path, FillMode fillMode);
        void DrawImage(Image image, RectangleF destRect, RectangleF srcRect,
                       GraphicsUnit srcUnit, ImageAttributes? attr);
        void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect,
                       GraphicsUnit srcUnit, ImageAttributes? attr);
        void DrawString(string s, Text.Font font, Brush brush,
                         RectangleF layoutRect, StringFormat? format);
    }
}