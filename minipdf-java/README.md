# MiniPdf for Java

MiniPdf for Java is a Java 17 library and command-line tool for converting
DOCX, XLSX, and PPTX files to PDF without Microsoft Office or LibreOffice.

## Install

The library is available from Maven Central:

```xml
<dependency>
    <groupId>io.github.mini-software</groupId>
    <artifactId>minipdf</artifactId>
    <version>0.1.2</version>
</dependency>
```

  Maven group IDs may contain hyphens, but Java package names may not. Therefore,
  the dependency uses `io.github.mini-software`, while imports use
  `io.github.minisoftware.minipdf`.

## Library

```java
import io.github.minisoftware.minipdf.MiniPdf;

import java.nio.file.Path;

MiniPdf.convertToPdf(
        Path.of("document.docx"),
        Path.of("document.pdf"));
```

Use `MiniPdf.convertBytesToPdf` when the Office document is already in memory.
`ConversionOptions.withPageSize` overrides the output page size.

## CLI

Download the executable JAR with Maven:

```powershell
mvn dependency:copy `
  -Dartifact=io.github.mini-software:minipdf-cli:0.1.2 `
  -DoutputDirectory=.
```

Convert a document:

```powershell
java -jar minipdf-cli-0.1.2.jar input.pptx -o output.pdf
```

The CLI accepts `.docx`, `.xlsx`, and `.pptx` files. Run it with `--help` for
page-size and font-directory options.

## Current Scope

The Java implementation currently provides text-first Office conversion:

- DOCX paragraphs, tabs, and line breaks
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