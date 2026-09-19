# MiniPdf for Java

MiniPdf for Java supports JDK 8 through JDK 25 as a library and command-line tool for converting
DOCX, XLSX, and PPTX files to PDF without Microsoft Office or LibreOffice.

## Install

The library is available from Maven Central:

```xml
<dependency>
    <groupId>io.github.mini-software</groupId>
    <artifactId>minipdf</artifactId>
    <version>0.3.0</version>
</dependency>
```

  Maven group IDs may contain hyphens, but Java package names may not. Therefore,
  the dependency uses `io.github.mini-software`, while imports use
  `io.github.minisoftware.minipdf`.

## Library

```java
import io.github.minisoftware.minipdf.MiniPdf;

import java.nio.file.Paths;

MiniPdf.convertToPdf(
  Paths.get("document.docx"),
  Paths.get("document.pdf"));
```

Use `MiniPdf.convertBytesToPdf` when the Office document is already in memory.
`ConversionOptions.withPageSize` overrides the output page size.

Register fonts before converting when DOCX, XLSX, or PPTX content needs glyphs
that are not available in the standard PDF fonts:

```java
import java.nio.file.Files;
import java.nio.file.Paths;

try {
    MiniPdf.registerFont(
            "Noto Sans",
          Files.readAllBytes(Paths.get("fonts/NotoSans-Regular.ttf")));
    MiniPdf.convertToPdf(
          Paths.get("document.docx"),
          Paths.get("document.pdf"));
} finally {
    MiniPdf.clearRegisteredFonts();
}
```

Registered fonts are process-wide. `clearRegisteredFonts` removes them when the
same process needs a different font set for a later conversion.

## CLI

Download the executable JAR with Maven:

```powershell
mvn dependency:copy `
  -Dartifact=io.github.mini-software:minipdf-cli:0.3.0 `
  -DoutputDirectory=.
```

Convert a document:

```powershell
java -jar minipdf-cli-0.3.0.jar input.pptx -o output.pdf
```

The CLI accepts `.docx`, `.xlsx`, and `.pptx` files. Run it with `--help` for
page-size and font-directory options.

## Current Scope

The Java implementation currently provides text-first Office conversion:

- DOCX paragraphs, tabs, line breaks, explicit and section page breaks, and landscape page size
- XLSX shared strings, inline strings, numbers, and booleans
- PPTX slide text, native slide dimensions, and one PDF page per slide
- bounded OOXML ZIP loading and XML external-entity protection

Advanced Office layout, images, charts, and embedded fonts are not yet fully
reproduced by the Java renderer.

## Build and Test

From this directory:

```powershell
mvn -B -ntp verify
```

## Publishing

The `Maven Central Publish` GitHub Actions workflow publishes both `minipdf`
and `minipdf-cli`. Configure these repository or `maven-central` environment
secrets before running it:

- `MAVEN_CENTRAL_USERNAME`
- `MAVEN_CENTRAL_TOKEN`
- `GPG_PRIVATE_KEY`
- `GPG_PASSPHRASE`

Publish a GitHub Release whose tag matches `java-v<version>`, such as
`java-v0.3.0`, or manually dispatch the workflow with `version` set to `0.3.0`.
The requested version must match the Maven project version.
