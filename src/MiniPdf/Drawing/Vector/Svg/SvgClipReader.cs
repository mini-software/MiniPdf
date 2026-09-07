using System.Xml;
using MiniSoftware.Drawing.Drawing2D;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Parses SVG <c>&lt;clipPath&gt;</c> def elements into
    /// <see cref="GraphicsPath"/> objects for use as clipping regions.
    /// </summary>
    internal static class SvgClipReader
    {
        /// <summary>
        /// Reads a <c>&lt;clipPath&gt;</c> element and returns the combined
        /// clip path. The reader must be positioned on the clipPath start
        /// element; reads through the end element.
        /// </summary>
        public static GraphicsPath ReadClipPath(XmlReader reader)
        {
            var combined = new GraphicsPath();

            if (reader.IsEmptyElement)
                return combined;

            string? clipRule = reader.GetAttribute("clip-rule");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement) break;
                if (reader.NodeType != XmlNodeType.Element) continue;

                string name = reader.LocalName;
                var style = new SvgStyle();
                var attrs = SvgStyleParser.ApplyAttributes(reader, style);

                GraphicsPath? shape = SvgShapeReader.ReadShape(name, attrs, style);
                if (shape != null && shape.PointCount > 0)
                {
                    // Union the shape into the combined clip path
                    // (GraphicsPath doesn't have Union, so we just add the points)
                    var pts = shape.PathPoints;
                    var types = shape.PathTypes;
                    for (int i = 0; i < pts.Length; i++)
                    {
                        combined.AddLine(pts[i], pts[i]);
                    }
                    shape.Dispose();
                }
            }

            return combined;
        }
    }
}