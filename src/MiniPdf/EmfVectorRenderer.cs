using System.IO.Compression;

namespace MiniSoftware;

// Owned MS-EMF subset used by portable conversion. Unsupported records fail explicitly.
internal sealed class EmfVectorRenderer
{
    private readonly byte[] source;
    private readonly int width;
    private readonly int height;
    private readonly byte[] pixels;
    private readonly Dictionary<uint, DrawingObject> objects = new();
    private readonly Dictionary<uint, MetafileFont> fonts = new();
    private readonly Func<string, bool, bool, byte[]>? fontResolver;
    private readonly List<DeviceState> savedStates = new();
    private readonly double left;
    private readonly double top;
    private readonly double scaleX;
    private readonly double scaleY;
    private readonly int handleCount;
    private DeviceState state;
    private List<PathFigure>? path;
    private PathFigure? activeFigure;
    private bool buildingPath;
    private int pathPointCount;
    private int recordEnd;
    private int commentGroups;
    private bool plusStarted, plusEnded, plusDc, plusUsedDc;
    private WorldTransform plusWorld = WorldTransform.Identity;
    private long remainingWork = 64_000_000;

    private readonly struct DrawingObject
    {
        internal readonly bool IsPen;
        internal readonly bool IsNull;
        internal readonly uint Color;
        internal readonly int Width;

        internal DrawingObject(bool isPen, uint color, bool isNull = false, int width = 0)
        {
            IsPen = isPen;
            Color = color;
            IsNull = isNull;
            Width = width;
        }
    }

    private sealed class DeviceState
    {
        internal DrawingObject Pen = new(true, 0);
        internal DrawingObject Brush = new(false, 0xFFFFFF);
        internal int MapMode = 1;
        internal int StretchMode = 1;
        internal uint RasterOperation = 13;
        internal int WindowX, WindowY, ViewportX, ViewportY;
        internal int WindowWidth = 1, WindowHeight = 1, ViewportWidth = 1, ViewportHeight = 1;
        internal int CurrentX, CurrentY;
        internal int ClipLeft, ClipTop, ClipRight, ClipBottom;
        internal bool Winding;
        internal WorldTransform World = WorldTransform.Identity;
        internal MetafileFont? Font;
        internal uint TextColor, BackgroundColor = 0xFFFFFF;
        internal int BackgroundMode = 2;
        internal uint TextAlignment;
        internal DeviceState Copy() => (DeviceState)MemberwiseClone();
    }

    private sealed class MetafileFont
    {
        internal readonly string Name;
        internal readonly double Height;
        internal readonly int Width;
        internal readonly bool Bold, Italic;
        internal TrueTypeGlyphReader? Outlines;
        internal MetafileFont(string name, double height, int width, bool bold, bool italic)
        {
            Name = name; Height = height; Width = width; Bold = bold; Italic = italic;
        }
    }

