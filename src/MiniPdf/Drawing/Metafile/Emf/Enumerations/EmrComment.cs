using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPdf.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The EmrComment enumeration defines the types of data that a public comment record can contain, as specified in section 2.3.3.4.
    /// </summary>
    /// <remarks>
    ///     2.1.10 EmrComment Enumeration
    /// </remarks>
    internal enum EmrComment : uint
    {
        /// <summary>
        ///     This comment record contains a specification of an image in WMF. See [MS-WMF] for more information.
        /// </summary>
        EmrCommentWindowsMetafile = 0x80000001,

        /// <summary>
        ///     This comment record identifies the beginning of a group of drawing records. It identifies an object within an EMF metafile.
        /// </summary>
        EmrCommentBegingroup = 0x00000002,

        /// <summary>
        ///     This comment record identifies the end of a group of drawing records. For every EMR_COMMENT_BEGINGROUP record, an EMR_COMMENT_ENDGROUP record MUST be included in the metafile, and they MAY be nested.
        /// </summary>
        EMR_COMMENT_ENDGROUP = 0x00000003,

        /// <summary>
        ///     This comment record allows multiple definitions of an image to be included in the metafile. Using this comment, for example, an application can include encapsulated PostScript text as well as an EMF definition of an image.
        /// </summary>
        EMR_COMMENT_MULTIFORMATS = 0x40000004,

        /// <summary>
        ///     This comment record is reserved and MUST NOT be used in an EMF metafile.
        /// </summary>
        EMR_COMMENT_UNICODE_STRING = 0x00000040,

        /// <summary>
        ///     This comment record is reserved and MUST NOT be used in an EMF metafile.
        /// </summary>
        EMR_COMMENT_UNICODE_END = 0x00000080
    }
}
