using System.Collections.Generic;

namespace MiniPdf.Drawing.Text.Native
{
    /// <summary>
    /// Dispatches font discovery to the platform-specific native implementation. When
    /// the native API is unavailable or returns no results, the caller falls back to
    /// directory scanning via <see cref="SystemFontScanner"/>.
    /// </summary>
    internal static class NativeFontDiscovery
    {
        /// <summary>
        /// Tries to discover installed fonts using the platform's native font service.
        /// </summary>
        /// <param name="fonts">
        /// When this method returns <c>true</c>, a list of discovered font faces with
        /// their family names and file paths. When <c>false</c>, the list is empty and
        /// the caller should fall back to directory scanning.
        /// </param>
        /// <returns>
        /// <c>true</c> if the native API was available and returned results; otherwise
        /// <c>false</c>.
        /// </returns>
        public static bool TryDiscoverFonts(out IReadOnlyList<NativeFontInfo> fonts)
        {
            if (global::MiniSoftware.Compat.IsWindows())
                return WindowsFontDiscovery.TryDiscoverFonts(out fonts);

            if (global::MiniSoftware.Compat.IsMacOS())
                return MacFontDiscovery.TryDiscoverFonts(out fonts);

            if (global::MiniSoftware.Compat.IsLinux())
                return LinuxFontDiscovery.TryDiscoverFonts(out fonts);

            fonts = System.Array.Empty<NativeFontInfo>();
            return false;
        }

        /// <summary>
        /// Tries to get the operating system's default UI font family name.
        /// </summary>
        /// <param name="familyName">
        /// When this method returns <c>true</c>, the default font family name.
        /// </param>
        /// <returns>
        /// <c>true</c> if the operating system returned a default family name.
        /// </returns>
        public static bool TryGetDefaultFamilyName(out string? familyName)
        {
            if (global::MiniSoftware.Compat.IsWindows())
                return WindowsFontDiscovery.TryGetDefaultFamilyName(out familyName);

            if (global::MiniSoftware.Compat.IsMacOS())
                return MacFontDiscovery.TryGetDefaultFamilyName(out familyName);

            if (global::MiniSoftware.Compat.IsLinux())
                return LinuxFontDiscovery.TryGetDefaultFamilyName(out familyName);

            familyName = null;
            return false;
        }
    }
}