using System;
using System.Collections.Generic;
using System.IO;

namespace MiniPdf.Drawing.Text
{
    /// <summary>
    /// Enumerates the platform-specific directories that hold installed fonts and
    /// yields the font file paths within them. Used by <see cref="InstalledFontCollection"/>
    /// to populate the system font collection without any native API calls.
    /// </summary>
    internal static class SystemFontScanner
    {
        /// <summary>
        /// File extensions recognised by the integrated font engine. The comparison is
        /// case-insensitive (see <see cref="SupportedExtensions"/>).
        /// </summary>
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".ttf",
            ".otf",
            ".ttc",
            ".otc",
            ".woff",
            ".woff2",
        };

        /// <summary>
        /// Gets the platform-specific directories that conventionally hold installed fonts.
        /// Only directories that exist on the current machine are returned.
        /// </summary>
        /// <returns>An enumerable of existing font directory paths.</returns>
        public static IEnumerable<string> GetStandardFontDirectories()
        {
            foreach (string candidate in GetCandidateDirectories())
            {
                if (Directory.Exists(candidate))
                    yield return candidate;
            }
        }

        /// <summary>
        /// Enumerates every supported font file in <paramref name="directory"/>, recursing
        /// into subdirectories. Directories that cannot be read (permissions) are skipped
        /// silently, mirroring the behaviour of SixLabors.Fonts.
        /// </summary>
        /// <param name="directory">The directory to scan.</param>
        /// <returns>An enumerable of font file paths.</returns>
        public static IEnumerable<string> EnumerateFontFiles(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                yield break;

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);
            }
            catch
            {
                // Permissions / IO errors — skip this directory entirely.
                yield break;
            }

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file);
                if (extension.Length != 0 && SupportedExtensions.Contains(extension))
                    yield return file;
            }
        }

        /// <summary>
        /// Returns the platform-specific candidate font directories without checking
        /// existence. The caller is expected to filter by existence.
        /// </summary>
        private static IEnumerable<string> GetCandidateDirectories()
        {
            if (global::MiniSoftware.Compat.IsWindows())
            {
                string systemRoot = Environment.GetEnvironmentVariable("SYSTEMROOT");
                if (string.IsNullOrWhiteSpace(systemRoot))
                    systemRoot = @"C:\Windows";

                yield return Path.Combine(systemRoot, "Fonts");

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!string.IsNullOrEmpty(appData))
                    yield return Path.Combine(appData, "Microsoft", "Windows", "Fonts");

                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(localAppData))
                    yield return Path.Combine(localAppData, "Microsoft", "Windows", "Fonts");
            }
            else if (global::MiniSoftware.Compat.IsLinux())
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(home))
                {
                    yield return Path.Combine(home, ".fonts");
                    yield return Path.Combine(home, ".local", "share", "fonts");
                }

                yield return "/usr/local/share/fonts";
                yield return "/usr/share/fonts";
            }
            else if (global::MiniSoftware.Compat.IsMacOS())
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(home))
                    yield return Path.Combine(home, "Library", "Fonts");

                yield return "/Library/Fonts";
                yield return "/System/Library/Fonts";
                yield return "/Network/Library/Fonts";
            }
        }
    }
}