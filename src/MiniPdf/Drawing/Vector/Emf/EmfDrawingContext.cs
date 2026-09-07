using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;
using MiniPdf.Drawing.Metafile;
using MiniPdf.Drawing.Metafile.Emf.Records;
using MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes;
using MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes;
using MiniPdf.Drawing.Pens;
using MiniPdf.Drawing.Rendering;
using MiniPdf.Drawing.Text;
using MiniPdfFont = MiniPdf.Drawing.Text.Font;
using DRegion = MiniPdf.Drawing.Drawing2D.Region;
using DMatrix = MiniPdf.Drawing.Drawing2D.Matrix;
using DGraphicsPath = MiniPdf.Drawing.Drawing2D.GraphicsPath;
using EmfRecord = MiniPdf.Drawing.Metafile.Emf.Records.Record;

namespace MiniPdf.Drawing.Vector.Emf
{
    /// <summary>
    /// An <see cref="IDrawingContext"/> implementation that accumulates EMF
    /// records into a <see cref="MetafileDocument"/>. This is the VDM->EMF
    /// bridge: replay a <see cref="VectorScene"/> onto this context to produce
    /// EMF output. Manages the EMF object table (pen/brush/font handles),
    /// world transforms, clip regions, state stack, and path records.
    /// </summary>
    internal sealed class EmfDrawingContext : IDrawingContext
    {
        private readonly int _width;
        private readonly int _height;
        private readonly EmfRecordMapper _mapper = new EmfRecordMapper();
        private readonly List<EmfRecord> _allRecords = new List<EmfRecord>();

        private DMatrix _transform = new DMatrix();
        private DRegion _clip = new DRegion();
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

        public EmfDrawingContext(int width, int height)
        {
            _width = width;
            _height = height;
            _clip.MakeInfinite();
        }

        public int Width => _width;
        public int Height => _height;
        public float DpiX => 96f;
        public float DpiY => 96f;

        public DMatrix Transform
        {
            get => _transform;
            set
            {
                _transform = value ?? new DMatrix();
                _mapper.SetTransform(_transform);
            }
        }

