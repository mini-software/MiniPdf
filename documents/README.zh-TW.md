<div align="center">

# MiniPdf

**專為 .NET、Rust、Java、Python、Node.js 與 Go 打造的輕量級 Office 轉 PDF 程式庫與命令列工具。**

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

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | 繁體中文 | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[線上展示](https://mini-software.github.io/MiniPdf/)** · **[版本發布](https://github.com/mini-software/MiniPdf/releases)** · **[回報問題](https://github.com/mini-software/MiniPdf/issues)**

你的 Star 或捐款將幫助專案持續發展。

🤝 **尋找共同開發者：**[快速貢獻](#quick-contribution)

</div>

MiniPdf 不需要在執行階段安裝 Microsoft Office、LibreOffice、Adobe Acrobat 或使用 COM 自動化，即可直接將 Office 文件轉換為 PDF。請選擇適合專案的實作。

## 選擇實作

| 實作 | 輸入 | 介面 | 成熟度 | 文件 | 視覺結果 |
|---|---|---|---|---|---|
| .NET | XLSX、DOCX、PPTX | 程式庫、CLI、Native AOT 二進位檔 | 穩定 | **[.NET 指南](README.nuget.md)** | **[XLSX](../tests/MiniPdf.Benchmark/reports/comparison_report.md)**<br>**[DOCX](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md)**<br>**[PPTX](../tests/Issue_Files/reports_pptx/comparison_report.md)** |
| Rust | XLSX、DOCX、PPTX | Crate、CLI | 實驗性 | **[Rust 指南](../minipdf-rs/README.md)** | **[XLSX](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md)**<br>**[DOCX](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md)** |
| Java | XLSX、DOCX | 程式庫、CLI | 實驗性 | **[Java 原始碼](../minipdf-java/)** | **[XLSX](../artifacts/java-benchmark/issue/xlsx/report/comparison_report.md)** |
| Python | DOCX | 套件、CLI | 實驗性 | **[Python 指南](../minipdf-python/README.md)** | **[XLSX](../artifacts/python-benchmark/issue/xlsx/report/comparison_report.md)** |
| Node.js | XLSX、DOCX、PPTX | 原生套件 | 實驗性 | **[Node.js 指南](../minipdf-node/README.md)** | **[XLSX](../artifacts/node-benchmark/issue/xlsx/report/comparison_report.md)** |
| Go | XLSX、DOCX、PPTX | 套件、CLI | 實驗性 | **[Go 指南](../minipdf-go/README.md)** | **[XLSX](../artifacts/go-benchmark/issue/xlsx/report/comparison_report.md)** |

## 快速開始

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

命令列用法：

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

[.NET 指南](README.nuget.md)涵蓋轉換、自訂字型、CLI 選項與部署。

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

[Rust 指南](../minipdf-rs/README.md)說明 crate API、CLI、支援範圍、已知限制與開發流程。

### Java

```xml
<dependency>
	<groupId>io.github.mini-software</groupId>
	<artifactId>minipdf</artifactId>
	<version>0.1.2</version>
</dependency>
```

Maven `groupId` 可以包含連字號，但 Java 套件名稱不能。因此相依套件座標使用
`io.github.mini-software`，import 使用 `io.github.minisoftware.minipdf`。

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

[Python 指南](../minipdf-python/README.md)列出目前的 DOCX 功能範圍與 CLI 選項。

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

[Node.js 指南](../minipdf-node/README.md)涵蓋記憶體內轉換、頁面尺寸、字型註冊與支援的原生平台。

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

[Go 指南](../minipdf-go/README.md)說明套件、原生 CLI、目前的呈現範圍與發布標籤。

## 為什麼選擇 MiniPdf

- **不需要 Office 套件**：轉換直接在應用程式或 CLI 內執行。
- **精簡部署**：相依套件少，不需要外部程序。
- **適合伺服器與 CI**：可用於容器、雲端服務與工作流程。
- **多語言選擇**：可從 .NET、Rust、Java、Python、Node.js 或 Go 使用 MiniPdf。
- **原生命令列選項**：適用於 .NET、Rust、Java、Python 與 Go。

MiniPdf 專注於實用文件轉換，不追求完整複製 Microsoft Office 版面。複雜範本的呈現可能不同，請使用線上展示或基準報告評估代表性檔案。

<a id="quick-contribution"></a>

## 快速貢獻

在任意程式設計 Agent 中開啟乾淨的 fork 或 clone，然後將以下指令貼到 Agent 聊天中：

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop from start to finish. Detect the installed supported language toolchains, randomly choose one available implementation, diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

## 專案資源

| 資源 | 說明 |
|---|---|
| [線上展示](https://mini-software.github.io/MiniPdf/) | 在瀏覽器中試用轉換 |
| [.NET 文件](README.nuget.md) | 穩定版程式庫與 CLI 用法 |
| [Rust 文件](../minipdf-rs/README.md) | 實驗版 crate 與 CLI 用法 |
| [Java 實作](../minipdf-java/) | 實驗性 Maven 程式庫與 CLI 原始碼 |
| [Python 文件](../minipdf-python/README.md) | 實驗性套件與 CLI 用法 |
| [Node.js 文件](../minipdf-node/README.md) | 實驗性原生套件用法 |
| [Go 文件](../minipdf-go/README.md) | 實驗性套件與 CLI 用法 |
| [.NET XLSX 基準](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | 試算表視覺比較結果 |
| [.NET DOCX 基準](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | 文件視覺比較結果 |
| [Rust XLSX 基準](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Rust 試算表視覺比較結果 |
| [Rust DOCX 基準](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Rust 文件視覺比較結果 |
| [Rust 基準流程](../scripts/Run-Rust-Benchmark.ps1) | 產生測試涵蓋率與比較報告 |
| [社群治理](../GOVERNANCE.md) | 決策、角色、投票與維護者選任 |
| [路線圖](../ROADMAP.md) | 專案範圍、實作狀態與目前優先事項 |
| [安全政策](../SECURITY.md) | 私密漏洞回報與支援版本 |
| [貢獻指南](../CONTRIBUTING.md) | 開發環境、測試、審查與來源要求 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | 套件與獨立二進位檔 |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | 錯誤、相容性報告與功能建議 |

## 授權

採用 [Apache License 2.0](../LICENSE)。允許商業使用，但須保留必要聲明與署名。
