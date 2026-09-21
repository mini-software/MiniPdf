<div align="center">

**Librerie e strumenti da riga di comando Office-to-PDF leggeri, veloci e nativi per Rust, .NET, Java, Python, JS e Go.**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://central.sonatype.com/artifact/io.github.mini-software/minipdf"><img src="https://img.shields.io/maven-central/v/io.github.mini-software/minipdf.svg" alt="Maven Central"></a>
<a href="https://pypi.org/project/minipdf/"><img src="https://img.shields.io/pypi/v/minipdf.svg" alt="PyPI"></a>
<a href="https://www.npmjs.com/package/minipdf"><img src="https://img.shields.io/npm/v/minipdf.svg" alt="npm"></a>
<a href="https://pkg.go.dev/github.com/mini-software/MiniPdf/minipdf-go"><img src="https://pkg.go.dev/badge/github.com/mini-software/MiniPdf/minipdf-go.svg" alt="Go Reference"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | Italiano | <a href="README.fr.md">Français</a>

**[Demo online](https://mini-software.github.io/MiniPdf/)** · **[Release](https://github.com/mini-software/MiniPdf/releases)** · **[Segnala un problema](https://github.com/mini-software/MiniPdf/issues)**

La tua stella o donazione può rendere MiniPdf migliore.

🤝 **Cerchiamo co-sviluppatori:** [Contribuisci subito](#quick-contribution)

</div>

MiniPdf converte direttamente i documenti Office in PDF senza richiedere Microsoft Office, LibreOffice, Adobe Acrobat o automazione COM durante l'esecuzione.

## Scegli un'implementazione

| Implementazione | Input | Interfacce | Maturità | Documentazione | Risultati visivi |
|---|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | Libreria, CLI, binari Native AOT | Stabile | **[Guida .NET](README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |
| Rust | XLSX, DOCX, PPTX | Crate, CLI | Stabile | **[Guida Rust](../minipdf-rs/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=docx)** |
| Java | XLSX, DOCX, PPTX | Libreria, CLI | Sperimentale | **[Sorgenti Java](../minipdf-java/)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=java&suite=issue&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=java&suite=issue&format=docx)** |
| Python | DOCX | Pacchetto, CLI | Stabile | **[Guida Python](../minipdf-python/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=python&suite=issue&format=xlsx)** |
| Node.js | XLSX, DOCX, PPTX | Pacchetto nativo | Stabile | **[Guida Node.js](../minipdf-node/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=node&suite=issue&format=xlsx)** |
| Go | XLSX, DOCX, PPTX | Pacchetto, CLI | Sperimentale | **[Guida Go](../minipdf-go/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=go&suite=issue&format=xlsx)** |

## Avvio rapido

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

Per la riga di comando:

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

Per l'uso, consulta la [guida .NET](README.nuget.md).

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

Per l'uso, consulta la [guida Rust](../minipdf-rs/README.md).

### Java

[Maven Central artifact](https://central.sonatype.com/artifact/io.github.mini-software/minipdf)

```java
import io.github.minisoftware.minipdf.MiniPdf;
import java.nio.file.Files;
import java.nio.file.Path;

try {
	MiniPdf.registerFont(
			"Noto Sans",
			Files.readAllBytes(Path.of("fonts/NotoSans-Regular.ttf")));
	MiniPdf.convertToPdf(Path.of("report.docx"), Path.of("report.pdf"));
} finally {
	MiniPdf.clearRegisteredFonts();
}
```

Per l'uso, consulta la [guida Java](../minipdf-java/README.md).

### Python

```bash
pip install minipdf
```

```python
import minipdf

minipdf.convert_to_pdf("report.docx", "report.pdf")
```

Per l'uso, consulta la [guida Python](../minipdf-python/README.md).

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

Per l'uso, consulta la [guida Node.js](../minipdf-node/README.md).

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

Per l'uso, consulta la [guida Go](../minipdf-go/README.md).

## Perché MiniPdf

- **Nessuna suite Office richiesta**: la conversione viene eseguita nell'applicazione o nella CLI.
- **Distribuzione essenziale**: poche dipendenze e nessun processo esterno.
- **Adatto a server e CI**: funziona in container, servizi cloud e pipeline.
- **Più linguaggi disponibili**: usa MiniPdf da .NET, Rust, Java, Python, Node.js o Go.
- **Opzioni native da riga di comando**: disponibili per .NET, Rust, Java, Python e Go.

MiniPdf punta alla conversione pratica dei documenti, non alla compatibilità completa con il layout di Microsoft Office. I modelli complessi possono essere visualizzati diversamente; usa la demo online o i report di benchmark per valutare file rappresentativi.

<a id="quick-contribution"></a>

## Contribuisci subito

Apri un fork o clone pulito in qualsiasi agente di programmazione, quindi incolla questa istruzione nella chat dell'Agent:

```text
Run the minipdf-contribution skill.
```

## Risorse del progetto

| Risorsa | Descrizione |
|---|---|
| [Demo online](https://mini-software.github.io/MiniPdf/) | Prova la conversione nel browser |
| [Documentazione .NET](README.nuget.md) | Uso della libreria stabile e della CLI |
| [Documentazione Rust](../minipdf-rs/README.md) | Uso del crate sperimentale e della CLI |
| [Implementazione Java](../minipdf-java/) | Sorgenti della libreria Maven e della CLI sperimentali |
| [Documentazione Python](../minipdf-python/README.md) | Uso del pacchetto sperimentale e della CLI |
| [Documentazione Node.js](../minipdf-node/README.md) | Uso del pacchetto nativo sperimentale |
| [Documentazione Go](../minipdf-go/README.md) | Uso del pacchetto sperimentale e della CLI |
| [Benchmark XLSX .NET](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx) | Risultati visivi per i fogli di calcolo |
| [Benchmark DOCX .NET](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx) | Risultati visivi per i documenti |
| [Benchmark XLSX Rust](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=xlsx) | Risultati del confronto visivo Rust per i fogli di calcolo |
| [Benchmark DOCX Rust](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=docx) | Risultati del confronto visivo Rust per i documenti |
| [Flusso benchmark Rust](../scripts/Run-Rust-Benchmark.ps1) | Genera copertura dei test e report comparativi |
| [Governance della comunità](../GOVERNANCE.md) | Decisioni, ruoli, votazioni e selezione dei maintainer |
| [Roadmap](../ROADMAP.md) | Ambito del progetto, stato delle implementazioni e priorità |
| [Sicurezza](../SECURITY.md) | Segnalazione privata delle vulnerabilità e versioni supportate |
| [Contribuire](../CONTRIBUTING.md) | Ambiente, test, revisioni e requisiti di provenienza |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | Pacchetti e binari autonomi |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bug, problemi di compatibilità e richieste di funzionalità |

## Licenza

[Apache License 2.0](../LICENSE). L'uso commerciale è benvenuto.
