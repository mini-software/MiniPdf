namespace MiniSoftware.Drawing.Metafile.Emf.Enumerations
{
    using System;

    [Flags]
    internal enum IcmProfileFlags : uint
    {
        None = 0x00000000,
        SourceLinked = 0x00000001,
        EmbedProfile = 0x00000002
    }
}
