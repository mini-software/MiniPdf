using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MiniPdf.Drawing.Text.Native
{
    /// <summary>
    /// Discovers installed fonts on Linux using fontconfig. Fontconfig is the
    /// authoritative font service on virtually all Linux desktop and server
    /// distributions and respects aliases, non-standard install locations, and
    /// per-user configuration.
    /// </summary>
    internal static class LinuxFontDiscovery
    {
        private const string FontconfigLibrary = "fontconfig";

        /// <summary>FcSetSystem — the system-wide font set.</summary>
        private const int FcSetSystem = 0;

        /// <summary>FcResultMatch — the pattern field was found.</summary>
        private const int FcResultMatch = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct FcFontSet
        {
            public int NFont;
            public int SFont;
            public IntPtr Fonts; // FcPattern**
        }

        [DllImport(FontconfigLibrary)]
        private static extern IntPtr FcInitLoadConfigAndFonts();

        [DllImport(FontconfigLibrary)]
        private static extern IntPtr FcConfigGetFonts(IntPtr config, int set);

        [DllImport(FontconfigLibrary)]
        private static extern void FcFontSetDestroy(IntPtr fontSet);

        [DllImport(FontconfigLibrary)]
        private static extern int FcPatternGetString(IntPtr pattern, string field, int id, out IntPtr result);

        [DllImport(FontconfigLibrary)]
        private static extern void FcPatternDestroy(IntPtr pattern);

        /// <summary>
        /// Tries to discover installed fonts using fontconfig.
        /// </summary>
        public static bool TryDiscoverFonts(out IReadOnlyList<NativeFontInfo> fonts)
        {
            var results = new List<NativeFontInfo>();

            try
            {
                IntPtr config = FcInitLoadConfigAndFonts();
                if (config == IntPtr.Zero)
                {
                    fonts = Array.Empty<NativeFontInfo>();
                    return false;
                }

                IntPtr fontSetPtr = FcConfigGetFonts(config, FcSetSystem);
                if (fontSetPtr == IntPtr.Zero)
                {
                    fonts = Array.Empty<NativeFontInfo>();
                    return false;
                }

                try
                {
                    FcFontSet fontSet = Marshal.PtrToStructure<FcFontSet>(fontSetPtr);
                    IntPtr[] patternPtrs = new IntPtr[fontSet.NFont];
                    Marshal.Copy(fontSet.Fonts, patternPtrs, 0, fontSet.NFont);

                    for (int i = 0; i < fontSet.NFont; i++)
                    {
                        IntPtr pattern = patternPtrs[i];
                        if (pattern == IntPtr.Zero)
                            continue;

                        string? family = GetStringField(pattern, "family");
                        string? file = GetStringField(pattern, "file");

                        if (!string.IsNullOrEmpty(family) && !string.IsNullOrEmpty(file))
                            results.Add(new NativeFontInfo(family!, file!));
                    }
                }
                finally
                {
                    FcFontSetDestroy(fontSetPtr);
                }
            }
            catch
            {
                // fontconfig may not be installed (minimal containers, Alpine without fontconfig).
                fonts = Array.Empty<NativeFontInfo>();
                return false;
            }

            if (results.Count == 0)
            {
                fonts = Array.Empty<NativeFontInfo>();
                return false;
            }

            fonts = results;
            return true;
        }

        /// <summary>
        /// Tries to get the Linux default UI font family name. There is no single
        /// "default" font on Linux; fontconfig's <c>monospace</c> and <c>sans-serif</c>
        /// aliases resolve to the desktop environment's preferred font. We return
        /// <c>"sans-serif"</c> as the fontconfig alias that every conformant installation
        /// resolves to a real family.
        /// </summary>
        public static bool TryGetDefaultFamilyName(out string? familyName)
        {
            // fontconfig guarantees that "sans-serif", "serif", and "monospace" aliases
            // always resolve to an installed font. We return the generic alias; the caller
            // can resolve it to a concrete family via the discovered font list.
            familyName = "sans-serif";
            return true;
        }

        private static string? GetStringField(IntPtr pattern, string field)
        {
            int result = FcPatternGetString(pattern, field, 0, out IntPtr stringPtr);
            if (result != FcResultMatch || stringPtr == IntPtr.Zero)
                return null;

            // fontconfig strings are null-terminated UTF-8. The pointer is owned by the
            // pattern and must not be freed.
            return PtrToUtf8String(stringPtr);
        }

        private static string? PtrToUtf8String(IntPtr ptr)
        {
            // Read bytes until null terminator.
            int length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
                length++;

            if (length == 0)
                return null;

            byte[] buffer = new byte[length];
            Marshal.Copy(ptr, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}