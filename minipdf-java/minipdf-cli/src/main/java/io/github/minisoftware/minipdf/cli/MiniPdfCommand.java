package io.github.minisoftware.minipdf.cli;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdf;
import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.PageSize;

import java.io.IOException;
import java.io.PrintWriter;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Locale;
import java.util.stream.Stream;

public final class MiniPdfCommand {
    private static final String VERSION = "minipdf-java 0.4.0";

    private PrintWriter out = new PrintWriter(System.out, true);
    private PrintWriter err = new PrintWriter(System.err, true);

    public static void main(String[] args) {
        MiniPdfCommand commandLine = createCommandLine();
        int exitCode = commandLine.execute(args);
        System.exit(exitCode);
    }

    public static MiniPdfCommand createCommandLine() {
        return new MiniPdfCommand();
    }

    public void setOut(PrintWriter out) {
        this.out = out;
    }

    public void setErr(PrintWriter err) {
        this.err = err;
    }

    public int execute(String... args) {
        try {
            return executeCommand(args);
        } catch (Exception exception) {
            String message = exception.getMessage();
            err.println("Error: " + (message == null ? exception.toString() : message));
            return 1;
        }
    }

    private int executeCommand(String[] args) throws IOException, MiniPdfException, CliException {
        boolean convertCommand = args.length > 0 && "convert".equals(args[0]);
        ParsedCommand parsed = parse(args, convertCommand ? 1 : 0);
        if (parsed.help) {
            printUsage(convertCommand);
            return 0;
        }
        if (parsed.version) {
            out.println(VERSION);
            return 0;
        }
        if (parsed.input == null) {
            throw new CliException("input file is required");
        }
        return convert(parsed.input, parsed.arguments);
    }

    private ParsedCommand parse(String[] args, int startIndex) throws CliException {
        ParsedCommand parsed = new ParsedCommand();
        boolean optionsEnabled = true;
        for (int index = startIndex; index < args.length; index++) {
            String argument = args[index];
            if (optionsEnabled && "--".equals(argument)) {
                optionsEnabled = false;
            } else if (optionsEnabled && ("-h".equals(argument) || "--help".equals(argument))) {
                parsed.help = true;
            } else if (optionsEnabled && ("-V".equals(argument) || "--version".equals(argument))) {
                parsed.version = true;
            } else if (optionsEnabled && ("-o".equals(argument) || "--output".equals(argument))) {
                parsed.arguments.output = Paths.get(optionValue(args, ++index, argument));
            } else if (optionsEnabled && argument.startsWith("--output=")) {
                parsed.arguments.output = Paths.get(inlineOptionValue(argument, "--output"));
            } else if (optionsEnabled && "--fonts".equals(argument)) {
                parsed.arguments.fonts = Paths.get(optionValue(args, ++index, argument));
            } else if (optionsEnabled && argument.startsWith("--fonts=")) {
                parsed.arguments.fonts = Paths.get(inlineOptionValue(argument, "--fonts"));
            } else if (optionsEnabled && "--paper-size".equals(argument)) {
                parsed.arguments.paperSize = parsePaperSize(optionValue(args, ++index, argument));
            } else if (optionsEnabled && argument.startsWith("--paper-size=")) {
                parsed.arguments.paperSize = parsePaperSize(inlineOptionValue(argument, "--paper-size"));
            } else if (optionsEnabled && "--page-width".equals(argument)) {
                parsed.arguments.pageWidth = parseFloat(optionValue(args, ++index, argument), argument);
            } else if (optionsEnabled && argument.startsWith("--page-width=")) {
                parsed.arguments.pageWidth = parseFloat(inlineOptionValue(argument, "--page-width"), "--page-width");
            } else if (optionsEnabled && "--page-height".equals(argument)) {
                parsed.arguments.pageHeight = parseFloat(optionValue(args, ++index, argument), argument);
            } else if (optionsEnabled && argument.startsWith("--page-height=")) {
                parsed.arguments.pageHeight = parseFloat(inlineOptionValue(argument, "--page-height"), "--page-height");
            } else if (optionsEnabled && argument.startsWith("-")) {
                throw new CliException("unknown option: " + argument);
            } else if (parsed.input != null) {
                throw new CliException("unexpected argument: " + argument);
            } else {
                parsed.input = Paths.get(argument);
            }
        }
        return parsed;
    }

    private static String optionValue(String[] args, int index, String option) throws CliException {
        if (index >= args.length) {
            throw new CliException("missing value for " + option);
        }
        return args[index];
    }

    private static String inlineOptionValue(String argument, String option) throws CliException {
        String value = argument.substring(option.length() + 1);
        if (value.isEmpty()) {
            throw new CliException("missing value for " + option);
        }
        return value;
    }

