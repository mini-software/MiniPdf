<div align="center">

# MiniPdf

**專為 .NET 與 Rust 打造的輕量級 Office 轉 PDF 程式庫與命令列工具。**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | 繁體中文 | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[線上展示](https://mini-software.github.io/MiniPdf/)** · **[版本發布](https://github.com/mini-software/MiniPdf/releases)** · **[回報問題](https://github.com/mini-software/MiniPdf/issues)**

你的 Star 或捐款將幫助專案持續發展。

</div>

MiniPdf 不需要在執行階段安裝 Microsoft Office、LibreOffice、Adobe Acrobat 或使用 COM 自動化，即可直接將 Office 文件轉換為 PDF。請選擇適合專案的實作。

## 選擇實作

| | .NET | Rust |
|---|---|---|
| 輸入 | XLSX、DOCX、PPTX | XLSX、DOCX |
| 介面 | .NET 程式庫、CLI、獨立 Native AOT 二進位檔 | Rust crate、CLI |
| 文件 | **[開啟 .NET 指南](README.nuget.md)** | **[開啟 Rust 指南](../minipdf-rs/README.md)** |

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

## 為什麼選擇 MiniPdf

- **不需要 Office 套件**：轉換直接在應用程式或 CLI 內執行。
- **精簡部署**：相依套件少，不需要外部程序。
- **適合伺服器與 CI**：可用於容器、雲端服務與工作流程。
- **原生命令列選項**：提供 .NET Native AOT 版本與 Rust CLI。

MiniPdf 專注於實用文件轉換，不追求完整複製 Microsoft Office 版面。複雜範本的呈現可能不同，請使用線上展示或基準報告評估代表性檔案。

## 貢獻你的算力

在 GitHub Copilot、Claude Code、Cursor、Codex，或任何可編輯檔案並執行 PowerShell 的程式設計 Agent 中開啟乾淨的 fork 或 clone。最簡單的貢獻方式是將以下指令貼到 Agent 聊天中：

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop for dotnet from start to finish. Diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

如要改善 Rust 實作，請將 `dotnet` 替換為 `rust`。通用 PowerShell 入口為：

```powershell
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation dotnet
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation rust
```

貢獻循環會檢查工具鏈、安裝基準測試所需的 Python 套件、建立本機分支，並自動挑選所選渲染器中視覺分數最低的兩個差異。Agent 對每個案例最多嘗試三次；分數沒有改善時會自動還原，再繼續下一個案例。只有所選實作的完整測試及 XLSX/DOCX 視覺基準都通過，且沒有明顯退步時，才允許建立 PR。

兩條路徑都需要 Git、Python 3.10+ 與 LibreOffice。.NET 需要 .NET 9 SDK；Rust 需要 Cargo，並使用桌面版 Excel 與 Word 產生目前的主要視覺參考。流程會檢查是否有已驗證的 GitHub CLI；若沒有，則產生 PR 標題、內容、推送命令與瀏覽器連結。Copilot、Claude Code、Cursor、Codex 與終端的快捷方式及安全規則請參閱[貢獻指南](../CONTRIBUTING.md)。未經你的確認，它不會 commit、push 或建立 PR。

## 專案資源

| 資源 | 說明 |
|---|---|
| [線上展示](https://mini-software.github.io/MiniPdf/) | 在瀏覽器中試用轉換 |
| [.NET 文件](README.nuget.md) | 穩定版程式庫與 CLI 用法 |
| [Rust 文件](../minipdf-rs/README.md) | 實驗版 crate 與 CLI 用法 |
| [.NET XLSX 基準](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | 試算表視覺比較結果 |
| [.NET DOCX 基準](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | 文件視覺比較結果 |
| [Rust XLSX 基準](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Rust 試算表視覺比較結果 |
| [Rust DOCX 基準](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Rust 文件視覺比較結果 |
| [Rust 基準流程](../scripts/Run-Rust-Benchmark.ps1) | 產生測試涵蓋率與比較報告 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | 套件與獨立二進位檔 |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | 錯誤、相容性報告與功能建議 |

## 授權

採用 [Apache License 2.0](../LICENSE)。允許商業使用，但須保留必要聲明與署名。

你的 [Star](https://github.com/mini-software/MiniPdf) 或[贊助](https://mini-software.github.io/)將協助專案持續發展。
