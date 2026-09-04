package io.github.minisoftware.minipdf.cli;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;
import picocli.CommandLine;

import java.io.PrintWriter;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

class MiniPdfCommandTest {
    private static final Path REPOSITORY_ROOT = Path.of("..", "..").toAbsolutePath().normalize();

    @TempDir
    Path temporaryDirectory;

    @Test
    void convertsXlsxWithDirectSyntax() throws Exception {
                Path source = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/Simple invoice1.xlsx");
        Path output = temporaryDirectory.resolve("direct.pdf");

        CommandResult result = execute(source.toString(), "-o", output.toString());

        assertEquals(0, result.exitCode());
        assertTrue(Files.readString(output, StandardCharsets.ISO_8859_1).startsWith("%PDF-1.4"));
        assertTrue(result.stdout().contains(output.toString()));
    }

    @Test
    void convertsDocxWithSubcommandAndCustomSize() throws Exception {
                Path source = REPOSITORY_ROOT.resolve("tests/Issue_Files/docx/Invoice.docx");
        Path output = temporaryDirectory.resolve("subcommand.pdf");

        CommandResult result = execute(
                "convert", source.toString(), "-o", output.toString(),
                "--page-width", "400", "--page-height", "500");

        assertEquals(0, result.exitCode());
        assertTrue(Files.readString(output, StandardCharsets.ISO_8859_1)
                .contains("/MediaBox [0 0 400 500]"));
    }

        @Test
        void convertsPptxWithDirectSyntax() throws Exception {
                Path source = REPOSITORY_ROOT.resolve("tests/Issue_Files/pptx/Asian Pacific.pptx");
                Path output = temporaryDirectory.resolve("slides.pdf");

                CommandResult result = execute(source.toString(), "-o", output.toString());

                assertEquals(0, result.exitCode());
                assertTrue(Files.readString(output, StandardCharsets.ISO_8859_1).startsWith("%PDF-1.4"));
                assertTrue(result.stdout().contains(output.toString()));
        }

    @Test
    void rejectsConflictingPageOptions() {
                Path source = REPOSITORY_ROOT.resolve("tests/Issue_Files/xlsx/Simple invoice1.xlsx");

        CommandResult result = execute(
                source.toString(), "--paper-size", "a4", "--page-width", "400", "--page-height", "500");

        assertEquals(1, result.exitCode());
        assertTrue(result.stderr().contains("use either --paper-size"));
    }

    private static CommandResult execute(String... arguments) {
        CommandLine commandLine = MiniPdfCommand.createCommandLine();
        StringWriter stdout = new StringWriter();
        StringWriter stderr = new StringWriter();
        commandLine.setOut(new PrintWriter(stdout, true));
        commandLine.setErr(new PrintWriter(stderr, true));
        int exitCode = commandLine.execute(arguments);
        return new CommandResult(exitCode, stdout.toString(), stderr.toString());
    }

    private record CommandResult(int exitCode, String stdout, String stderr) {
    }
}