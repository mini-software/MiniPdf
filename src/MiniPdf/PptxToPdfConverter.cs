namespace MiniSoftware;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

/// <summary>
/// Converts PowerPoint (.pptx) files to PDF documents.
/// The v1 renderer supports slide-local text, basic shapes, and embedded PNG/JPEG pictures.
/// </summary>
internal static class PptxToPdfConverter
{
    private static readonly Regex SvgNumberRegex = new(@"[-+]?(?:\d*\.\d+|\d+\.?)(?:[eE][-+]?\d+)?", RegexOptions.Compiled);
    private static readonly Regex SvgPathTokenRegex = new(@"[AaCcHhLlMmQqSsTtVvZz]|[-+]?(?:\d*\.\d+|\d+\.?)(?:[eE][-+]?\d+)?", RegexOptions.Compiled);

    internal sealed class ConversionOptions
    {
        public float TextInset { get; set; } = 6f;
        public float LineSpacing { get; set; } = 1.15f;
    }

    internal static PdfDocument Convert(string pptxPath, ConversionOptions? options = null)
    {
        using var stream = new FileStream(pptxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        return Convert(stream, options);
    }

    internal static PdfDocument Convert(Stream pptxStream, ConversionOptions? options = null)
    {
        options ??= new ConversionOptions();
        var presentation = PptxReader.Read(pptxStream);
        var document = new PdfDocument();

        if (presentation.Slides.Count == 0)
        {
            document.AddPage(presentation.SlideWidth, presentation.SlideHeight);
            return document;
        }

        foreach (var slide in presentation.Slides)
            RenderSlide(document, slide, options);

        return document;
    }

    internal static void ConvertToFile(string pptxPath, string pdfPath, ConversionOptions? options = null)
    {
        var document = Convert(pptxPath, options);
        document.Save(pdfPath);
    }

    private static void RenderSlide(PdfDocument document, PptxSlide slide, ConversionOptions options)
    {
        var page = document.AddPage(slide.Width, slide.Height);
        if (slide.BackgroundColor != null)
            page.AddRectangle(0, 0, slide.Width, slide.Height, slide.BackgroundColor.Value);

        foreach (var element in slide.Elements)
        {
            if (element is PptxPicture picture)
                RenderPicture(page, picture, slide.BackgroundColor ?? PdfColor.White);
            else if (element is PptxLine line)
                RenderLine(page, line);
            else if (element is PptxShape shape)
                RenderShape(page, shape, options);
        }
    }

    private static void RenderPicture(PdfPage page, PptxPicture picture, PdfColor backgroundColor)
    {
        if (picture.Format.Equals("svg", StringComparison.OrdinalIgnoreCase))
        {
            RenderSvgPicture(page, picture, backgroundColor);
            return;
        }

        var y = ToPdfY(page, picture.Bounds.Y, picture.Bounds.Height);
        page.AddImage(picture.Data, picture.Format, picture.Bounds.X, y, picture.Bounds.Width, picture.Bounds.Height);
    }

    private static void RenderSvgPicture(PdfPage page, PptxPicture picture, PdfColor backgroundColor)
    {
        var svgText = Encoding.UTF8.GetString(picture.Data);
        XDocument svg;
        try
        {
            svg = XDocument.Parse(svgText);
        }
        catch
        {
            return;
        }

        var root = svg.Root;
        if (root == null)
            return;

        var viewBox = ReadSvgViewBox(root);
        if (viewBox.Width <= 0 || viewBox.Height <= 0)
            return;

        var scaleX = picture.Bounds.Width / viewBox.Width;
        var scaleY = picture.Bounds.Height / viewBox.Height;
        var pdfBottom = ToPdfY(page, picture.Bounds.Y, picture.Bounds.Height);

        PdfPoint Map(float x, float y)
        {
            var mappedX = picture.Bounds.X + (x - viewBox.X) * scaleX;
            var mappedY = pdfBottom + (viewBox.Height - (y - viewBox.Y)) * scaleY;
            return new PdfPoint(mappedX, mappedY);
        }

        foreach (var pathElement in root.Descendants().Where(element => element.Name.LocalName == "path"))
        {
            var data = pathElement.Attribute("d")?.Value;
            if (string.IsNullOrWhiteSpace(data))
                continue;

            var fill = ReadSvgFill(pathElement, backgroundColor);
            if (fill == null)
                continue;

            var commands = ParseSvgPath(data!, Map);
            if (commands.Count > 0)
                page.AddPath(commands, fill.Value);
        }
    }

    private static (float X, float Y, float Width, float Height) ReadSvgViewBox(XElement root)
    {
        var viewBox = root.Attribute("viewBox")?.Value;
        if (!string.IsNullOrWhiteSpace(viewBox))
        {
            var values = SvgNumberRegex.Matches(viewBox!).Cast<Match>()
                .Select(match => float.Parse(match.Value, CultureInfo.InvariantCulture))
                .ToArray();
            if (values.Length >= 4)
                return (values[0], values[1], values[2], values[3]);
        }

        var width = ReadSvgLength(root.Attribute("width")?.Value, 0);
        var height = ReadSvgLength(root.Attribute("height")?.Value, 0);
        return (0, 0, width, height);
    }

    private static float ReadSvgLength(string? value, float fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        var match = SvgNumberRegex.Match(value!);
        return match.Success ? float.Parse(match.Value, CultureInfo.InvariantCulture) : fallback;
    }

    private static PdfColor? ReadSvgFill(XElement pathElement, PdfColor backgroundColor)
    {
        var fillValue = pathElement.Attribute("fill")?.Value;
        if (string.IsNullOrWhiteSpace(fillValue) || fillValue!.Equals("none", StringComparison.OrdinalIgnoreCase))
            return null;

        var color = ReadSvgColor(fillValue!);
        if (color == null)
            return null;

        var opacity = ReadSvgOpacity(pathElement.Attribute("fill-opacity")?.Value);
        if (opacity < 0.999f)
            return Blend(color.Value, backgroundColor, opacity);

        return color.Value;
    }

    private static PdfColor? ReadSvgColor(string value)
    {
        value = value.Trim();
        if (value.StartsWith("#", StringComparison.Ordinal))
        {
            var hex = value.Substring(1);
            if (hex.Length == 3)
                hex = string.Concat(hex.Select(ch => new string(ch, 2)));
            if (hex.Length == 6)
                return PdfColor.FromHex(hex);
        }

        return value.ToLowerInvariant() switch
        {
            "white" => PdfColor.White,
            "black" => PdfColor.Black,
            "red" => PdfColor.Red,
            "green" => PdfColor.Green,
            "blue" => PdfColor.Blue,
            _ => null,
        };
    }

    private static float ReadSvgOpacity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 1f;
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var opacity)
            ? Math.Max(0f, Math.Min(1f, opacity))
            : 1f;
    }

    private static PdfColor Blend(PdfColor foreground, PdfColor background, float alpha)
    {
        return new PdfColor(
            foreground.R * alpha + background.R * (1f - alpha),
            foreground.G * alpha + background.G * (1f - alpha),
            foreground.B * alpha + background.B * (1f - alpha));
    }

    private static List<PdfPathCommand> ParseSvgPath(string pathData, Func<float, float, PdfPoint> map)
    {
        var tokens = SvgPathTokenRegex.Matches(pathData).Cast<Match>().Select(match => match.Value).ToList();
        var commands = new List<PdfPathCommand>();
        var index = 0;
        var command = ' ';
        var currentX = 0f;
        var currentY = 0f;
        var startX = 0f;
        var startY = 0f;
        var lastC2X = 0f;
        var lastC2Y = 0f;
        var hasLastC2 = false;

        bool HasNumber() => index < tokens.Count && !char.IsLetter(tokens[index][0]);
        float Number() => float.Parse(tokens[index++], CultureInfo.InvariantCulture);
        PdfPathCommand Cmd(char op, params float[] values) => new(op, values);

        while (index < tokens.Count)
        {
            if (char.IsLetter(tokens[index][0]))
                command = tokens[index++][0];
            if (command == ' ')
                break;

            var relative = char.IsLower(command);
            var upper = char.ToUpperInvariant(command);
            switch (upper)
            {
                case 'M':
                    {
                        var first = true;
                        while (HasNumber())
                        {
                            var x = Number();
                            var y = Number();
                            if (relative)
                            {
                                x += currentX;
                                y += currentY;
                            }
                            currentX = x;
                            currentY = y;
                            if (first)
                            {
                                startX = x;
                                startY = y;
                                var p = map(x, y);
                                commands.Add(Cmd('M', p.X, p.Y));
                                first = false;
                            }
                            else
                            {
                                var p = map(x, y);
                                commands.Add(Cmd('L', p.X, p.Y));
                            }
                            hasLastC2 = false;
                        }
                        break;
                    }
                case 'L':
                    while (HasNumber())
                    {
                        var x = Number();
                        var y = Number();
                        if (relative)
                        {
                            x += currentX;
                            y += currentY;
                        }
                        currentX = x;
                        currentY = y;
                        var p = map(x, y);
                        commands.Add(Cmd('L', p.X, p.Y));
                        hasLastC2 = false;
                    }
                    break;
                case 'H':
                    while (HasNumber())
                    {
                        var x = Number();
                        if (relative)
                            x += currentX;
                        currentX = x;
                        var p = map(currentX, currentY);
                        commands.Add(Cmd('L', p.X, p.Y));
                        hasLastC2 = false;
                    }
                    break;
                case 'V':
                    while (HasNumber())
                    {
                        var y = Number();
                        if (relative)
                            y += currentY;
                        currentY = y;
                        var p = map(currentX, currentY);
                        commands.Add(Cmd('L', p.X, p.Y));
                        hasLastC2 = false;
                    }
                    break;
                case 'C':
                    while (HasNumber())
                    {
                        var x1 = Number();
                        var y1 = Number();
                        var x2 = Number();
                        var y2 = Number();
                        var x = Number();
                        var y = Number();
                        if (relative)
                        {
                            x1 += currentX; y1 += currentY;
                            x2 += currentX; y2 += currentY;
                            x += currentX; y += currentY;
                        }
                        var p1 = map(x1, y1);
                        var p2 = map(x2, y2);
                        var p = map(x, y);
                        commands.Add(Cmd('C', p1.X, p1.Y, p2.X, p2.Y, p.X, p.Y));
                        currentX = x;
                        currentY = y;
                        lastC2X = x2;
                        lastC2Y = y2;
                        hasLastC2 = true;
                    }
                    break;
                case 'S':
                    while (HasNumber())
                    {
                        var x1 = hasLastC2 ? currentX * 2 - lastC2X : currentX;
                        var y1 = hasLastC2 ? currentY * 2 - lastC2Y : currentY;
                        var x2 = Number();
                        var y2 = Number();
                        var x = Number();
                        var y = Number();
                        if (relative)
                        {
                            x2 += currentX; y2 += currentY;
                            x += currentX; y += currentY;
                        }
                        var p1 = map(x1, y1);
                        var p2 = map(x2, y2);
                        var p = map(x, y);
                        commands.Add(Cmd('C', p1.X, p1.Y, p2.X, p2.Y, p.X, p.Y));
                        currentX = x;
                        currentY = y;
                        lastC2X = x2;
                        lastC2Y = y2;
                        hasLastC2 = true;
                    }
                    break;
                case 'Z':
                    commands.Add(Cmd('Z'));
                    currentX = startX;
                    currentY = startY;
                    hasLastC2 = false;
                    break;
                default:
                    return commands;
            }
        }

        return commands;
    }

    private static void RenderLine(PdfPage page, PptxLine line)
    {
        page.AddLine(
            line.X1,
            page.Height - line.Y1,
            line.X2,
            page.Height - line.Y2,
            line.Outline.Color,
            line.Outline.Width,
            line.Outline.DashPattern);
    }

    private static void RenderShape(PdfPage page, PptxShape shape, ConversionOptions options)
    {
        var y = ToPdfY(page, shape.Bounds.Y, shape.Bounds.Height);

        if (shape.FillColor != null)
        {
            if (IsEllipse(shape.ShapeType))
                page.AddEllipse(shape.Bounds.X, y, shape.Bounds.Width, shape.Bounds.Height, shape.FillColor.Value);
            else
                page.AddRectangle(shape.Bounds.X, y, shape.Bounds.Width, shape.Bounds.Height, shape.FillColor.Value);
        }

        if (shape.Outline != null && !IsEllipse(shape.ShapeType))
            DrawRectangleOutline(page, shape.Bounds, shape.Outline);

        if (shape.Paragraphs.Count > 0)
            RenderText(page, shape, options);
    }

    private static void RenderText(PdfPage page, PptxShape shape, ConversionOptions options)
    {
        var textX = shape.Bounds.X + options.TextInset;
        var textTop = shape.Bounds.Y + options.TextInset;
        var maxWidth = Math.Max(1f, shape.Bounds.Width - options.TextInset * 2);
        var clipY = ToPdfY(page, shape.Bounds.Y, shape.Bounds.Height);
        var clipRect = (shape.Bounds.X, clipY, shape.Bounds.Width, shape.Bounds.Height);
        var currentTop = textTop;

        foreach (var paragraph in shape.Paragraphs)
        {
            if (paragraph.Runs.Count == 0)
            {
                currentTop += 12f * options.LineSpacing;
                continue;
            }

            var paragraphHeight = RenderParagraph(page, paragraph, textX, currentTop, maxWidth, clipRect, options);
            currentTop += paragraphHeight;
            if (currentTop > shape.Bounds.Y + shape.Bounds.Height)
                break;
        }
    }

    private static float RenderParagraph(
        PdfPage page,
        PptxTextParagraph paragraph,
        float x,
        float top,
        float maxWidth,
        (float X, float Y, float Width, float Height) clipRect,
        ConversionOptions options)
    {
        if (CanRenderAsSingleStyle(paragraph))
        {
            var firstRun = paragraph.Runs[0];
            var text = string.Concat(paragraph.Runs.Select(run => run.Text));
            return RenderWrappedText(page, text, firstRun, x, top, maxWidth, clipRect, options);
        }

        var currentX = x;
        var currentTop = top;
        var maxLineHeight = 0f;
        foreach (var run in paragraph.Runs)
        {
            var pieces = run.Text.Split('\n');
            for (var pieceIndex = 0; pieceIndex < pieces.Length; pieceIndex++)
            {
                if (pieceIndex > 0)
                {
                    currentTop += Math.Max(maxLineHeight, run.FontSize * options.LineSpacing);
                    currentX = x;
                    maxLineHeight = 0f;
                }

                var piece = pieces[pieceIndex];
                if (piece.Length == 0)
                    continue;

                var estimatedWidth = EstimateTextWidth(piece, run.FontSize);
                if (currentX > x && currentX + estimatedWidth > x + maxWidth)
                {
                    currentTop += Math.Max(maxLineHeight, run.FontSize * options.LineSpacing);
                    currentX = x;
                    maxLineHeight = 0f;
                }

                AddText(page, piece, run, currentX, currentTop, maxWidth - (currentX - x), clipRect);
                currentX += estimatedWidth;
                maxLineHeight = Math.Max(maxLineHeight, run.FontSize * options.LineSpacing);
            }
        }

        return Math.Max(maxLineHeight, 12f * options.LineSpacing);
    }

    private static float RenderWrappedText(
        PdfPage page,
        string text,
        PptxTextRun style,
        float x,
        float top,
        float maxWidth,
        (float X, float Y, float Width, float Height) clipRect,
        ConversionOptions options)
    {
        var lines = new List<string>();
        foreach (var segment in text.Split('\n'))
        {
            if (segment.Length == 0)
            {
                lines.Add(string.Empty);
                continue;
            }

            lines.AddRange(WrapLine(segment, maxWidth, style.FontSize));
        }

        if (lines.Count == 0)
            lines.Add(string.Empty);

        var lineHeight = style.FontSize * options.LineSpacing;
        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            if (lines[lineIndex].Length == 0)
                continue;
            AddText(page, lines[lineIndex], style, x, top + lineHeight * lineIndex, maxWidth, clipRect);
        }

        return lineHeight * lines.Count;
    }

    private static List<string> WrapLine(string text, float maxWidth, float fontSize)
    {
        var result = new List<string>();
        var words = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            result.Add(text);
            return result;
        }

        var current = string.Empty;
        foreach (var word in words)
        {
            var candidate = current.Length == 0 ? word : current + " " + word;
            if (current.Length > 0 && EstimateTextWidth(candidate, fontSize) > maxWidth)
            {
                result.Add(current);
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0)
            result.Add(current);
        return result;
    }

    private static void AddText(
        PdfPage page,
        string text,
        PptxTextRun run,
        float x,
        float top,
        float maxWidth,
        (float X, float Y, float Width, float Height) clipRect)
    {
        var baselineY = page.Height - top - run.FontSize;
        page.AddText(
            text,
            x,
            baselineY,
            run.FontSize,
            run.Color,
            clipRect,
            Math.Max(1f, maxWidth),
            run.Bold,
            run.Italic,
            run.Underline,
            preferredFontName: run.FontName);
    }

    private static bool CanRenderAsSingleStyle(PptxTextParagraph paragraph)
    {
        if (paragraph.Runs.Count <= 1)
            return true;

        var first = paragraph.Runs[0];
        return paragraph.Runs.All(run =>
            Math.Abs(run.FontSize - first.FontSize) < 0.001f
            && run.Color == first.Color
            && run.Bold == first.Bold
            && run.Italic == first.Italic
            && run.Underline == first.Underline
            && string.Equals(run.FontName, first.FontName, StringComparison.OrdinalIgnoreCase));
    }

    private static void DrawRectangleOutline(PdfPage page, PptxRect bounds, PptxOutline outline)
    {
        var left = bounds.X;
        var right = bounds.X + bounds.Width;
        var top = page.Height - bounds.Y;
        var bottom = page.Height - bounds.Y - bounds.Height;

        page.AddLine(left, top, right, top, outline.Color, outline.Width, outline.DashPattern);
        page.AddLine(right, top, right, bottom, outline.Color, outline.Width, outline.DashPattern);
        page.AddLine(right, bottom, left, bottom, outline.Color, outline.Width, outline.DashPattern);
        page.AddLine(left, bottom, left, top, outline.Color, outline.Width, outline.DashPattern);
    }

    private static float ToPdfY(PdfPage page, float top, float height)
    {
        return page.Height - top - height;
    }

    private static bool IsEllipse(string shapeType)
    {
        return shapeType.Equals("ellipse", StringComparison.OrdinalIgnoreCase)
            || shapeType.Equals("arc", StringComparison.OrdinalIgnoreCase);
    }

    private static float EstimateTextWidth(string text, float fontSize)
    {
        var width = 0f;
        foreach (var character in text)
            width += char.IsWhiteSpace(character) ? fontSize * 0.28f : fontSize * 0.52f;
        return width;
    }
}