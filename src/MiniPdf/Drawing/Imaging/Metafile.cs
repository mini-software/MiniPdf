using System;
using System.Collections.Generic;
using System.IO;
using MiniPdf.Drawing.Enums;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes;
using MiniPdf.Drawing.Metafile.Emf.Records.StateRecordTypes;
using MiniPdf.Drawing.Rendering;
using EmfRecords = MiniPdf.Drawing.Metafile.Emf.Records;
using WmfRecords = MiniPdf.Drawing.Metafile.Wmf.Records;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// Represents a Windows metafile (.wmf or .emf), which is a vector image format
    /// containing a sequence of drawing commands.
    /// </summary>
    /// <remarks>
    /// A <see cref="Metafile"/> can be loaded from WMF or EMF files and rendered
    /// to a <see cref="Bitmap"/> using <see cref="ToBitmap"/>. Metafiles are
    /// commonly used for scalable graphics that maintain quality at any resolution.
    /// </remarks>
    public sealed class Metafile : Image
    {
        private readonly byte[] _rawBytes;
        private readonly Drawing.Metafile.MetafileDocument _document;
        private readonly Rectangle _logicalBounds;
        private readonly ImageFormat _rawFormat;
        private readonly MetafileHeader _header;

        /// <summary>
        /// Initializes a new <see cref="Metafile"/> from the specified file.
        /// </summary>
        /// <param name="filename">The file to load from.</param>
        public Metafile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("The file name cannot be null or whitespace.", nameof(filename));

            using var stream = File.OpenRead(filename);
            (_rawBytes, _document, _logicalBounds, _rawFormat, _header) = Load(stream);
        }

        /// <summary>
        /// Initializes a new <see cref="Metafile"/> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        public Metafile(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            (_rawBytes, _document, _logicalBounds, _rawFormat, _header) = Load(stream);
        }

        /// <summary>
        /// Gets the width of this metafile in pixels.
        /// </summary>
        public override int Width => Math.Max(1, _logicalBounds.Width);

        /// <summary>
        /// Gets the height of this metafile in pixels.
        /// </summary>
        public override int Height => Math.Max(1, _logicalBounds.Height);

        /// <summary>
        /// Gets the pixel format of this metafile.
        /// </summary>
        public override PixelFormat PixelFormat => PixelFormat.Format32bppArgb;

        /// <summary>
        /// Gets the format of this metafile.
        /// </summary>
        public override ImageFormat RawFormat => _rawFormat;

        /// <summary>
        /// Gets the flags for this metafile.
        /// </summary>
        public override int Flags
            => (int)(ImageFlags.Scalable | ImageFlags.ColorSpaceRgb | ImageFlags.HasRealDpi);

        /// <summary>
        /// Gets the metafile header information.
        /// </summary>
        /// <returns>The metafile header.</returns>
        public MetafileHeader GetMetafileHeader() => _header;

        /// <summary>
        /// Renders this metafile to a <see cref="Bitmap"/>.
        /// </summary>
        /// <returns>The rendered bitmap.</returns>
        public Bitmap ToBitmap() => RenderToBitmap();

        internal Bitmap RenderToBitmap()
            => RenderToBitmap(Width, Height);

        internal Bitmap RenderToBitmap(int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            switch (_document.Format)
            {
                case Drawing.Metafile.MetafileFormat.Wmf:
                    WmfPlayer.Render(_document, bitmap, _logicalBounds);
                    break;

                case Drawing.Metafile.MetafileFormat.Emf:
                case Drawing.Metafile.MetafileFormat.EmfPlus:
                    EmfPlayer.Render(_document, bitmap, _logicalBounds);
                    break;

                default:
                    throw new NotSupportedException("Unsupported metafile format.");
            }

            bitmap._rawFormat = _rawFormat;
            return bitmap;
        }

        /// <summary>
        /// Creates an exact copy of this <see cref="Metafile"/>.
        /// </summary>
        /// <returns>A new <see cref="Metafile"/> with the same content.</returns>
        public override Image Clone()
            => new Metafile(new MemoryStream(_rawBytes, writable: false));

        /// <summary>
        /// Saves this metafile to the specified file.
        /// </summary>
        /// <param name="filename">The file name to save to.</param>
        public override void Save(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("The file name cannot be null or whitespace.", nameof(filename));

            using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            Save(stream, _rawFormat);
        }

        /// <summary>
        /// Saves this metafile to the specified file in the specified format.
        /// </summary>
        /// <param name="filename">The file name to save to.</param>
        /// <param name="format">The format to save in.</param>
        public override void Save(string filename, ImageFormat format)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("The file name cannot be null or whitespace.", nameof(filename));

            using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            Save(stream, format);
        }

        /// <summary>
        /// Saves this metafile to the specified stream in the specified format.
        /// </summary>
        /// <param name="stream">The stream to save to.</param>
        /// <param name="format">The format to save in.</param>
        public override void Save(Stream stream, ImageFormat format)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            bool supported = format.Equals(ImageFormat.Emf) || format.Equals(ImageFormat.Wmf);
            if (!supported)
                throw new NotSupportedException("Metafile saving supports only EMF and WMF formats.");

            if (!format.Equals(_rawFormat))
                throw new NotSupportedException("Metafile transcoding is not supported.");

            stream.Write(_rawBytes, 0, _rawBytes.Length);
        }

        /// <summary>
        /// Rotates and flips the metafile.
        /// </summary>
        /// <param name="rotateFlipType">The rotation and flip type.</param>
        /// <exception cref="NotSupportedException">This operation is not supported for vector metafiles.</exception>
        public override void RotateFlip(RotateFlipType rotateFlipType)
            => throw new NotSupportedException("RotateFlip is not supported for vector metafiles.");

        private static (byte[] rawBytes, Drawing.Metafile.MetafileDocument document, Rectangle bounds, ImageFormat rawFormat, MetafileHeader header) Load(Stream stream)
        {
            byte[] rawBytes = ReadAllBytes(stream);
            using var parseStream = new MemoryStream(rawBytes, writable: false);

            var reader = new Drawing.Metafile.MetafileReader();
            var document = reader.Read(parseStream);

            Rectangle bounds = DetermineLogicalBounds(document);
            ImageFormat rawFormat = document.Format == Drawing.Metafile.MetafileFormat.Wmf
                ? ImageFormat.Wmf
                : ImageFormat.Emf;

            EmfType emfType = DetermineEmfType(document);
            bool isWmf = document.Format == Drawing.Metafile.MetafileFormat.Wmf;
            bool isEmf = document.Format == Drawing.Metafile.MetafileFormat.Emf;
            bool isEmfPlus = document.Format == Drawing.Metafile.MetafileFormat.EmfPlus || document.EmbeddedEmfPlusRecords.Count > 0;

            var header = new MetafileHeader(
                bounds,
                96f,
                96f,
                MetafileFrameUnit.Pixel,
                emfType,
                isWmf,
                isEmf,
                isEmfPlus);

            return (rawBytes, document, bounds, rawFormat, header);
        }

        private static EmfType DetermineEmfType(Drawing.Metafile.MetafileDocument document)
        {
            if (document.Format == Drawing.Metafile.MetafileFormat.EmfPlus)
                return EmfType.EmfPlusOnly;

            if (document.EmbeddedEmfPlusRecords.Count > 0)
                return EmfType.EmfPlusDual;

            return EmfType.EmfOnly;
        }

        private static Rectangle DetermineLogicalBounds(Drawing.Metafile.MetafileDocument document)
        {
            switch (document.Format)
            {
                case Drawing.Metafile.MetafileFormat.Wmf:
                    if (document.PlaceableHeader != null)
                    {
                        var box = document.PlaceableHeader.BoundingBox;
                        return NormalizeBounds(box.Left, box.Top, box.Right, box.Bottom);
                    }

                    return ComputeWmfBounds(document.WmfRecords);

                case Drawing.Metafile.MetafileFormat.Emf:
                case Drawing.Metafile.MetafileFormat.EmfPlus:
                {
                    foreach (var record in document.EmfRecords)
                    {
                        if (record is EMR_HEADER header)
                        {
                            var b = header.HeaderObject.Bounds;
                            return NormalizeBounds(b.Left, b.Top, b.Right, b.Bottom);
                        }
                    }

                    return ComputeEmfBounds(document.EmfRecords);
                }

                default:
                    return new Rectangle(0, 0, 1, 1);
            }
        }

        private static Rectangle ComputeWmfBounds(IReadOnlyList<WmfRecords.Record> records)
        {
            bool hasAny = false;
            int minX = 0;
            int minY = 0;
            int maxX = 0;
            int maxY = 0;

            void IncludePoint(int x, int y)
            {
                if (!hasAny)
                {
                    minX = maxX = x;
                    minY = maxY = y;
                    hasAny = true;
                    return;
                }

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            void IncludeRect(int left, int top, int right, int bottom)
            {
                IncludePoint(left, top);
                IncludePoint(right, bottom);
            }

            foreach (var record in records)
            {
                switch (record)
                {
                    case WmfRecords.META_SETPIXEL pixel:
                        IncludePoint(pixel.X, pixel.Y);
                        IncludePoint(pixel.X + 1, pixel.Y + 1);
                        break;

                    case WmfRecords.StateRecordTypes.META_MOVETO moveTo:
                        IncludePoint(moveTo.X, moveTo.Y);
                        break;

                    case WmfRecords.META_LINETO lineTo:
                        IncludePoint(lineTo.X, lineTo.Y);
                        break;

                    case WmfRecords.META_RECTANGLE rectangle:
                        IncludeRect(rectangle.LeftRect, rectangle.TopRect, rectangle.RightRect, rectangle.BottomRect);
                        break;

                    case WmfRecords.META_ELLIPSE ellipse:
                        IncludeRect(ellipse.LeftRect, ellipse.TopRect, ellipse.RightRect, ellipse.BottomRect);
                        break;

                    case WmfRecords.META_POLYLINE polyline:
                        foreach (var point in polyline.aPoints)
                        {
                            IncludePoint(point.x, point.y);
                        }
                        break;

                    case WmfRecords.META_POLYGON polygon:
                        foreach (Point point in polygon.Points)
                        {
                            IncludePoint(point.X, point.Y);
                        }
                        break;
                }
            }

            if (!hasAny)
                return new Rectangle(0, 0, 1, 1);

            return NormalizeBounds(minX, minY, maxX, maxY);
        }

        private static Rectangle ComputeEmfBounds(IReadOnlyList<Drawing.Metafile.Emf.Records.Record> records)
        {
            bool hasAny = false;
            int minX = 0;
            int minY = 0;
            int maxX = 0;
            int maxY = 0;

            void IncludePoint(int x, int y)
            {
                if (!hasAny)
                {
                    minX = maxX = x;
                    minY = maxY = y;
                    hasAny = true;
                    return;
                }

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            void IncludeRect(int left, int top, int right, int bottom)
            {
                IncludePoint(left, top);
                IncludePoint(right, bottom);
            }

            foreach (var record in records)
            {
                switch (record)
                {
                    case EMR_SETPIXELV pixel:
                        IncludePoint(pixel.Position.x, pixel.Position.y);
                        IncludePoint(pixel.Position.x + 1, pixel.Position.y + 1);
                        break;

                    case EMR_MOVETOEX moveTo:
                        IncludePoint(moveTo.Offset.x, moveTo.Offset.y);
                        break;

                    case EMR_LINETO lineTo:
                        IncludePoint(lineTo.Point.x, lineTo.Point.y);
                        break;

                    case EMR_RECTANGLE rectangle:
                        IncludeRect(rectangle.Box.Left, rectangle.Box.Top, rectangle.Box.Right, rectangle.Box.Bottom);
                        break;

                    case EMR_ELLIPSE ellipse:
                        IncludeRect(ellipse.Box.Left, ellipse.Box.Top, ellipse.Box.Right, ellipse.Box.Bottom);
                        break;

                    case EMR_POLYLINE polyline:
                        foreach (var point in polyline.Points)
                        {
                            IncludePoint(point.x, point.y);
                        }
                        break;

                    case EMR_POLYLINE16 polyline16:
                        foreach (var point in polyline16.Points)
                        {
                            IncludePoint(point.x, point.y);
                        }
                        break;

                    case EMR_POLYGON polygon:
                        foreach (var point in polygon.Points)
                        {
                            IncludePoint(point.x, point.y);
                        }
                        break;

                    case EMR_POLYGON16 polygon16:
                        foreach (var point in polygon16.Points)
                        {
                            IncludePoint(point.x, point.y);
                        }
                        break;
                }
            }

            if (!hasAny)
                return new Rectangle(0, 0, 1, 1);

            return NormalizeBounds(minX, minY, maxX, maxY);
        }

        private static Rectangle NormalizeBounds(int left, int top, int right, int bottom)
        {
            int minX = Math.Min(left, right);
            int minY = Math.Min(top, bottom);
            int width = Math.Abs(right - left);
            int height = Math.Abs(bottom - top);

            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            return new Rectangle(minX, minY, width, height);
        }

        private static byte[] ReadAllBytes(Stream stream)
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }
    }
}
