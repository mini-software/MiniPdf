using System.Text;

namespace MiniSoftware;

/// <summary>
/// Cross-platform compatibility helpers that work on both .NET Framework and .NET 6+.
/// </summary>
internal static class Compat
{
    // ── Math.Clamp (introduced in .NET Core 2.0) ──
    public static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(value, max));
    public static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(value, max));
    public static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(value, max));

    // ── MathF.Ceiling (introduced in .NET Core 2.0) ──
    public static float Ceiling(float value) => (float)Math.Ceiling(value);

    // ── Encoding.Latin1 (property introduced in .NET 5) ──
    public static readonly Encoding Latin1 = Encoding.GetEncoding(28591);

    // ── OperatingSystem.IsWindows / IsMacOS (introduced in .NET 5) ──
    public static bool IsWindows()
    {
#if NETFRAMEWORK
        return true; // .NET Framework only runs on Windows
#elif NETSTANDARD2_0
        return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows);
#else
        return OperatingSystem.IsWindows();
#endif
    }

    public static bool IsMacOS()
    {
#if NETFRAMEWORK
        return false;
#elif NETSTANDARD2_0
        return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.OSX);
#else
        return OperatingSystem.IsMacOS();
#endif
    }

    // ── Array.Fill (introduced in .NET Core 2.0) ──
    public static void ArrayFill<T>(T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++) array[i] = value;
    }

    // ── HashCode.Combine (introduced in .NET Core 2.1) ──
    public static int HashCombine<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + (v1?.GetHashCode() ?? 0);
            h = h * 31 + (v2?.GetHashCode() ?? 0);
            h = h * 31 + (v3?.GetHashCode() ?? 0);
            return h;
        }
    }
}
