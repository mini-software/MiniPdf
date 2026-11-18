<div align="center">

**轻量、快速且原生的 Office 转 PDF 库和命令行工具，支持 .NET。**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | 简体中文 | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[在线演示](https://mini-software.github.io/MiniPdf/)** · **[版本发布](https://github.com/mini-software/MiniPdf/releases)** · **[报告问题](https://github.com/mini-software/MiniPdf/issues)**

你的 Star 或捐赠能让 MiniPdf 变得更好。

🤝 **寻找共同开发者：**[快速贡献](#quick-contribution)

</div>



MiniPdf 无需在运行时安装 Microsoft Office、LibreOffice、Adobe Acrobat 或使用 COM 自动化，即可将 Office 文档直接转换为 PDF。

## .NET

| 实现 | 输入 | 接口 | 成熟度 | 文档 | 视觉结果 |
|---|---|---|---|---|---|
| .NET | XLSX、DOCX、PPTX | 库、CLI、Native AOT 二进制文件 | 稳定 | **[.NET 指南](README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |

## 快速开始

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

命令行用法：

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
minipdf workbook.xlsx -o workbook.pdf --compress
minipdf slides.pptx -o slides.pdf
minipdf --help
```

使用方式请查看 [.NET 指南](README.nuget.md)

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

## 为什么选择 MiniPdf

- **无需 Office 套件**：转换直接在应用程序或 CLI 内运行。
- **部署精简**：依赖少，无需外部进程。
- **适合服务器与 CI**：可用于容器、云服务和流水线。

MiniPdf 专注于实用文档转换，不追求完全复制 Microsoft Office 版式。复杂模板的渲染可能有所不同，请使用在线演示或基准报告评估代表性文件。

<a id="quick-contribution"></a>

## Contributing

Submit changes through [pull requests](https://github.com/mini-software/MiniPdf/pulls).
For bug reports, include a small input file and the steps needed to reproduce the problem.

## 项目资源

| 资源 | 说明 |
|---|---|
| [在线演示](https://mini-software.github.io/MiniPdf/) | 在浏览器中试用转换 |
| [.NET 文档](README.nuget.md) | 稳定版库与 CLI 用法 |
| [.NET XLSX 基准](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx) | 电子表格视觉比较结果 |
| [.NET DOCX 基准](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx) | 文档视觉比较结果 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | 软件包与独立二进制文件 |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | 缺陷、兼容性报告和功能建议 |

## 许可证

采用 [Apache License 2.0](../LICENSE)。欢迎商业使用。
