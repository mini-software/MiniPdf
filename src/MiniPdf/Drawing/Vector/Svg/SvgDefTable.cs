using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MiniPdf.Drawing.Brushes;
using MiniPdf.Drawing.Colors;
using MiniPdf.Drawing.Drawing2D;
using MiniPdf.Drawing.Geometry;

namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// Collects unique SVG definitions (gradients, patterns, clip paths) during
    /// the def-collection pass and assigns stable ids. Brushes and clips are
    /// deduplicated by value so identical objects share one def.
    /// </summary>
    internal sealed class SvgDefTable
    {
        private readonly List<SvgDef> _defs = new List<SvgDef>();
        private readonly Dictionary<string, SvgDef> _byKey = new Dictionary<string, SvgDef>();
        private int _gradientCounter;
        private int _patternCounter;
        private int _clipCounter;

        public IReadOnlyList<SvgDef> Defs => _defs;

        /// <summary>
        /// Registers a brush and returns the id to reference it (via
        /// <c>url(#id)</c>). Solid brushes return null (inlined as a color).
        /// </summary>
        public string? RegisterBrush(Brush brush)
        {
            if (brush == null) return null;

            switch (brush)
            {
                case SolidBrush sb:
                    return null; // inline color, no def

                case LinearGradientBrush lg:
                    return RegisterGradient(lg);

                case PathGradientBrush pg:
                    return RegisterRadialGradient(pg);

                case HatchBrush hb:
                    return RegisterHatchPattern(hb);

                case TextureBrush tb:
                    return RegisterTexturePattern(tb);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Registers a clip path derived from a <see cref="Region"/> or
        /// <see cref="GraphicsPath"/> and returns the clip id.
        /// </summary>
        public string RegisterClip(Region? region, GraphicsPath? path)
        {
            string key = "clip:" + BuildClipKey(region, path);
            if (_byKey.TryGetValue(key, out var existing))
                return existing.Id;

            string id = "clip" + (++_clipCounter);
            var def = new SvgDef(id, SvgDefKind.ClipPath, key, region, path);
            _byKey[key] = def;
            _defs.Add(def);
            return id;
        }

        // ── Brush registration ────────────────────────────────────────────────

        private string RegisterGradient(LinearGradientBrush lg)
        {
            string key = "lg:" + BuildLinearGradientKey(lg);
            if (_byKey.TryGetValue(key, out var existing))
                return existing.Id;

            string id = "grad" + (++_gradientCounter);
            var def = new SvgDef(id, SvgDefKind.LinearGradient, key, lg, null);
            _byKey[key] = def;
            _defs.Add(def);
            return id;
        }

        private string RegisterRadialGradient(PathGradientBrush pg)
        {
            string key = "rg:" + BuildRadialGradientKey(pg);
            if (_byKey.TryGetValue(key, out var existing))
                return existing.Id;

            string id = "grad" + (++_gradientCounter);
            var def = new SvgDef(id, SvgDefKind.RadialGradient, key, pg, null);
            _byKey[key] = def;
            _defs.Add(def);
            return id;
        }

        private string RegisterHatchPattern(HatchBrush hb)
        {
            string key = "hatch:" + (int)hb.HatchStyle + ":" + hb.ForegroundColor.ToArgb().ToString("X8")
                       + ":" + hb.BackgroundColor.ToArgb().ToString("X8");
            if (_byKey.TryGetValue(key, out var existing))
                return existing.Id;

            string id = "pat" + (++_patternCounter);
            var def = new SvgDef(id, SvgDefKind.HatchPattern, key, hb, null);
            _byKey[key] = def;
            _defs.Add(def);
            return id;
        }

        private string RegisterTexturePattern(TextureBrush tb)
        {
            string key = "tex:" + tb.Image.GetHashCode().ToString(CultureInfo.InvariantCulture)
                       + ":" + (int)tb.WrapMode;
            if (_byKey.TryGetValue(key, out var existing))
                return existing.Id;

            string id = "pat" + (++_patternCounter);
            var def = new SvgDef(id, SvgDefKind.TexturePattern, key, tb, null);
            _byKey[key] = def;
            _defs.Add(def);
            return id;
        }

        // ── Key builders ──────────────────────────────────────────────────────

        private static string BuildLinearGradientKey(LinearGradientBrush lg)
        {
            var sb = new StringBuilder();
            sb.Append(lg.Point1.X).Append(',').Append(lg.Point1.Y).Append(';');
            sb.Append(lg.Point2.X).Append(',').Append(lg.Point2.Y).Append(';');
            sb.Append((int)lg.WrapMode).Append(';');
            foreach (var c in lg.LinearColors)
                sb.Append(c.ToArgb().ToString("X8")).Append(',');
            var blend = lg.InterpolationColors;
            if (blend != null && blend.Colors.Length > 0)
            {
                for (int i = 0; i < blend.Colors.Length; i++)
                {
                    sb.Append(blend.Positions[i]).Append(':');
                    sb.Append(blend.Colors[i].ToArgb().ToString("X8")).Append(',');
                }
            }
            AppendMatrixKey(sb, lg.Transform);
            return sb.ToString();
        }

        private static string BuildRadialGradientKey(PathGradientBrush pg)
        {
            var sb = new StringBuilder();
            sb.Append(pg.CenterPoint.X).Append(',').Append(pg.CenterPoint.Y).Append(';');
            sb.Append(pg.CenterColor.ToArgb().ToString("X8")).Append(';');
            foreach (var c in pg.SurroundColors)
                sb.Append(c.ToArgb().ToString("X8")).Append(',');
            sb.Append(pg.FocusScales.X).Append(',').Append(pg.FocusScales.Y).Append(';');
            AppendMatrixKey(sb, pg.Transform);
            return sb.ToString();
        }

        private static string BuildClipKey(Region? region, GraphicsPath? path)
        {
            var sb = new StringBuilder();
            if (region != null)
            {
                var scans = region.GetRegionScans(new Matrix());
                sb.Append("r:").Append(scans.Length).Append(';');
                foreach (var r in scans)
                    sb.Append(r.X).Append(',').Append(r.Y).Append(',').Append(r.Width).Append(',').Append(r.Height).Append(';');
            }
            if (path != null)
            {
                var pts = path.PathPoints;
                var types = path.PathTypes;
                sb.Append("p:").Append(pts.Length).Append(';');
                for (int i = 0; i < pts.Length; i++)
                    sb.Append(pts[i].X).Append(',').Append(pts[i].Y).Append(',').Append(types[i]).Append(';');
            }
            return sb.ToString();
        }

        private static void AppendMatrixKey(StringBuilder sb, Matrix m)
        {
            if (m == null) return;
            var e = m.Elements;
            for (int i = 0; i < e.Length; i++)
                sb.Append(e[i]).Append(',');
        }
    }

    internal enum SvgDefKind
    {
        LinearGradient,
        RadialGradient,
        HatchPattern,
        TexturePattern,
        ClipPath,
    }

    /// <summary>One registered SVG def entry.</summary>
    internal sealed class SvgDef
    {
        public string Id { get; }
        public SvgDefKind Kind { get; }
        public string Key { get; }
        public object? Source { get; }
        public GraphicsPath? ClipPath { get; }

        public SvgDef(string id, SvgDefKind kind, string key, object? source, GraphicsPath? clipPath)
        {
            Id = id; Kind = kind; Key = key; Source = source; ClipPath = clipPath;
        }
    }
}