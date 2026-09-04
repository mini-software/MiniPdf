<div align="center">

# MiniPdf

**Librerie e strumenti da riga di comando leggeri per convertire documenti Office in PDF con .NET, Rust, Java, Python, Node.js e Go.**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://central.sonatype.com/artifact/io.github.shps951023/minipdf"><img src="https://img.shields.io/maven-central/v/io.github.shps951023/minipdf.svg" alt="Maven Central"></a>
<a href="https://pypi.org/project/minipdf/"><img src="https://img.shields.io/pypi/v/minipdf.svg" alt="PyPI"></a>
<a href="https://www.npmjs.com/package/minipdf"><img src="https://img.shields.io/npm/v/minipdf.svg" alt="npm"></a>
<a href="https://pkg.go.dev/github.com/mini-software/MiniPdf/minipdf-go"><img src="https://pkg.go.dev/badge/github.com/mini-software/MiniPdf/minipdf-go.svg" alt="Go Reference"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | Italiano | <a href="README.fr.md">Français</a>

**[Demo online](https://mini-software.github.io/MiniPdf/)** · **[Release](https://github.com/mini-software/MiniPdf/releases)** · **[Segnala un problema](https://github.com/mini-software/MiniPdf/issues)**

La tua stella o donazione aiuta a sostenere il progetto.

🤝 **Cerchiamo co-sviluppatori:** [Contribuisci subito](#quick-contribution)

</div>

MiniPdf converte direttamente i documenti Office in PDF senza richiedere Microsoft Office, LibreOffice, Adobe Acrobat o automazione COM durante l'esecuzione. Scegli l'implementazione adatta al tuo progetto.

## Scegli un'implementazione

| Implementazione | Input | Interfacce | Maturità | Documentazione |
|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | Libreria, CLI, binari Native AOT | Stabile | **[Guida .NET](README.nuget.md)** |
| Rust | XLSX, DOCX, PPTX | Crate, CLI | Sperimentale | **[Guida Rust](../minipdf-rs/README.md)** |
| Java | XLSX, DOCX | Libreria, CLI | Sperimentale | **[Sorgenti Java](../minipdf-java/)** |
| Python | DOCX | Pacchetto, CLI | Sperimentale | **[Guida Python](../minipdf-python/README.md)** |
| Node.js | XLSX, DOCX, PPTX | Pacchetto nativo | Sperimentale | **[Guida Node.js](../minipdf-node/README.md)** |
| Go | XLSX, DOCX, PPTX | Pacchetto, CLI | Sperimentale | **[Guida Go](../minipdf-go/README.md)** |

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

La [guida .NET](README.nuget.md) descrive conversione, font personalizzati, opzioni CLI e distribuzione.

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

La [guida Rust](../minipdf-rs/README.md) descrive API del crate, CLI, funzionalità supportate, limiti noti e flusso di sviluppo.

### Java

```xml
<dependency>
	<groupId>io.github.shps951023</groupId>
	<artifactId>minipdf</artifactId>
	<version>0.1.0</version>
</dependency>
```

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

La [guida Python](../minipdf-python/README.md) descrive l'ambito DOCX attuale e le opzioni CLI.

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

La [guida Node.js](../minipdf-node/README.md) descrive la conversione in memoria, le dimensioni della pagina, la registrazione dei font e le piattaforme native supportate.

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

La [guida Go](../minipdf-go/README.md) descrive il pacchetto, la CLI nativa, l'ambito di rendering attuale e i tag di release.

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
Read CONTRIBUTING.md and run the MiniPdf contribution loop from start to finish. Detect the installed supported language toolchains, randomly choose one available implementation, diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
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
| [Benchmark XLSX .NET](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | Risultati visivi per i fogli di calcolo |
| [Benchmark DOCX .NET](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | Risultati visivi per i documenti |
| [Benchmark XLSX Rust](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Risultati del confronto visivo Rust per i fogli di calcolo |
| [Benchmark DOCX Rust](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Risultati del confronto visivo Rust per i documenti |
| [Flusso benchmark Rust](../scripts/Run-Rust-Benchmark.ps1) | Genera copertura dei test e report comparativi |
| [Governance della comunità](../GOVERNANCE.md) | Decisioni, ruoli, votazioni e selezione dei maintainer |
| [Roadmap](../ROADMAP.md) | Ambito del progetto, stato delle implementazioni e priorità |
| [Sicurezza](../SECURITY.md) | Segnalazione privata delle vulnerabilità e versioni supportate |
| [Contribuire](../CONTRIBUTING.md) | Ambiente, test, revisioni e requisiti di provenienza |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | Pacchetti e binari autonomi |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bug, problemi di compatibilità e richieste di funzionalità |

## Licenza

[Apache License 2.0](../LICENSE). L'uso commerciale è consentito mantenendo gli avvisi e le attribuzioni richieste.
