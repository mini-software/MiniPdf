package io.github.minisoftware.minipdf.cli;

import io.github.minisoftware.minipdf.ConversionOptions;
import io.github.minisoftware.minipdf.MiniPdf;
import io.github.minisoftware.minipdf.MiniPdfException;
import io.github.minisoftware.minipdf.PageSize;
import picocli.CommandLine;
import picocli.CommandLine.Command;
import picocli.CommandLine.Mixin;
import picocli.CommandLine.Option;
import picocli.CommandLine.Parameters;
import picocli.CommandLine.Spec;
import picocli.CommandLine.Model.CommandSpec;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Locale;
import java.util.concurrent.Callable;

@Command(
        name = "minipdf",
    version = "minipdf-java 0.1.0",
        description = "Convert XLSX and DOCX files to PDF with the Java MiniPdf engine.",
        mixinStandardHelpOptions = true,
        subcommands = MiniPdfCommand.ConvertCommand.class)
public final class MiniPdfCommand implements Callable<Integer> {
    @Spec
    private CommandSpec spec;

    @Parameters(index = "0", arity = "0..1", paramLabel = "INPUT")
    private Path input;

    @Mixin
    private ConversionArguments arguments = new ConversionArguments();

    public static void main(String[] args) {
        CommandLine commandLine = createCommandLine();
        int exitCode = commandLine.execute(args);
        System.exit(exitCode);
    }

    public static CommandLine createCommandLine() {
        CommandLine commandLine = new CommandLine(new MiniPdfCommand());
        commandLine.setCaseInsensitiveEnumValuesAllowed(true);
        commandLine.setExecutionExceptionHandler((exception, current, parseResult) -> {
            current.getErr().println("Error: " + exception.getMessage());
            return 1;
        });
        commandLine.setParameterExceptionHandler((exception, args) -> {
            exception.getCommandLine().getErr().println("Error: " + exception.getMessage());
            return 1;
        });
        return commandLine;
    }

    @Override
    public Integer call() throws Exception {
        if (input == null) {
            throw new CommandLine.ParameterException(spec.commandLine(), "input file is required");
        }
        return convert(input, arguments, spec.commandLine());
    }

    @Command(name = "convert", description = "Convert an Office document to PDF.", mixinStandardHelpOptions = true)
    static final class ConvertCommand implements Callable<Integer> {
        @Spec
        private CommandSpec spec;

        @Parameters(index = "0", paramLabel = "INPUT")
        private Path input;

        @Mixin
        private ConversionArguments arguments = new ConversionArguments();

        @Override
        public Integer call() throws Exception {
            return convert(input, arguments, spec.commandLine());
        }
    }

    static final class ConversionArguments {
        @Option(names = {"-o", "--output"}, paramLabel = "OUTPUT")
        private Path output;

        @Option(names = "--fonts", paramLabel = "DIR")
        private Path fonts;

        @Option(names = "--paper-size", paramLabel = "SIZE")
        private PaperSizeArgument paperSize;

        @Option(names = "--page-width", paramLabel = "POINTS")
        private Float pageWidth;

        @Option(names = "--page-height", paramLabel = "POINTS")
        private Float pageHeight;

        private ConversionOptions conversionOptions(CommandLine commandLine) throws MiniPdfException {
            if (paperSize != null && (pageWidth != null || pageHeight != null)) {
                throw new CommandLine.ParameterException(
                        commandLine,
                        "use either --paper-size or --page-width/--page-height, not both");
            }
            if ((pageWidth == null) != (pageHeight == null)) {
                throw new CommandLine.ParameterException(
                        commandLine,
                        "--page-width and --page-height must be specified together");
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

    private static int convert(Path input, ConversionArguments arguments, CommandLine commandLine)
            throws MiniPdfException, IOException {
        if (!Files.isRegularFile(input)) {
            throw new CommandLine.ParameterException(commandLine, "file not found: " + input);
        }
        String fileName = input.getFileName().toString();
        int dot = fileName.lastIndexOf('.');
        String extension = dot < 0 ? "" : fileName.substring(dot + 1).toLowerCase(Locale.ROOT);
        if (!extension.equals("xlsx") && !extension.equals("docx")) {
            throw new CommandLine.ParameterException(
                    commandLine,
                    "unsupported file type '." + extension + "'. Supported: .xlsx, .docx");
        }

        if (arguments.fonts != null) {
            registerFonts(arguments.fonts, commandLine);
        }
        Path output = arguments.output == null ? replaceExtension(input, "pdf") : arguments.output;
        MiniPdf.convertToPdf(input, output, arguments.conversionOptions(commandLine));
        commandLine.getOut().println(output);
        return 0;
    }

    private static void registerFonts(Path directory, CommandLine commandLine) throws IOException {
        if (!Files.isDirectory(directory)) {
            throw new CommandLine.ParameterException(commandLine, "font directory not found: " + directory);
        }
        try (var paths = Files.list(directory)) {
            for (Path path : paths.filter(Files::isRegularFile).toList()) {
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
}