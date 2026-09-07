namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Controls global font-loading behaviour for the drawing library. All properties
    /// must be set before the first access to <see cref="InstalledFontCollection.Default"/>
    /// because the system font collection is scanned once and cached for the lifetime of
    /// the process.
    /// </summary>
    public static class FontSettings
    {
        private static bool _autoLoadSystemFonts = true;
        private static bool _useNativeFontApis = true;

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="InstalledFontCollection"/>
        /// automatically loads system fonts on first access. The default is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <c>true</c> (the default), <see cref="InstalledFontCollection.Default"/>
        /// scans the platform font directories (and optionally native OS APIs) and loads
        /// every discovered font. This is the expected behaviour for desktop and console
        /// applications.
        /// </para>
        /// <para>
        /// When <c>false</c>, <see cref="InstalledFontCollection.Default"/> contains only
        /// the three generic fallback families (<c>Sans Serif</c>, <c>Serif</c>,
        /// <c>Monospace</c>) with guessed metrics. Applications must load fonts manually
        /// via <see cref="PrivateFontCollection"/>. This is recommended for web and server
        /// scenarios where:
        /// </para>
        /// <list type="bullet">
        /// <item><description>Startup latency from scanning hundreds of system fonts is undesirable.</description></item>
        /// <item><description>The server/container has no system fonts installed.</description></item>
        /// <item><description>The application ships its own embedded fonts and wants deterministic behaviour.</description></item>
        /// <item><description>File-system access to font directories is restricted by sandboxing.</description></item>
        /// </list>
        /// <para>
        /// This property must be set before the first access to
        /// <see cref="InstalledFontCollection.Default"/>. Changes after the collection has
        /// been created have no effect.
        /// </para>
        /// </remarks>
        public static bool AutoLoadSystemFonts
        {
            get => _autoLoadSystemFonts;
            set => _autoLoadSystemFonts = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether native operating-system APIs are used
        /// for font discovery before falling back to directory scanning. The default is
        /// <c>true</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <c>true</c> (the default), the library queries the platform's native font
        /// service first:
        /// </para>
        /// <list type="bullet">
        /// <item><description><b>Windows</b>: reads the font registry keys (<c>HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts</c> and the per-user equivalent) to resolve family names to file paths, including fonts installed outside the standard <c>%SYSTEMROOT%\Fonts</c> directory.</description></item>
        /// <item><description><b>macOS</b>: queries CoreText (<c>CTFontManagerCopyAvailableFontFamilyNames</c>) for the authoritative family list and file paths.</description></item>
        /// <item><description><b>Linux</b>: queries fontconfig (<c>FcConfigGetFonts</c>) for the authoritative family list and file paths, respecting fontconfig aliases and non-standard install locations.</description></item>
        /// </list>
        /// <para>
        /// When <c>false</c>, only directory scanning of the standard font directories is
        /// used. This avoids any P/Invoke to native libraries and is useful in restricted
        /// environments where native library loading is prohibited.
        /// </para>
        /// <para>
        /// This property has no effect when <see cref="AutoLoadSystemFonts"/> is <c>false</c>.
        /// </para>
        /// </remarks>
        public static bool UseNativeFontApis
        {
            get => _useNativeFontApis;
            set => _useNativeFontApis = value;
        }
    }
}