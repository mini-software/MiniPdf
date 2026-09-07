using System;
using System.Collections.Generic;
using System.Xml;
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
    /// An <see cref="IDrawingContext"/> implementation that emits SVG XML from
    /// replayed <see cref="DrawingCommand"/>s. Used as the emit-pass target in
    /// the two-pass SVG writer pipeline.
    /// </summary>
    internal sealed class SvgDrawingContext : IDrawingContext
    {
        private readonly SvgXmlWriter _w;
        private readonly SvgDefTable _defs;
        private readonly SvgEncoderParameters _params;
        private readonly int _width;
        private readonly int _height;

        private Matrix _transform = new Matrix();
        private Region _clip = new Region();
        private SmoothingMode _smoothingMode = SmoothingMode.None;
        private InterpolationMode _interpolationMode = InterpolationMode.NearestNeighbor;
        private CompositingMode _compositingMode = CompositingMode.SourceOver;
        private CompositingQuality _compositingQuality = CompositingQuality.Default;
        private PixelOffsetMode _pixelOffsetMode = PixelOffsetMode.Default;
        private TextRenderingHint _textRenderingHint = TextRenderingHint.SystemDefault;
        private GraphicsUnit _pageUnit = GraphicsUnit.Pixel;
        private float _pageScale = 1f;
        private Point _renderingOrigin = new Point(0, 0);

        private readonly Stack<StateFrame> _stateStack = new Stack<StateFrame>();
        private bool _disposed;

        public SvgDrawingContext(SvgXmlWriter w, SvgDefTable defs, SvgEncoderParameters parameters,
                                  int width, int height)
        {
            _w = w;
            _defs = defs;
            _params = parameters;
            _width = width;
            _height = height;
            _clip.MakeInfinite();
        }

        // ── Dimensions / DPI ──────────────────────────────────────────────────

        public int Width => _width;
        public int Height => _height;
        public float DpiX => 96f;
        public float DpiY => 96f;

        // ── Rendering state ───────────────────────────────────────────────────

        public Matrix Transform
        {
            get => _transform;
            set => _transform = value ?? new Matrix();
        }

        public Region Clip
        {
            get => _clip;
            set => _clip = value ?? new Region();
        }

        public SmoothingMode SmoothingMode { get => _smoothingMode; set => _smoothingMode = value; }
        public InterpolationMode InterpolationMode { get => _interpolationMode; set => _interpolationMode = value; }
        public CompositingMode CompositingMode { get => _compositingMode; set => _compositingMode = value; }
        public CompositingQuality CompositingQuality { get => _compositingQuality; set => _compositingQuality = value; }
        public PixelOffsetMode PixelOffsetMode { get => _pixelOffsetMode; set => _pixelOffsetMode = value; }
        public TextRenderingHint TextRenderingHint { get => _textRenderingHint; set => _textRenderingHint = value; }
        public GraphicsUnit PageUnit { get => _pageUnit; set => _pageUnit = value; }
        public float PageScale { get => _pageScale; set => _pageScale = value; }
        public Point RenderingOrigin { get => _renderingOrigin; set => _renderingOrigin = value; }

        // ── Transform mutators ────────────────────────────────────────────────

        public void ResetTransform() => _transform.Reset();

        public void MultiplyTransform(Matrix m, MatrixOrder order = MatrixOrder.Prepend)
            => _transform.Multiply(m, order);

        public void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend)
            => _transform.Rotate(angle, order);

        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)
            => _transform.Scale(sx, sy, order);

        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend)
            => _transform.Translate(dx, dy, order);

        // ── Clip mutators ─────────────────────────────────────────────────────

        public void SetClip(RectangleF rect, CombineMode mode = CombineMode.Replace)
        {
            _clip = new Region(rect);
        }

        public void SetClip(GraphicsPath path, CombineMode mode = CombineMode.Replace)
        {
            _clip = new Region(path);
        }

        public void SetClip(Region region, CombineMode mode = CombineMode.Replace)
        {
            _clip = region.Clone();
        }

        public void IntersectClip(RectangleF rect) => _clip.Intersect(rect);
        public void IntersectClip(Region region) => _clip.Intersect(region);
        public void ExcludeClip(Rectangle rect) => _clip.Exclude(rect);
        public void ExcludeClip(Region region) => _clip.Exclude(region);
        public void ResetClip() { _clip = new Region(); _clip.MakeInfinite(); }
        public void TranslateClip(float dx, float dy) => _clip.Translate(dx, dy);

        // ── State stack ───────────────────────────────────────────────────────

        private readonly Dictionary<GraphicsState, int> _stateDepths = new Dictionary<GraphicsState, int>();
        private readonly Dictionary<GraphicsContainer, int> _containerDepths = new Dictionary<GraphicsContainer, int>();

        public GraphicsState Save()
        {
            _stateStack.Push(CaptureState());
            var token = new GraphicsState(_transform.Clone(), _clip.Clone(), _pageUnit, _pageScale,
                _smoothingMode, _interpolationMode, _compositingMode, _compositingQuality,
                _pixelOffsetMode, _textRenderingHint, _renderingOrigin);
            _stateDepths[token] = _stateStack.Count;
            return token;
        }

        public void Restore(GraphicsState state)
        {
            if (state == null) return;
            if (!_stateDepths.TryGetValue(state, out int depth)) return;
            while (_stateStack.Count >= depth && _stateStack.Count > 0)
                RestoreState(_stateStack.Pop());
        }

        public GraphicsContainer BeginContainer()
        {
            _stateStack.Push(CaptureState());
            var gs = new GraphicsState(_transform.Clone(), _clip.Clone(), _pageUnit, _pageScale,
                _smoothingMode, _interpolationMode, _compositingMode, _compositingQuality,
                _pixelOffsetMode, _textRenderingHint, _renderingOrigin);
            var token = new GraphicsContainer(gs);
            _containerDepths[token] = _stateStack.Count;
            return token;
        }

        public GraphicsContainer BeginContainer(RectangleF dst, RectangleF src, GraphicsUnit unit)
            => BeginContainer();

        public void EndContainer(GraphicsContainer container)
        {
            if (container == null) return;
            if (!_containerDepths.TryGetValue(container, out int depth)) return;
            while (_stateStack.Count >= depth && _stateStack.Count > 0)
                RestoreState(_stateStack.Pop());
        }

        private StateFrame CaptureState() => new StateFrame
        {
            Transform = _transform.Clone(),
            Clip = _clip.Clone(),
            SmoothingMode = _smoothingMode,
            InterpolationMode = _interpolationMode,
            CompositingMode = _compositingMode,
            CompositingQuality = _compositingQuality,
            PixelOffsetMode = _pixelOffsetMode,
            TextRenderingHint = _textRenderingHint,
            PageUnit = _pageUnit,
            PageScale = _pageScale,
            RenderingOrigin = _renderingOrigin,
        };

        private void RestoreState(StateFrame frame)
        {
            _transform = frame.Transform;
            _clip = frame.Clip;
            _smoothingMode = frame.SmoothingMode;
            _interpolationMode = frame.InterpolationMode;
            _compositingMode = frame.CompositingMode;
            _compositingQuality = frame.CompositingQuality;
            _pixelOffsetMode = frame.PixelOffsetMode;
            _textRenderingHint = frame.TextRenderingHint;
            _pageUnit = frame.PageUnit;
            _pageScale = frame.PageScale;
            _renderingOrigin = frame.RenderingOrigin;
        }

        // ── Primitives ────────────────────────────────────────────────────────

        public void Clear(Color color)
        {
            // Emit a full-canvas background rect
            _w.WriteStartElement("rect");
            _w.WriteAttribute("x", 0);
            _w.WriteAttribute("y", 0);
            _w.WriteAttribute("width", _width);
            _w.WriteAttribute("height", _height);
            _w.WriteAttribute("fill", SvgBrushWriter.ColorToCss(color));
            if (color.A < 255)
                _w.WriteAttribute("fill-opacity", color.A / 255f);
            _w.WriteEndElement();
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            if (pen == null || path == null || path.PointCount == 0) return;

            BeginGroup();
            string d = SvgPathWriter.WritePathData(path, _params.CoordinatePrecision);
            _w.WriteStartElement("path");
            _w.WriteAttribute("d", d);
            _w.WriteAttribute("fill", "none");
            SvgPenWriter.WriteStroke(_w, pen, _defs);
            _w.WriteEndElement();
            EndGroup();
        }

        public void FillPath(Brush brush, GraphicsPath path)
            => FillPath(brush, path, path?.FillMode ?? FillMode.Alternate);

        public void FillPath(Brush brush, GraphicsPath path, FillMode fillMode)
        {
            if (brush == null || path == null || path.PointCount == 0) return;

            BeginGroup();
            string d = SvgPathWriter.WritePathData(path, _params.CoordinatePrecision);
            _w.WriteStartElement("path");
            _w.WriteAttribute("d", d);
            _w.WriteAttribute("fill", SvgBrushWriter.WriteFillRef(brush, _defs));
            var opacity = SvgBrushWriter.WriteFillOpacity(brush);
            if (opacity != null)
                _w.WriteAttribute("fill-opacity", opacity);
            // fill-rule: Alternate → evenodd, Winding → nonzero
            _w.WriteAttributeOptional("fill-rule",
                fillMode == FillMode.Alternate ? "evenodd" : "nonzero", "nonzero");
            _w.WriteAttribute("stroke", "none");
            _w.WriteEndElement();
            EndGroup();
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect,
                               GraphicsUnit srcUnit, ImageAttributes? attr)
        {
            if (image == null) return;
            BeginGroup();
            SvgImageWriter.WriteImage(_w, image, destRect);
            EndGroup();
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect,
                               GraphicsUnit srcUnit, ImageAttributes? attr)
        {
            if (image == null || destPoints == null || destPoints.Length < 3) return;

            // Approximate affine transform: compute dest rect from dest points
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in destPoints)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            var destRect = new RectangleF(minX, minY, maxX - minX, maxY - minY);
            BeginGroup();
            SvgImageWriter.WriteImage(_w, image, destRect);
            EndGroup();
        }

        public void DrawString(string s, MiniPdf.Drawing.Text.Font font, Brush brush,
                                RectangleF layoutRect, StringFormat? format)
        {
            if (string.IsNullOrEmpty(s) || font == null || brush == null) return;

            BeginGroup();
            var textWriter = new SvgTextWriter(_w, _defs, _params);
            textWriter.WriteText(s, font, brush, layoutRect, format);
            EndGroup();
        }

        // ── Group wrapping (transform + clip) ─────────────────────────────────

        private void BeginGroup()
        {
            bool hasTransform = !_transform.IsIdentity;
            bool hasClip = !_clip.IsInfinite();
            if (!hasTransform && !hasClip) return;

            _w.WriteStartElement("g");
            if (hasTransform)
            {
                string t = SvgTransformWriter.WriteTransform(_transform, _params.CoordinatePrecision);
                if (!string.IsNullOrEmpty(t))
                    _w.WriteAttribute("transform", t);
            }
            if (hasClip)
            {
                string? clipRef = SvgClipWriter.WriteClipRef(_defs, _clip);
                if (clipRef != null)
                    _w.WriteAttribute("clip-path", clipRef);
            }
        }

        private void EndGroup()
        {
            if (!_transform.IsIdentity || !_clip.IsInfinite())
                _w.WriteEndElement();
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }

        private sealed class StateFrame
        {
            public Matrix Transform = null!;
            public Region Clip = null!;
            public SmoothingMode SmoothingMode;
            public InterpolationMode InterpolationMode;
            public CompositingMode CompositingMode;
            public CompositingQuality CompositingQuality;
            public PixelOffsetMode PixelOffsetMode;
            public TextRenderingHint TextRenderingHint;
            public GraphicsUnit PageUnit;
            public float PageScale;
            public Point RenderingOrigin;
        }
    }
}