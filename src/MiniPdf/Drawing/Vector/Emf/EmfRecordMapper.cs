using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;
using MiniSoftware.Drawing.Metafile.Emf.Enumerations;
using MiniSoftware.Drawing.Metafile.Emf.Objects;
using MiniSoftware.Drawing.Metafile.Emf.Records;
using MiniSoftware.Drawing.Metafile.Emf.Records.DrawingRecordTypes;
using MiniSoftware.Drawing.Metafile.Emf.Records.ObjectRecordTypes;
using MiniSoftware.Drawing.Metafile.Emf.Records.StateRecordTypes;
using MiniSoftware.Drawing.Metafile.Wmf.Enumerations;
using MiniSoftware.Drawing.Metafile.Wmf.Objects;
using MiniSoftware.Drawing.Pens;
using Pen = MiniSoftware.Drawing.Pens.Pen;
using Brush = MiniSoftware.Drawing.Brushes.Brush;
using Color = MiniSoftware.Drawing.Colors.Color;
using DashStyle = MiniSoftware.Drawing.Drawing2D.DashStyle;
using EmfRecord = MiniSoftware.Drawing.Metafile.Emf.Records.Record;
using WmfHatchStyle = MiniSoftware.Drawing.Metafile.Wmf.Enumerations.HatchStyle;

namespace MiniSoftware.Drawing.Vector.Emf
{
    /// <summary>
    /// Maps <see cref="DrawingCommand"/> state to EMF records. Manages the EMF
    /// object table (pen/brush/font handle allocation and selection), world
    /// transforms, clip regions, state stack, and path-to-records conversion.
    /// Used by <see cref="EmfDrawingContext"/>.
    /// </summary>
    internal sealed class EmfRecordMapper
    {
        private readonly List<EmfRecord> _records = new List<EmfRecord>();
        private readonly Dictionary<object, uint> _objectTable = new Dictionary<object, uint>();
        private uint _nextHandle = 1;
        private uint _currentPenHandle;
        private uint _currentBrushHandle;
        private uint _currentFontHandle;

        public IReadOnlyList<EmfRecord> Records => _records;
        public ushort HandleCount => (ushort)_nextHandle;

        // ── Object table management ────────────────────────────────────────────

        private uint GetOrCreatePen(Pen pen)
        {
            if (pen == null) return 0;
            var key = (pen.Color, pen.Width);
            if (_objectTable.TryGetValue(key, out uint handle))
                return handle;

            handle = _nextHandle++;
            var penStyle = pen.DashStyle == DashStyle.Solid ? PenStyle.PS_SOLID : PenStyle.PS_DASH;
            var createPen = new EMR_CREATEPEN
            {
                IhPen = handle,
                Pen = new Metafile.Wmf.Objects.Pen
                {
                    PenStyle = penStyle,
                    Width = new PointS { x = (int)Math.Max(1, pen.Width), y = 0 },
                    ColorRef = new ColorRef { Red = pen.Color.R, Green = pen.Color.G, Blue = pen.Color.B, Reserved = 0 },
                },
            };
            _records.Add(createPen);
            _objectTable[key] = handle;
            return handle;
        }

        private uint GetOrCreateBrush(Brushes.Brush brush)
        {
            if (brush == null) return 0;
            Color color = brush is Brushes.SolidBrush sb ? sb.Color : Color.Black;
            var key = ("brush", color);
            if (_objectTable.TryGetValue(key, out uint handle))
                return handle;

            handle = _nextHandle++;
            var createBrush = new EMR_CREATEBRUSHINDIRECT
            {
                IhBrush = handle,
                LogBrush = new LogBrush
                {
                    BrushStyle = BrushStyle.BS_SOLID,
                    ColorRef = new ColorRef { Red = color.R, Green = color.G, Blue = color.B, Reserved = 0 },
                    BrushHatch = WmfHatchStyle.HS_HORIZONTAL,
                },
            };
            _records.Add(createBrush);
            _objectTable[key] = handle;
            return handle;
        }

