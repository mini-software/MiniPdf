<div align="center">

**.NET 向けの軽量・高速・ネイティブな Office-to-PDF ライブラリおよびコマンドラインツール。**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | 日本語 | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[オンラインデモ](https://mini-software.github.io/MiniPdf/)** · **[リリース](https://github.com/mini-software/MiniPdf/releases)** · **[問題を報告](https://github.com/mini-software/MiniPdf/issues)**

スターや寄付が MiniPdf をより良くします。

🤝 **共同開発者を募集中：**[すぐに貢献する](#quick-contribution)

</div>

MiniPdf provides a .NET library, command-line tool, and standalone Native AOT binaries for XLSX, DOCX, and PPTX conversion.

MiniPdf は、実行時に Microsoft Office、LibreOffice、Adobe Acrobat、COM オートメーションを必要とせず、Office 文書を直接 PDF に変換します。

## .NET

| 実装 | 入力 | インターフェイス | 成熟度 | ドキュメント | 視覚結果 |
|---|---|---|---|---|---|
| .NET | XLSX、DOCX、PPTX | ライブラリ、CLI、Native AOT バイナリ | 安定版 | **[.NET ガイド](README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |

## クイックスタート

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

コマンドラインを使う場合：

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
minipdf workbook.xlsx -o workbook.pdf --compress
minipdf slides.pptx -o slides.pdf
minipdf --help
```

使用方法については [.NET ガイド](README.nuget.md) をご覧ください。

## Usage examples

### Convert streams

```csharp
using System.IO;
using MiniSoftware;

using var input = File.OpenRead("report.docx");
using var output = File.Create("stream-output.pdf");
MiniPdf.ConvertToPdf(input, output);
```

The caller owns the streams and disposes them. Use the overload returning
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

This opt-in XLSX mode uses temporary disk storage. Unsupported features, such
as formulas, merged cells, and drawings, produce an explicit error rather than
silently falling back. Read the [supported features and limitations](README.nuget.md#low-memory-xlsx-conversion-opt-in)
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

## MiniPdf を選ぶ理由

- **Office スイート不要**：変換はアプリケーションまたは CLI 内で実行されます。
- **小さなデプロイ構成**：依存関係が少なく、外部プロセスも不要です。
- **サーバーと CI に対応**：コンテナー、クラウドサービス、パイプラインで利用できます。

MiniPdf は実用的な文書変換を目的としており、Microsoft Office のレイアウトを完全に再現するものではありません。複雑なテンプレートは表示が異なる場合があるため、オンラインデモやベンチマークレポートで代表的なファイルを評価してください。

<a id="quick-contribution"></a>

## すぐに貢献する

任意のコーディング Agent でクリーンな fork または clone を開き、次の指示を Agent のチャットに貼り付けます。

```text
Run the minipdf-contribution skill.
```

## プロジェクトリソース

| リソース | 説明 |
|---|---|
| [オンラインデモ](https://mini-software.github.io/MiniPdf/) | ブラウザーで変換を試す |
| [.NET ドキュメント](README.nuget.md) | 安定版ライブラリと CLI の使用方法 |
| [.NET XLSX ベンチマーク](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx) | スプレッドシートの視覚比較結果 |
| [.NET DOCX ベンチマーク](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx) | 文書の視覚比較結果 |
| [コミュニティガバナンス](../GOVERNANCE.md) | 意思決定、役割、投票、メンテナー選出 |
| [ロードマップ](../ROADMAP.md) | プロジェクト範囲、実装状況、現在の優先事項 |
| [セキュリティ](../SECURITY.md) | 脆弱性の非公開報告とサポート対象バージョン |
| [コントリビューション](../CONTRIBUTING.md) | 開発環境、テスト、レビュー、来歴要件 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | パッケージと単体バイナリ |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | バグ、互換性レポート、機能リクエスト |

## ライセンス

[Apache License 2.0](../LICENSE) で提供されています。商用利用を歓迎します。
