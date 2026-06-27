using MiniSoftware;

var app = new CliApp(args);
return app.Run();

internal sealed class CliApp
{
    private readonly string[] _args;

    public CliApp(string[] args) => _args = args;

    public int Run()
    {
        if (_args.Length == 0 || _args[0] is "-h" or "--help")
            return ShowHelp();

        if (_args[0] is "-v" or "--version")
            return ShowVersion();

        if (_args[0] is "convert")
            return RunConvert(_args.AsSpan(1));

        // Default: treat first arg as input file (shorthand)
        return RunConvert(_args.AsSpan(0));
    }

    private static int RunConvert(ReadOnlySpan<string> args)
    {
        string? inputPath = null;
        string? outputPath = null;
        string? fontDir = null;
        string[]? sheets = null;
        int[]? sheetIndexes = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o" or "--output" when i + 1 < args.Length:
                    outputPath = args[++i];
                    break;
                case "--fonts" when i + 1 < args.Length:
                    fontDir = args[++i];
                    break;
                case "--sheets" when i + 1 < args.Length:
                    (sheets, sheetIndexes) = ParseSheets(args[++i]);
                    break;
                case "-h" or "--help":
                    ShowConvertHelp();
                    return 0;
                default:
                    if (inputPath == null && !args[i].StartsWith("-"))
                        inputPath = args[i];
                    else
                    {
                        Console.Error.WriteLine($"Unknown option: {args[i]}");
                        return 1;
                    }
                    break;
            }
        }

        if (inputPath == null)
        {
            Console.Error.WriteLine("Error: input file is required.");
            ShowConvertHelp();
            return 1;
        }

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Error: file not found: {inputPath}");
            return 1;
        }

        var ext = Path.GetExtension(inputPath).ToLowerInvariant();
        if (ext is not (".xlsx" or ".docx" or ".pptx"))
        {
            Console.Error.WriteLine($"Error: unsupported file type '{ext}'. Supported: .xlsx, .docx, .pptx");
            return 1;
        }

        outputPath ??= Path.ChangeExtension(inputPath, ".pdf");

        // Register custom fonts if specified
        if (fontDir != null)
        {
            if (!Directory.Exists(fontDir))
            {
                Console.Error.WriteLine($"Error: font directory not found: {fontDir}");
                return 1;
            }
            RegisterFontsFromDirectory(fontDir);
        }

        try
        {
            MiniPdf.ConvertToPdf(inputPath, outputPath, sheets, sheetIndexes);
            Console.WriteLine(outputPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static void RegisterFontsFromDirectory(string dir)
    {
        foreach (var file in Directory.EnumerateFiles(dir))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is ".ttf" or ".ttc" or ".otf")
            {
                var name = Path.GetFileNameWithoutExtension(file);
                MiniPdf.RegisterFont(name, File.ReadAllBytes(file));
            }
        }
    }

    private static (string[]? Names, int[]? Indexes) ParseSheets(string value)
    {
        var names = new List<string>();
        var indexes = new List<int>();
        foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(token, out var index))
                indexes.Add(index);
            else
                names.Add(token);
        }

        return (
            names.Count > 0 ? names.ToArray() : null,
            indexes.Count > 0 ? indexes.ToArray() : null);
    }

    private static int ShowHelp()
    {
        Console.WriteLine("""
            MiniPdf CLI - Convert Excel/Word/PowerPoint to PDF

            Usage:
              minipdf <input>              Convert file to PDF (output beside input)
              minipdf convert <input>      Same as above
              minipdf convert <input> -o <output>

            Options:
              -h, --help       Show help
              -v, --version    Show version

            Use 'minipdf convert --help' for convert options.
            """);
        return 0;
    }

    private static void ShowConvertHelp()
    {
        Console.WriteLine("""
            Usage: minipdf convert <input> [options]

            Arguments:
              <input>          Path to .xlsx, .docx, or .pptx file

            Options:
              -o, --output     Output PDF path (default: <input>.pdf)
              --fonts <dir>    Directory of .ttf/.ttc fonts to register
              --sheets <items> Comma-separated Excel sheet names or 1-based indexes to render
              -h, --help       Show this help
            """);
    }

    private static int ShowVersion()
    {
        var asm = typeof(MiniPdf).Assembly;
        var ver = asm.GetName().Version;
        Console.WriteLine($"minipdf {ver?.ToString(3) ?? "0.0.0"}");
        return 0;
    }
}
