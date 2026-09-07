using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MiniSoftware.Drawing.Text.Native
{
    /// <summary>
    /// Discovers installed fonts on Windows by reading the font registry keys via
    /// raw P/Invoke to <c>advapi32.dll</c>. This avoids any NuGet dependency on
    /// <c>Microsoft.Win32.Registry</c> while still resolving family names to file
    /// paths — including fonts installed outside the standard
    /// <c>%SYSTEMROOT%\Fonts</c> directory — and following font substitution rules
    /// for the default UI font.
    /// </summary>
    internal static class WindowsFontDiscovery
    {
        private const string MachineFontsKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";
        private const string UserFontsKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";
        private const string FontSubstitutesKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontSubstitutes";

        private const int HKEY_LOCAL_MACHINE = unchecked((int)0x80000002);
        private const int HKEY_CURRENT_USER = unchecked((int)0x80000001);

        private const int ERROR_SUCCESS = 0;
        private const int ERROR_NO_MORE_ITEMS = 259;

        // Registry value types
        private const int REG_SZ = 1;

        /// <summary>
        /// The suffixes that Windows appends to font family names in the registry value
        /// names, e.g. "Arial (TrueType)" or "Arial Bold (OpenType)".
        /// </summary>
        private static readonly string[] StyleSuffixes =
        {
            " (TrueType)",
            " (OpenType)",
            " (VGA res)",
            " (All res)",
        };

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegOpenKeyEx(int hKey, string subKey, int options, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegEnumValueW(IntPtr hKey, int dwIndex, StringBuilder lpValueName, ref int lpcchValueName, IntPtr lpReserved, ref int lpType, byte[] lpData, ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegQueryValueExW(IntPtr hKey, string lpValueName, IntPtr lpReserved, ref int lpType, byte[] lpData, ref int lpcbData);

        private const int KEY_READ = 0x20019;

        /// <summary>
        /// Tries to discover installed fonts by reading the Windows font registry keys.
        /// </summary>
        public static bool TryDiscoverFonts(out IReadOnlyList<NativeFontInfo> fonts)
        {
            var results = new List<NativeFontInfo>();
            string systemRoot = Environment.GetEnvironmentVariable("SYSTEMROOT");
            if (string.IsNullOrWhiteSpace(systemRoot))
                systemRoot = @"C:\Windows";

            string fontsDir = Path.Combine(systemRoot, "Fonts");

            // Machine-wide fonts (HKLM).
            ReadRegistryFonts(HKEY_LOCAL_MACHINE, MachineFontsKey, fontsDir, results);

            // Per-user fonts (HKCU).
            ReadRegistryFonts(HKEY_CURRENT_USER, UserFontsKey, fontsDir, results);

            if (results.Count == 0)
            {
                fonts = Array.Empty<NativeFontInfo>();
                return false;
            }

            fonts = results;
            return true;
        }

        /// <summary>
        /// Tries to get the Windows default UI font family name by resolving the
        /// "MS Shell Dlg 2" font substitute (which maps to "Segoe UI" on modern Windows).
        /// </summary>
        public static bool TryGetDefaultFamilyName(out string? familyName)
        {
            IntPtr hKey = IntPtr.Zero;
            try
            {
                int rc = RegOpenKeyEx(HKEY_LOCAL_MACHINE, FontSubstitutesKey, 0, KEY_READ, out hKey);
                if (rc != ERROR_SUCCESS)
                {
                    familyName = null;
                    return false;
                }

                string? substitute = ReadStringValue(hKey, "MS Shell Dlg 2");
                if (!string.IsNullOrWhiteSpace(substitute))
                {
                    familyName = substitute;
                    return true;
                }
            }
            catch
            {
                // Registry access may fail in restricted environments.
            }
            finally
            {
                if (hKey != IntPtr.Zero)
                    RegCloseKey(hKey);
            }

            familyName = null;
            return false;
        }

        private static void ReadRegistryFonts(
            int rootKey,
            string subKey,
            string fontsDir,
            List<NativeFontInfo> results)
        {
            IntPtr hKey = IntPtr.Zero;
            try
            {
                int rc = RegOpenKeyEx(rootKey, subKey, 0, KEY_READ, out hKey);
                if (rc != ERROR_SUCCESS)
                    return;

                int index = 0;
                while (true)
                {
                    var valueNameBuilder = new StringBuilder(256);
                    int valueNameLen = valueNameBuilder.Capacity;
                    int type = 0;
                    int dataLen = 0;

                    rc = RegEnumValueW(hKey, index, valueNameBuilder, ref valueNameLen, IntPtr.Zero, ref type, null, ref dataLen);
                    if (rc == ERROR_NO_MORE_ITEMS)
                        break;
                    if (rc != ERROR_SUCCESS)
                        break;

                    index++;

                    // Only interested in string values (REG_SZ).
                    if (type != REG_SZ)
                        continue;

                    // Read the data.
                    byte[] data = new byte[dataLen];
                    int dataLen2 = dataLen;
                    rc = RegEnumValueW(hKey, index - 1, valueNameBuilder, ref valueNameLen, IntPtr.Zero, ref type, data, ref dataLen2);
                    if (rc != ERROR_SUCCESS || type != REG_SZ)
                        continue;

                    string valueName = valueNameBuilder.ToString();
                    string filePath = ReadRegSzString(data, dataLen2);

                    string familyName = ExtractFamilyName(valueName);
                    if (string.IsNullOrEmpty(familyName))
                        continue;

                    string resolvedPath = ResolveFilePath(filePath, fontsDir);
                    if (resolvedPath != null)
                        results.Add(new NativeFontInfo(familyName, resolvedPath));
                }
            }
            catch
            {
                // Registry access may fail due to permissions.
            }
            finally
            {
                if (hKey != IntPtr.Zero)
                    RegCloseKey(hKey);
            }
        }

        /// <summary>
        /// Reads a REG_SZ value from an open registry key.
        /// </summary>
        private static string? ReadStringValue(IntPtr hKey, string valueName)
        {
            int type = 0;
            int dataLen = 0;

            int rc = RegQueryValueExW(hKey, valueName, IntPtr.Zero, ref type, null, ref dataLen);
            if (rc != ERROR_SUCCESS || type != REG_SZ || dataLen == 0)
                return null;

            byte[] data = new byte[dataLen];
            rc = RegQueryValueExW(hKey, valueName, IntPtr.Zero, ref type, data, ref dataLen);
            if (rc != ERROR_SUCCESS)
                return null;

            return ReadRegSzString(data, dataLen);
        }

        /// <summary>
        /// Converts a REG_SZ byte buffer to a string, trimming the trailing null.
        /// </summary>
        private static string ReadRegSzString(byte[] data, int dataLen)
        {
            // REG_SZ is UTF-16LE, null-terminated. Trim trailing nulls.
            int charCount = dataLen / 2;
            if (charCount > 0 && data[dataLen - 1] == 0 && data[dataLen - 2] == 0)
                charCount--;

            return Encoding.Unicode.GetString(data, 0, charCount * 2);
        }

        /// <summary>
        /// Extracts the font family name from a registry value name by stripping the
        /// style suffix in parentheses, e.g. "Arial Bold (TrueType)" → "Arial Bold".
        /// </summary>
        private static string ExtractFamilyName(string valueName)
        {
            string name = valueName;
            foreach (string suffix in StyleSuffixes)
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                    break;
                }
            }
            return name.Trim();
        }

        /// <summary>
        /// Resolves a font file path from the registry value. The value may be a bare
        /// filename (relative to <c>%SYSTEMROOT%\Fonts</c>) or an absolute path.
        /// </summary>
        private static string? ResolveFilePath(string filePath, string fontsDir)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            // Per-user fonts may store absolute paths.
            if (Path.IsPathRooted(filePath))
                return File.Exists(filePath) ? filePath : null;

            // Machine fonts are typically bare filenames relative to %SYSTEMROOT%\Fonts.
            string combined = Path.Combine(fontsDir, filePath);
            if (File.Exists(combined))
                return combined;

            // Some entries use relative paths with subdirectories.
            combined = Path.GetFullPath(Path.Combine(fontsDir, filePath));
            return File.Exists(combined) ? combined : null;
        }
    }
}