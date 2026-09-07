using System;
using System.Collections.Generic;
using System.IO;
using MiniSoftware.Drawing.Drawing2D;
using MiniSoftware.Drawing.Enums;
using MiniSoftware.Drawing.Geometry;
using MiniSoftware.Drawing.Imaging;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Parses SVG <c>&lt;image&gt;</c> elements, resolving data URIs and
    /// local file references into <see cref="Bitmap"/> objects.
    /// </summary>
    internal static class SvgImageReader
    {
        /// <summary>
        /// Reads an <c>&lt;image&gt;</c> element and returns the decoded image
        /// plus its destination rectangle, or null if the image cannot be resolved.
        /// </summary>
        public static (Image? Image, RectangleF DestRect)? ReadImage(
            Dictionary<string, string> attrs)
        {
            string? href = GetValue(attrs, "href") ?? GetValue(attrs, "xlink:href");

            if (string.IsNullOrWhiteSpace(href))
                return null;

            Image? image = ResolveHref(href);
            if (image == null)
                return null;

            float x = SvgShapeReader.GetFloat(attrs, "x");
            float y = SvgShapeReader.GetFloat(attrs, "y");
            float w = SvgShapeReader.GetFloat(attrs, "width", image.Width);
            float h = SvgShapeReader.GetFloat(attrs, "height", image.Height);

            return (image, new RectangleF(x, y, w, h));
        }

        private static Image? ResolveHref(string href)
        {
            href = href.Trim();

            // Data URI: data:image/png;base64,...
            if (href.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return DecodeDataUri(href);
            }

            // Local file reference
            if (!href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !href.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (File.Exists(href))
                        return new Bitmap(href);
                }
                catch
                {
                    // Ignore file loading errors
                }
            }

            // Remote URLs are not fetched (security)
            return null;
        }

        private static Image? DecodeDataUri(string uri)
        {
            int comma = uri.IndexOf(',');
            if (comma < 0) return null;

            string meta = uri.Substring(5, comma - 5); // after "data:"
            string data = uri.Substring(comma + 1);

            bool isBase64 = meta.IndexOf("base64", StringComparison.OrdinalIgnoreCase) >= 0;

            byte[] bytes;
            if (isBase64)
            {
                try
                {
                    bytes = Convert.FromBase64String(data);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                // URL-encoded text data — not supported for images
                return null;
            }

            try
            {
                using var ms = new MemoryStream(bytes);
                return new Bitmap(ms);
            }
            catch
            {
                return null;
            }
        }

        private static string? GetValue(Dictionary<string, string> attrs, string name)
        {
            attrs.TryGetValue(name, out string? v);
            return v;
        }
    }
}