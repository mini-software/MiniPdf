namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Defines the types of records that can be used in WMF metafiles.
    /// </summary>
    internal enum RecordType
    {
        /// <summary>
        ///     Specifies the end of the file, the last record in the metafile.
        /// </summary>
        META_EOF = 0x0000,

        /// <summary>
        ///     This record maps entries from the logical palette that is defined in the playback device context to the system palette.
        /// </summary>
        META_REALIZEPALETTE = 0x0035,

        /// <summary>
        ///     This record defines red green blue (RGB) color values in a range of entries in the logical palette that is defined in the playback device context.
        /// </summary>
        META_SETPALENTRIES = 0x0037,

        /// <summary>
        ///     This record defines the background raster operation mix mode in the playback device context. The background mix mode is the mode for combining pens, text, hatched brushes, and interiors of filled objects with background colors on the output surface.
        /// </summary>
        META_SETBKMODE = 0x0102,

        /// <summary>
        ///     This record defines the mapping mode in the playback device context. The mapping mode defines the unit of measure used to transform page-space coordinates into coordinates of the output device, and also defines the orientation of the device's x and y axes.
        /// </summary>
        META_SETMAPMODE = 0x0103,

        /// <summary>
        ///     This record defines the foreground raster operation mix mode in the playback device context. The foreground mix mode is the mode for combining pens and interiors of filled objects with foreground colors on the output surface.
        /// </summary>
        META_SETROP2 = 0x0104,
        META_SETRELABS = 0x0105,
        META_SETPOLYFILLMODE = 0x0106,
        META_SETSTRETCHBLTMODE = 0x0107,
        META_SETTEXTCHAREXTRA = 0x0108,
        META_RESTOREDC = 0x0127,
        META_RESIZEPALETTE = 0x0139,
        META_DIBCREATEPATTERNBRUSH = 0x0142,
        META_SETLAYOUT = 0x0149,
        META_SETBKCOLOR = 0x0201,
        META_SETTEXTCOLOR = 0x0209,
        META_OFFSETVIEWPORTORG = 0x0211,
        META_LINETO = 0x0213,
        META_MOVETO = 0x0214,
        META_OFFSETCLIPRGN = 0x0220,
        META_FILLREGION = 0x0228,
        META_SETMAPPERFLAGS = 0x0231,
        META_SELECTPALETTE = 0x0234,
        META_POLYGON = 0x0324,
        META_POLYLINE = 0x0325,
        META_SETTEXTJUSTIFICATION = 0x020A,
        META_SETWINDOWORG = 0x020B,
        META_SETWINDOWEXT = 0x020C,
        META_SETVIEWPORTORG = 0x020D,
        META_SETVIEWPORTEXT = 0x020E,
        META_OFFSETWINDOWORG = 0x020F,
        META_SCALEWINDOWEXT = 0x0410,
        META_SCALEVIEWPORTEXT = 0x0412,
        META_EXCLUDECLIPRECT = 0x0415,
        META_INTERSECTCLIPRECT = 0x0416,
        META_ELLIPSE = 0x0418,
        META_FLOODFILL = 0x0419,
        META_FRAMEREGION = 0x0429,
        META_ANIMATEPALETTE = 0x0436,
        META_TEXTOUT = 0x0521,
        META_POLYPOLYGON = 0x0538,
        META_EXTFLOODFILL = 0x0548,
        META_RECTANGLE = 0x041B,
        META_SETPIXEL = 0x041F,
        META_ROUNDRECT = 0x061C,
        META_PATBLT = 0x061D,
        META_SAVEDC = 0x001E,
        META_PIE = 0x081A,
        META_STRETCHBLT = 0x0B23,
        META_ESCAPE = 0x0626,
        META_INVERTREGION = 0x012A,
        META_PAINTREGION = 0x012B,
        META_SELECTCLIPREGION = 0x012C,
        META_SELECTOBJECT = 0x012D,
        META_SETTEXTALIGN = 0x012E,
        META_ARC = 0x0817,
        META_CHORD = 0x0830,
        META_BITBLT = 0x0922,
        META_EXTTEXTOUT = 0x0a32,
        META_SETDIBTODEV = 0x0d33,
        META_DIBBITBLT = 0x0940,
        META_DIBSTRETCHBLT = 0x0b41,
        META_STRETCHDIB = 0x0f43,
        META_DELETEOBJECT = 0x01f0,
        META_CREATEPALETTE = 0x00f7,
        META_CREATEPATTERNBRUSH = 0x01F9,
        META_CREATEPENINDIRECT = 0x02FA,
        META_CREATEFONTINDIRECT = 0x02FB,
        META_CREATEBRUSHINDIRECT = 0x02FC,
        META_CREATEREGION = 0x06FF
    }
}
