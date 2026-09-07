using System;
using System.IO;
using MiniPdf.Drawing.Geometry;
using MiniPdf.Drawing.Imaging;

namespace MiniPdf.Drawing.Vector.Svg
{
    /// <summary>
    /// Writes SVG <c>&lt;image&gt;</c> elements with embedded base64 PNG data.
    /// </summary>
    internal static class SvgImageWriter
    {
        /// <summary>
        /// Writes an <c>&lt;image&gt;</c> element for a rect-based DrawImage.
        /// </summary>
        public static void WriteImage(SvgXmlWriter w, Image image, RectangleF destRect)
        {
            w.WriteStartElement("image");
            w.WriteAttribute("x", destRect.X);
            w.WriteAttribute("y", destRect.Y);
            w.WriteAttribute("width", destRect.Width);
            w.WriteAttribute("height", destRect.Height);
            w.WriteAttribute("preserveAspectRatio", "none");
            w.WriteXLinkHref(EmbedImage(image));
            w.WriteEndElement();
        }

        /// <summary>
        /// Embeds an image as a base64 data URI. The image is encoded as PNG.
        /// </summary>
        public static string EmbedImage(Image image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            byte[] bytes = ms.ToArray();
            return "data:image/png;base64," + Convert.ToBase64String(bytes);
        }
    }
}