        public DRegion Clip
        {
            get => _clip;
            set => _clip = value ?? new DRegion();
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

        public void ResetTransform()
        {
            _transform.Reset();
            _mapper.SetTransform(_transform);
        }

        public void MultiplyTransform(DMatrix m, MatrixOrder order = MatrixOrder.Prepend)
        {
            _transform.Multiply(m, order);
            _mapper.SetTransform(_transform);
        }

        public void RotateTransform(float angle, MatrixOrder order = MatrixOrder.Prepend)
        {
            _transform.Rotate(angle, order);
            _mapper.SetTransform(_transform);
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)
        {
            _transform.Scale(sx, sy, order);
            _mapper.SetTransform(_transform);
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend)
        {
            _transform.Translate(dx, dy, order);
            _mapper.SetTransform(_transform);
        }

        public void SetClip(RectangleF rect, CombineMode mode = CombineMode.Replace)
        {
            _clip = new DRegion(rect);
            _mapper.IntersectClipRect(rect);
        }

        public void SetClip(DGraphicsPath path, CombineMode mode = CombineMode.Replace)
        {
            _clip = new DRegion(path);
        }

public void SetClip(DRegion region, CombineMode mode = CombineMode.Replace)
        {
            _clip = region.Clone();
            var rect = region.GetBounds();
            if (rect.Width > 0 && rect.Height > 0 && !float.IsInfinity(rect.Width) && !float.IsInfinity(rect.Height))
                _mapper.IntersectClipRect(rect);
        }

        public void IntersectClip(RectangleF rect)
        {
            _clip.Intersect(rect);
            _mapper.IntersectClipRect(rect);
        }

        public void IntersectClip(DRegion region)
        {
            _clip.Intersect(region);
        }

        public void ExcludeClip(Rectangle rect) => _clip.Exclude(rect);
        public void ExcludeClip(DRegion region) => _clip.Exclude(region);

        public void ResetClip()
        {
            _clip = new DRegion();
            _clip.MakeInfinite();
        }

        public void TranslateClip(float dx, float dy) => _clip.Translate(dx, dy);

        private readonly Dictionary<GraphicsState, int> _stateDepths = new Dictionary<GraphicsState, int>();
        private readonly Dictionary<GraphicsContainer, int> _containerDepths = new Dictionary<GraphicsContainer, int>();

        public GraphicsState Save()
        {
            _stateStack.Push(CaptureState());
            _mapper.SaveDC();
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
            _mapper.RestoreDC();
        }

        public GraphicsContainer BeginContainer()
        {
            _stateStack.Push(CaptureState());
            _mapper.SaveDC();
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
            _mapper.RestoreDC();
        }

        public void Clear(Color color)
        {
            var rect = new RectangleF(0, 0, _width, _height);
            var path = new DGraphicsPath();
            path.AddRectangle(rect);
            var brush = new SolidBrush(color);
            _mapper.SelectBrush(brush);
            _mapper.BuildPath(path);
            _mapper.FillPath(rect);
            brush.Dispose();
        }

        public void DrawPath(Pen pen, DGraphicsPath path)
        {
            if (pen == null || path == null || path.PointCount == 0) return;
            _mapper.SelectPen(pen);
            var bounds = GetPathBounds(path);
            _mapper.BuildPath(path);
            _mapper.StrokePath(bounds);
        }

        public void FillPath(Brush brush, DGraphicsPath path)
            => FillPath(brush, path, path?.FillMode ?? FillMode.Alternate);

        public void FillPath(Brush brush, DGraphicsPath path, FillMode fillMode)
        {
            if (brush == null || path == null || path.PointCount == 0) return;
            _mapper.SelectBrush(brush);
            _mapper.SetPolyFillMode(fillMode);
            var bounds = GetPathBounds(path);
            _mapper.BuildPath(path);
            _mapper.FillPath(bounds);
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect,
                               GraphicsUnit srcUnit, ImageAttributes? attr)
        {
            if (image == null) return;
            if (image is Bitmap bmp)
            {
                _mapper.StretchDIBits(destRect, bmp);
            }
            else
            {
                using var rasterized = image.Clone() as Bitmap;
                if (rasterized != null)
                    _mapper.StretchDIBits(destRect, rasterized);
            }
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect,
                               GraphicsUnit srcUnit, ImageAttributes? attr)
        {
            if (image == null || destPoints == null || destPoints.Length < 3) return;
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
            DrawImage(image, destRect, srcRect, srcUnit, attr);
        }

        public void DrawString(string s, MiniPdfFont font, Brush brush,
                                RectangleF layoutRect, StringFormat? format)
        {
            if (string.IsNullOrEmpty(s) || font == null || brush == null) return;
            var color = brush is SolidBrush sb ? sb.Color : Color.Black;
            _mapper.ExtTextOutW(s, font, color, layoutRect);
        }

        /// <summary>
        /// Builds a complete <see cref="MetafileDocument"/> from the accumulated
        /// records, including EMR_HEADER and EMR_EOF.
        /// </summary>
        public MetafileDocument ToDocument()
        {
            var records = new List<EmfRecord>();
            var header = CreateHeader();
            records.Add(header);
            foreach (var r in _mapper.Records)
                records.Add(r);
            records.Add(new EMR_EOF
            {
                NumberOfHandles = _mapper.HandleCount,
                NumberOfRecords = (ushort)records.Count,
            });
            header.HeaderObject.Records = (uint)records.Count;
            header.HeaderObject.Bytes = 0;
            var hdr = header.Header;
            hdr.Size = 0;
            header.Header = hdr;
            return new MetafileDocument
            {
                Format = MetafileFormat.Emf,
                EmfRecords = records,
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }

        private EMR_HEADER CreateHeader()
        {
            return new EMR_HEADER
            {
                HeaderObject = new Metafile.Emf.Objects.Header
                {
                    Bounds = new Metafile.Wmf.Objects.RectL
                    {
                        Left = 0, Top = 0, Right = _width, Bottom = _height,
                    },
                    Frame = new Metafile.Wmf.Objects.RectL
                    {
                        Left = 0, Top = 0, Right = _width * 100, Bottom = _height * 100,
                    },
                    RecordSignature = 0x464D4520,
                    Version = 0x00010000,
                    Bytes = 0,
                    Records = 0,
                    Handles = _mapper.HandleCount,
                    nDescription = 0,
                    offDescription = 0,
                    nPalEntries = 0,
                    Device = new Metafile.Wmf.Objects.SizeL
                    {
                        cx = (uint)_width, cy = (uint)_height,
                    },
                    Millimeters = new Metafile.Wmf.Objects.SizeL
                    {
                        cx = (uint)Math.Max(1, _width * 3),
                        cy = (uint)Math.Max(1, _height * 3),
                    },
                },
            };
        }

        private static RectangleF GetPathBounds(DGraphicsPath path)
        {
            var pts = path.PathPoints;
            if (pts.Length == 0) return RectangleF.Empty;
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in pts)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
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

        private sealed class StateFrame
        {
            public DMatrix Transform = null!;
            public DRegion Clip = null!;
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