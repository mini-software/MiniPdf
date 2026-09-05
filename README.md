<div align="center">

# MiniPdf

**Lightweight Office-to-PDF libraries and command-line tools for .NET, Rust, Java, Python, Node.js, and Go.**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://central.sonatype.com/artifact/io.github.mini-software/minipdf"><img src="https://img.shields.io/maven-central/v/io.github.mini-software/minipdf.svg" alt="Maven Central"></a>
<a href="https://pypi.org/project/minipdf/"><img src="https://img.shields.io/pypi/v/minipdf.svg" alt="PyPI"></a>
<a href="https://www.npmjs.com/package/minipdf"><img src="https://img.shields.io/npm/v/minipdf.svg" alt="npm"></a>
<a href="https://pkg.go.dev/github.com/mini-software/MiniPdf/minipdf-go"><img src="https://pkg.go.dev/badge/github.com/mini-software/MiniPdf/minipdf-go.svg" alt="Go Reference"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

English | <a href="documents/README.zh-CN.md">简体中文</a> | <a href="documents/README.zh-TW.md">繁體中文</a> | <a href="documents/README.ja.md">日本語</a> | <a href="documents/README.ko.md">한국어</a> | <a href="documents/README.it.md">Italiano</a> | <a href="documents/README.fr.md">Français</a>

**[Online Demo](https://mini-software.github.io/MiniPdf/)** · **[Releases](https://github.com/mini-software/MiniPdf/releases)** · **[Report an issue](https://github.com/mini-software/MiniPdf/issues)**

Your star or donation helps sustain the project.

🤝 **Looking for co-developers:** [Quick contribution](#quick-contribution)

</div>

MiniPdf converts Office documents directly to PDF without Microsoft Office,
LibreOffice, Adobe Acrobat, or COM automation at runtime. Choose the
implementation that matches your project.

## Choose an implementation

| Implementation | Inputs | Interfaces | Maturity | Documentation |
|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | Library, CLI, Native AOT binaries | Stable | **[.NET guide](documents/README.nuget.md)** |
| Rust | XLSX, DOCX, PPTX | Crate, CLI | Experimental | **[Rust guide](minipdf-rs/README.md)** |
| Java | XLSX, DOCX | Library, CLI | Experimental | **[Java source](minipdf-java/)** |
| Python | DOCX | Package, CLI | Experimental | **[Python guide](minipdf-python/README.md)** |
| Node.js | XLSX, DOCX, PPTX | Native package | Experimental | **[Node.js guide](minipdf-node/README.md)** |
| Go | XLSX, DOCX, PPTX | Package, CLI | Experimental | **[Go guide](minipdf-go/README.md)** |

## Quick start

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

Prefer a command-line tool?

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

The [.NET guide](documents/README.nuget.md) covers conversion, custom fonts,
CLI options, and deployment.

### Rust

```bash
cargo add minipdf
cargo install minipdf-cli
```

```rust
minipdf::convert_to_pdf("report.docx", "report.pdf")?;
```

```bash
minipdf report.docx -o report.pdf
```

[The Rust guide](minipdf-rs/README.md) documents the crate API, CLI, supported
features, known gaps, and development workflow.

### Java

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

```java
import io.github.minisoftware.minipdf.MiniPdf;
import java.nio.file.Path;

MiniPdf.convertToPdf(Path.of("report.docx"), Path.of("report.pdf"));
```

### Python

```bash
pip install minipdf
```

```python
import minipdf

minipdf.convert_to_pdf("report.docx", "report.pdf")
```

The [Python guide](minipdf-python/README.md) lists the current DOCX feature scope and CLI options.

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

The [Node.js guide](minipdf-node/README.md) covers in-memory conversion, page sizing, font registration, and supported native platforms.

### Go

```bash
go get github.com/mini-software/MiniPdf/minipdf-go@latest
```

```go
import minipdf "github.com/mini-software/MiniPdf/minipdf-go"

if err := minipdf.ConvertToPDF("report.docx", "report.pdf"); err != nil {
	panic(err)
}
```

The [Go guide](minipdf-go/README.md) documents the package, native CLI, current rendering scope, and release tags.

## Why MiniPdf

- **No office suite required**: conversion runs inside your application or CLI.
- **Small deployment surface**: minimal dependencies and no external process.
- **Server and CI friendly**: works in containers, cloud services, and pipelines.
- **Multiple language options**: use MiniPdf from .NET, Rust, Java, Python, Node.js, or Go.
- **Native command-line options**: available for .NET, Rust, Java, Python, and Go.
- **Open development**: Apache 2.0 licensed with reproducible visual benchmarks.

MiniPdf targets practical document conversion, not complete Microsoft Office
layout compatibility. Complex templates may render differently; use the online
demo or benchmark reports to evaluate representative files.

<a id="quick-contribution"></a>

## Quick contribution

Open a clean fork or clone in any coding agent, then paste this prompt into the
agent chat:

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop from start to finish. Detect the installed supported language toolchains, randomly choose one available implementation, diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

## Project resources

| Resource | Description |
|---|---|
| [Online demo](https://mini-software.github.io/MiniPdf/) | Try conversion in a browser |
| [.NET documentation](documents/README.nuget.md) | Stable library and CLI usage |
| [Rust documentation](minipdf-rs/README.md) | Experimental crate and CLI usage |
| [Java implementation](minipdf-java/) | Experimental Maven library and CLI source |
| [Python documentation](minipdf-python/README.md) | Experimental package and CLI usage |
| [Node.js documentation](minipdf-node/README.md) | Experimental native package usage |
| [Go documentation](minipdf-go/README.md) | Experimental package and CLI usage |
| [.NET XLSX benchmark](tests/MiniPdf.Benchmark/reports/comparison_report.md) | Visual comparison results for spreadsheets |
| [.NET DOCX benchmark](tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | Visual comparison results for documents |
| [Rust XLSX benchmark](artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Rust spreadsheet visual comparison results |
| [Rust DOCX benchmark](artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Rust document visual comparison results |
| [Rust benchmark workflow](scripts/Run-Rust-Benchmark.ps1) | Generate fixture coverage and comparison reports |
| [Community governance](GOVERNANCE.md) | Decisions, roles, voting, and maintainer selection |
| [Roadmap](ROADMAP.md) | Project scope, implementation status, and current priorities |
| [Security](SECURITY.md) | Private vulnerability reporting and supported versions |
| [Contributing](CONTRIBUTING.md) | Development setup, tests, reviews, and provenance requirements |
| [GitHub releases](https://github.com/mini-software/MiniPdf/releases) | Packages and standalone binaries |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bugs, compatibility reports, and feature requests |

## License

[Apache License 2.0](LICENSE). Commercial use is welcome; retain the required
notices and attribution.
