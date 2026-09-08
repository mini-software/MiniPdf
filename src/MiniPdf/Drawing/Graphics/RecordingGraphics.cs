using System;
using System.Collections.Generic;
using MiniSoftware.Drawing.Brushes;
using MiniSoftware.Drawing.Colors;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;
using MiniSoftware.Drawing.Pens;
using MiniSoftware.Drawing.Rendering;
using MiniSoftware.Drawing.Text;
using MiniSoftware.Drawing.Vector.Commands;

namespace MiniSoftware.Drawing
{
    /// <summary>
    /// A <see cref="Graphics"/> surface that records drawing operations into a
    /// <see cref="Vector.VectorScene"/> instead of rasterizing. Same public API
    /// as <see cref="Graphics"/>. Reuses all convenience overloads unchanged;
    /// only primitives and state hooks are intercepted.
    /// </summary>
    public sealed class RecordingGraphics : Graphics
    {
        private readonly Vector.VectorScene _scene;
        private readonly int _width;
        private readonly int _height;
        private readonly float _dpiX;
        private readonly float _dpiY;
        private bool _suppressRecording;
        private readonly Dictionary<GraphicsState, SaveStateCommand> _saveCommands
            = new Dictionary<GraphicsState, SaveStateCommand>();

        /// <summary>
        /// Initializes a new <see cref="RecordingGraphics"/> bound to the specified scene.
        /// </summary>
        /// <param name="scene">The scene to record into.</param>
        /// <param name="width">The canvas width.</param>
        /// <param name="height">The canvas height.</param>
        internal RecordingGraphics(Vector.VectorScene scene, int width, int height)
            : base()
        {
            _scene  = scene;
            _width  = width;
            _height = height;
            _dpiX   = scene.DpiX;
            _dpiY   = scene.DpiY;
        }

        /// <inheritdoc/>
        protected override bool IsRecording => true;

        /// <inheritdoc/>
        public override int Width  => _width;

        /// <inheritdoc/>
        public override int Height => _height;

        /// <inheritdoc/>
        public override float DpiX => _dpiX;

        /// <inheritdoc/>
        public override float DpiY => _dpiY;

        // ── Primitive overrides: record instead of rasterize ──────────────────

        /// <inheritdoc/>
        protected override void DrawPathCore(Pen pen, GraphicsPath path)
            => _scene.Add(new DrawPathCommand(pen, path));

        /// <inheritdoc/>
        protected override void FillPathCore(Brush brush, GraphicsPath path, FillMode fillMode)
            => _scene.Add(new FillPathCommand(brush, path, fillMode));

        /// <inheritdoc/>
        protected override void DrawImageCore(Image image, RectangleF destRect, RectangleF srcRect,
                                             GraphicsUnit srcUnit, ImageAttributes? attr)
            => _scene.Add(new DrawImageCommand(image, destRect, srcRect, srcUnit, attr));

        /// <inheritdoc/>
        protected override void DrawImageCore(Image image, PointF[] destPoints, RectangleF srcRect,
                                             GraphicsUnit srcUnit, ImageAttributes? attr)
            => _scene.Add(new DrawImageAffineCommand(image, destPoints, srcRect, srcUnit, attr));

        /// <inheritdoc/>
        protected override void DrawStringCore(string s, Text.Font font, Brush brush,
                                                RectangleF layoutRect, StringFormat? format)
            => _scene.Add(new DrawStringCommand(s, font, brush, layoutRect, format));

        /// <inheritdoc/>
        protected override void ClearCore(Color color)
            => _scene.Add(new ClearCommand(color));

        // ── State hooks: record state commands ─────────────────────────────────

        /// <inheritdoc/>
        protected override void OnTransformChanged()
        {
            if (_suppressRecording) return;
            _scene.Add(new SetTransformCommand(Transform));
        }

        /// <inheritdoc/>
        protected override void OnClipChanged()
        {
            if (_suppressRecording) return;
            _scene.Add(new SetClipCommand(Clip, CombineMode.Replace));
        }

        /// <inheritdoc/>
        protected override void OnHintChanged(object hintValue)
        {
            if (_suppressRecording) return;
            _scene.Add(new SetHintCommand(hintValue));
        }

        // ── State stack: record Save/Restore commands ──────────────────────────

        /// <inheritdoc/>
        public override GraphicsState Save()
        {
            var state = base.Save();
            var cmd = new SaveStateCommand();
            _scene.Add(cmd);
            _saveCommands[state] = cmd;
            return state;
        }

        /// <inheritdoc/>
        public override void Restore(GraphicsState graphicsState)
        {
            if (graphicsState == null) throw new ArgumentNullException(nameof(graphicsState));

            if (_saveCommands.TryGetValue(graphicsState, out var saveCmd))
                _scene.Add(new RestoreStateCommand(saveCmd));

            _suppressRecording = true;
            try
            {
                base.Restore(graphicsState);
            }
            finally
            {
                _suppressRecording = false;
            }
        }
    }
}