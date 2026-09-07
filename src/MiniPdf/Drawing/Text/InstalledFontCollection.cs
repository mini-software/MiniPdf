using System;
using System.Collections.Generic;
using MiniPdf.Drawing.Font.Sources;
using MiniPdf.Drawing.Text.Native;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Represents the collection of fonts installed on the system. The collection is
    /// populated on first use by querying the platform's native font service (Windows
    /// registry, macOS CoreText, Linux fontconfig) and falling back to directory
    /// scanning. The result is cached for the lifetime of the process.
    /// </summary>
    /// <remarks>
    /// The auto-loading behaviour can be controlled via <see cref="FontSettings"/>. Set
    /// <see cref="FontSettings.AutoLoadSystemFonts"/> to <c>false</c> before first access
    /// to <see cref="Default"/> to disable system font scanning entirely (useful in web
    /// and server scenarios). Set <see cref="FontSettings.UseNativeFontApis"/> to
    /// <c>false</c> to skip native API calls and use directory scanning only.
    /// </remarks>
    public sealed class InstalledFontCollection : FontCollection
    {
        /// <summary>
        /// The process-wide cached instance, created once on first access. The scan is
        /// thread-safe and re-entrant.
        /// </summary>
        private static readonly Lazy<InstalledFontCollection> LazyDefault =
            new Lazy<InstalledFontCollection>(CreateDefaultInstance, true);

        /// <summary>
        /// Gets the process-wide default <see cref="InstalledFontCollection"/>. The
        /// system fonts are discovered once on first access and the result is cached for
        /// subsequent calls.
        /// </summary>
        /// <remarks>
        /// The behaviour of this property is controlled by <see cref="FontSettings"/>.
        /// Changes to <see cref="FontSettings"/> after first access have no effect.
        /// </remarks>
        public static InstalledFontCollection Default => LazyDefault.Value;

        /// <summary>
        /// The default font family name as reported by the operating system, or
        /// <c>null</c> when the OS did not provide one.
        /// </summary>
        public string? DefaultFamilyName { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="InstalledFontCollection"/> and discovers the
        /// system fonts. This matches the GDI+ <c>new InstalledFontCollection()</c>
        /// semantics where construction enumerates installed fonts.
        /// </summary>
        public InstalledFontCollection()
            : this(scanSystem: true)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="InstalledFontCollection"/> optionally
        /// discovering the system fonts.
        /// </summary>
        /// <param name="scanSystem">
        /// <c>true</c> to discover system fonts; <c>false</c> to create an empty
        /// collection containing only the generic fallback families.
        /// </param>
        private InstalledFontCollection(bool scanSystem)
        {
            // Always register the generic families first so that GenericSansSerif,
            // GenericSerif and GenericMonospace resolve even when no system fonts are found.
            AddOrMergeFamily(FontFamily.GenericSansSerif);
            AddOrMergeFamily(FontFamily.GenericSerif);
            AddOrMergeFamily(FontFamily.GenericMonospace);

            if (scanSystem)
                DiscoverSystemFonts();
        }

        /// <summary>
        /// Creates the default instance used by <see cref="LazyDefault"/>, respecting
        /// the current <see cref="FontSettings"/>.
        /// </summary>
        private static InstalledFontCollection CreateDefaultInstance()
        {
            if (!FontSettings.AutoLoadSystemFonts)
                return new InstalledFontCollection(scanSystem: false);

            return new InstalledFontCollection(scanSystem: true);
        }

        /// <summary>
        /// Discovers system fonts using the configured strategy: native APIs first
        /// (when enabled), then directory scanning as a fallback.
        /// </summary>
        private void DiscoverSystemFonts()
        {
            bool nativeSucceeded = false;

            if (FontSettings.UseNativeFontApis)
            {
                nativeSucceeded = TryDiscoverViaNativeApi();

                // Query the OS default font family regardless of whether font discovery
                // succeeded — the default family query is independent.
                if (NativeFontDiscovery.TryGetDefaultFamilyName(out string? defaultName))
                    DefaultFamilyName = defaultName;
            }

            // Fall back to directory scanning when native APIs are disabled or returned
            // no results.
            if (!nativeSucceeded)
                ScanSystemFontDirectories();
        }

        /// <summary>
        /// Tries to discover fonts using the platform's native API. Returns
        /// <c>true</c> when the native API was available and returned results.
        /// </summary>
        private bool TryDiscoverViaNativeApi()
        {
            if (!NativeFontDiscovery.TryDiscoverFonts(out IReadOnlyList<NativeFontInfo> fonts))
                return false;

            foreach (NativeFontInfo font in fonts)
            {
                try
                {
                    AddFacesFromSource(new FileSystemStreamSource(font.FilePath));
                }
                catch
                {
                    // Skip corrupt, locked, or unsupported font files.
                }
            }

            return true;
        }

        /// <summary>
        /// Scans every platform-specific font directory and loads each supported font
        /// file into this collection. This is the fallback when native APIs are
        /// unavailable or disabled.
        /// </summary>
        private void ScanSystemFontDirectories()
        {
            foreach (string directory in SystemFontScanner.GetStandardFontDirectories())
            {
                foreach (string file in SystemFontScanner.EnumerateFontFiles(directory))
                {
                    try
                    {
                        AddFacesFromSource(new FileSystemStreamSource(file));
                    }
                    catch
                    {
                        // Skip corrupt, locked, or unsupported font files.
                    }
                }
            }
        }
    }
}