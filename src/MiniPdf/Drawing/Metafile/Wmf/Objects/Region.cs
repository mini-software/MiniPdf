namespace MiniPdf.Drawing.Metafile.Wmf.Objects
{
    using System;
    using System.IO;

    /// <summary>
    ///     The Region Object defines a potentially non-rectilinear shape defined by an array of scanlines.
    /// </summary>
    /// <remarks>
    ///     2.2.1.5 Region Object
    /// </remarks>
    internal class Region
    {
        /// <summary>
        ///     A value that MUST be ignored.
        /// </summary>
        public short nextInChain { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that specifies the region identifier. It MUST be 0x0006.
        /// </summary>
        public short ObjectType { get; set; }

        /// <summary>
        ///     A value that MUST be ignored.
        /// </summary>
        public int ObjectCount { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that defines the size of the region in bytes plus the size of aScans in bytes.
        /// </summary>
        public short RegionSize { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that defines the number of scanlines composing the region.
        /// </summary>
        public short ScanCount { get; set; }

        /// <summary>
        ///     A 16-bit signed integer that defines the maximum number of points in any one scan in this region.
        /// </summary>
        internal short maxScan { get; set; }

        /// <summary>
        ///     A Rect object (section 2.2.2.18) that defines the bounding rectangle.
        /// </summary>
        public Rect BoundingRectangle { get; set; }

        /// <summary>
        ///     An array of Scan objects (section 2.2.2.21) that define the scanlines in the region.
        /// </summary>
        public Scan[] aScans { get; set; } = Array.Empty<Scan>();

        public void Read(BinaryReader reader)
        {
            nextInChain = reader.ReadInt16();
            ObjectType = reader.ReadInt16();
            ObjectCount = reader.ReadInt32();
            RegionSize = reader.ReadInt16();
            ScanCount = reader.ReadInt16();
            maxScan = reader.ReadInt16();
            var boundingBox = new Rect();
            boundingBox.Read(reader);
            BoundingRectangle = boundingBox;
            aScans = new Scan[ScanCount];
            for (int i = 0; i < ScanCount; i++)
            {
                aScans[i] = new Scan();
                aScans[i].Read(reader);
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(nextInChain);
            writer.Write(ObjectType);
            writer.Write(ObjectCount);
            writer.Write(RegionSize);
            writer.Write(ScanCount);
            writer.Write(maxScan);
            var boundingBox = BoundingRectangle;
            boundingBox.Write(writer);
            if (aScans != null)
            {
                foreach (var scan in aScans)
                {
                    scan.Write(writer);
                }
            }
        }
    }
}
