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

This repository now maintains **MiniPdf for .NET** only. Rust, Java, Go,
Python, and Node.js have moved to their own [PdfLibrarys repositories](https://github.com/PdfLibrarys).
The implementation links below point to those independent repositories.

MiniPdf は、実行時に Microsoft Office、LibreOffice、Adobe Acrobat、COM オートメーションを必要とせず、Office 文書を直接 PDF に変換します。

## 実装を選ぶ

| 実装 | 入力 | インターフェイス | 成熟度 | ドキュメント | 視覚結果 |
|---|---|---|---|---|---|
| .NET | XLSX、DOCX、PPTX | ライブラリ、CLI、Native AOT バイナリ | 安定版 | **[.NET ガイド](README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |
| Rust | XLSX、DOCX、PPTX | Crate、CLI | 安定版 | **[Rust ガイド](https://github.com/PdfLibrarys/minipdf-rs/blob/main/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=docx)** |
| Java | XLSX、DOCX、PPTX | ライブラリ、CLI | 実験版 | **[Java ソース](https://github.com/PdfLibrarys/minipdf-java)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=java&suite=issue&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=java&suite=issue&format=docx)** |
| Python | DOCX | パッケージ、CLI | 安定版 | **[Python ガイド](https://github.com/PdfLibrarys/minipdf-python/blob/main/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=python&suite=issue&format=xlsx)** |
| Node.js | XLSX、DOCX、PPTX | ネイティブパッケージ | 安定版 | **[Node.js ガイド](https://github.com/PdfLibrarys/minipdf-node/blob/main/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=node&suite=issue&format=xlsx)** |
| Go | XLSX、DOCX、PPTX | パッケージ、CLI | 実験版 | **[Go ガイド](https://github.com/PdfLibrarys/minipdf-go/blob/main/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=go&suite=issue&format=xlsx)** |

## クイックスタート

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

コマンドラインを使う場合：

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

使用方法については [.NET ガイド](README.nuget.md) をご覧ください。

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
| [Rust ドキュメント](https://github.com/PdfLibrarys/minipdf-rs/blob/main/README.md) | 実験版 crate と CLI の使用方法 |
| [Java 実装](https://github.com/PdfLibrarys/minipdf-java) | 実験版 Maven ライブラリと CLI のソース |
| [Python ドキュメント](https://github.com/PdfLibrarys/minipdf-python/blob/main/README.md) | 実験版パッケージと CLI の使用方法 |
| [Node.js ドキュメント](https://github.com/PdfLibrarys/minipdf-node/blob/main/README.md) | 実験版ネイティブパッケージの使用方法 |
| [Go ドキュメント](https://github.com/PdfLibrarys/minipdf-go/blob/main/README.md) | 実験版パッケージと CLI の使用方法 |
| [.NET XLSX ベンチマーク](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx) | スプレッドシートの視覚比較結果 |
| [.NET DOCX ベンチマーク](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx) | 文書の視覚比較結果 |
| [Rust XLSX ベンチマーク](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=xlsx) | Rust スプレッドシートの視覚比較結果 |
| [Rust DOCX ベンチマーク](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=docx) | Rust ドキュメントの視覚比較結果 |
| [Rust ベンチマーク手順](https://github.com/PdfLibrarys/minipdf-rs) | フィクスチャ範囲と比較レポートを生成 |
| [コミュニティガバナンス](../GOVERNANCE.md) | 意思決定、役割、投票、メンテナー選出 |
| [ロードマップ](../ROADMAP.md) | プロジェクト範囲、実装状況、現在の優先事項 |
| [セキュリティ](../SECURITY.md) | 脆弱性の非公開報告とサポート対象バージョン |
| [コントリビューション](../CONTRIBUTING.md) | 開発環境、テスト、レビュー、来歴要件 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | パッケージと単体バイナリ |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | バグ、互換性レポート、機能リクエスト |

## ライセンス

[Apache License 2.0](../LICENSE) で提供されています。商用利用を歓迎します。
