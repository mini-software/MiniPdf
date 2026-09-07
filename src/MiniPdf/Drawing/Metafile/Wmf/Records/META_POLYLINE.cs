using MiniPdf.Drawing.Metafile.Wmf;

namespace MiniPdf.Drawing.Metafile.Wmf.Records
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Wmf.Enumerations;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    /// <summary>
    ///     The META_POLYLINE record draws a series of line segments by connecting the points in the specified array.
    /// </summary>
    /// <remarks>
    ///     2.3.3.14 META_POLYLINE Record
    /// </remarks>
    internal class META_POLYLINE : Record
    {
        /// <summary>
        ///     Defines the number of points in the array.
        /// </summary>
        public short NumberOfPoints { get; set; }

        /// <summary>
        ///     A NumberOfPoints array of 32-bit PointS Objects, in logical units.
        /// </summary>
        public PointS[] aPoints { get; set; } = Array.Empty<PointS>();

        public META_POLYLINE()
        {
            Header.RecordFunction = RecordType.META_POLYLINE;
        }

        public override void Read(BinaryReader reader)
        {
            NumberOfPoints = reader.ReadInt16();
            aPoints = new PointS[NumberOfPoints];
            for (int i = 0; i < NumberOfPoints; i++)
            {
                var point = new PointS();
                point.Read(reader);
                aPoints[i] = point;
            }
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(NumberOfPoints);
            foreach (var point in aPoints)
            {
                point.Write(writer);
            }
        }
    }
}
