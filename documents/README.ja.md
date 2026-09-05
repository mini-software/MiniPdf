<div align="center">

# MiniPdf

**.NET、Rust、Java、Python、Node.js、Go 向けの軽量な Office-to-PDF ライブラリおよびコマンドラインツール。**

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

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | 日本語 | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[オンラインデモ](https://mini-software.github.io/MiniPdf/)** · **[リリース](https://github.com/mini-software/MiniPdf/releases)** · **[問題を報告](https://github.com/mini-software/MiniPdf/issues)**

スターや寄付がプロジェクトの継続的な発展を支えます。

🤝 **共同開発者を募集中：**[すぐに貢献する](#quick-contribution)

</div>

MiniPdf は、実行時に Microsoft Office、LibreOffice、Adobe Acrobat、COM オートメーションを必要とせず、Office 文書を直接 PDF に変換します。プロジェクトに合う実装を選択してください。

## 実装を選ぶ

| 実装 | 入力 | インターフェイス | 成熟度 | ドキュメント |
|---|---|---|---|---|
| .NET | XLSX、DOCX、PPTX | ライブラリ、CLI、Native AOT バイナリ | 安定版 | **[.NET ガイド](README.nuget.md)** |
| Rust | XLSX、DOCX、PPTX | Crate、CLI | 実験版 | **[Rust ガイド](../minipdf-rs/README.md)** |
| Java | XLSX、DOCX | ライブラリ、CLI | 実験版 | **[Java ソース](../minipdf-java/)** |
| Python | DOCX | パッケージ、CLI | 実験版 | **[Python ガイド](../minipdf-python/README.md)** |
| Node.js | XLSX、DOCX、PPTX | ネイティブパッケージ | 実験版 | **[Node.js ガイド](../minipdf-node/README.md)** |
| Go | XLSX、DOCX、PPTX | パッケージ、CLI | 実験版 | **[Go ガイド](../minipdf-go/README.md)** |

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

[.NET ガイド](README.nuget.md)では、変換、カスタムフォント、CLI オプション、デプロイを説明しています。

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

### Java

```xml
<dependency>
	<groupId>io.github.mini-software</groupId>
	<artifactId>minipdf</artifactId>
	<version>0.1.2</version>
</dependency>
```

Maven の `groupId` ではハイフンを使用できますが、Java のパッケージ名では使用できません。
そのため、依存関係には `io.github.mini-software`、import には
`io.github.minisoftware.minipdf` を使用します。

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

[Python ガイド](../minipdf-python/README.md)では、現在の DOCX 対応範囲と CLI オプションを説明しています。

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

[Node.js ガイド](../minipdf-node/README.md)では、メモリ内変換、ページサイズ、フォント登録、対応ネイティブプラットフォームを説明しています。

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

[Go ガイド](../minipdf-go/README.md)では、パッケージ、ネイティブ CLI、現在の描画範囲、リリースタグを説明しています。

## MiniPdf を選ぶ理由

- **Office スイート不要**：変換はアプリケーションまたは CLI 内で実行されます。
- **小さなデプロイ構成**：依存関係が少なく、外部プロセスも不要です。
- **サーバーと CI に対応**：コンテナー、クラウドサービス、パイプラインで利用できます。
- **複数言語に対応**：.NET、Rust、Java、Python、Node.js、Go から MiniPdf を利用できます。
- **ネイティブ CLI**：.NET、Rust、Java、Python、Go で利用できます。

MiniPdf は実用的な文書変換を目的としており、Microsoft Office のレイアウトを完全に再現するものではありません。複雑なテンプレートは表示が異なる場合があるため、オンラインデモやベンチマークレポートで代表的なファイルを評価してください。

<a id="quick-contribution"></a>

## すぐに貢献する

任意のコーディング Agent でクリーンな fork または clone を開き、次の指示を Agent のチャットに貼り付けます。

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop from start to finish. Detect the installed supported language toolchains, randomly choose one available implementation, diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

## プロジェクトリソース

| リソース | 説明 |
|---|---|
| [オンラインデモ](https://mini-software.github.io/MiniPdf/) | ブラウザーで変換を試す |
| [.NET ドキュメント](README.nuget.md) | 安定版ライブラリと CLI の使用方法 |
| [Rust ドキュメント](../minipdf-rs/README.md) | 実験版 crate と CLI の使用方法 |
| [Java 実装](../minipdf-java/) | 実験版 Maven ライブラリと CLI のソース |
| [Python ドキュメント](../minipdf-python/README.md) | 実験版パッケージと CLI の使用方法 |
| [Node.js ドキュメント](../minipdf-node/README.md) | 実験版ネイティブパッケージの使用方法 |
| [Go ドキュメント](../minipdf-go/README.md) | 実験版パッケージと CLI の使用方法 |
| [.NET XLSX ベンチマーク](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | スプレッドシートの視覚比較結果 |
| [.NET DOCX ベンチマーク](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | 文書の視覚比較結果 |
| [Rust XLSX ベンチマーク](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Rust スプレッドシートの視覚比較結果 |
| [Rust DOCX ベンチマーク](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Rust ドキュメントの視覚比較結果 |
| [Rust ベンチマーク手順](../scripts/Run-Rust-Benchmark.ps1) | フィクスチャ範囲と比較レポートを生成 |
| [コミュニティガバナンス](../GOVERNANCE.md) | 意思決定、役割、投票、メンテナー選出 |
| [ロードマップ](../ROADMAP.md) | プロジェクト範囲、実装状況、現在の優先事項 |
| [セキュリティ](../SECURITY.md) | 脆弱性の非公開報告とサポート対象バージョン |
| [コントリビューション](../CONTRIBUTING.md) | 開発環境、テスト、レビュー、来歴要件 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | パッケージと単体バイナリ |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | バグ、互換性レポート、機能リクエスト |

## ライセンス

[Apache License 2.0](../LICENSE) で提供されています。必要な通知と帰属表示を保持すれば、商用利用も可能です。
