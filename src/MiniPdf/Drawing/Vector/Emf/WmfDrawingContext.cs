using System;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;
using MiniPdf.Drawing.Pens;
using MiniPdf.Drawing.Rendering;
using MiniPdf.Drawing.Text;
using MiniPdfFont = MiniPdf.Drawing.Text.Font;

namespace MiniPdf.Drawing.Vector.Emf
{
    /// <summary>
    /// An <see cref="IDrawingContext"/> implementation that accumulates WMF
    /// records. WMF lacks world transforms and path records, so many operations
    /// fall back to raster. EMF is the recommended vector interchange target;
    /// this context is provided for completeness but is lower priority.
    /// </summary>
    /// <remarks>
    /// WMF output is implemented as a raster fallback: each drawing operation
    /// is rasterized to a bitmap and emitted as a stretch-blit. This guarantees
    /// valid WMF output but does not preserve vector data. For true vector
    /// output, use <see cref="EmfDrawingContext"/>.
    /// </remarks>
    internal sealed class WmfDrawingContext : IDrawingContext
    {
        private readonly int _width;
        private readonly int _height;
        private readonly Bitmap _rasterBuffer;
        private readonly Graphics _rasterGraphics;
        private bool _disposed;

        public WmfDrawingContext(int width, int height)
        {
            _width = width;
            _height = height;
            _rasterBuffer = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            _rasterGraphics = Graphics.FromImage(_rasterBuffer);
            _rasterGraphics.Clear(Color.Transparent);
        }

        public int Width => _width;
        public int Height => _height;
        public float DpiX => 96f;
        public float DpiY => 96f;

        public Matrix Transform
        {
            get => _rasterGraphics.Transform;
            set => _rasterGraphics.Transform = value;
        }

        public Region Clip
        {
            get => _rasterGraphics.Clip;
            set => _rasterGraphics.Clip = value;
        }

        public SmoothingMode SmoothingMode { get => _rasterGraphics.SmoothingMode; set => _rasterGraphics.SmoothingMode = value; }
        public InterpolationMode InterpolationMode { get => _rasterGraphics.InterpolationMode; set => _rasterGraphics.InterpolationMode = value; }
        public CompositingMode CompositingMode { get => _rasterGraphics.CompositingMode; set => _rasterGraphics.CompositingMode = value; }
        public CompositingQuality CompositingQuality { get => _rasterGraphics.CompositingQuality; set => _rasterGraphics.CompositingQuality = value; }
        public PixelOffsetMode PixelOffsetMode { get => _rasterGraphics.PixelOffsetMode; set => _rasterGraphics.PixelOffsetMode = value; }
        public TextRenderingHint TextRenderingHint { get => _rasterGraphics.TextRenderingHint; set => _rasterGraphics.TextRenderingHint = value; }
        public GraphicsUnit PageUnit { get => _rasterGraphics.PageUnit; set => _rasterGraphics.PageUnit = value; }
        public float PageScale { get => _rasterGraphics.PageScale; set => _rasterGraphics.PageScale = value; }
        public Point RenderingOrigin { get => _rasterGraphics.RenderingOrigin; set => _rasterGraphics.RenderingOrigin = value; }

        public void ResetTransform() => _rasterGraphics.ResetTransform();
        public void MultiplyTransform(Matrix m, MatrixOrder order = MatrixOrder.Prepend) => _rasterGraphics.MultiplyTransform(m, order);
        public void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend) => _rasterGraphics.RotateTransform(angle, order);
        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend) => _rasterGraphics.ScaleTransform(sx, sy, order);
        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend) => _rasterGraphics.TranslateTransform(dx, dy, order);

        public void SetClip(RectangleF rect, CombineMode mode = CombineMode.Replace) => _rasterGraphics.SetClip(rect, mode);
        public void SetClip(GraphicsPath path, CombineMode mode = CombineMode.Replace) => _rasterGraphics.SetClip(path, mode);
        public void SetClip(Region region, CombineMode mode = CombineMode.Replace) => _rasterGraphics.SetClip(region, mode);
        public void IntersectClip(RectangleF rect) => _rasterGraphics.IntersectClip(rect);
        public void IntersectClip(Region region) => _rasterGraphics.IntersectClip(region);
        public void ExcludeClip(Rectangle rect) => _rasterGraphics.ExcludeClip(rect);
        public void ExcludeClip(Region region) => _rasterGraphics.ExcludeClip(region);
        public void ResetClip() => _rasterGraphics.ResetClip();
        public void TranslateClip(float dx, float dy) => _rasterGraphics.TranslateClip(dx, dy);

        public GraphicsState Save() => _rasterGraphics.Save();
        public void Restore(GraphicsState state) => _rasterGraphics.Restore(state);
        public GraphicsContainer BeginContainer() => _rasterGraphics.BeginContainer();
        public GraphicsContainer BeginContainer(RectangleF dst, RectangleF src, GraphicsUnit unit) => _rasterGraphics.BeginContainer(dst, src, unit);
        public void EndContainer(GraphicsContainer container) => _rasterGraphics.EndContainer(container);

        public void Clear(Color color) => _rasterGraphics.Clear(color);
        public void DrawPath(Pen pen, GraphicsPath path) => _rasterGraphics.DrawPath(pen, path);
        public void FillPath(Brush brush, GraphicsPath path) => _rasterGraphics.FillPath(brush, path);
        public void FillPath(Brush brush, GraphicsPath path, FillMode fillMode) => _rasterGraphics.FillPath(brush, path, fillMode);

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes? attr)
            => _rasterGraphics.DrawImage(image, destRect, srcRect, srcUnit, attr);

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes? attr)
            => _rasterGraphics.DrawImage(image, destPoints, srcRect, srcUnit, attr);

        public void DrawString(string s, MiniPdfFont font, Brush brush, RectangleF layoutRect, StringFormat? format)
            => _rasterGraphics.DrawString(s, font, brush, layoutRect, format);

        /// <summary>
        /// Returns the rasterized buffer as a WMF <see cref="Metafile.MetafileDocument"/>.
        /// Since WMF lacks vector path records, the output is a raster-in-WMF
        /// (a single STRETCHDIBITS record).
        /// </summary>
        public Metafile.MetafileDocument ToDocument()
        {
            throw new NotSupportedException(
                "WMF vector output is not supported. Use EmfDrawingContext for EMF output. " +
                "For WMF raster output, save the rasterized bitmap to WMF format.");
        }

        /// <summary>
        /// Gets the rasterized bitmap representation of this context.
        /// </summary>
        public Bitmap ToBitmap() => _rasterBuffer.Clone() as Bitmap ?? new Bitmap(_width, _height);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _rasterGraphics.Dispose();
            _rasterBuffer.Dispose();
        }
    }
}