        private uint GetOrCreateFont(Text.Font font)
        {
            if (font == null) return 0;
            var key = ("font", font.Name, font.Size, font.Style);
            if (_objectTable.TryGetValue(key, out uint handle))
                return handle;

            handle = _nextHandle++;
            short weight = font.Bold ? (short)700 : (short)400;
            var createFont = new EMR_EXTCREATEFONTINDIRECTW
            {
                IhFont = handle,
                Font = new Metafile.Wmf.Objects.Font
                {
                    Height = (short)Math.Round(font.Size),
                    Width = 0,
                    Escapement = 0,
                    Orientation = 0,
                    Weight = weight,
                    Italic = (byte)(font.Italic ? 1 : 0),
                    Underline = (byte)(font.Underline ? 1 : 0),
                    StrikeOut = (byte)(font.Strikeout ? 1 : 0),
                },
            };
            createFont.Font.Facename = font.Name;
            _records.Add(createFont);
            _objectTable[key] = handle;
            return handle;
        }

        public void SelectPen(Pen pen)
        {
            uint handle = GetOrCreatePen(pen);
            if (handle == 0 || handle == _currentPenHandle) return;
            _currentPenHandle = handle;
            _records.Add(new EMR_SELECTOBJECT { ObjectIndex = handle });
        }

        public void SelectBrush(Brushes.Brush brush)
        {
            uint handle = GetOrCreateBrush(brush);
            if (handle == 0 || handle == _currentBrushHandle) return;
            _currentBrushHandle = handle;
            _records.Add(new EMR_SELECTOBJECT { ObjectIndex = handle });
        }

        public void SelectFont(Text.Font font)
        {
            uint handle = GetOrCreateFont(font);
            if (handle == 0 || handle == _currentFontHandle) return;
            _currentFontHandle = handle;
            _records.Add(new EMR_SELECTOBJECT { ObjectIndex = handle });
        }

        // ── State records ─────────────────────────────────────────────────────

        public void SetTransform(Matrix matrix)
        {
            var e = matrix.Elements;
            _records.Add(new EMR_SETWORLDTRANSFORM
            {
                M11 = e[0], M12 = e[1],
                M21 = e[2], M22 = e[3],
                Dx = e[4], Dy = e[5],
            });
        }

        public void SaveDC() => _records.Add(new EMR_SAVEDC());

        public void RestoreDC() => _records.Add(new EMR_RESTOREDC { SavedDC = -1 });

        public void SetPolyFillMode(FillMode fillMode)
        {
            _records.Add(new EMR_SETPOLYFILLMODE
            {
                PolyFillMode = fillMode == FillMode.Alternate
                    ? PolyFillMode.ALTERNATE
                    : PolyFillMode.WINDING,
            });
        }

        public void SetTextColor(Color color)
        {
            _records.Add(new EMR_SETTEXTCOLOR
            {
                Color = new ColorRef { Red = color.R, Green = color.G, Blue = color.B, Reserved = 0 },
            });
        }

        public void IntersectClipRect(RectangleF rect)
        {
            _records.Add(new EMR_INTERSECTCLIPRECT { Clip = ToRectL(rect) });
        }

        // ── Path records ──────────────────────────────────────────────────────

        public void BuildPath(GraphicsPath path)
        {
            _records.Add(new EMR_BEGINPATH());

            var points = path.PathPoints;
            var types = path.PathTypes;

            bool firstPoint = true;
            for (int i = 0; i < points.Length; i++)
            {
                byte type = types[i];
                PathPointType ptType = (PathPointType)(type & (byte)PathPointType.PathTypeMask);
                bool closeSubpath = (type & (byte)PathPointType.CloseSubpath) != 0;

                if (ptType == PathPointType.Start)
                {
                    _records.Add(new EMR_MOVETOEX { Offset = ToPointL(points[i]) });
                    firstPoint = false;
                }
                else if (ptType == PathPointType.Line)
                {
                    _records.Add(new EMR_LINETO { Point = ToPointL(points[i]) });
                }
                else if (ptType == PathPointType.Bezier)
                {
                    var bezierPoints = new List<PointL>();
                    while (i < points.Length &&
                           (PathPointType)(types[i] & (byte)PathPointType.PathTypeMask) == PathPointType.Bezier)
                    {
                        bezierPoints.Add(ToPointL(points[i]));
                        i++;
                    }
                    i--;

                    if (bezierPoints.Count >= 3)
                    {
                        _records.Add(new EMR_POLYBEZIERTO
                        {
                            Bounds = new RectL(),
                            Count = (uint)bezierPoints.Count,
                            Points = bezierPoints.ToArray(),
                        });
                    }
                }

                if (closeSubpath)
                    _records.Add(new EMR_CLOSEFIGURE());
            }

            _records.Add(new EMR_ENDPATH());
        }

