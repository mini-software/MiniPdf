#if NETFRAMEWORK || NETSTANDARD2_0
// Polyfill types and extension methods for .NET Framework 4.6.2 / .NET Standard 2.0 compatibility.
// These are automatically available on .NET 6+ and are excluded via #if.

using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

// ── IsExternalInit: required by C# 9 records and init-only properties ──
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

// ── Index / Range: required by C# 8 range operators (e.g. str[1..], arr[^1]) ──
namespace System
{
    internal readonly struct Index : IEquatable<Index>
    {
        private readonly int _value;

        public Index(int value, bool fromEnd = false)
        {
            _value = fromEnd ? ~value : value;
        }

        public int Value => _value < 0 ? ~_value : _value;
        public bool IsFromEnd => _value < 0;

        public static Index Start => new Index(0);
        public static Index End => new Index(~0);

        public int GetOffset(int length)
        {
            var offset = _value;
            if (IsFromEnd) offset += length;
            return offset;
        }

        public static implicit operator Index(int value) => new Index(value);
        public bool Equals(Index other) => _value == other._value;
        public override bool Equals(object? obj) => obj is Index other && Equals(other);
        public override int GetHashCode() => _value;
        public override string ToString() => IsFromEnd ? $"^{Value}" : Value.ToString();
    }

    internal readonly struct Range : IEquatable<Range>
    {
        public Index Start { get; }
        public Index End { get; }

        public Range(Index start, Index end) { Start = start; End = end; }

        public static Range StartAt(Index start) => new Range(start, Index.End);
        public static Range EndAt(Index end) => new Range(Index.Start, end);
        public static Range All => new Range(Index.Start, Index.End);

        public (int Offset, int Length) GetOffsetAndLength(int length)
        {
            var start = Start.GetOffset(length);
            var end = End.GetOffset(length);
            return (start, end - start);
        }

        public bool Equals(Range other) => Start.Equals(other.Start) && End.Equals(other.End);
        public override bool Equals(object? obj) => obj is Range other && Equals(other);
        public override int GetHashCode() => Start.GetHashCode() * 31 + End.GetHashCode();
        public override string ToString() => $"{Start}..{End}";
    }
}

// ── RuntimeHelpers.GetSubArray: required when using range operators on arrays ──
namespace System.Runtime.CompilerServices
{
    internal static class RuntimeHelpersPolyfill
    {
        // The compiler calls RuntimeHelpers.GetSubArray<T> for array[range].
        // We provide it here so that pattern compiles on .NET Framework.
    }
}

namespace System
{
    internal static class RuntimeHelpersEx
    {
        public static T[] GetSubArray<T>(T[] array, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(array.Length);
            var dest = new T[length];
            Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
    }
}

// ── Extension methods for .NET Framework ──
namespace MiniSoftware
{
    internal static class NetFxPolyfills
    {
        // string.Contains(string, StringComparison) — .NET Core 2.1+
        public static bool Contains(this string s, string value, StringComparison comparison)
            => s.IndexOf(value, comparison) >= 0;

        // string.Split(char, StringSplitOptions) — .NET Core 2.1+
        public static string[] Split(this string s, char separator, StringSplitOptions options)
            => s.Split(new[] { separator }, options);

        // string.StartsWith(char) — .NET Core only
        public static bool StartsWith(this string s, char c)
            => s.Length > 0 && s[0] == c;

        // string.EndsWith(char) — .NET Core only
        public static bool EndsWith(this string s, char c)
            => s.Length > 0 && s[s.Length - 1] == c;

        // Stream.Write(byte[]) — Stream.Write(ReadOnlySpan<byte>) is .NET Core 2.1+
        public static void Write(this Stream stream, byte[] buffer)
            => stream.Write(buffer, 0, buffer.Length);

        // KeyValuePair Deconstruct — .NET Core 2.0+
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        // Math.Clamp — .NET Core 2.0+
        public static int Clamp(int value, int min, int max) =>
            value < min ? min : value > max ? max : value;
        public static float Clamp(float value, float min, float max) =>
            value < min ? min : value > max ? max : value;
        public static double Clamp(double value, double min, double max) =>
            value < min ? min : value > max ? max : value;

        // Array.Fill — .NET Core 2.0+
        public static void Fill<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++) array[i] = value;
        }
        public static void Fill<T>(T[] array, T value, int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++) array[i] = value;
        }

        // Encoding.Latin1 — .NET 5+
        public static Encoding Latin1 => Encoding.GetEncoding(28591);
    }
}
#endif
