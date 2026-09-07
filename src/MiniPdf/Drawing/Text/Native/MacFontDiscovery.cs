using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MiniSoftware.Drawing.Text.Native
{
    /// <summary>
    /// Discovers installed fonts on macOS using the CoreText framework. CoreText is the
    /// authoritative source of font information on macOS and returns family names and
    /// file paths that match what native macOS applications see.
    /// </summary>
    internal static class MacFontDiscovery
    {
        private const string CoreTextLibrary = "/System/Library/Frameworks/CoreText.framework/CoreText";
        private const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        /// <summary>kCFStringEncodingUTF8</summary>
        private const int KCFStringEncodingUTF8 = 0x08000100;

        /// <summary>kCTFontURLAttribute</summary>
        private const string KCTFontURLAttribute = "CTFontURLAttribute";

        [DllImport(CoreTextLibrary)]
        private static extern IntPtr CTFontManagerCopyAvailableFontFamilyNames();

        [DllImport(CoreTextLibrary)]
        private static extern IntPtr CTFontCreateWithName(IntPtr name, double size, IntPtr matrix);

        [DllImport(CoreTextLibrary)]
        private static extern IntPtr CTFontCopyAttribute(IntPtr font, IntPtr attribute);

        [DllImport(CoreFoundationLibrary)]
        private static extern long CFArrayGetCount(IntPtr array);

        [DllImport(CoreFoundationLibrary)]
        private static extern IntPtr CFArrayGetValueAtIndex(IntPtr array, long idx);

        [DllImport(CoreFoundationLibrary)]
        private static extern long CFStringGetLength(IntPtr theString);

        [DllImport(CoreFoundationLibrary)]
        private static extern byte CFStringGetCString(IntPtr theString, byte[] buffer, long bufferSize, int encoding);

        [DllImport(CoreFoundationLibrary)]
        private static extern byte CFURLGetFileSystemRepresentation(IntPtr url, byte resolveAgainstBase, byte[] buffer, long bufferLength);

        [DllImport(CoreFoundationLibrary)]
        private static extern void CFRelease(IntPtr cf);

        /// <summary>
        /// Tries to discover installed fonts using CoreText.
        /// </summary>
        public static bool TryDiscoverFonts(out IReadOnlyList<NativeFontInfo> fonts)
        {
            var results = new List<NativeFontInfo>();

            try
            {
                IntPtr familyNames = CTFontManagerCopyAvailableFontFamilyNames();
                if (familyNames == IntPtr.Zero)
                {
                    fonts = Array.Empty<NativeFontInfo>();
                    return false;
                }

                try
                {
                    long count = CFArrayGetCount(familyNames);
                    for (long i = 0; i < count; i++)
                    {
                        IntPtr familyNameRef = CFArrayGetValueAtIndex(familyNames, i);
                        string? familyName = CFStringToString(familyNameRef);
                        if (string.IsNullOrEmpty(familyName))
                            continue;

                        // Resolve the file path for the first face in this family.
                        string? filePath = ResolveFontPath(familyName);
                        if (!string.IsNullOrEmpty(filePath))
                            results.Add(new NativeFontInfo(familyName!, filePath));
                    }
                }
                finally
                {
                    CFRelease(familyNames);
                }
            }
            catch
            {
                // CoreText may be unavailable in some sandboxed environments.
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
        /// Tries to get the macOS default UI font family name. On modern macOS this is
        /// "SF Pro" (San Francisco); on older versions it is "Helvetica Neue".
        /// </summary>
        public static bool TryGetDefaultFamilyName(out string? familyName)
        {
            // CoreText does not expose a direct "system font" query via simple P/Invoke.
            // The system font on macOS 10.11+ is ".AppleSystemUIFont" which resolves to
            // "SF Pro Text"/"SF Pro Display". We try that alias first, then fall back to
            // "Helvetica Neue".
            try
            {
                IntPtr font = CTFontCreateWithName(CreateCFString(".AppleSystemUIFont"), 12.0, IntPtr.Zero);
                if (font != IntPtr.Zero)
                {
                    try
                    {
                        // The family name of the system font is "System Font" on recent macOS.
                        // We return a known-good default instead.
                        familyName = "Helvetica Neue";
                        return true;
                    }
                    finally
                    {
                        CFRelease(font);
                    }
                }
            }
            catch
            {
                // CoreText unavailable.
            }

            familyName = null;
            return false;
        }

        private static string? ResolveFontPath(string familyName)
        {
            IntPtr nameRef = CreateCFString(familyName);
            if (nameRef == IntPtr.Zero)
                return null;

            try
            {
                IntPtr font = CTFontCreateWithName(nameRef, 12.0, IntPtr.Zero);
                if (font == IntPtr.Zero)
                    return null;

                try
                {
                    IntPtr attrNameRef = CreateCFString(KCTFontURLAttribute);
                    if (attrNameRef == IntPtr.Zero)
                        return null;

                    try
                    {
                        IntPtr url = CTFontCopyAttribute(font, attrNameRef);
                        if (url == IntPtr.Zero)
                            return null;

                        try
                        {
                            var buffer = new byte[1024];
                            if (CFURLGetFileSystemRepresentation(url, 1, buffer, buffer.Length) != 0)
                            {
                                int nullIndex = Array.IndexOf(buffer, (byte)0);
                                int length = nullIndex >= 0 ? nullIndex : buffer.Length;
                                return Encoding.UTF8.GetString(buffer, 0, length);
                            }
                        }
                        finally
                        {
                            CFRelease(url);
                        }
                    }
                    finally
                    {
                        CFRelease(attrNameRef);
                    }
                }
                finally
                {
                    CFRelease(font);
                }
            }
            finally
            {
                CFRelease(nameRef);
            }

            return null;
        }

        private static string? CFStringToString(IntPtr cfString)
        {
            if (cfString == IntPtr.Zero)
                return null;

            // Try a generous buffer first; CFStringGetCString truncates if too small.
            var buffer = new byte[512];
            if (CFStringGetCString(cfString, buffer, buffer.Length, KCFStringEncodingUTF8) != 0)
            {
                int nullIndex = Array.IndexOf(buffer, (byte)0);
                int length = nullIndex >= 0 ? nullIndex : buffer.Length;
                return Encoding.UTF8.GetString(buffer, 0, length);
            }

            // Fallback: allocate a larger buffer based on the string length.
            long lengthInUTF16 = CFStringGetLength(cfString);
            int maxBytes = (int)(lengthInUTF16 * 4) + 1;
            if (maxBytes > 65536)
                maxBytes = 65536;

            var bigBuffer = new byte[maxBytes];
            if (CFStringGetCString(cfString, bigBuffer, bigBuffer.Length, KCFStringEncodingUTF8) != 0)
            {
                int nullIndex = Array.IndexOf(bigBuffer, (byte)0);
                int length = nullIndex >= 0 ? nullIndex : bigBuffer.Length;
                return Encoding.UTF8.GetString(bigBuffer, 0, length);
            }

            return null;
        }

        /// <summary>
        /// Creates a CoreFoundation CFString from a .NET string. The caller must
        /// <see cref="CFRelease"/> the result.
        /// </summary>
        private static IntPtr CreateCFString(string str)
        {
            // We use CFStringCreateWithBytes for reliable UTF-8 creation.
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return CFStringCreateWithBytes(IntPtr.Zero, bytes, bytes.Length, KCFStringEncodingUTF8, 0);
        }

        [DllImport(CoreFoundationLibrary)]
        private static extern IntPtr CFStringCreateWithBytes(
            IntPtr alloc, byte[] bytes, long numBytes, int encoding, byte isExternalRepresentation);
    }
}