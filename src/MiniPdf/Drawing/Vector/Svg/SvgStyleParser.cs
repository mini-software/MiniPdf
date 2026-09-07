using System;
using System.Collections.Generic;
using System.Globalization;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Fully-resolved SVG presentation attributes for the current element.
    /// All properties are concrete strings (SVG defaults applied). Null means
    /// "not applicable" or "use default".
    /// </summary>
    internal sealed class SvgStyle
    {
        public string Fill          = "black";
        public string Stroke        = "none";
        public string StrokeWidth   = "1";
        public string? StrokeDashArray;
        public string StrokeLinecap   = "butt";
        public string StrokeLinejoin  = "miter";
        public string StrokeMiterlimit = "4";
        public string StrokeDashoffset = "0";
        public string FillRule      = "nonzero";
        public string Opacity       = "1";
        public string FillOpacity   = "1";
        public string StrokeOpacity = "1";
        public string FontFamily    = "Arial";
        public string FontSize      = "16";
        public string FontWeight    = "normal";
        public string FontStyleVal  = "normal";
        public string TextAnchor    = "start";
        public string Display       = "inline";
        public string Visibility    = "visible";
        public string ClipPath      = "none";

        /// <summary>Creates a shallow copy.</summary>
        public SvgStyle Clone() => (SvgStyle)MemberwiseClone();

        /// <summary>
        /// Returns true if fill is not "none" and not "inherit".
        /// </summary>
        public bool HasFill => !IsNone(Fill) && !IsInherit(Fill);

        /// <summary>
        /// Returns true if stroke is not "none" and not "inherit".
        /// </summary>
        public bool HasStroke => !IsNone(Stroke) && !IsInherit(Stroke);

        private static bool IsNone(string? s) => string.Equals(s, "none", StringComparison.OrdinalIgnoreCase);
        private static bool IsInherit(string? s) => string.Equals(s, "inherit", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses <c>style="..."</c> CSS property lists and individual SVG
    /// presentation attributes, applying them onto a <see cref="SvgStyle"/>.
    /// </summary>
    internal static class SvgStyleParser
    {
        /// <summary>
        /// Applies a CSS-style property string (e.g. <c>fill:red;stroke:blue</c>)
        /// to the given style.
        /// </summary>
        public static void ApplyStyleString(SvgStyle style, string? styleAttr)
        {
            if (string.IsNullOrWhiteSpace(styleAttr)) return;

            foreach (string pair in styleAttr.Split(';'))
            {
                int colon = pair.IndexOf(':');
                if (colon < 0) continue;
                string name = pair.Substring(0, colon).Trim();
                string value = pair.Substring(colon + 1).Trim();
                if (name.Length > 0 && value.Length > 0)
                    ApplyProperty(style, name, value);
            }
        }

        /// <summary>
        /// Applies a single SVG presentation attribute (e.g.
        /// <c>fill="red"</c>) to the given style.
        /// </summary>
        public static void ApplyAttribute(SvgStyle style, string name, string value)
        {
            ApplyProperty(style, name, value);
        }

        /// <summary>
        /// Reads all attributes from the current XmlReader position, applies
        /// style-related ones to the style, and returns a dictionary of all
        /// attributes for further processing (e.g. geometry attributes).
        /// </summary>
        public static Dictionary<string, string> ApplyAttributes(
            System.Xml.XmlReader reader, SvgStyle style)
        {
            var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    string name = reader.Name;
                    string value = reader.Value;
                    attrs[name] = value;

                    if (name.Equals("style", StringComparison.OrdinalIgnoreCase))
                        ApplyStyleString(style, value);
                    else
                        ApplyAttribute(style, name, value);
                }
                reader.MoveToElement();
            }

            return attrs;
        }

        private static void ApplyProperty(SvgStyle style, string name, string value)
        {
            switch (name.ToLowerInvariant())
            {
                case "fill":              style.Fill = value; break;
                case "stroke":            style.Stroke = value; break;
                case "stroke-width":      style.StrokeWidth = value; break;
                case "stroke-dasharray":  style.StrokeDashArray = value; break;
                case "stroke-linecap":    style.StrokeLinecap = value; break;
                case "stroke-linejoin":   style.StrokeLinejoin = value; break;
                case "stroke-miterlimit": style.StrokeMiterlimit = value; break;
                case "stroke-dashoffset": style.StrokeDashoffset = value; break;
                case "fill-rule":         style.FillRule = value; break;
                case "opacity":           style.Opacity = value; break;
                case "fill-opacity":      style.FillOpacity = value; break;
                case "stroke-opacity":    style.StrokeOpacity = value; break;
                case "font-family":       style.FontFamily = value; break;
                case "font-size":         style.FontSize = value; break;
                case "font-weight":       style.FontWeight = value; break;
                case "font-style":        style.FontStyleVal = value; break;
                case "text-anchor":       style.TextAnchor = value; break;
                case "display":           style.Display = value; break;
                case "visibility":        style.Visibility = value; break;
                case "clip-path":         style.ClipPath = value; break;
            }
        }

        /// <summary>
        /// Parses a length value (e.g. "10", "10px", "50%") to a float,
        /// stripping units. Percentages return NaN (caller must handle).
        /// </summary>
        public static float ParseLength(string? s, float defaultValue = 0f)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;
            s = s.Trim();

            // Strip common unit suffixes
            string[] units = { "px", "pt", "em", "ex", "%", "pc", "in", "cm", "mm" };
            bool isPercent = false;
            foreach (var u in units)
            {
                if (s.EndsWith(u, StringComparison.OrdinalIgnoreCase))
                {
                    if (u == "%") isPercent = true;
                    s = s.Substring(0, s.Length - u.Length).Trim();
                    break;
                }
            }

            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                return isPercent ? float.NaN : val;
            return defaultValue;
        }

        /// <summary>
        /// Parses a float value, returning the default on failure.
        /// </summary>
        public static float ParseFloat(string? s, float defaultValue = 0f)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;
            if (float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                return val;
            return defaultValue;
        }
    }
}