    private static PaperSizeArgument parsePaperSize(String value) throws CliException {
        try {
            return PaperSizeArgument.valueOf(value.toUpperCase(Locale.ROOT));
        } catch (IllegalArgumentException exception) {
            throw new CliException("invalid value for --paper-size: " + value + " (expected A4 or LETTER)");
        }
    }

    private static Float parseFloat(String value, String option) throws CliException {
        try {
            return Float.valueOf(value);
        } catch (NumberFormatException exception) {
            throw new CliException("invalid number for " + option + ": " + value);
        }
    }

    private void printUsage(boolean convertCommand) {
        if (convertCommand) {
            out.println("Usage: minipdf convert [OPTIONS] INPUT");
            out.println("Convert an Office document to PDF.");
        } else {
            out.println("Usage: minipdf [OPTIONS] INPUT");
            out.println("       minipdf convert [OPTIONS] INPUT");
            out.println("Convert XLSX, DOCX, and PPTX files to PDF with the Java MiniPdf engine.");
        }
        out.println("  -o, --output OUTPUT       Output PDF path");
        out.println("      --fonts DIR           Directory containing font files");
        out.println("      --paper-size SIZE     A4 or LETTER");
        out.println("      --page-width POINTS   Custom page width");
        out.println("      --page-height POINTS  Custom page height");
        out.println("  -h, --help                Show this help message");
        out.println("  -V, --version             Show version information");
    }

    static final class ConversionArguments {
        private Path output;
        private Path fonts;
        private PaperSizeArgument paperSize;
        private Float pageWidth;
        private Float pageHeight;

        private ConversionOptions conversionOptions() throws MiniPdfException, CliException {
            if (paperSize != null && (pageWidth != null || pageHeight != null)) {
                throw new CliException("use either --paper-size or --page-width/--page-height, not both");
            }
            if ((pageWidth == null) != (pageHeight == null)) {
                throw new CliException("--page-width and --page-height must be specified together");
            }
            if (paperSize != null) {
                return ConversionOptions.withPageSize(paperSize == PaperSizeArgument.A4
                        ? PageSize.A4
                        : PageSize.LETTER);
            }
            if (pageWidth != null) {
                return ConversionOptions.withPageSize(PageSize.of(pageWidth, pageHeight));
            }
            return ConversionOptions.defaults();
        }
    }

    enum PaperSizeArgument {
        A4,
        LETTER
    }

    private int convert(Path input, ConversionArguments arguments)
            throws MiniPdfException, IOException, CliException {
        if (!Files.isRegularFile(input)) {
            throw new CliException("file not found: " + input);
        }
        String fileName = input.getFileName().toString();
        int dot = fileName.lastIndexOf('.');
        String extension = dot < 0 ? "" : fileName.substring(dot + 1).toLowerCase(Locale.ROOT);
        if (!extension.equals("xlsx") && !extension.equals("docx") && !extension.equals("pptx")) {
            throw new CliException("unsupported file type '." + extension + "'. Supported: .xlsx, .docx, .pptx");
        }

        if (arguments.fonts != null) {
            registerFonts(arguments.fonts);
        }
        Path output = arguments.output == null ? replaceExtension(input, "pdf") : arguments.output;
        MiniPdf.convertToPdf(input, output, arguments.conversionOptions());
        out.println(output);
        return 0;
    }

    private static void registerFonts(Path directory) throws IOException, CliException {
        if (!Files.isDirectory(directory)) {
            throw new CliException("font directory not found: " + directory);
        }
        try (Stream<Path> paths = Files.list(directory)) {
            for (Path path : (Iterable<Path>) paths.filter(Files::isRegularFile)::iterator) {
                String name = path.getFileName().toString();
                int dot = name.lastIndexOf('.');
                String extension = dot < 0 ? "" : name.substring(dot + 1).toLowerCase(Locale.ROOT);
                if (extension.equals("ttf") || extension.equals("ttc") || extension.equals("otf")) {
                    MiniPdf.registerFont(dot < 0 ? name : name.substring(0, dot), Files.readAllBytes(path));
                }
            }
        }
    }

    private static Path replaceExtension(Path input, String extension) {
        String fileName = input.getFileName().toString();
        int dot = fileName.lastIndexOf('.');
        String outputName = (dot < 0 ? fileName : fileName.substring(0, dot)) + '.' + extension;
        return input.resolveSibling(outputName);
    }

    private static final class ParsedCommand {
        private final ConversionArguments arguments = new ConversionArguments();
        private Path input;
        private boolean help;
        private boolean version;
    }

    private static final class CliException extends Exception {
        private CliException(String message) {
            super(message);
        }
    }
}