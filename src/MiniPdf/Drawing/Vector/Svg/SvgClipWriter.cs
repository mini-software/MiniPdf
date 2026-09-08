using MiniSoftware.Drawing.Drawing2D;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Registers clip regions/paths with the <see cref="SvgDefTable"/> and
    /// produces the <c>clip-path="url(#id)"</c> reference.
    /// </summary>
    internal static class SvgClipWriter
    {
        /// <summary>
        /// Registers a region clip and returns the <c>url(#id)</c> reference,
        /// or null if the region is null or infinite (no clip needed).
        /// </summary>
        public static string? WriteClipRef(SvgDefTable defs, Region? clip)
        {
            if (clip == null) return null;
            if (clip.IsInfinite()) return null;

            string id = defs.RegisterClip(clip, null);
            return "url(#" + id + ")";
        }

        /// <summary>
        /// Registers a path clip and returns the <c>url(#id)</c> reference,
        /// or null if the path is null.
        /// </summary>
        public static string? WriteClipRef(SvgDefTable defs, GraphicsPath? path)
        {
            if (path == null) return null;
            if (path.PointCount == 0) return null;

            string id = defs.RegisterClip(null, path);
            return "url(#" + id + ")";
        }
    }
}