    private readonly struct WorldTransform
    {
        internal readonly double M11, M12, M21, M22, X, Y;
        internal static WorldTransform Identity => new(1, 0, 0, 1, 0, 0);

        internal WorldTransform(double m11, double m12, double m21, double m22, double x, double y)
        {
            var determinant = m11 * m22 - m12 * m21;
            if (double.IsNaN(determinant) || double.IsInfinity(determinant) || determinant == 0 ||
                double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
                throw new InvalidDataException("Invalid or singular EMF world transform.");
            M11 = m11; M12 = m12; M21 = m21; M22 = m22; X = x; Y = y;
        }

        internal static WorldTransform Multiply(WorldTransform a, WorldTransform b) => new(
            a.M11 * b.M11 + a.M12 * b.M21, a.M11 * b.M12 + a.M12 * b.M22,
            a.M21 * b.M11 + a.M22 * b.M21, a.M21 * b.M12 + a.M22 * b.M22,
            a.X * b.M11 + a.Y * b.M21 + b.X, a.X * b.M12 + a.Y * b.M22 + b.Y);
    }

    private sealed class PathFigure
    {
        internal readonly List<(double X, double Y)> Points;
        internal bool Closed;

        internal PathFigure(IReadOnlyList<(double X, double Y)> points, bool closed)
        {
            Points = new List<(double X, double Y)>(points);
            Closed = closed;
        }
    }

    private EmfVectorRenderer(byte[] source, int width, int height, Func<string, bool, bool, byte[]>? fontResolver)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (width < 1 || width > 4096) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1 || height > 4096) throw new ArgumentOutOfRangeException(nameof(height));
        this.source = source;
        this.width = width;
        this.height = height;
        this.fontResolver = fontResolver;
        recordEnd = source.Length;
        if (source.Length < 88 || U32(0) != 1 || U32(40) != 0x464D4520 || U32(44) != 0x10000)
            throw new InvalidDataException("Invalid EMF header.");
        if (U32(48) != source.Length || U32(4) < 88 || U32(4) > source.Length)
            throw new InvalidDataException("Invalid EMF byte count or header size.");
        left = I32(8);
        top = I32(12);
        var boundsWidth = (long)I32(16) - left;
        var boundsHeight = (long)I32(20) - top;
        var frameWidth = (long)I32(32) - I32(24);
        var frameHeight = (long)I32(36) - I32(28);
        var deviceWidth = I32(72);
        var deviceHeight = I32(76);
        var millimetersWidth = I32(80);
        var millimetersHeight = I32(84);
        if (frameWidth > 0 && frameHeight > 0 && deviceWidth > 0 && deviceHeight > 0 &&
            millimetersWidth > 0 && millimetersHeight > 0)
        {
            if (boundsWidth < 0 || boundsHeight < 0)
                throw new InvalidDataException("EMF drawing bounds are inverted.");
            // The physical frame defines placement; sparse ink bounds are not the canvas.
            var frameUnitsPerPixelX = millimetersWidth * 100d / deviceWidth;
            var frameUnitsPerPixelY = millimetersHeight * 100d / deviceHeight;
            left = I32(24) / frameUnitsPerPixelX;
            top = I32(28) / frameUnitsPerPixelY;
            scaleX = width * frameUnitsPerPixelX / frameWidth;
            scaleY = height * frameUnitsPerPixelY / frameHeight;
        }
        else
        {
            if (boundsWidth <= 0 || boundsHeight <= 0)
                throw new InvalidDataException("EMF bounds must have positive dimensions when reference device metrics are unavailable.");
            scaleX = width / (double)boundsWidth;
            scaleY = height / (double)boundsHeight;
        }
        handleCount = U16(56);
        if (handleCount < 1) throw new InvalidDataException("Invalid EMF handle count.");
        if (U32(68) != 0) throw new NotSupportedException("EMF palettes are not supported.");
        pixels = new byte[checked(width * height * 4)];
        state = new DeviceState { ClipRight = width, ClipBottom = height };
    }

    internal static byte[] Render(byte[] source, int width, int height, Func<string, bool, bool, byte[]>? fontResolver = null)
        => RenderCropped(source, width, height, fontResolver, 0, 0, 0, 0);

    internal static byte[] RenderCropped(byte[] source, int width, int height, Func<string, bool, bool, byte[]>? fontResolver,
        float cropLeft, float cropTop, float cropRight, float cropBottom)
    {
        if (float.IsNaN(cropLeft) || float.IsInfinity(cropLeft) || float.IsNaN(cropTop) || float.IsInfinity(cropTop) ||
            float.IsNaN(cropRight) || float.IsInfinity(cropRight) || float.IsNaN(cropBottom) || float.IsInfinity(cropBottom))
            throw new ArgumentException("Metafile crop values must be finite.");
        var renderer = new EmfVectorRenderer(source, width, height, fontResolver);
        renderer.Play();
        return renderer.EncodePng(cropLeft, cropTop, cropRight, cropBottom);
    }

    private void Play()
    {
        var offset = 0;
        var count = 0u;
        while (offset < source.Length)
        {
            recordEnd = source.Length;
            var type = U32(offset);
            var size = U32(offset + 4);
            if (size < 8 || size % 4 != 0 || size > source.Length - offset)
                throw new InvalidDataException($"Invalid EMF record size at {offset}.");
            recordEnd = offset + (int)size;
            count++;
            if (offset == 0)
            {
                if (type != 1) throw new InvalidDataException("EMF must begin with a header.");
            }
            else
            {
                if (plusStarted && !plusDc && type != 70 && type != 14)
                {
                    offset = recordEnd;
                    continue;
                }
                switch (type)
                {
                    case 14:
                        Require(offset, 20);
                        if (plusStarted && !plusEnded)
                            throw new InvalidDataException("Missing EMF+ EOF record.");
                        if (commentGroups != 0)
                            throw new InvalidDataException("Unclosed EMF comment group.");
                        if (U32(offset + 8) != 0)
                            throw new NotSupportedException("EMF EOF palettes are not supported.");
                        if (recordEnd != source.Length || count != U32(52))
                            throw new InvalidDataException("Invalid EMF EOF or record count.");
                        return;
                    case 9:
                        state.WindowWidth = Nonzero(I32(offset + 8));
                        state.WindowHeight = Nonzero(I32(offset + 12));
                        break;
                    case 10:
                        state.WindowX = I32(offset + 8);
                        state.WindowY = I32(offset + 12);
                        break;
                    case 11:
                        state.ViewportWidth = Nonzero(I32(offset + 8));
                        state.ViewportHeight = Nonzero(I32(offset + 12));
                        break;
                    case 12:
                        state.ViewportX = I32(offset + 8);
                        state.ViewportY = I32(offset + 12);
                        break;
                    case 15:
                        var point = Map(I32(offset + 8), I32(offset + 12));
                        var color = U32(offset + 16);
                        CheckColor(color);
                        if (point.X >= state.ClipLeft && point.X < state.ClipRight &&
                            point.Y >= state.ClipTop && point.Y < state.ClipBottom && state.RasterOperation != 11)
                            PutPixel((int)Math.Floor(point.X), (int)Math.Floor(point.Y), RasterColor(color));
                        break;
                    case 17:
                        var mode = I32(offset + 8);
                        if (mode != 1 && mode != 8)
                            throw new NotSupportedException($"EMF mapping mode {mode} is not supported.");
                        state.MapMode = mode;
                        break;
                    case 19:
                        var fillMode = I32(offset + 8);
                        if (fillMode != 1 && fillMode != 2)
                            throw new InvalidDataException("Invalid EMF polygon fill mode.");
                        state.Winding = fillMode == 2;
                        break;
                    case 18:
                        var backgroundMode = I32(offset + 8);
                        if (backgroundMode != 1 && backgroundMode != 2)
                            throw new InvalidDataException("Invalid EMF background mode.");
                        state.BackgroundMode = backgroundMode;
                        break;
                    case 20:
                        var rasterOperation = U32(offset + 8);
                        // Native EMF playback treats the recorded zero ROP2 as white, not as a no-op.
                        if (rasterOperation == 0) rasterOperation = 16;
                        if (rasterOperation is not (1 or 4 or 11 or 13 or 16))
                            throw new NotSupportedException("This EMF raster operation is not supported.");
                        state.RasterOperation = rasterOperation;
                        break;
                    case 21:
                        var stretchMode = I32(offset + 8);
                        if (stretchMode < 1 || stretchMode > 4)
                            throw new InvalidDataException("Invalid EMF bitmap stretch mode.");
                        state.StretchMode = stretchMode;
                        break;
                    case 22:
                        var alignment = U32(offset + 8);
                        if ((alignment & ~0x11Fu) != 0 || (alignment & 6) == 4 || (alignment & 24) == 16)
                            throw new InvalidDataException("Invalid EMF text alignment.");
                        state.TextAlignment = alignment;
                        break;
                    case 24:
                    case 25:
                        var textColor = U32(offset + 8);
                        CheckColor(textColor);
                        if (type == 24) state.TextColor = textColor;
                        else state.BackgroundColor = textColor;
                        break;
                    case 27:
                        state.CurrentX = I32(offset + 8);
                        state.CurrentY = I32(offset + 12);
                        if (buildingPath) activeFigure = null;
                        break;
                    case 30:
                        IntersectClip(offset);
                        break;
                    case 33:
                        if (path != null)
                            throw new NotSupportedException("Saving a device context with a pending EMF path is not supported.");
                        if (savedStates.Count >= 1024)
                            throw new InvalidDataException("EMF saved device context limit exceeded.");
                        savedStates.Add(state.Copy());
                        break;
                    case 34:
                        Restore(I32(offset + 8));
                        break;
                    case 35:
                        state.World = ReadWorldTransform(offset + 8);
                        break;
                    case 36:
                        ModifyWorldTransform(offset);
                        break;
                    case 37:
                        Select(U32(offset + 8));
                        break;
                    case 38:
                        var penStyle = U32(offset + 12);
                        var penWidth = I32(offset + 16);
                        Require(offset, 28);
                        if (penStyle != 0 && penStyle != 5)
                            throw new NotSupportedException("Only solid or null EMF pens are supported.");
                        if (penWidth < 0)
                            throw new InvalidDataException("Negative EMF pen width.");
                        if (penWidth > 1)
                            throw new NotSupportedException("Wide EMF pens are not supported.");
                        AddObject(U32(offset + 8), new DrawingObject(true, U32(offset + 24), penStyle == 5, penWidth));
                        break;
                    case 39:
                        var brushStyle = U32(offset + 12);
                        Require(offset, 24);
                        if (brushStyle != 0 && brushStyle != 1)
                            throw new NotSupportedException("Only solid or null EMF brushes are supported.");
                        AddObject(U32(offset + 8), new DrawingObject(false, U32(offset + 16), brushStyle == 1));
                        break;
                    case 40:
                        var deletedHandle = U32(offset + 8);
                        if (!objects.Remove(deletedHandle) && !fonts.Remove(deletedHandle))
                            throw new InvalidDataException("Deleting an unknown EMF object.");
                        break;
                    case 82:
                        CreateFont(offset);
                        break;
                    case 84:
                        DrawText(offset);
                        break;
                    case 43:
                        Rectangle(offset);
                        break;
                    case 48:
                        if (U32(offset + 8) != 0x8000000F)
                            throw new NotSupportedException("Only the stock default EMF palette is supported.");
                        break;
                    case 54:
                        var x = I32(offset + 8);
                        var y = I32(offset + 12);
                        EmitFigure(new[] { CurrentPathPoint(), Map(x, y) }, false, true);
                        state.CurrentX = x;
                        state.CurrentY = y;
                        break;
                    case 59:
                        path = new List<PathFigure>();
                        activeFigure = null;
                        pathPointCount = 0;
                        buildingPath = true;
                        break;
                    case 60:
                        RequireOpenPath();
                        buildingPath = false;
                        activeFigure = null;
                        break;
                    case 61:
                        RequireOpenPath();
                        if (activeFigure == null) throw new InvalidDataException("No open EMF path figure to close.");
                        activeFigure.Closed = true;
                        activeFigure = null;
                        break;
                    case 62:
                    case 63:
                    case 64:
                        Require(offset, 24);
                        PaintPath(type != 64, type != 62);
                        break;
                    case 68:
                        ClearPath();
                        break;
                    case 70:
                        ReadComment(offset);
                        break;
                    case 75:
                        SelectClipRegion(offset);
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 85:
                    case 86:
                    case 87:
                    case 88:
                    case 89:
                        Poly(offset, type);
                        break;
                    case 81:
                    case 114:
                        DrawBitmap(offset, type == 114);
                        break;
                    default:
                        throw new NotSupportedException($"EMF record {type} at {offset} is not supported.");
                }
            }
            offset = recordEnd;
        }
        throw new InvalidDataException("Missing EMF EOF record.");
    }

    private void Poly(int offset, uint type)
    {
        var count = U32(offset + 24);
        var shortCoordinates = type >= 85;
        var continues = type is 5 or 6 or 88 or 89;
        var bezier = type is 2 or 5 or 85 or 88;
        var stride = shortCoordinates ? 4 : 8;
        if (count < (continues ? 1 : 2) || count > 1_000_000 || count > (recordEnd - offset - 28) / stride ||
            (bezier && (continues ? count % 3 != 0 : count < 4 || count % 3 != 1)))
            throw new InvalidDataException("Invalid EMF point count.");
        Spend(count);
        var points = new (double X, double Y)[(int)count + (continues ? 1 : 0)];
        if (continues) points[0] = CurrentPathPoint();
        var lastX = 0;
        var lastY = 0;
        for (var index = 0; index < count; index++)
        {
            var position = offset + 28 + index * stride;
            lastX = shortCoordinates ? (short)U16(position) : I32(position);
            lastY = shortCoordinates ? (short)U16(position + 2) : I32(position + 4);
            points[index + (continues ? 1 : 0)] = Map(lastX, lastY);
        }
        var closed = type == 3 || type == 86;
        EmitFigure(bezier ? FlattenBeziers(points) : points, closed, continues);
        if (continues)
        {
            state.CurrentX = lastX;
            state.CurrentY = lastY;
        }
    }

    private void Rectangle(int offset)
    {
        var x1 = I32(offset + 8);
        var y1 = I32(offset + 12);
        var x2 = I32(offset + 16);
        var y2 = I32(offset + 20);
        if (x2 <= x1 || y2 <= y1) return;
        var points = new[] { Map(x1, y1), Map(x2, y1), Map(x2, y2), Map(x1, y2) };
        if (buildingPath)
        {
            EmitFigure(points, true, false);
            return;
        }
        Fill(points);
        points = new[] { Map(x1, y1), Map(x2 - 1, y1), Map(x2 - 1, y2 - 1), Map(x1, y2 - 1) };
        for (var i = 0; i < 4; i++) Stroke(points[i], points[(i + 1) % 4]);
    }

    private void Fill(IReadOnlyList<(double X, double Y)> points, bool applyRasterOperation = true) =>
        FillFigures(new[] { points }, applyRasterOperation);

    private void FillFigures(IReadOnlyList<IReadOnlyList<(double X, double Y)>> figures, bool applyRasterOperation = true)
    {
        if (state.Brush.IsNull || figures.Count == 0 || (applyRasterOperation && state.RasterOperation == 11)) return;
        var color = applyRasterOperation ? RasterColor(state.Brush.Color) : state.Brush.Color;
        var pointCount = figures.Sum(figure => figure.Count);
        if (pointCount == 0) return;
        var minY = figures.SelectMany(figure => figure).Min(point => point.Y);
        var maxY = figures.SelectMany(figure => figure).Max(point => point.Y);
        var firstRow = Bound(minY, state.ClipTop, state.ClipBottom);
        var lastRow = Bound(maxY + 1, state.ClipTop, state.ClipBottom);
        var intersections = new List<(double X, int Direction)>();
        for (var row = firstRow; row < lastRow; row++)
        {
            Spend(pointCount);
            intersections.Clear();
            var sampleY = row + 0.5;
            foreach (var points in figures)
            {
                for (var index = 0; index < points.Count; index++)
                {
                    var a = points[index];
                    var b = points[(index + 1) % points.Count];
                    if ((a.Y <= sampleY && b.Y > sampleY) || (b.Y <= sampleY && a.Y > sampleY))
                        intersections.Add((a.X + (sampleY - a.Y) / (b.Y - a.Y) * (b.X - a.X), b.Y > a.Y ? 1 : -1));
                }
            }
            intersections.Sort((a, b) => a.X.CompareTo(b.X));
            var winding = 0;
            for (var index = 0; index + 1 < intersections.Count; index++)
            {
                winding += intersections[index].Direction;
                var inside = state.Winding ? winding != 0 : (index & 1) == 0;
                if (!inside) continue;
                var start = Bound(Math.Ceiling(intersections[index].X - 0.5), state.ClipLeft, state.ClipRight);
                var end = Bound(Math.Ceiling(intersections[index + 1].X - 0.5), state.ClipLeft, state.ClipRight);
                Spend(end - start);
                for (var column = start; column < end; column++) PutPixel(column, row, color);
            }
        }
    }

    private void Stroke((double X, double Y) a, (double X, double Y) b)
    {
        if (state.Pen.IsNull || state.RasterOperation == 11) return;
        if (state.Pen.Width == 0)
        {
            StrokeCosmetic(a, b);
            return;
        }
        if (state.Pen.Width > 0 && (state.World.M11 != 1 || state.World.M12 != 0 ||
            state.World.M21 != 0 || state.World.M22 != 1))
            throw new NotSupportedException("World-transformed noncosmetic EMF pens are not supported.");
        var penScaleX = scaleX;
        var penScaleY = scaleY;
        if (state.MapMode == 8 && state.Pen.Width > 0)
        {
            penScaleX *= Math.Abs((double)state.ViewportWidth / state.WindowWidth);
            penScaleY *= Math.Abs((double)state.ViewportHeight / state.WindowHeight);
        }
        if (state.Pen.Width * Math.Max(penScaleX, penScaleY) > 1)
            throw new NotSupportedException("Scaled EMF pens wider than one device pixel are not supported.");
        StrokeCosmetic(a, b);
    }

    private void StrokeCosmetic((double X, double Y) a, (double X, double Y) b)
    {
        if (state.ClipLeft >= state.ClipRight || state.ClipTop >= state.ClipBottom) return;
        if (Math.Abs(a.X) > 1e12 || Math.Abs(a.Y) > 1e12 || Math.Abs(b.X) > 1e12 || Math.Abs(b.Y) > 1e12)
            throw new InvalidDataException("EMF cosmetic stroke coordinate precision limit exceeded.");
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var first = 0d;
        var last = 1d;
        if (!Clip(-dx, a.X - state.ClipLeft + 1) || !Clip(dx, state.ClipRight - a.X) ||
            !Clip(-dy, a.Y - state.ClipTop + 1) || !Clip(dy, state.ClipBottom - a.Y)) return;
        var x = Bound(Math.Floor(a.X + first * dx + 0.5), state.ClipLeft - 1, state.ClipRight);
        var y = Bound(Math.Floor(a.Y + first * dy + 0.5), state.ClipTop - 1, state.ClipBottom);
        var endX = Bound(Math.Floor(a.X + last * dx + 0.5), state.ClipLeft - 1, state.ClipRight);
        var endY = Bound(Math.Floor(a.Y + last * dy + 0.5), state.ClipTop - 1, state.ClipBottom);
        var stepX = x < endX ? 1 : -1;
        var stepY = y < endY ? 1 : -1;
        var lengthX = Math.Abs(endX - x);
        var lengthY = Math.Abs(endY - y);
        Spend((long)Math.Max(lengthX, lengthY) + 1);
        var error = lengthX - lengthY;
        while (true)
        {
            if (x == endX && y == endY)
            {
                if (last < 1) PutPixel(x, y, RasterColor(state.Pen.Color));
                return;
            }
            PutPixel(x, y, RasterColor(state.Pen.Color));
            var twiceError = error * 2;
            if (twiceError > -lengthY) { error -= lengthY; x += stepX; }
            if (twiceError < lengthX) { error += lengthX; y += stepY; }
        }

        bool Clip(double direction, double distance)
        {
            if (direction == 0) return distance >= 0;
            var fraction = distance / direction;
            if (direction < 0)
            {
                if (fraction > last) return false;
                first = Math.Max(first, fraction);
            }
            else
            {
                if (fraction < first) return false;
                last = Math.Min(last, fraction);
            }
            return true;
        }
    }

    private (double X, double Y) Map(double x, double y)
    {
        var transformedX = x * state.World.M11 + y * state.World.M21 + state.World.X;
        y = x * state.World.M12 + y * state.World.M22 + state.World.Y;
        x = transformedX;
        var deviceX = (double)x;
        var deviceY = (double)y;
        if (state.MapMode == 8)
        {
            deviceX = ((double)x - state.WindowX) * state.ViewportWidth / state.WindowWidth + state.ViewportX;
            deviceY = ((double)y - state.WindowY) * state.ViewportHeight / state.WindowHeight + state.ViewportY;
        }
        else
        {
            deviceX += (double)state.ViewportX - state.WindowX;
            deviceY += (double)state.ViewportY - state.WindowY;
        }
        return MapDevice(deviceX, deviceY);
    }

    private (double X, double Y) MapDevice(double x, double y)
    {
        var mapped = ((x - left) * scaleX, (y - top) * scaleY);
        if (double.IsNaN(mapped.Item1) || double.IsNaN(mapped.Item2) ||
            Math.Abs(mapped.Item1) > 1e50 || Math.Abs(mapped.Item2) > 1e50)
            throw new InvalidDataException("EMF mapped coordinate limit exceeded.");
        return mapped;
    }

    private void ReadComment(int offset)
    {
        var length = U32(offset + 8);
        var available = recordEnd - offset - 12;
        if (length > available || available - length > 3)
            throw new InvalidDataException("Invalid EMF comment data length.");
        if (length < 4) return;
        var identifier = U32(offset + 12);
        if (identifier == 0x2B464D45)
        {
            ReadGdiOnlyPlusComment(offset + 16, offset + 12 + (int)length);
            return;
        }
        if (identifier == 0)
            throw new NotSupportedException("EMFSPOOL comments are not supported.");
        if (identifier != 0x43494447) return;
        if (length < 8) throw new InvalidDataException("Truncated public EMF comment.");
        switch (U32(offset + 16))
        {
            case 2:
                if (length < 28 || U32(offset + 36) > (length - 28) / 2)
                    throw new InvalidDataException("Invalid EMF comment group description.");
                if (commentGroups >= 1024) throw new InvalidDataException("EMF comment group nesting limit exceeded.");
                commentGroups++;
                break;
            case 3:
                if (length != 8 || commentGroups == 0)
                    throw new InvalidDataException("Invalid EMF comment group end.");
                commentGroups--;
                break;
            case 0x80000001:
                if (length < 24 || U32(offset + 32) != length - 24)
                    throw new InvalidDataException("Invalid original WMF comment length.");
                if (U32(offset + 28) != 0)
                    throw new NotSupportedException("Original WMF comment flags are not supported.");
                // This is an equivalent original WMF, not additional drawing commands.
                break;
            default:
                throw new NotSupportedException("This public EMF comment extension is not supported.");
        }
    }

    private void ReadGdiOnlyPlusComment(int position, int end)
    {
        if (position == end) throw new InvalidDataException("Empty EMF+ comment.");
        if (path != null) throw new NotSupportedException("An EMF path cannot cross an EMF+ comment.");
        plusDc = false;
        while (position < end)
        {
            Spend(1);
            if (position > end - 12) throw new InvalidDataException("Truncated EMF+ record header.");
            var type = U16(position);
            var flags = U16(position + 2);
            var size = U32(position + 4);
            var dataSize = U32(position + 8);
            if (size < 12 || size % 4 != 0 || size > end - position || dataSize > size - 12 ||
                ((long)dataSize + 3) / 4 * 4 != size - 12)
                throw new InvalidDataException("Invalid EMF+ record size.");
            if (plusEnded || (!plusStarted && type != 0x4001))
                throw new InvalidDataException("Invalid EMF+ record order.");
            switch (type)
            {
                case 0x4001:
                    if (plusStarted || dataSize != 16 || (flags & ~1) != 0 ||
                        (U32(position + 12) & 0xFFFFF000) != 0xDBC01000 ||
                        (U32(position + 12) & 0xFFF) is not (1 or 2) ||
                        (U32(position + 16) & ~1u) != 0 || U32(position + 20) == 0 || U32(position + 24) == 0)
                        throw new InvalidDataException("Invalid EMF+ header.");
                    plusStarted = true;
                    break;
                case 0x4002:
                    if (dataSize != 0 || flags != 0) throw new InvalidDataException("Invalid EMF+ EOF.");
                    plusEnded = true;
                    plusDc = false;
                    break;
                case 0x4004:
                    if (dataSize != 0 || flags != 0) throw new InvalidDataException("Invalid EMF+ GetDC.");
                    if (plusUsedDc || plusWorld.M11 != 1 || plusWorld.M22 != 1 || plusWorld.M12 != 0 ||
                        plusWorld.M21 != 0 || plusWorld.X != 0 || plusWorld.Y != 0)
                        throw new NotSupportedException("Repeated or transformed EMF+ GetDC is not supported.");
                    plusDc = plusUsedDc = true;
                    break;
                case 0x401E:
                case 0x4021:
                case 0x4022:
                case 0x4024:
                    if (dataSize != 0 || (flags & 0xFF00) != 0)
                        throw new InvalidDataException("Invalid EMF+ rendering-quality state.");
                    break;
                case 0x4030:
                    if (dataSize != 4) throw new InvalidDataException("Invalid EMF+ page transform.");
                    if (flags != 2 || ReadFloat(position + 12) != 1)
                        throw new NotSupportedException("Only identity pixel-unit EMF+ page transforms are supported.");
                    break;
                case 0x402A:
                    if (dataSize != 24 || flags != 0) throw new InvalidDataException("Invalid EMF+ world transform.");
                    plusWorld = ReadWorldTransform(position + 12);
                    break;
                case 0x402B:
                    if (dataSize != 0 || flags != 0) throw new InvalidDataException("Invalid EMF+ world-transform reset.");
                    plusWorld = WorldTransform.Identity;
                    break;
                default:
                    throw new NotSupportedException($"EMF+ record 0x{type:X4} requires unsupported EMF+ drawing or state semantics.");
            }
            position += (int)size;
        }
    }

    private void SelectClipRegion(int offset)
    {
        Require(offset, 16);
        var size = U32(offset + 8);
        var mode = U32(offset + 12);
        if (size > recordEnd - offset - 16 || mode < 1 || mode > 5)
            throw new InvalidDataException("Invalid EMF clipping region size or mode.");
        if (mode != 1 && mode != 5)
            throw new NotSupportedException("Only copy and intersect EMF clipping regions are supported.");
        if (size == 0)
        {
            if (mode != 5) throw new InvalidDataException("Only copy mode permits a null EMF clipping region.");
            state.ClipLeft = state.ClipTop = 0;
            state.ClipRight = width; state.ClipBottom = height;
            return;
        }
        if (size < 32 || U32(offset + 16) != 32 || U32(offset + 20) != 1)
            throw new InvalidDataException("Invalid EMF region header.");
        var rectangles = U32(offset + 24);
        if (rectangles > (size - 32) / 16 || size != 32L + rectangles * 16L || U32(offset + 28) != rectangles * 16L)
            throw new InvalidDataException("Invalid EMF region rectangle count.");
        if (rectangles > 1)
            throw new NotSupportedException("Multiple-rectangle EMF clipping regions are not supported.");
        if (rectangles == 0)
        {
            state.ClipLeft = state.ClipRight = state.ClipTop = state.ClipBottom = 0;
            return;
        }
        var l = I32(offset + 48); var t = I32(offset + 52);
        var r = I32(offset + 56); var b = I32(offset + 60);
        if (l > r || t > b) throw new InvalidDataException("Inverted EMF region rectangle.");
        // Region data is in reference-device coordinates, unlike logical clip rectangles.
        var a = MapDevice(l, t);
        var z = MapDevice(r, b);
        var clipLeft = mode == 5 ? 0 : state.ClipLeft;
        var clipTop = mode == 5 ? 0 : state.ClipTop;
        var clipRight = mode == 5 ? width : state.ClipRight;
        var clipBottom = mode == 5 ? height : state.ClipBottom;
        state.ClipLeft = Bound(Math.Ceiling(a.X - 0.5), clipLeft, clipRight);
        state.ClipRight = Bound(Math.Ceiling(z.X - 0.5), state.ClipLeft, clipRight);
        state.ClipTop = Bound(Math.Ceiling(a.Y - 0.5), clipTop, clipBottom);
        state.ClipBottom = Bound(Math.Ceiling(z.Y - 0.5), state.ClipTop, clipBottom);
    }

    private WorldTransform ReadWorldTransform(int offset) => new(
        ReadFloat(offset), ReadFloat(offset + 4), ReadFloat(offset + 8),
        ReadFloat(offset + 12), ReadFloat(offset + 16), ReadFloat(offset + 20));

    private void ModifyWorldTransform(int offset)
    {
        var mode = U32(offset + 32);
        if (mode == 1) state.World = WorldTransform.Identity;
        else if (mode == 2) state.World = WorldTransform.Multiply(ReadWorldTransform(offset + 8), state.World);
        else if (mode == 3) state.World = WorldTransform.Multiply(state.World, ReadWorldTransform(offset + 8));
        else throw new NotSupportedException($"EMF world-transform modification mode {mode} is not supported.");
    }

    private (double X, double Y) CurrentPathPoint() => Map(state.CurrentX, state.CurrentY);

    private PathFigure AddFigure(IReadOnlyList<(double X, double Y)> points, bool closed)
    {
        RequireOpenPath();
        var figures = path ?? throw new InvalidDataException("No EMF path to extend.");
        if (figures.Count >= 65_536 || points.Count > 1_000_000 - pathPointCount)
            throw new InvalidDataException("EMF path point or figure limit exceeded.");
        Spend(points.Count);
        var figure = new PathFigure(points, closed);
        figures.Add(figure);
        pathPointCount += points.Count;
        return figure;
    }

    private void EmitFigure(IReadOnlyList<(double X, double Y)> points, bool closed, bool continues)
    {
        if (buildingPath)
        {
            if (continues && activeFigure != null && activeFigure.Points[activeFigure.Points.Count - 1] == points[0])
            {
                if (points.Count - 1 > 1_000_000 - pathPointCount)
                    throw new InvalidDataException("EMF path point limit exceeded.");
                Spend(points.Count - 1);
                for (var index = 1; index < points.Count; index++) activeFigure.Points.Add(points[index]);
                pathPointCount += points.Count - 1;
                activeFigure.Closed = closed;
            }
            else activeFigure = AddFigure(points, closed);
            if (!continues || closed) activeFigure = null;
            return;
        }
        if (closed) Fill(points);
        StrokeFigure(points, closed);
    }

    private void StrokeFigure(IReadOnlyList<(double X, double Y)> points, bool closed)
    {
        for (var index = 1; index < points.Count; index++) Stroke(points[index - 1], points[index]);
        if (closed && points.Count > 1) Stroke(points[points.Count - 1], points[0]);
    }

    private void RequireOpenPath()
    {
        if (!buildingPath || path == null) throw new InvalidDataException("No open EMF path bracket.");
    }

    private void PaintPath(bool fill, bool stroke)
    {
        var figures = path;
        if (figures == null || buildingPath) throw new InvalidDataException("No completed EMF path to paint.");
        if (fill) FillFigures(figures.Select(figure => (IReadOnlyList<(double X, double Y)>)figure.Points).ToArray());
        if (stroke)
            foreach (var figure in figures) StrokeFigure(figure.Points, figure.Closed || fill);
        ClearPath();
    }

    private void ClearPath()
    {
        path = null;
        activeFigure = null;
        buildingPath = false;
        pathPointCount = 0;
    }

    private List<(double X, double Y)> FlattenBeziers(IReadOnlyList<(double X, double Y)> points)
    {
        var result = new List<(double X, double Y)> { points[0] };
        for (var index = 1; index < points.Count; index += 3)
            Subdivide(points[index - 1], points[index], points[index + 1], points[index + 2], 0);
        return result;

        void Subdivide((double X, double Y) a, (double X, double Y) b,
            (double X, double Y) c, (double X, double Y) d, int depth)
        {
            Spend(1);
            if (result.Count >= 1_000_000) throw new InvalidDataException("EMF curve point limit exceeded.");
            var dx = d.X - a.X;
            var dy = d.Y - a.Y;
            var lengthSquared = dx * dx + dy * dy;
            if (Flat(b) && Flat(c))
            {
                result.Add(d);
                return;
            }
            if (depth >= 20) throw new InvalidDataException("EMF curve subdivision limit exceeded.");
            var ab = Midpoint(a, b);
            var bc = Midpoint(b, c);
            var cd = Midpoint(c, d);
            var abc = Midpoint(ab, bc);
            var bcd = Midpoint(bc, cd);
            var middle = Midpoint(abc, bcd);
            Subdivide(a, ab, abc, middle, depth + 1);
            Subdivide(middle, bcd, cd, d, depth + 1);

            bool Flat((double X, double Y) point)
            {
                var px = point.X - a.X;
                var py = point.Y - a.Y;
                if (lengthSquared == 0) return px * px + py * py <= 0.015625;
                var projection = px * dx + py * dy;
                if (projection < 0) return px * px + py * py <= 0.015625;
                if (projection > lengthSquared)
                {
                    var endX = point.X - d.X;
                    var endY = point.Y - d.Y;
                    return endX * endX + endY * endY <= 0.015625;
                }
                var cross = px * dy - py * dx;
                return cross * cross <= 0.015625 * lengthSquared;
            }
        }
    }

    private static (double X, double Y) Midpoint((double X, double Y) a, (double X, double Y) b) =>
        ((a.X + b.X) / 2, (a.Y + b.Y) / 2);

    private void CreateFont(int offset)
    {
        Require(offset, 104);
        var height = I32(offset + 12);
        var fontWidth = I32(offset + 16);
        if (height == 0 || fontWidth < 0 || I32(offset + 20) != 0 || I32(offset + 24) != 0 ||
            source[offset + 33] != 0 || source[offset + 34] != 0)
            throw new NotSupportedException("EMF fonts require nonzero height, nonnegative width and no rotation or decorations.");
        var weight = I32(offset + 28);
        if (weight != 0 && weight != 400 && weight != 700)
            throw new NotSupportedException("Only regular and bold EMF font weights are supported.");
        var name = System.Text.Encoding.Unicode.GetString(source, offset + 40, 64).Split('\0')[0];
        if (string.IsNullOrWhiteSpace(name) || name.StartsWith("@", StringComparison.Ordinal))
            throw new NotSupportedException("An explicit horizontal EMF font face is required.");
        var handle = U32(offset + 8);
        CheckNewHandle(handle);
        fonts.Add(handle, new MetafileFont(name, height, fontWidth, weight == 700, source[offset + 32] != 0));
    }

    private void DrawText(int offset)
    {
        Require(offset, 76);
        if (buildingPath) throw new NotSupportedException("EMF text inside a path bracket is not supported.");
        var graphicsMode = U32(offset + 24);
        var xScale = ReadFloat(offset + 28);
        var yScale = ReadFloat(offset + 32);
        if (graphicsMode != 1 && graphicsMode != 2)
            throw new NotSupportedException("Unsupported EMF text graphics mode.");
        if (graphicsMode == 1 && (xScale <= 0 || yScale <= 0))
            throw new NotSupportedException("Nonpositive compatible-mode EMF text scales are not supported.");
        var count = U32(offset + 44);
        var options = U32(offset + 52);
        if ((options & ~0x2016u) != 0)
            throw new NotSupportedException("Unsupported EMF text options or shaping.");
        if (count > 65_536) throw new InvalidDataException("EMF text character limit exceeded.");
        var stringOffset = count == 0 ? 0 : BitmapRange(offset, U32(offset + 48), count * 2, 76);
        var pairedSpacing = (options & 0x2000) != 0;
        var spacingOffset = U32(offset + 72) == 0 || count == 0 ? 0
            : BitmapRange(offset, U32(offset + 72), count * (pairedSpacing ? 8u : 4u), 76);
        if (count > 0 && ((state.TextAlignment is not (0 or 2 or 6 or 24 or 26 or 30)) ||
            state.BackgroundMode != 1 ||
            (pairedSpacing && (state.TextAlignment & 6) != 0)))
            throw new NotSupportedException("EMF text requires transparent top or baseline layout; paired spacing requires left alignment.");
        var savedBrush = state.Brush;
        var savedWinding = state.Winding;
        var clip = (state.ClipLeft, state.ClipTop, state.ClipRight, state.ClipBottom);
        try
        {
            if ((options & 4) != 0) IntersectClip(offset + 48);
            if ((options & 2) != 0)
            {
                state.Brush = new DrawingObject(false, state.BackgroundColor);
                Fill(new[] { Map(I32(offset + 56), I32(offset + 60)), Map(I32(offset + 64), I32(offset + 60)),
                    Map(I32(offset + 64), I32(offset + 68)), Map(I32(offset + 56), I32(offset + 68)) }, false);
            }
            if (count == 0) return;
            var font = state.Font ?? throw new NotSupportedException("An explicit EMF font must be selected before drawing text.");
            var resolver = fontResolver ?? throw new NotSupportedException("EMF text requires an explicit TrueType font resolver.");
            var outlines = font.Outlines ??= new TrueTypeGlyphReader(resolver(font.Name, font.Bold, font.Italic));
            var emScale = font.Height < 0 ? -font.Height / outlines.UnitsPerEm : font.Height / outlines.CellHeight;
            var horizontalScale = font.Width == 0 ? emScale : (double)font.Width / outlines.AverageWidth;
            // Compatible scales describe physical units; their ratio adjusts glyph width, not supplied advances.
            if (graphicsMode == 1) horizontalScale *= (double)yScale / xScale;
            var syntheticBoldOffset = 0d;
            if (font.Bold && outlines.NeedsSyntheticBold)
            {
                RequireAxisAlignedWorldTransform("synthetic bold text");
                var deviceStep = Map(1, 0).X - Map(0, 0).X;
                if (deviceStep <= 0) throw new NotSupportedException("Synthetic bold text requires a positive horizontal device scale.");
                syntheticBoldOffset = 1 / deviceStep;
            }
            var glyphs = new ushort[count];
            var dx = new double[count];
            var dy = new double[count];
            double totalWidth = 0;
            for (var index = 0; index < count; index++)
            {
                var value = U16(stringOffset + index * 2);
                if ((options & 16) != 0) glyphs[index] = value;
                else
                {
                    if (!IsSimpleTextCharacter(value))
                        throw new NotSupportedException("This EMF Unicode text requires shaping or glyph indices.");
                    glyphs[index] = outlines.GlyphForCharacter(value);
                }
                dx[index] = spacingOffset == 0 ? outlines.Advance(glyphs[index]) * horizontalScale + syntheticBoldOffset
                    : I32(spacingOffset + index * (pairedSpacing ? 8 : 4));
                dy[index] = spacingOffset != 0 && pairedSpacing ? I32(spacingOffset + index * 8 + 4) : 0;
                totalWidth += dx[index];
            }
            var x = (double)I32(offset + 36);
            var y = (double)I32(offset + 40);
            if ((state.TextAlignment & 24) == 0) y += outlines.CellAscent * emScale;
            if ((state.TextAlignment & 6) == 2) x -= totalWidth;
            else if ((state.TextAlignment & 6) == 6) x -= totalWidth / 2;
            state.Brush = new DrawingObject(false, state.TextColor);
            state.Winding = true;
            for (var index = 0; index < count; index++)
            {
                Spend(1);
                var contours = outlines.ReadGlyph(glyphs[index]);
                FillFigures(contours.Select(contour => GlyphContour(contour, x, y, horizontalScale, emScale)).ToArray(), false);
                if (syntheticBoldOffset != 0)
                    FillFigures(contours.Select(contour => GlyphContour(contour, x + syntheticBoldOffset, y, horizontalScale, emScale)).ToArray(), false);
                x += dx[index]; y += dy[index];
            }

        }
        finally
        {
            state.Brush = savedBrush;
            state.Winding = savedWinding;
            (state.ClipLeft, state.ClipTop, state.ClipRight, state.ClipBottom) = clip;
        }
    }

    private static bool IsSimpleTextCharacter(ushort value) =>
        value is >= 0x20 and <= 0x7E or >= 0xA0 and <= 0x24F or >= 0x1E00 and <= 0x1EFF
            or >= 0x3000 and <= 0x303F or >= 0x3400 and <= 0x4DBF or >= 0x4E00 and <= 0x9FFF
            or >= 0xF900 and <= 0xFAFF;

    private IReadOnlyList<(double X, double Y)> GlyphContour(
        IReadOnlyList<TrueTypeGlyphReader.Point> contour, double x, double y, double horizontalScale, double verticalScale)
    {
        Spend(contour.Count);
        var first = contour[0];
        var last = contour[contour.Count - 1];
        var start = first.OnCurve ? (first.X, first.Y) : last.OnCurve ? (last.X, last.Y)
            : ((last.X + first.X) / 2, (last.Y + first.Y) / 2);
        var current = start;
        var result = new List<(double X, double Y)> { MapGlyph(start) };
        for (var index = first.OnCurve ? 1 : 0; index < contour.Count;)
        {
            var point = contour[index];
            if (point.OnCurve)
            {
                current = (point.X, point.Y);
                result.Add(MapGlyph(current));
                index++;
            }
            else
            {
                var next = contour[(index + 1) % contour.Count];
                var end = next.OnCurve ? (next.X, next.Y) : ((point.X + next.X) / 2, (point.Y + next.Y) / 2);
                var curve = FlattenBeziers(new[] { MapGlyph(current),
                    MapGlyph((current.Item1 + (point.X - current.Item1) * 2 / 3, current.Item2 + (point.Y - current.Item2) * 2 / 3)),
                    MapGlyph((end.Item1 + (point.X - end.Item1) * 2 / 3, end.Item2 + (point.Y - end.Item2) * 2 / 3)), MapGlyph(end) });
                result.AddRange(curve.Skip(1));
                current = end;
                index += next.OnCurve ? 2 : 1;
            }
            if (result.Count > 1_000_000) throw new InvalidDataException("EMF glyph contour point limit exceeded.");
        }
        return result;

        (double X, double Y) MapGlyph((double X, double Y) point) =>
            Map(x + point.X * horizontalScale, y - point.Y * verticalScale);
    }

    private readonly struct ColorMask
    {
        private readonly uint mask;
        private readonly int shift;
        private readonly uint maximum;

        internal ColorMask(uint mask, int bitCount)
        {
            if (mask == 0 || (bitCount == 16 && mask > ushort.MaxValue))
                throw new InvalidDataException("Invalid DIB color mask.");
            this.mask = mask;
            shift = 0;
            while ((mask & 1) == 0)
            {
                mask >>= 1;
                shift++;
            }
            if ((mask & unchecked(mask + 1)) != 0)
                throw new InvalidDataException("Noncontiguous DIB color mask.");
            maximum = mask;
        }

        internal byte Read(uint value) => (byte)(((ulong)((value & mask) >> shift) * 255 + maximum / 2) / maximum);
    }

    private sealed class DibImage
    {
        internal int Width, Height, Stride, BitsOffset, BitCount;
        internal bool TopDown;
        internal ColorMask Red, Green, Blue;
    }

    private void DrawBitmap(int offset, bool alphaBlend)
    {
        RequireAxisAlignedWorldTransform("bitmap");
        var fixedSize = alphaBlend ? 108 : 80;
        Require(offset, fixedSize);
        var xDest = I32(offset + 24);
        var yDest = I32(offset + 28);
        var destWidth = I32(offset + (alphaBlend ? 32 : 72));
        var destHeight = I32(offset + (alphaBlend ? 36 : 76));
        var sourceX = I32(offset + (alphaBlend ? 44 : 32));
        var sourceY = I32(offset + (alphaBlend ? 48 : 36));
        var sourceWidth = I32(offset + (alphaBlend ? 100 : 40));
        var sourceHeight = I32(offset + (alphaBlend ? 104 : 44));
        if (sourceWidth == 0 || sourceHeight == 0)
            throw new InvalidDataException("Empty EMF bitmap source rectangle.");
        if (sourceWidth < 0 || sourceHeight < 0 || (alphaBlend && (destWidth < 0 || destHeight < 0)))
            throw new NotSupportedException("Mirrored source or alpha-blend extents are not supported.");
        var useAlpha = false;
        var constantAlpha = 255;
        if (alphaBlend)
        {
            var blend = U32(offset + 40);
            // Recorded GDI playback ignores the reserved BlendFlags byte.
            if ((blend & 0xFF) != 0 || (blend >> 24) > 1)
                throw new NotSupportedException("Unsupported EMF alpha-blend operation or alpha format.");
            constantAlpha = (int)(blend >> 16 & 255);
            useAlpha = blend >> 24 == 1;
            if (ReadFloat(offset + 52) != 1 || ReadFloat(offset + 56) != 0 ||
                ReadFloat(offset + 60) != 0 || ReadFloat(offset + 64) != 1 ||
                ReadFloat(offset + 68) != 0 || ReadFloat(offset + 72) != 0)
                throw new NotSupportedException("Transformed EMF source bitmaps are not supported.");
        }
        else if (U32(offset + 68) != 0x00CC0020)
            throw new NotSupportedException("Only SRCCOPY EMF bitmap raster operations are supported.");
        if (U32(offset + (alphaBlend ? 80 : 64)) != 0)
            throw new NotSupportedException("Palette-indexed EMF bitmap usage is not supported.");
        var image = ReadDib(offset, alphaBlend ? 84 : 48, fixedSize, useAlpha);
        if (sourceX < 0 || sourceY < 0 ||
            (long)sourceX + sourceWidth > image.Width || (long)sourceY + sourceHeight > image.Height)
            throw new InvalidDataException("EMF bitmap source rectangle is outside the DIB.");
        // GDI measures StretchDIBits source crops from the bottom even for
        // top-down storage. AlphaBlend instead uses top-left source DC coordinates.
        if (!alphaBlend) sourceY = image.Height - sourceY - sourceHeight;
        var start = Map(xDest, yDest);
        var end = Map((double)xDest + destWidth, (double)yDest + destHeight);
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        // Mirrored GDI extents include the starting pixel, not the ending pixel.
        if (deltaX < 0) { start.X++; end.X++; }
        if (deltaY < 0) { start.Y++; end.Y++; }
        if (deltaX == 0 || deltaY == 0) return;
        if (!alphaBlend && state.StretchMode != 3 &&
            (Math.Abs(deltaX) != sourceWidth || Math.Abs(deltaY) != sourceHeight))
            throw new NotSupportedException("Only COLORONCOLOR EMF bitmap resampling is supported.");
        var firstColumn = Bound(Math.Ceiling(Math.Min(start.X, end.X) - 0.5), state.ClipLeft, state.ClipRight);
        var lastColumn = Bound(Math.Ceiling(Math.Max(start.X, end.X) - 0.5), state.ClipLeft, state.ClipRight);
        var firstRow = Bound(Math.Ceiling(Math.Min(start.Y, end.Y) - 0.5), state.ClipTop, state.ClipBottom);
        var lastRow = Bound(Math.Ceiling(Math.Max(start.Y, end.Y) - 0.5), state.ClipTop, state.ClipBottom);
        Spend((long)(lastColumn - firstColumn) * (lastRow - firstRow));
        for (var row = firstRow; row < lastRow; row++)
        {
            var sy = sourceY + Bound(Math.Floor((row + 0.5 - start.Y) / deltaY * sourceHeight), 0, sourceHeight - 1);
            var storageRow = image.TopDown ? sy : image.Height - 1 - sy;
            for (var column = firstColumn; column < lastColumn; column++)
            {
                var sx = sourceX + Bound(Math.Floor((column + 0.5 - start.X) / deltaX * sourceWidth), 0, sourceWidth - 1);
                var pixel = image.BitsOffset + storageRow * image.Stride + sx * (image.BitCount / 8);
                uint value = (uint)(source[pixel] | source[pixel + 1] << 8);
                if (image.BitCount >= 24) value |= (uint)source[pixel + 2] << 16;
                if (image.BitCount == 32) value |= (uint)source[pixel + 3] << 24;
                var red = image.Red.Read(value);
                var green = image.Green.Read(value);
                var blue = image.Blue.Read(value);
                if (alphaBlend)
                    BlendPixel(column, row, red, green, blue, useAlpha ? source[pixel + 3] : (byte)255, constantAlpha);
                else
                    PutPixel(column, row, (uint)(red | green << 8 | blue << 16));
            }
        }
    }

    private DibImage ReadDib(int record, int fieldsOffset, int fixedSize, bool useAlpha)
    {
        var infoSize = U32(record + fieldsOffset + 4);
        var bitsSize = U32(record + fieldsOffset + 12);
        var info = BitmapRange(record, U32(record + fieldsOffset), infoSize, fixedSize);
        var bits = BitmapRange(record, U32(record + fieldsOffset + 8), bitsSize, fixedSize);
        if ((long)info < bits + (long)bitsSize && (long)bits < info + (long)infoSize)
            throw new InvalidDataException("Overlapping EMF bitmap information and pixel data.");
        if (infoSize < 40) throw new InvalidDataException("Truncated DIB header.");
        var headerSize = U32(info);
        if (headerSize > infoSize) throw new InvalidDataException("Truncated DIB header.");
        if (headerSize != 40) throw new NotSupportedException($"DIB header size {headerSize} is not supported.");
        var imageWidth = I32(info + 4);
        var signedHeight = I32(info + 8);
        var imageHeight = Math.Abs((long)signedHeight);
        if (imageWidth <= 0 || imageHeight == 0 || (long)imageWidth * imageHeight > 16_777_216)
            throw new InvalidDataException("Invalid DIB dimensions or bitmap pixel limit exceeded.");
        if (U16(info + 12) != 1) throw new InvalidDataException("Invalid DIB plane count.");
        var bitCount = U16(info + 14);
        var compression = U32(info + 16);
        if (bitCount is not (16 or 24 or 32) || (compression != 0 && compression != 3) ||
            (compression == 3 && bitCount == 24))
            throw new NotSupportedException("Unsupported DIB bit depth or compression.");
        if (useAlpha && bitCount != 32)
            throw new InvalidDataException("Per-pixel alpha blending requires a 32-bit DIB.");
        var stride = ((long)imageWidth * bitCount + 31) / 32 * 4;
        var requiredSize = stride * imageHeight;
        if (requiredSize > bitsSize)
            throw new InvalidDataException("Truncated DIB pixel data.");
        var declaredSize = U32(info + 20);
        if (declaredSize != 0 && (declaredSize < requiredSize || declaredSize > bitsSize))
            throw new InvalidDataException("Invalid DIB image byte count.");
        uint red = bitCount == 16 ? 0x7C00u : 0xFF0000u;
        uint green = bitCount == 16 ? 0x03E0u : 0x00FF00u;
        uint blue = bitCount == 16 ? 0x001Fu : 0x0000FFu;
        if (compression == 3)
        {
            if (infoSize < 52) throw new InvalidDataException("Missing DIB color masks.");
            red = U32(info + 40);
            green = U32(info + 44);
            blue = U32(info + 48);
            if ((red & green) != 0 || (red & blue) != 0 || (green & blue) != 0)
                throw new InvalidDataException("Overlapping DIB color masks.");
        }
        if (useAlpha && (red != 0xFF0000 || green != 0x00FF00 || blue != 0x0000FF))
            throw new NotSupportedException("Per-pixel alpha requires standard BGRA DIB channels.");
        return new DibImage
        {
            Width = imageWidth, Height = (int)imageHeight, Stride = (int)stride,
            BitsOffset = bits, BitCount = bitCount, TopDown = signedHeight < 0,
            Red = new ColorMask(red, bitCount), Green = new ColorMask(green, bitCount), Blue = new ColorMask(blue, bitCount)
        };
    }

    private int BitmapRange(int record, uint relativeOffset, uint size, int fixedSize)
    {
        var available = recordEnd - record;
        if (relativeOffset < fixedSize || relativeOffset > available || size > available - relativeOffset)
            throw new InvalidDataException("EMF bitmap range is outside its record.");
        return record + (int)relativeOffset;
    }

    private float ReadFloat(int offset)
    {
        Require(offset, 4);
        var value = BitConverter.ToSingle(source, offset);
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new InvalidDataException("Nonfinite EMF transform.");
        return value;
    }

    private void BlendPixel(int x, int y, byte red, byte green, byte blue, byte alpha, int constantAlpha)
    {
        if (red > alpha || green > alpha || blue > alpha)
            throw new InvalidDataException("EMF alpha-blend source pixels must be premultiplied.");
        var position = (y * width + x) * 4;
        var factor = constantAlpha / 255d;
        var sourceAlpha = alpha / 255d * factor;
        var destinationAlpha = pixels[position + 3] / 255d;
        var remaining = destinationAlpha * (1 - sourceAlpha);
        var outputAlpha = sourceAlpha + remaining;
        if (outputAlpha == 0) return;
        pixels[position] = (byte)Bound((red * factor + pixels[position] * remaining) / outputAlpha + 0.5, 0, 255);
        pixels[position + 1] = (byte)Bound((green * factor + pixels[position + 1] * remaining) / outputAlpha + 0.5, 0, 255);
        pixels[position + 2] = (byte)Bound((blue * factor + pixels[position + 2] * remaining) / outputAlpha + 0.5, 0, 255);
        pixels[position + 3] = (byte)Bound(outputAlpha * 255 + 0.5, 0, 255);
    }

    private void IntersectClip(int offset)
    {
        RequireAxisAlignedWorldTransform("rectangular clipping");
        var a = Map(I32(offset + 8), I32(offset + 12));
        var b = Map(I32(offset + 16), I32(offset + 20));
        state.ClipLeft = Bound(Math.Ceiling(Math.Min(a.X, b.X) - 0.5), state.ClipLeft, state.ClipRight);
        state.ClipRight = Bound(Math.Ceiling(Math.Max(a.X, b.X) - 0.5), state.ClipLeft, state.ClipRight);
        state.ClipTop = Bound(Math.Ceiling(Math.Min(a.Y, b.Y) - 0.5), state.ClipTop, state.ClipBottom);
        state.ClipBottom = Bound(Math.Ceiling(Math.Max(a.Y, b.Y) - 0.5), state.ClipTop, state.ClipBottom);
    }

    private void RequireAxisAlignedWorldTransform(string operation)
    {
        if (state.World.M12 != 0 || state.World.M21 != 0)
            throw new NotSupportedException($"Rotated or sheared EMF {operation} is not supported.");
    }

    private void Restore(int level)
    {
        if (path != null)
            throw new NotSupportedException("Restoring a device context with a pending EMF path is not supported.");
        var index = level < 0 ? (long)savedStates.Count + level : (long)level - 1;
        if (level == 0 || index < 0 || index >= savedStates.Count)
            throw new InvalidDataException("Invalid EMF saved device context.");
        state = savedStates[(int)index];
        savedStates.RemoveRange((int)index, savedStates.Count - (int)index);
    }

    private void AddObject(uint handle, DrawingObject value)
    {
        CheckColor(value.Color);
        CheckNewHandle(handle);
        objects.Add(handle, value);
    }

    private void CheckNewHandle(uint handle)
    {
        if (handle == 0 || handle >= handleCount || objects.ContainsKey(handle) || fonts.ContainsKey(handle))
            throw new InvalidDataException("Invalid or duplicate EMF object handle.");
    }

    private void Select(uint handle)
    {
        if ((handle & 0x80000000) != 0 && (handle & 0x7FFFFFFF) is 10 or 11 or 12 or 13 or 14 or 16 or 17)
        {
            // Stock font selection is valid; unsupported outlines are required only when text is drawn.
            state.Font = null;
            return;
        }
        if (fonts.TryGetValue(handle, out var font))
        {
            state.Font = font;
            return;
        }
        DrawingObject value;
        if ((handle & 0x80000000) != 0)
        {
            value = (handle & 0x7FFFFFFF) switch
            {
                0 => new DrawingObject(false, 0xFFFFFF),
                4 => new DrawingObject(false, 0),
                5 => new DrawingObject(false, 0, true),
                6 => new DrawingObject(true, 0xFFFFFF),
                7 => new DrawingObject(true, 0),
                8 => new DrawingObject(true, 0, true),
                _ => throw new NotSupportedException($"EMF stock object {handle} is not supported.")
            };
        }
        else if (!objects.TryGetValue(handle, out value))
            throw new InvalidDataException("Selecting an unknown EMF object.");
        if (value.IsPen) state.Pen = value;
        else state.Brush = value;
    }

    private void PutPixel(int x, int y, uint color)
    {
        if (x < state.ClipLeft || x >= state.ClipRight || y < state.ClipTop || y >= state.ClipBottom) return;
        var position = (y * width + x) * 4;
        pixels[position] = (byte)color;
        pixels[position + 1] = (byte)(color >> 8);
        pixels[position + 2] = (byte)(color >> 16);
        pixels[position + 3] = 255;
    }

    private uint RasterColor(uint color) => state.RasterOperation switch
    {
        1 => 0,
        4 => ~color & 0xFFFFFF,
        13 => color,
        16 => 0xFFFFFF,
        _ => throw new InvalidDataException("Invalid EMF raster painting state.")
    };

    private void Require(int offset, int count)
    {
        if (offset < 0 || count < 0 || offset > recordEnd - count)
            throw new InvalidDataException("Truncated EMF record.");
    }

    private ushort U16(int offset)
    {
        Require(offset, 2);
        return (ushort)(source[offset] | source[offset + 1] << 8);
    }

    private uint U32(int offset)
    {
        Require(offset, 4);
        return (uint)(source[offset] | source[offset + 1] << 8 | source[offset + 2] << 16 | source[offset + 3] << 24);
    }

    private int I32(int offset) => unchecked((int)U32(offset));
    private static int Bound(double value, int min, int max) => (int)Math.Max(min, Math.Min(max, value));
    private static int Nonzero(int value) => value != 0 ? value : throw new InvalidDataException("Zero EMF mapping extent.");

    private static void CheckColor(uint color)
    {
        // PALETTERGB is a direct RGB request on this true-color surface, unlike PALETTEINDEX.
        if ((color & 0xFF000000) is not (0 or 0x02000000))
            throw new NotSupportedException("Indexed palettes and this EMF color encoding are not supported.");
    }

    private void Spend(long work)
    {
        remainingWork -= work;
        if (remainingWork < 0) throw new InvalidDataException("EMF rendering work limit exceeded.");
    }

    private byte[] EncodePng(float cropLeft, float cropTop, float cropRight, float cropBottom)
    {
        var x = 0; var y = 0; var encodedWidth = width; var encodedHeight = height;
        if (cropLeft > 0 || cropTop > 0 || cropRight > 0 || cropBottom > 0)
        {
            x = Bound(Math.Round(width * cropLeft), 0, width - 1);
            y = Bound(Math.Round(height * cropTop), 0, height - 1);
            encodedWidth = Bound(Math.Round(width * (1 - cropLeft - cropRight)), 1, width - x);
            encodedHeight = Bound(Math.Round(height * (1 - cropTop - cropBottom)), 1, height - y);
        }
        using var output = new MemoryStream();
        output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);
        var header = new byte[13];
        WriteBigEndian(header, 0, (uint)encodedWidth);
        WriteBigEndian(header, 4, (uint)encodedHeight);
        header[8] = 8;
        header[9] = 6;
        Chunk(output, "IHDR", header);
        using var compressed = new MemoryStream();
        compressed.WriteByte(0x78);
        compressed.WriteByte(0x9C);
        uint sum1 = 1, sum2 = 0;
        using (var deflate = new DeflateStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var row = 0; row < encodedHeight; row++)
            {
                deflate.WriteByte(0);
                sum2 = (sum2 + sum1) % 65521;
                var start = ((row + y) * width + x) * 4;
                deflate.Write(pixels, start, encodedWidth * 4);
                for (var index = start; index < start + encodedWidth * 4; index++)
                {
                    sum1 = (sum1 + pixels[index]) % 65521;
                    sum2 = (sum2 + sum1) % 65521;
                }
            }
        }
        var checksum = new byte[4];
        WriteBigEndian(checksum, 0, sum2 << 16 | sum1);
        compressed.Write(checksum, 0, 4);
        Chunk(output, "IDAT", compressed.ToArray());
        Chunk(output, "IEND", Array.Empty<byte>());
        return output.ToArray();
    }

    private static void Chunk(Stream output, string type, byte[] data)
    {
        var buffer = new byte[checked(data.Length + 12)];
        WriteBigEndian(buffer, 0, (uint)data.Length);
        for (var index = 0; index < 4; index++) buffer[index + 4] = (byte)type[index];
        Buffer.BlockCopy(data, 0, buffer, 8, data.Length);
        var crc = uint.MaxValue;
        for (var index = 4; index < data.Length + 8; index++)
        {
            crc ^= buffer[index];
            for (var bit = 0; bit < 8; bit++)
                crc = crc >> 1 ^ ((crc & 1) != 0 ? 0xEDB88320u : 0u);
        }
        WriteBigEndian(buffer, data.Length + 8, ~crc);
        output.Write(buffer, 0, buffer.Length);
    }

    private static void WriteBigEndian(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }
}
