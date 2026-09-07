using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSoftware.Drawing.Metafile.Emf.Enumerations
{
    /// <summary>
    ///     The FormatSignature enumeration defines values that are used to identify the format of embedded data in EMF records.
    /// </summary>
    /// <remarks>
    ///     2.1.14 FormatSignature Enumeration
    /// </remarks>
    internal enum FormatSignature
    {
        /// <summary>
        ///     The value of this member is the sequence of ASCII characters "FME ", which happens to be the reverse of the string "EMF", and it denotes EMF record data.
        /// </summary>
        ENHMETA_SIGNATURE,

        /// <summary>
        ///     The value of this member is the sequence of ASCII characters "FSPE", which happens to be the reverse of the string "EPSF", and it denotes encapsulated PostScript (EPS) format data.
        /// </summary>
        EPS_SIGNATURE
    }
}
