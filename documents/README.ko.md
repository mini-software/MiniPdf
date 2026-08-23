<div align="center">

# MiniPdf

**.NET 및 Rust용 경량 Office-to-PDF 라이브러리와 명령줄 도구입니다.**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | 한국어 | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[온라인 데모](https://mini-software.github.io/MiniPdf/)** · **[릴리스](https://github.com/mini-software/MiniPdf/releases)** · **[문제 보고](https://github.com/mini-software/MiniPdf/issues)**

</div>

MiniPdf는 런타임에 Microsoft Office, LibreOffice, Adobe Acrobat 또는 COM 자동화 없이 Office 문서를 PDF로 직접 변환합니다. 프로젝트에 맞는 구현을 선택하세요.

## 구현 선택

| | .NET | Rust |
|---|---|---|
| 상태 | 안정적이며 기능이 완전함 | 실험적이며 개발 중 |
| 입력 | XLSX, DOCX, PPTX, PDF 병합 | XLSX, DOCX |
| 인터페이스 | .NET 라이브러리, CLI, 독립 실행형 Native AOT 바이너리 | Rust crate, CLI |
| 콘텐츠 추출 | Markdown, JSON, 의미 기반 .NET 모델 | 아직 지원하지 않음 |
| 문서 | **[.NET 가이드 열기](README.nuget.md)** | **[Rust 가이드 열기](../minipdf-rs/README.md)** |

프로덕션 환경과 가장 폭넓은 문서 지원에는 **.NET**을 권장합니다. 네이티브 Rust 의존성이 필요하고 현재 변환 범위로 충분하다면 **Rust**를 선택하세요.

## 빠른 시작

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

명령줄 사용:

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

[.NET 가이드](README.nuget.md)에서는 변환, 콘텐츠 추출, PDF 병합, 사용자 지정 글꼴, CLI 옵션 및 배포를 설명합니다.

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

[Rust 가이드](../minipdf-rs/README.md)에서는 crate API, CLI, 지원 범위, 알려진 제한 사항 및 개발 절차를 설명합니다.

## MiniPdf를 선택하는 이유

- **Office 제품군 불필요**: 애플리케이션 또는 CLI 내부에서 변환합니다.
- **간결한 배포**: 의존성이 적고 외부 프로세스가 필요하지 않습니다.
- **서버 및 CI 친화적**: 컨테이너, 클라우드 서비스 및 파이프라인에서 실행됩니다.
- **네이티브 명령줄 옵션**: .NET Native AOT 릴리스와 Rust CLI를 제공합니다.
- **개방형 개발**: Apache 2.0 라이선스와 재현 가능한 시각적 벤치마크를 제공합니다.

MiniPdf는 실용적인 문서 변환을 목표로 하며 Microsoft Office 레이아웃을 완전히 재현하지는 않습니다. 복잡한 템플릿은 다르게 렌더링될 수 있으므로 온라인 데모나 벤치마크 보고서로 대표 파일을 평가하세요.

## 프로젝트 리소스

| 리소스 | 설명 |
|---|---|
| [온라인 데모](https://mini-software.github.io/MiniPdf/) | 브라우저에서 변환 체험 |
| [.NET 문서](README.nuget.md) | 안정 버전 라이브러리 및 CLI 사용법 |
| [Rust 문서](../minipdf-rs/README.md) | 실험 버전 crate 및 CLI 사용법 |
| [.NET XLSX 벤치마크](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | 스프레드시트 시각 비교 결과 |
| [.NET DOCX 벤치마크](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | 문서 시각 비교 결과 |
| [Rust 벤치마크 절차](../scripts/Run-Rust-Benchmark.ps1) | 픽스처 범위 및 비교 보고서 생성 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | 패키지 및 독립 실행형 바이너리 |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | 버그, 호환성 보고 및 기능 요청 |

## 라이선스

[Apache License 2.0](../LICENSE)에 따라 제공됩니다. 필수 고지와 저작자 표시를 유지하면 상업적으로 사용할 수 있습니다.

[Star](https://github.com/mini-software/MiniPdf) 또는 [후원](https://mini-software.github.io/)은 프로젝트 유지에 도움이 됩니다.
