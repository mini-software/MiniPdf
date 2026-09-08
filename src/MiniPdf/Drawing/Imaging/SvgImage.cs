using System;
using System.IO;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Vector;
using MiniSoftware.Drawing.Vector.Svg;

namespace MiniSoftware.Drawing.Imaging
{
    /// <summary>
    /// A vector image backed by a <see cref="VectorScene"/>, loadable from and
    /// savable to SVG, EMF, WMF, and raster formats. Mirrors <see cref="Metafile"/>.
    /// </summary>
    public sealed class SvgImage : Image
    {
        private readonly VectorScene _scene;

        /// <summary>
        /// Initializes a new empty <see cref="SvgImage"/> with the specified
        /// dimensions.
        /// </summary>
        /// <param name="width">The canvas width in pixels.</param>
        /// <param name="height">The canvas height in pixels.</param>
        public SvgImage(int width, int height)
        {
            _scene = new VectorScene { Width = width, Height = height };
        }

        /// <summary>
        /// Initializes a new <see cref="SvgImage"/> by loading an SVG document
        /// from the specified file.
        /// </summary>
        /// <param name="filename">The SVG file to load.</param>
        public SvgImage(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or whitespace.", nameof(filename));
            using var stream = File.OpenRead(filename);
            _scene = LoadScene(stream);
        }

        /// <summary>
        /// Initializes a new <see cref="SvgImage"/> by loading an SVG document
        /// from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        public SvgImage(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            _scene = LoadScene(stream);
        }

        /// <summary>
        /// Gets the underlying <see cref="VectorScene"/>.
        /// </summary>
        internal VectorScene Scene => _scene;

        /// <inheritdoc/>
        public override int Width => _scene.Width;

        /// <inheritdoc/>
        public override int Height => _scene.Height;

        /// <inheritdoc/>
        public override PixelFormat PixelFormat => PixelFormat.Format32bppArgb;

        /// <inheritdoc/>
        public override ImageFormat RawFormat => ImageFormat.Svg;

        /// <inheritdoc/>
        public override int Flags
            => (int)(ImageFlags.Scalable | ImageFlags.ColorSpaceRgb | ImageFlags.HasRealDpi);

        /// <summary>
        /// Creates a <see cref="Graphics"/> surface for drawing onto this SVG
        /// image. Drawing operations are recorded as vector commands; no
        /// rasterization occurs until <see cref="RenderToBitmap"/> is called.
        /// </summary>
        /// <returns>A <see cref="Graphics"/> backed by a <see cref="RecordingGraphics"/>.</returns>
        public Graphics CreateGraphics()
        {
            return new RecordingGraphics(_scene, _scene.Width, _scene.Height);
        }

        /// <summary>
        /// Renders this SVG image to a <see cref="Bitmap"/> by replaying the
        /// recorded commands onto a raster <see cref="Graphics"/> surface.
        /// </summary>
        /// <returns>The rendered bitmap.</returns>
        public Bitmap RenderToBitmap()
            => RenderToBitmap(Width, Height);

        /// <summary>
        /// Renders this SVG image to a <see cref="Bitmap"/> of the specified
        /// size by replaying the recorded commands onto a scaled raster surface.
        /// </summary>
        /// <param name="width">The output bitmap width.</param>
        /// <param name="height">The output bitmap height.</param>
        /// <returns>The rendered bitmap.</returns>
        public Bitmap RenderToBitmap(int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bitmap);

            // If scaled, apply a scale transform
            if (width != _scene.Width || height != _scene.Height)
            {
                float sx = (float)width / _scene.Width;
                float sy = (float)height / _scene.Height;
                g.ScaleTransform(sx, sy);
            }

            VectorScenePlayer.Play(_scene, g);
            return bitmap;
        }

        /// <inheritdoc/>
        public override void Save(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or whitespace.", nameof(filename));
            using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            Save(stream, ImageFormat.Svg);
        }

        /// <inheritdoc/>
        public override void Save(string filename, ImageFormat format)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or whitespace.", nameof(filename));
            using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            Save(stream, format);
        }

        /// <inheritdoc/>
        public override void Save(Stream stream, ImageFormat format)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (format == null) throw new ArgumentNullException(nameof(format));

            if (format.Equals(ImageFormat.Svg))
            {
                SvgWriter.Write(stream, _scene, SvgEncoderParameters.Default);
                return;
            }

            // Raster formats: render to bitmap then save
            if (format.Equals(ImageFormat.Png) || format.Equals(ImageFormat.Bmp) ||
                format.Equals(ImageFormat.Jpeg) || format.Equals(ImageFormat.Gif) ||
                format.Equals(ImageFormat.Tiff))
            {
                using var bmp = RenderToBitmap();
                bmp.Save(stream, format);
                return;
            }

            throw new NotSupportedException($"SvgImage does not support saving to format '{format.Name}'.");
        }

        /// <inheritdoc/>
        public override void Save(Stream stream, ImageFormat format, EncoderParameters? encoderParams)
            => Save(stream, format);

        /// <inheritdoc/>
        public override Image Clone()
        {
            var clone = new SvgImage(_scene.Width, _scene.Height);
            // Deep-copy commands
            foreach (var cmd in _scene.Commands)
                clone._scene.Commands.Add(cmd.Clone());
            clone._scene.DpiX = _scene.DpiX;
            clone._scene.DpiY = _scene.DpiY;
            return clone;
        }

        /// <inheritdoc/>
        public override void RotateFlip(RotateFlipType rotateFlipType)
        {
            // For v1, apply the transform to the scene by wrapping commands
            // in a transform group. A full implementation would transform
            // each command's coordinates.
            // For now, throw as vector rotation is non-trivial.
            if (rotateFlipType == RotateFlipType.RotateNoneFlipNone) return;
            throw new NotSupportedException("RotateFlip is not supported for SvgImage in this version.");
        }

        // ── SVG loading ───────────────────────────────────────────────────────

        private static VectorScene LoadScene(Stream stream)
        {
            return SvgReader.Read(stream);
        }
    }
}