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
        public float TextInset { get; set; } = 0f;
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

        viewBox = ApplySvgCrop(viewBox, picture.Crop);
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

    private static (float X, float Y, float Width, float Height) ApplySvgCrop((float X, float Y, float Width, float Height) viewBox, PptxCrop crop)
    {
        var left = Math.Max(0f, Math.Min(0.95f, crop.Left));
        var top = Math.Max(0f, Math.Min(0.95f, crop.Top));
        var right = Math.Max(0f, Math.Min(0.95f, crop.Right));
        var bottom = Math.Max(0f, Math.Min(0.95f, crop.Bottom));

        var widthFactor = Math.Max(0.01f, 1f - left - right);
        var heightFactor = Math.Max(0.01f, 1f - top - bottom);
        return (
            viewBox.X + viewBox.Width * left,
            viewBox.Y + viewBox.Height * top,
            viewBox.Width * widthFactor,
            viewBox.Height * heightFactor);
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
        var bodyProperties = shape.TextBodyProperties;
        var textX = shape.Bounds.X + options.TextInset + bodyProperties.LeftInset;
        var textTop = shape.Bounds.Y + options.TextInset;
        var maxWidth = Math.Max(1f, shape.Bounds.Width - options.TextInset * 2 - bodyProperties.LeftInset - bodyProperties.RightInset);
        var availableHeight = Math.Max(1f, shape.Bounds.Height - options.TextInset * 2);
        var clipY = ToPdfY(page, shape.Bounds.Y, shape.Bounds.Height);
        var clipRect = (shape.Bounds.X, clipY, shape.Bounds.Width, shape.Bounds.Height);
        var textHeight = EstimateTextHeight(shape.Paragraphs, maxWidth, options);
        var currentTop = AlignTextTop(bodyProperties.VerticalAnchor, textTop, availableHeight, textHeight);

        foreach (var paragraph in shape.Paragraphs)
        {
            currentTop += paragraph.SpaceBefore;
            if (paragraph.Runs.Count == 0)
            {
                currentTop += EmptyParagraphHeight(paragraph, options);
                continue;
            }

            var paragraphX = textX + Math.Max(0f, paragraph.MarginLeft + Math.Min(0f, paragraph.Indent));
            var paragraphWidth = Math.Max(1f, maxWidth - Math.Max(0f, paragraph.MarginLeft + Math.Min(0f, paragraph.Indent)));
            var paragraphHeight = RenderParagraph(page, paragraph, paragraphX, currentTop, paragraphWidth, clipRect, options);
            currentTop += paragraphHeight;
            if (currentTop > shape.Bounds.Y + shape.Bounds.Height)
                break;
        }
    }

    private static float AlignTextTop(string verticalAnchor, float top, float availableHeight, float textHeight)
    {
        if (verticalAnchor.Equals("middle", StringComparison.OrdinalIgnoreCase))
            return top + Math.Max(0f, (availableHeight - textHeight) / 2f);
        if (verticalAnchor.Equals("bottom", StringComparison.OrdinalIgnoreCase))
            return top + Math.Max(0f, availableHeight - textHeight);
        return top;
    }

    private static float EstimateTextHeight(List<PptxTextParagraph> paragraphs, float maxWidth, ConversionOptions options)
    {
        var height = 0f;
        foreach (var paragraph in paragraphs)
        {
            height += paragraph.SpaceBefore;
            if (paragraph.Runs.Count == 0)
            {
                height += EmptyParagraphHeight(paragraph, options);
                continue;
            }

            var paragraphWidth = Math.Max(1f, maxWidth - Math.Max(0f, paragraph.MarginLeft + Math.Min(0f, paragraph.Indent)));
            if (CanRenderAsSingleStyle(paragraph))
            {
                var firstRun = paragraph.Runs[0];
                var text = string.Concat(paragraph.Runs.Select(run => run.Text));
                var lineCount = CountWrappedLines(text, paragraphWidth, firstRun.FontSize);
                height += firstRun.FontSize * (paragraph.LineSpacing ?? options.LineSpacing) * lineCount;
                continue;
            }

            height += EstimateStyledTextHeight(paragraph, paragraphWidth, options);
        }

        return height;
    }

    private static float EmptyParagraphHeight(PptxTextParagraph paragraph, ConversionOptions options)
    {
        return 21f * (paragraph.LineSpacing ?? options.LineSpacing);
    }

    private static int CountWrappedLines(string text, float maxWidth, float fontSize)
    {
        var lineCount = 0;
        foreach (var segment in text.Split('\n'))
        {
            if (segment.Length == 0)
            {
                lineCount++;
                continue;
            }

            lineCount += Math.Max(1, WrapLine(segment, maxWidth, fontSize).Count);
        }

        return Math.Max(1, lineCount);
    }

    private static float EstimateStyledTextHeight(PptxTextParagraph paragraph, float maxWidth, ConversionOptions options)
    {
        var lineHeights = BuildStyledLineHeights(paragraph, maxWidth, options);
        return lineHeights.Count == 0
            ? 12f * options.LineSpacing
            : lineHeights.Sum();
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
            if (paragraph.IsBullet && paragraph.MarginLeft > 0f && paragraph.Indent < 0f && text.StartsWith("\u2022 ", StringComparison.Ordinal))
            {
                AddText(page, "\u2022", firstRun, x, top, Math.Max(1f, paragraph.MarginLeft), clipRect);
                var contentX = x + paragraph.MarginLeft;
                var contentWidth = Math.Max(1f, maxWidth - paragraph.MarginLeft);
                return RenderWrappedText(page, text.Substring(2), firstRun, paragraph.Alignment, paragraph.LineSpacing ?? options.LineSpacing, contentX, top, contentWidth, clipRect);
            }

            return RenderWrappedText(page, text, firstRun, paragraph.Alignment, paragraph.LineSpacing ?? options.LineSpacing, x, top, maxWidth, clipRect);
        }

        return RenderStyledWrappedText(page, paragraph, x, top, maxWidth, clipRect, options);
    }

    private static float RenderStyledWrappedText(
        PdfPage page,
        PptxTextParagraph paragraph,
        float x,
        float top,
        float maxWidth,
        (float X, float Y, float Width, float Height) clipRect,
        ConversionOptions options)
    {
        var lines = BuildStyledLines(paragraph, maxWidth);
        if (lines.Count == 0)
            return 12f * options.LineSpacing;

        var currentTop = top;
        foreach (var line in lines)
        {
            if (line.Count == 0)
            {
                currentTop += EmptyParagraphHeight(paragraph, options);
                continue;
            }

            var lineWidth = line.Sum(segment => segment.Width);
            var lineHeight = line.Max(segment => segment.Run.FontSize) * (paragraph.LineSpacing ?? options.LineSpacing);
            var currentX = AlignLineX(paragraph.Alignment, x, maxWidth, lineWidth);
            foreach (var segment in line)
            {
                if (!string.IsNullOrWhiteSpace(segment.Text))
                    AddText(page, segment.Text, segment.Run, currentX, currentTop, maxWidth - (currentX - x), clipRect);
                currentX += segment.Width;
            }

            currentTop += lineHeight;
        }

        return currentTop - top;
    }

    private static IEnumerable<string> SplitStyledText(string text)
    {
        var token = new StringBuilder();
        foreach (var character in text)
        {
            if (character == '\r')
                continue;
            if (character == '\n')
            {
                if (token.Length > 0)
                {
                    yield return token.ToString();
                    token.Clear();
                }
                yield return "\n";
                continue;
            }

            token.Append(character);
            if (char.IsWhiteSpace(character))
            {
                yield return token.ToString();
                token.Clear();
            }
        }

        if (token.Length > 0)
            yield return token.ToString();
    }

    private static float RenderWrappedText(
        PdfPage page,
        string text,
        PptxTextRun style,
        string alignment,
        float lineSpacing,
        float x,
        float top,
        float maxWidth,
        (float X, float Y, float Width, float Height) clipRect)
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

        var lineHeight = style.FontSize * lineSpacing;
        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            if (lines[lineIndex].Length == 0)
                continue;
            var alignedX = AlignTextX(lines[lineIndex], style.FontSize, alignment, x, maxWidth);
            AddText(page, lines[lineIndex], style, alignedX, top + lineHeight * lineIndex, maxWidth - (alignedX - x), clipRect);
        }

        return lineHeight * lines.Count;
    }

    private static List<List<StyledTextSegment>> BuildStyledLines(PptxTextParagraph paragraph, float maxWidth)
    {
        var lines = new List<List<StyledTextSegment>>();
        var currentLine = new List<StyledTextSegment>();
        var currentWidth = 0f;

        void CommitLine()
        {
            lines.Add(currentLine);
            currentLine = new List<StyledTextSegment>();
            currentWidth = 0f;
        }

        foreach (var run in paragraph.Runs)
        {
            foreach (var token in SplitStyledText(run.Text))
            {
                if (token == "\n")
                {
                    CommitLine();
                    continue;
                }

                if (token.Length == 0)
                    continue;

                var tokenWidth = EstimateTextWidth(token, run.FontSize);
                if (currentLine.Count > 0 && currentWidth + tokenWidth > maxWidth)
                    CommitLine();

                currentLine.Add(new StyledTextSegment(token, run, tokenWidth));
                currentWidth += tokenWidth;
            }
        }

        if (currentLine.Count > 0)
            lines.Add(currentLine);

        return lines;
    }

    private static List<float> BuildStyledLineHeights(PptxTextParagraph paragraph, float maxWidth, ConversionOptions options)
    {
        return BuildStyledLines(paragraph, maxWidth)
            .Select(line => line.Count == 0 ? 12f * options.LineSpacing : line.Max(segment => segment.Run.FontSize) * (paragraph.LineSpacing ?? options.LineSpacing))
            .ToList();
    }

    private static float AlignTextX(string text, float fontSize, string alignment, float x, float maxWidth)
    {
        var width = EstimateTextWidth(text, fontSize);
        return AlignLineX(alignment, x, maxWidth, width);
    }

    private static float AlignLineX(string alignment, float x, float maxWidth, float width)
    {
        if (alignment.Equals("center", StringComparison.OrdinalIgnoreCase))
            return x + Math.Max(0, (maxWidth - width) / 2f);
        if (alignment.Equals("right", StringComparison.OrdinalIgnoreCase))
            return x + Math.Max(0, maxWidth - width);
        return x;
    }

    private sealed class StyledTextSegment
    {
        public string Text { get; }
        public PptxTextRun Run { get; }
        public float Width { get; }

        public StyledTextSegment(string text, PptxTextRun run, float width)
        {
            Text = text;
            Run = run;
            Width = width;
        }
    }

    private static List<string> WrapLine(string text, float maxWidth, float fontSize)
    {
        var result = new List<string>();
        if (text.Length == 0)
        {
            result.Add(text);
            return result;
        }

        var current = new StringBuilder();
        foreach (var token in SplitWrapTokens(text))
        {
            var candidate = current.Length == 0 ? token : current + token;
            if (current.Length > 0 && EstimateTextWidth(candidate, fontSize) > maxWidth)
            {
                result.Add(current.ToString());
                current.Clear();
            }

            current.Append(token);
        }

        if (current.Length > 0)
            result.Add(current.ToString());
        if (result.Count == 0)
            result.Add(text);
        return result;
    }

    private static IEnumerable<string> SplitWrapTokens(string text)
    {
        var token = new StringBuilder();
        var tokenIsWhiteSpace = false;
        var hasToken = false;

        foreach (var character in text)
        {
            var isWhiteSpace = char.IsWhiteSpace(character);
            if (hasToken && isWhiteSpace != tokenIsWhiteSpace)
            {
                yield return token.ToString();
                token.Clear();
            }

            token.Append(character);
            tokenIsWhiteSpace = isWhiteSpace;
            hasToken = true;
        }

        if (hasToken)
            yield return token.ToString();
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
