namespace MiniPdf.Drawing.Metafile.Wmf.Enumerations
{
    /// <summary>
    ///     Specifies printer driver functionality that might not be directly accessible through WMF records defined in the RecordType Enumeration (section 2.1.1.1).
    /// </summary>
    /// <remarks>
    ///     2.1.1.17 MetafileEscapes Enumeration
    /// </remarks>
    internal enum MetafileEscapes
    {
        /// <summary>
        ///     Notifies the printer driver that the application has finished writing to a page.
        /// </summary>
        NEWFRAME = 0x0001,
        ABORTDOC = 0x0002,
        NEXTBAND = 0x0003,
        SETCOLORTABLE = 0x0004,
        GETCOLORTABLE = 0x0005,
        FLUSHOUT = 0x0006,
        DRAFTMODE = 0x0007,
        QUERYESCSUPPORT = 0x0008,
        SETABORTPROC = 0x0009,
        STARTDOC = 0x000A,
        ENDDOC = 0x000B,
        GETPHYSPAGESIZE = 0x000C,
        GETPRINTINGOFFSET = 0x000D,
        GETSCALINGFACTOR = 0x000E,
        META_ESCAPE_ENHANCED_METAFILE = 0x000F,
        SETPENWIDTH = 0x0010,
        SETCOPYCOUNT = 0x0011,
        SETPAPERSOURCE = 0x0012,
        PASSTHROUGH = 0x0013,
        GETTECHNOLOGY = 0x0014,
        SETLINECAP = 0x0015,
        SETLINEJOIN = 0x0016,
        SETMITERLIMIT = 0x0017,
        BANDINFO = 0x0018,
        DRAWPATTERNRECT = 0x0019,
        GETVECTORPENSIZE = 0x001A,
        GETVECTORBRUSHSIZE = 0x001B,
        ENABLEDUPLEX = 0x001C,
        GETSETPAPERBINS = 0x001D,
        GETSETPRINTORIENT = 0x001E,
        ENUMPAPERBINS = 0x001F,
        SETDIBSCALING = 0x0020,
        EPSPRINTING = 0x0021,
        ENUMPAPERMETRICS = 0x0022,
        GETSETPAPERMETRICS = 0x0023,
        POSTSCRIPT_DATA = 0x0025,
        POSTSCRIPT_IGNORE = 0x0026,
        GETDEVICEUNITS = 0x002A,
        GETEXTENDEDTEXTMETRICS = 0x0100,
        GETPAIRKERNTABLE = 0x0102,
        EXTTEXTOUT = 0x0200,
        GETFACENAME = 0x0201,
        DOWNLOADFACE = 0x0202,
        METAFILE_DRIVER = 0x0801,
        QUERYDIBSUPPORT = 0x0C01,
        BEGIN_PATH = 0x1000,
        CLIP_TO_PATH = 0x1001,
        END_PATH = 0x1002,
        OPEN_CHANNEL = 0x100E,
        DOWNLOADHEADER = 0x100F,
        CLOSE_CHANNEL = 0x1010,
        POSTSCRIPT_PASSTHROUGH = 0x1013,
        ENCAPSULATED_POSTSCRIPT = 0x1014,
        POSTSCRIPT_IDENTIFY = 0x1015,
        POSTSCRIPT_INJECTION = 0x1016,
        CHECKJPEGFORMAT = 0x1017,
        CHECKPNGFORMAT = 0x1018,
        GET_PS_FEATURESETTING = 0x1019,
        MXDC_ESCAPE = 0x101A,
        SPCLPASSTHROUGH2 = 0x11D8
    }
}