        public void StrokePath(RectangleF bounds)
            => _records.Add(new EMR_STROKEPATH { Bounds = ToRectL(bounds) });

        public void FillPath(RectangleF bounds)
            => _records.Add(new EMR_FILLPATH { Bounds = ToRectL(bounds) });

        public void StrokeAndFillPath(RectangleF bounds)
            => _records.Add(new EMR_STROKEANDFILLPATH { Bounds = ToRectL(bounds) });

        // ── Drawing records ───────────────────────────────────────────────────

        public void StretchDIBits(RectangleF destRect, Bitmap bitmap)
        {
            var (bmi, bits) = BitmapToDib(bitmap);
            _records.Add(new EMR_STRETCHDIBITS
            {
                Bounds = ToRectL(destRect),
                XDest = (int)destRect.X,
                YDest = (int)destRect.Y,
                XSrc = 0,
                YSrc = 0,
                CxSrc = bitmap.Width,
                CySrc = bitmap.Height,
                OffBmiSrc = 0,
                CbBmiSrc = (uint)bmi.Length,
                OffBitsSrc = 0,
                CbBitsSrc = (uint)bits.Length,
                UsageSrc = DIBColors.DibRgbColors,
                BitBltRasterOperation = TernaryRasterOperation.SRCCOPY,
                CxDest = (int)destRect.Width,
                CyDest = (int)destRect.Height,
                BmiSrc = bmi,
                BitsSrc = bits,
            });
        }

        public void ExtTextOutW(string text, Text.Font font, Color color, RectangleF layoutRect)
        {
            SelectFont(font);
            SetTextColor(color);

            var outputString = System.Text.Encoding.Unicode.GetBytes(text);
            var emrText = new EmrText
            {
                Reference = new PointL { x = (int)layoutRect.X, y = (int)layoutRect.Y },
                Chars = (uint)text.Length,
                offString = 0,
                Options = ExtTextOutOptions.ETO_OPAQUE,
                Rectangle = ToRectL(layoutRect),
                offDx = 0,
                OutputString = outputString,
            };

            _records.Add(new EMR_EXTTEXTOUTW
            {
                Bounds = ToRectL(layoutRect),
                IGraphicsMode = GraphicsMode.GM_COMPATIBLE,
                ExScale = 1f,
                EyScale = 1f,
                EmrText = emrText,
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static PointL ToPointL(PointF p)
            => new PointL { x = (int)Math.Round(p.X), y = (int)Math.Round(p.Y) };

        private static RectL ToRectL(RectangleF r)
            => new RectL
            {
                Left = (int)Math.Round(r.Left),
                Top = (int)Math.Round(r.Top),
                Right = (int)Math.Round(r.Right),
                Bottom = (int)Math.Round(r.Bottom),
            };

        private static (byte[] bmi, byte[] bits) BitmapToDib(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            PixelFormat format = bitmap.PixelFormat;
            int bpp = format == PixelFormat.Format24bppRgb ? 24 : 32;

            int stride = ((width * bpp + 31) / 32) * 4;
            int headerSize = 40;
            int pixelDataSize = stride * height;

            var bmi = new byte[headerSize];
            BitConverter.GetBytes((uint)headerSize).CopyTo(bmi, 0);
            BitConverter.GetBytes(width).CopyTo(bmi, 4);
            BitConverter.GetBytes(height).CopyTo(bmi, 8);
            BitConverter.GetBytes((ushort)1).CopyTo(bmi, 12);
            BitConverter.GetBytes((ushort)bpp).CopyTo(bmi, 14);
            BitConverter.GetBytes((uint)0).CopyTo(bmi, 16);
            BitConverter.GetBytes((uint)pixelDataSize).CopyTo(bmi, 20);
            for (int i = 24; i < headerSize; i++)
                bmi[i] = 0;

            var bits = new byte[pixelDataSize];
            int srcStride = Bitmap.CalcStride(width, format);
            int copyStride = Math.Min(srcStride, stride);

            for (int y = 0; y < height; y++)
            {
                int srcOffset = y * srcStride;
                int dstOffset = (height - 1 - y) * stride;
                Buffer.BlockCopy(bitmap._pixels, srcOffset, bits, dstOffset, copyStride);
            }

            return (bmi, bits);
        }
    }
}