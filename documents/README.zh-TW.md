<div align="center">

**輕量、快速且原生的 Office 轉 PDF 程式庫與命令列工具，支援 .NET。**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | 繁體中文 | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[線上展示](https://mini-software.github.io/MiniPdf/)** · **[版本發布](https://github.com/mini-software/MiniPdf/releases)** · **[回報問題](https://github.com/mini-software/MiniPdf/issues)**

你的 Star 或捐款能讓 MiniPdf 更好。

🤝 **尋找共同開發者：**[快速貢獻](#quick-contribution)

</div>



MiniPdf 不需要在執行階段安裝 Microsoft Office、LibreOffice、Adobe Acrobat 或使用 COM 自動化，即可直接將 Office 文件轉換為 PDF。

## .NET

| 實作 | 輸入 | 介面 | 成熟度 | 文件 | 視覺結果 |
|---|---|---|---|---|---|
| .NET | XLSX、DOCX、PPTX | 程式庫、CLI、Native AOT 二進位檔 | 穩定 | **[.NET 指南](README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |

## 快速開始

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("workbook.xlsx", "workbook.pdf");
MiniPdf.ConvertToPdf("report.docx", "report.pdf");
MiniPdf.ConvertToPdf("slides.pptx", "slides.pdf");

byte[] pdfBytes = MiniPdf.ConvertToPdf("report.docx");
```

命令列用法：

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
minipdf workbook.xlsx -o workbook.pdf --compress
minipdf slides.pptx -o slides.pdf
minipdf --help
```

使用方式請參閱 [.NET 指南](README.nuget.md)。

## Usage examples

### Convert streams

```csharp
using System.IO;
using MiniSoftware;

using var input = File.OpenRead("report.docx");
using var output = File.Create("stream-output.pdf");
MiniPdf.ConvertToPdf(input, output);
```

Dispose the input and output streams in the calling code. Use the overload returning
`byte[]` when an in-memory result is needed.

### Select Excel sheets and control layout

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("workbook.xlsx", "summary.pdf", new MiniPdfConversionOptions
{
    Sheets = new[] { "Summary" },
    Landscape = true,
    FitToPage = true,
    Compress = true,
});
```

Sheet names, 1-based `SheetIndexes`, orientation, print scale, and fit-to-page
options apply to XLSX input. See the [conversion options](README.nuget.md#conversion-options)
for the complete list.

### Set Word page size and margins

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "a4-report.pdf", new MiniPdfConversionOptions
{
    PageSize = MiniPdfPageSize.A4,
    Margins = new MiniPdfMargins(left: 36, top: 48, right: 36, bottom: 48),
});
```

`PageSize` and `Margins` apply to DOCX input. Dimensions use PDF points:
72 points equal one inch. They do not override XLSX worksheet or PPTX slide sizes.

### Reduce memory for large, simple Excel tables

```csharp
using System.IO;
using MiniSoftware;

using var input = File.OpenRead("workbook.xlsx");
using var output = File.Create("low-memory.pdf");
MiniPdf.ConvertToPdf(input, output, new MiniPdfConversionOptions
{
    LowMemory = true,
    Compress = true,
});
```

Low-memory XLSX conversion requires writable temporary storage. Features such
as formulas, merged cells, and drawings are unsupported and return an error; there is no
automatic fallback. Read the [supported features and limitations](README.nuget.md#low-memory-xlsx-conversion-opt-in)
before enabling it. Returning `byte[]` still retains the complete PDF in memory.

### Provide fonts for headless hosts

```csharp
using System.IO;
using MiniSoftware;

MiniPdf.RegisterFont("Arial", File.ReadAllBytes(Path.Combine("Fonts", "arial.ttf")));
MiniPdf.ConvertToPdf("report.docx", "font-report.pdf");
```

Supply licensed `.ttf` or `.ttc` files that contain the required glyphs.
Register fonts once at application startup: registrations are process-wide.
MiniPdf does not distribute these font files.

See the [.NET library guide](README.nuget.md) and
[CLI reference](README.nuget.cli.md) for more examples and options.

## 為什麼選擇 MiniPdf

- **不需要 Office 套件**：轉換直接在應用程式或 CLI 內執行。
- **精簡部署**：相依套件少，不需要外部程序。
- **適合伺服器與 CI**：可用於容器、雲端服務與工作流程。

MiniPdf 專注於實用文件轉換，不追求完整複製 Microsoft Office 版面。複雜範本的呈現可能不同，請使用線上展示或基準報告評估代表性檔案。

<a id="quick-contribution"></a>

## Contributing

Submit changes through [pull requests](https://github.com/mini-software/MiniPdf/pulls).
For bug reports, include a small input file and the steps needed to reproduce the problem.

## 專案資源

| 資源 | 說明 |
|---|---|
| [線上展示](https://mini-software.github.io/MiniPdf/) | 在瀏覽器中試用轉換 |
| [.NET 文件](README.nuget.md) | 穩定版程式庫與 CLI 用法 |
| [.NET XLSX 基準](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx) | 試算表視覺比較結果 |
| [.NET DOCX 基準](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx) | 文件視覺比較結果 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | 套件與獨立二進位檔 |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | 錯誤、相容性報告與功能建議 |

## 授權

採用 [Apache License 2.0](../LICENSE)。歡迎商業使用。
