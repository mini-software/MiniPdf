using System;
using System.Collections.Generic;
using System.Xml;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Geometry;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Helper methods for parsing SVG structural elements (<c>&lt;g&gt;</c>,
    /// <c>&lt;svg&gt;</c>, <c>&lt;defs&gt;</c>, <c>&lt;use&gt;</c>) and
    /// resolving element attributes. The main orchestration lives in
    /// <see cref="SvgReadContext"/>; this class provides reusable utilities.
    /// </summary>
    internal static class SvgElementReader
    {
        /// <summary>
        /// Extracts the id from a <c>url(#id)</c> reference string.
        /// Returns null if the string is not a url reference.
        /// </summary>
        public static string? ExtractUrlId(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            if (!s.StartsWith("url(", StringComparison.OrdinalIgnoreCase)) return null;

            int hash = s.IndexOf('#');
            int end = s.IndexOf(')', hash);
            if (hash < 0 || end < 0) return null;
            return s.Substring(hash + 1, end - hash - 1).Trim();
        }

        /// <summary>
        /// Reads all attributes from the current element into a dictionary.
        /// The reader should be positioned on an element start tag.
        /// Returns to the element after reading.
        /// </summary>
        public static Dictionary<string, string> ReadAllAttributes(XmlReader reader)
        {
            var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                    attrs[reader.Name] = reader.Value;
                reader.MoveToElement();
            }
            return attrs;
        }

        /// <summary>
        /// Skips all child content of the current element.
        /// </summary>
        public static void SkipChildren(XmlReader reader)
        {
            if (reader.IsEmptyElement) return;
            int depth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth)
                    break;
            }
        }

        /// <summary>
        /// Determines whether an SVG element name is a drawable shape.
        /// </summary>
        public static bool IsShape(string name)
        {
            switch (name)
            {
                case "rect":
                case "circle":
                case "ellipse":
                case "line":
                case "polyline":
                case "polygon":
                case "path":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether an SVG element name is a structural element.
        /// </summary>
        public static bool IsStructural(string name)
        {
            switch (name)
            {
                case "svg":
                case "g":
                case "defs":
                case "use":
                case "symbol":
                    return true;
                default:
                    return false;
            }
        }
    }
}