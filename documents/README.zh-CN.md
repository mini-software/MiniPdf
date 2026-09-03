<div align="center">

# MiniPdf

**面向 .NET 与 Rust 的轻量级 Office 转 PDF 库和命令行工具。**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | 简体中文 | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[在线演示](https://mini-software.github.io/MiniPdf/)** · **[版本发布](https://github.com/mini-software/MiniPdf/releases)** · **[报告问题](https://github.com/mini-software/MiniPdf/issues)**

你的 Star 或捐赠将帮助项目持续发展。

</div>

MiniPdf 无需在运行时安装 Microsoft Office、LibreOffice、Adobe Acrobat 或使用 COM 自动化，即可将 Office 文档直接转换为 PDF。请选择适合你项目的实现。

## 选择实现

| | .NET | Rust |
|---|---|---|
| 输入 | XLSX、DOCX、PPTX | XLSX、DOCX |
| 接口 | .NET 库、CLI、独立 Native AOT 二进制文件 | Rust crate、CLI |
| 文档 | **[打开 .NET 指南](README.nuget.md)** | **[打开 Rust 指南](../minipdf-rs/README.md)** |

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

## 为什么选择 MiniPdf

- **无需 Office 套件**：转换直接在应用程序或 CLI 内运行。
- **部署精简**：依赖少，无需外部进程。
- **适合服务器与 CI**：可用于容器、云服务和流水线。
- **原生命令行方案**：提供 .NET Native AOT 版本与 Rust CLI。

MiniPdf 专注于实用文档转换，不追求完全复制 Microsoft Office 版式。复杂模板的渲染可能有所不同，请使用在线演示或基准报告评估代表性文件。

## 贡献你的算力

在 GitHub Copilot、Claude Code、Cursor、Codex，或任何可编辑文件并运行 PowerShell 的编码 Agent 中打开干净的 fork 或 clone。最简单的贡献方式是将以下指令粘贴到 Agent 聊天中：

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop for dotnet from start to finish. Diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

如需改进 Rust 实现，请将 `dotnet` 替换为 `rust`。通用 PowerShell 入口为：

```powershell
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation dotnet
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation rust
```

贡献循环会检查工具链、安装基准测试所需的 Python 包、创建本地分支，并自动挑选所选渲染器中视觉分数最低的两个差异。Agent 对每个案例最多尝试三次；分数没有改善时会自动还原，再继续下一个案例。只有所选实现的完整测试以及 XLSX/DOCX 视觉基准都通过，且没有明显回归时，才允许创建 PR。

两条路径都需要 Git、Python 3.10+ 和 LibreOffice。.NET 需要 .NET 9 SDK；Rust 需要 Cargo，并使用桌面版 Excel 与 Word 生成目前的主要视觉参考。流程会检查是否有已认证的 GitHub CLI；若没有，则生成 PR 标题、正文、推送命令和浏览器链接。Copilot、Claude Code、Cursor、Codex 与终端的快捷方式及安全规则请参阅[贡献指南](../CONTRIBUTING.md)。未经你的确认，它不会提交、推送或创建 PR。

## 项目资源

| 资源 | 说明 |
|---|---|
| [在线演示](https://mini-software.github.io/MiniPdf/) | 在浏览器中试用转换 |
| [.NET 文档](README.nuget.md) | 稳定版库与 CLI 用法 |
| [Rust 文档](../minipdf-rs/README.md) | 实验版 crate 与 CLI 用法 |
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
