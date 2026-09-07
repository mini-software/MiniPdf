using System;
using System.Globalization;
using System.Text;
using MiniSoftware.Drawing.Drawing2D;

namespace MiniSoftware.Drawing.Vector.Svg
{
    /// <summary>
    /// Converts a <see cref="GraphicsPath"/> (points + PathPointType flags) into
    /// SVG path data (<c>M</c>/<c>L</c>/<c>C</c>/<c>Z</c>). Arcs and ellipses are
    /// already flattened to Béziers at path-build time, so only these commands
    /// are needed for output.
    /// </summary>
    internal static class SvgPathWriter
    {
        /// <summary>
        /// Writes the path data string for the given path, using up to
        /// <paramref name="precision"/> decimal places.
        /// </summary>
        public static string WritePathData(GraphicsPath path, int precision = 4)
        {
            if (path == null || path.PointCount == 0)
                return string.Empty;

            var pts = path.PathPoints;
            var types = path.PathTypes;
            var sb = new StringBuilder();
            var fmt = new NumberFormat(precision);
            int i = 0;

            while (i < pts.Length)
            {
                byte type = types[i];
                PathPointType baseType = (PathPointType)(type & (byte)PathPointType.PathTypeMask);

                switch (baseType)
                {
                    case PathPointType.Start:
                        sb.Append('M').Append(fmt.Num(pts[i].X)).Append(' ').Append(fmt.Num(pts[i].Y));
                        i++;
                        // Subsequent implicit lineto until next Start/Bezier/Close
                        while (i < pts.Length)
                        {
                            byte t2 = types[i];
                            PathPointType bt2 = (PathPointType)(t2 & (byte)PathPointType.PathTypeMask);
                            if (bt2 == PathPointType.Start) break;
                            if (bt2 == PathPointType.Bezier) break;
                            if ((t2 & (byte)PathPointType.CloseSubpath) != 0 && bt2 == PathPointType.Line)
                            {
                                // last point of subpath, line-to then close
                                sb.Append('L').Append(fmt.Num(pts[i].X)).Append(' ').Append(fmt.Num(pts[i].Y));
                                sb.Append('Z');
                                i++;
                                break;
                            }
                            sb.Append('L').Append(fmt.Num(pts[i].X)).Append(' ').Append(fmt.Num(pts[i].Y));
                            if ((t2 & (byte)PathPointType.CloseSubpath) != 0)
                            {
                                sb.Append('Z');
                                i++;
                                break;
                            }
                            i++;
                        }
                        break;

                    case PathPointType.Line:
                        sb.Append('L').Append(fmt.Num(pts[i].X)).Append(' ').Append(fmt.Num(pts[i].Y));
                        if ((type & (byte)PathPointType.CloseSubpath) != 0)
                            sb.Append('Z');
                        i++;
                        break;

                    case PathPointType.Bezier:
                        // 3 points per cubic Bézier: ctrl1, ctrl2, end
                        if (i + 2 < pts.Length)
                        {
                            sb.Append('C')
                              .Append(fmt.Num(pts[i].X)).Append(' ').Append(fmt.Num(pts[i].Y)).Append(' ')
                              .Append(fmt.Num(pts[i + 1].X)).Append(' ').Append(fmt.Num(pts[i + 1].Y)).Append(' ')
                              .Append(fmt.Num(pts[i + 2].X)).Append(' ').Append(fmt.Num(pts[i + 2].Y));
                            if ((types[i + 2] & (byte)PathPointType.CloseSubpath) != 0)
                                sb.Append('Z');
                            i += 3;
                        }
                        else
                        {
                            i++;
                        }
                        break;

                    default:
                        i++;
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>Helper to format SVG path numbers compactly.</summary>
        private readonly struct NumberFormat
        {
            private readonly int _precision;
            public NumberFormat(int precision) { _precision = Math.Max(0, precision); }

            public string Num(float v)
            {
                if (float.IsNaN(v) || float.IsInfinity(v)) v = 0f;
                string s = v.ToString("F" + _precision, CultureInfo.InvariantCulture);
                if (s.IndexOf('.') >= 0)
                {
                    s = s.TrimEnd('0').TrimEnd('.');
                    if (s.Length == 0 || s == "-") s = "0";
                }
                return s;
            }
        }
    }
}