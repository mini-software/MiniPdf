<div align="center">

# MiniPdf

**面向 .NET、Rust、Java、Python、Node.js 与 Go 的轻量级 Office 转 PDF 库和命令行工具。**

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

<a href="../README.md">English</a> | 简体中文 | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[在线演示](https://mini-software.github.io/MiniPdf/)** · **[版本发布](https://github.com/mini-software/MiniPdf/releases)** · **[报告问题](https://github.com/mini-software/MiniPdf/issues)**

你的 Star 或捐赠将帮助项目持续发展。

🤝 **寻找共同开发者：**[快速贡献](#quick-contribution)

</div>

MiniPdf 无需在运行时安装 Microsoft Office、LibreOffice、Adobe Acrobat 或使用 COM 自动化，即可将 Office 文档直接转换为 PDF。请选择适合你项目的实现。

## 选择实现

| 实现 | 输入 | 接口 | 成熟度 | 文档 | 视觉结果 |
|---|---|---|---|---|---|
| .NET | XLSX、DOCX、PPTX | 库、CLI、Native AOT 二进制文件 | 稳定 | **[.NET 指南](README.nuget.md)** | **[XLSX](../tests/MiniPdf.Benchmark/reports/comparison_report.md)**<br>**[DOCX](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md)**<br>**[PPTX](../tests/Issue_Files/reports_pptx/comparison_report.md)** |
| Rust | XLSX、DOCX、PPTX | Crate、CLI | 实验性 | **[Rust 指南](../minipdf-rs/README.md)** | **[XLSX](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md)**<br>**[DOCX](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md)** |
| Java | XLSX、DOCX | 库、CLI | 实验性 | **[Java 源代码](../minipdf-java/)** | **[XLSX](../artifacts/java-benchmark/issue/xlsx/report/comparison_report.md)** |
| Python | DOCX | 包、CLI | 实验性 | **[Python 指南](../minipdf-python/README.md)** | **[XLSX](../artifacts/python-benchmark/issue/xlsx/report/comparison_report.md)** |
| Node.js | XLSX、DOCX、PPTX | 原生包 | 实验性 | **[Node.js 指南](../minipdf-node/README.md)** | **[XLSX](../artifacts/node-benchmark/issue/xlsx/report/comparison_report.md)** |
| Go | XLSX、DOCX、PPTX | 包、CLI | 实验性 | **[Go 指南](../minipdf-go/README.md)** | **[XLSX](../artifacts/go-benchmark/issue/xlsx/report/comparison_report.md)** |

## 快速开始

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

命令行用法：

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

[.NET 指南](README.nuget.md)涵盖转换、自定义字体、CLI 选项和部署。

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

[Rust 指南](../minipdf-rs/README.md)介绍 crate API、CLI、支持范围、已知限制和开发流程。

### Java

```xml
<dependency>
	<groupId>io.github.mini-software</groupId>
	<artifactId>minipdf</artifactId>
	<version>0.1.2</version>
</dependency>
```

Maven `groupId` 可以包含连字符，但 Java 包名不能。因此依赖坐标使用
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

[Python 指南](../minipdf-python/README.md)列出了当前 DOCX 功能范围和 CLI 选项。

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

[Node.js 指南](../minipdf-node/README.md)涵盖内存转换、页面尺寸、字体注册和支持的原生平台。

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

[Go 指南](../minipdf-go/README.md)介绍包、原生 CLI、当前渲染范围和发布标签。

## 为什么选择 MiniPdf

- **无需 Office 套件**：转换直接在应用程序或 CLI 内运行。
- **部署精简**：依赖少，无需外部进程。
- **适合服务器与 CI**：可用于容器、云服务和流水线。
- **多语言选择**：可从 .NET、Rust、Java、Python、Node.js 或 Go 使用 MiniPdf。
- **原生命令行方案**：适用于 .NET、Rust、Java、Python 与 Go。

MiniPdf 专注于实用文档转换，不追求完全复制 Microsoft Office 版式。复杂模板的渲染可能有所不同，请使用在线演示或基准报告评估代表性文件。

<a id="quick-contribution"></a>

## 快速贡献

在任意编码 Agent 中打开干净的 fork 或 clone，然后将以下指令粘贴到 Agent 聊天中：

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop from start to finish. Detect the installed supported language toolchains, randomly choose one available implementation, diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

## 项目资源

| 资源 | 说明 |
|---|---|
| [在线演示](https://mini-software.github.io/MiniPdf/) | 在浏览器中试用转换 |
| [.NET 文档](README.nuget.md) | 稳定版库与 CLI 用法 |
| [Rust 文档](../minipdf-rs/README.md) | 实验版 crate 与 CLI 用法 |
| [Java 实现](../minipdf-java/) | 实验性 Maven 库与 CLI 源代码 |
| [Python 文档](../minipdf-python/README.md) | 实验性包与 CLI 用法 |
| [Node.js 文档](../minipdf-node/README.md) | 实验性原生包用法 |
| [Go 文档](../minipdf-go/README.md) | 实验性包与 CLI 用法 |
| [.NET XLSX 基准](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | 电子表格视觉比较结果 |
| [.NET DOCX 基准](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | 文档视觉比较结果 |
| [Rust XLSX 基准](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Rust 电子表格视觉比较结果 |
| [Rust DOCX 基准](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Rust 文档视觉比较结果 |
| [Rust 基准流程](../scripts/Run-Rust-Benchmark.ps1) | 生成测试覆盖与比较报告 |
| [社区治理](../GOVERNANCE.md) | 决策、角色、投票和维护者选任 |
| [路线图](../ROADMAP.md) | 项目范围、实现状态和当前优先事项 |
| [安全政策](../SECURITY.md) | 私密漏洞报告和支持版本 |
| [贡献指南](../CONTRIBUTING.md) | 开发环境、测试、审查和来源要求 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | 软件包与独立二进制文件 |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | 缺陷、兼容性报告和功能建议 |

## 许可证

采用 [Apache License 2.0](../LICENSE)。允许商业使用，但须保留必要声明和署名。
