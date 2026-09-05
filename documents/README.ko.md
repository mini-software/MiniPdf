<div align="center">

# MiniPdf

**.NET, Rust, Java, Python, Node.js 및 Go용 경량 Office-to-PDF 라이브러리와 명령줄 도구입니다.**

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

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | 한국어 | <a href="README.it.md">Italiano</a> | <a href="README.fr.md">Français</a>

**[온라인 데모](https://mini-software.github.io/MiniPdf/)** · **[릴리스](https://github.com/mini-software/MiniPdf/releases)** · **[문제 보고](https://github.com/mini-software/MiniPdf/issues)**

스타 또는 후원은 프로젝트가 지속되는 데 도움이 됩니다.

🤝 **공동 개발자를 찾고 있습니다:** [빠르게 기여하기](#quick-contribution)

</div>

MiniPdf는 런타임에 Microsoft Office, LibreOffice, Adobe Acrobat 또는 COM 자동화 없이 Office 문서를 PDF로 직접 변환합니다. 프로젝트에 맞는 구현을 선택하세요.

## 구현 선택

| 구현 | 입력 | 인터페이스 | 성숙도 | 문서 |
|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | 라이브러리, CLI, Native AOT 바이너리 | 안정 | **[.NET 가이드](README.nuget.md)** |
| Rust | XLSX, DOCX, PPTX | Crate, CLI | 실험적 | **[Rust 가이드](../minipdf-rs/README.md)** |
| Java | XLSX, DOCX | 라이브러리, CLI | 실험적 | **[Java 소스](../minipdf-java/)** |
| Python | DOCX | 패키지, CLI | 실험적 | **[Python 가이드](../minipdf-python/README.md)** |
| Node.js | XLSX, DOCX, PPTX | 네이티브 패키지 | 실험적 | **[Node.js 가이드](../minipdf-node/README.md)** |
| Go | XLSX, DOCX, PPTX | 패키지, CLI | 실험적 | **[Go 가이드](../minipdf-go/README.md)** |

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

[.NET 가이드](README.nuget.md)에서는 변환, 사용자 지정 글꼴, CLI 옵션 및 배포를 설명합니다.

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

### Java

```xml
<dependency>
	<groupId>io.github.mini-software</groupId>
	<artifactId>minipdf</artifactId>
	<version>0.1.2</version>
</dependency>
```

Maven `groupId`에는 하이픈을 사용할 수 있지만 Java 패키지 이름에는 사용할 수 없습니다.
따라서 의존성에는 `io.github.mini-software`를 사용하고 import에는
`io.github.minisoftware.minipdf`를 사용합니다.

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

[Python 가이드](../minipdf-python/README.md)는 현재 DOCX 기능 범위와 CLI 옵션을 설명합니다.

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

[Node.js 가이드](../minipdf-node/README.md)는 메모리 내 변환, 페이지 크기, 글꼴 등록 및 지원 네이티브 플랫폼을 설명합니다.

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

[Go 가이드](../minipdf-go/README.md)는 패키지, 네이티브 CLI, 현재 렌더링 범위 및 릴리스 태그를 설명합니다.

## MiniPdf를 선택하는 이유

- **Office 제품군 불필요**: 애플리케이션 또는 CLI 내부에서 변환합니다.
- **간결한 배포**: 의존성이 적고 외부 프로세스가 필요하지 않습니다.
- **서버 및 CI 친화적**: 컨테이너, 클라우드 서비스 및 파이프라인에서 실행됩니다.
- **다양한 언어 선택**: .NET, Rust, Java, Python, Node.js 또는 Go에서 MiniPdf를 사용할 수 있습니다.
- **네이티브 명령줄 옵션**: .NET, Rust, Java, Python 및 Go에서 사용할 수 있습니다.

MiniPdf는 실용적인 문서 변환을 목표로 하며 Microsoft Office 레이아웃을 완전히 재현하지는 않습니다. 복잡한 템플릿은 다르게 렌더링될 수 있으므로 온라인 데모나 벤치마크 보고서로 대표 파일을 평가하세요.

<a id="quick-contribution"></a>

## 빠르게 기여하기

원하는 코딩 Agent에서 깨끗한 fork 또는 clone을 연 다음 아래 지시문을 Agent 채팅에 붙여 넣으세요.

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop from start to finish. Detect the installed supported language toolchains, randomly choose one available implementation, diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

## 프로젝트 리소스

| 리소스 | 설명 |
|---|---|
| [온라인 데모](https://mini-software.github.io/MiniPdf/) | 브라우저에서 변환 체험 |
| [.NET 문서](README.nuget.md) | 안정 버전 라이브러리 및 CLI 사용법 |
| [Rust 문서](../minipdf-rs/README.md) | 실험 버전 crate 및 CLI 사용법 |
| [Java 구현](../minipdf-java/) | 실험적 Maven 라이브러리 및 CLI 소스 |
| [Python 문서](../minipdf-python/README.md) | 실험적 패키지 및 CLI 사용법 |
| [Node.js 문서](../minipdf-node/README.md) | 실험적 네이티브 패키지 사용법 |
| [Go 문서](../minipdf-go/README.md) | 실험적 패키지 및 CLI 사용법 |
| [.NET XLSX 벤치마크](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | 스프레드시트 시각 비교 결과 |
| [.NET DOCX 벤치마크](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | 문서 시각 비교 결과 |
| [Rust XLSX 벤치마크](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Rust 스프레드시트 시각 비교 결과 |
| [Rust DOCX 벤치마크](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Rust 문서 시각 비교 결과 |
| [Rust 벤치마크 절차](../scripts/Run-Rust-Benchmark.ps1) | 픽스처 범위 및 비교 보고서 생성 |
| [커뮤니티 거버넌스](../GOVERNANCE.md) | 의사 결정, 역할, 투표 및 메인테이너 선정 |
| [로드맵](../ROADMAP.md) | 프로젝트 범위, 구현 상태 및 현재 우선순위 |
| [보안](../SECURITY.md) | 비공개 취약점 신고 및 지원 버전 |
| [기여 가이드](../CONTRIBUTING.md) | 개발 환경, 테스트, 리뷰 및 출처 요건 |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | 패키지 및 독립 실행형 바이너리 |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | 버그, 호환성 보고 및 기능 요청 |

## 라이선스

[Apache License 2.0](../LICENSE)에 따라 제공됩니다. 필수 고지와 저작자 표시를 유지하면 상업적으로 사용할 수 있습니다.
