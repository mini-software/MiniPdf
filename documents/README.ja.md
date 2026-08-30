<div align="center">

# MiniPdf

**.NET と Rust 向けの軽量な Office-to-PDF ライブラリおよびコマンドラインツール。**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | 日本語 | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[オンラインデモ](https://mini-software.github.io/MiniPdf/)** · **[リリース](https://github.com/mini-software/MiniPdf/releases)** · **[問題を報告](https://github.com/mini-software/MiniPdf/issues)**

</div>

MiniPdf は、実行時に Microsoft Office、LibreOffice、Adobe Acrobat、COM オートメーションを必要とせず、Office 文書を直接 PDF に変換します。プロジェクトに合う実装を選択してください。

## 実装を選ぶ

| | .NET | Rust |
|---|---|---|
| 状態 | 安定版、完全な実装 | 実験版、開発中 |
| 入力 | XLSX、DOCX、PPTX、PDF 結合 | XLSX、DOCX |
| インターフェイス | .NET ライブラリ、CLI、Native AOT 単体バイナリ | Rust crate、CLI |
| コンテンツ抽出 | Markdown、JSON、セマンティック .NET モデル | 未対応 |
| ドキュメント | **[.NET ガイドを開く](README.nuget.md)** | **[Rust ガイドを開く](../minipdf-rs/README.md)** |

本番用途と最も広い文書対応には **.NET** を推奨します。ネイティブな Rust 依存関係が必要で、現在の変換範囲で十分な場合は **Rust** を選択してください。

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

[.NET ガイド](README.nuget.md)では、変換、コンテンツ抽出、PDF 結合、カスタムフォント、CLI オプション、デプロイを説明しています。

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

[Rust ガイド](../minipdf-rs/README.md)では、crate API、CLI、対応範囲、既知の制限、開発手順を説明しています。

## MiniPdf を選ぶ理由

- **Office スイート不要**：変換はアプリケーションまたは CLI 内で実行されます。
- **小さなデプロイ構成**：依存関係が少なく、外部プロセスも不要です。
- **サーバーと CI に対応**：コンテナー、クラウドサービス、パイプラインで利用できます。
- **ネイティブ CLI**：.NET Native AOT リリースと Rust CLI を提供します。

MiniPdf は実用的な文書変換を目的としており、Microsoft Office のレイアウトを完全に再現するものではありません。複雑なテンプレートは表示が異なる場合があるため、オンラインデモやベンチマークレポートで代表的なファイルを評価してください。

## 計算リソースで貢献する

クリーンな fork または clone を GitHub Copilot Agent 対応の VS Code で開き、次を実行します。

```text
/skill-minipdf-contribution .NET
/skill-minipdf-contribution Rust
```

この貢献ループはツールチェーンを確認し、ベンチマーク用 Python パッケージをインストールしてローカルブランチを作成し、選択したレンダラーで視覚スコアが最も低い差分を 2 件選びます。Agent は各ケースを最大 3 回修正し、スコアが改善しなければ自動的に元へ戻して次へ進みます。選択した実装の全テストと XLSX/DOCX 視覚ベンチマークを通過し、重大な回帰がない場合にのみ PR を作成できます。

どちらも Git、Python 3.10 以降、LibreOffice が必要です。.NET には .NET 9 SDK、Rust には Cargo と現在の主要な視覚参照を生成するデスクトップ版 Excel/Word が必要です。認証済み GitHub CLI がなければ、PR のタイトル、本文、push コマンド、ブラウザー URL を生成します。コマンドと安全規則は[貢献ループ](../.github/skills/skill-minipdf-contribution/SKILL.md)を参照してください。承認なしに commit、push、PR 作成を行うことはありません。

## プロジェクトリソース

| リソース | 説明 |
|---|---|
| [オンラインデモ](https://mini-software.github.io/MiniPdf/) | ブラウザーで変換を試す |
| [.NET ドキュメント](README.nuget.md) | 安定版ライブラリと CLI の使用方法 |
| [Rust ドキュメント](../minipdf-rs/README.md) | 実験版 crate と CLI の使用方法 |
| [.NET XLSX ベンチマーク](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | スプレッドシートの視覚比較結果 |
| [.NET DOCX ベンチマーク](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | 文書の視覚比較結果 |
| [Rust XLSX ベンチマーク](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Rust スプレッドシートの視覚比較結果 |
| [Rust DOCX ベンチマーク](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Rust ドキュメントの視覚比較結果 |
| [Rust ベンチマーク手順](../scripts/Run-Rust-Benchmark.ps1) | フィクスチャ範囲と比較レポートを生成 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | パッケージと単体バイナリ |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | バグ、互換性レポート、機能リクエスト |

## ライセンス

[Apache License 2.0](../LICENSE) で提供されています。必要な通知と帰属表示を保持すれば、商用利用も可能です。

[Star](https://github.com/mini-software/MiniPdf) または[寄付](https://mini-software.github.io/)がプロジェクトの継続を支えます。
