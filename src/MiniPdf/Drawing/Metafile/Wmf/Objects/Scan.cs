namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    using System;
    using System.IO;

    /// <summary>
    ///     The Scan Object specifies a collection of scanlines.
    /// </summary>
    /// <remarks>
    ///     2.2.2.21 Scan Object
    /// </remarks>
    internal class Scan
    {
        public struct ScanLine
        {
            /// <summary>
            ///     A 16-bit unsigned integer that defines the horizontal (x-axis) coordinate, in logical units, of the left endpoint of the scanline.
            /// </summary>
            public ushort Left;

            /// <summary>
            ///     A 16-bit unsigned integer that defines the horizontal (x-axis) coordinate, in logical units, of the right endpoint of the scanline.
            /// </summary>
            public ushort Right;
        }

        /// <summary>
        ///     A 16-bit unsigned integer that specifies the number of horizontal (x-axis) coordinates in the ScanLines array. 
        ///     This value MUST be a multiple of 2, since left and right endpoints are required to specify each scanline.
        /// </summary>
        public ushort Count { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that defines the vertical (y-axis) coordinate, in logical units, of the top scanline.
        /// </summary>
        public ushort Top { get; set; }

        /// <summary>
        ///     A 16-bit unsigned integer that defines the vertical (y-axis) coordinate, in logical units, of the bottom scanline.
        /// </summary>
        public ushort Bottom { get; set; }

        /// <summary>
        ///     An array of scanlines, each specified by left and right horizontal (x-axis) coordinates of its endpoints.
        /// </summary>
        public ScanLine[] ScanLines { get; set; } = Array.Empty<ScanLine>();

        /// <summary>
        ///     A 16-bit unsigned integer that MUST be the same as the value of the Count field; it is present to allow upward travel in the structure.
        /// </summary>
        public ushort Count2 { get; set; }

        public void Read(BinaryReader reader)
        {
            Count = reader.ReadUInt16();
            Top = reader.ReadUInt16();
            Bottom = reader.ReadUInt16();
            ScanLines = new ScanLine[Count];
            for (int i = 0; i < ScanLines.Length; i++)
            {
                ScanLines[i].Left = reader.ReadUInt16();
                ScanLines[i].Right = reader.ReadUInt16();
            }
            Count2 = reader.ReadUInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Count);
            writer.Write(Top);
            writer.Write(Bottom);
            if (ScanLines != null)
            {
                foreach (var scanLine in ScanLines)
                {
                    writer.Write(scanLine.Left);
                    writer.Write(scanLine.Right);
                }
            }
            writer.Write(Count2);
        }
    }
}
