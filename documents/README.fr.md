<div align="center">

# MiniPdf

**Bibliothèques et outils en ligne de commande légers pour convertir des documents Office en PDF avec .NET, Rust, Java, Python, Node.js et Go.**

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

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | Français

**[Démo en ligne](https://mini-software.github.io/MiniPdf/)** · **[Versions](https://github.com/mini-software/MiniPdf/releases)** · **[Signaler un problème](https://github.com/mini-software/MiniPdf/issues)**

Votre étoile ou votre don contribue à soutenir le projet.

🤝 **Nous recherchons des co-développeurs :** [Contribuer rapidement](#quick-contribution)

</div>

MiniPdf convertit directement les documents Office en PDF sans nécessiter Microsoft Office, LibreOffice, Adobe Acrobat ni automatisation COM à l'exécution. Choisissez l'implémentation adaptée à votre projet.

## Choisir une implémentation

| Implémentation | Entrées | Interfaces | Maturité | Documentation | Résultats visuels |
|---|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | Bibliothèque, CLI, binaires Native AOT | Stable | **[Guide .NET](README.nuget.md)** | **[XLSX](../tests/MiniPdf.Benchmark/reports/comparison_report.md)**<br>**[DOCX](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md)**<br>**[PPTX](../tests/Issue_Files/reports_pptx/comparison_report.md)** |
| Rust | XLSX, DOCX, PPTX | Crate, CLI | Expérimental | **[Guide Rust](../minipdf-rs/README.md)** | **[XLSX](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md)**<br>**[DOCX](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md)** |
| Java | XLSX, DOCX | Bibliothèque, CLI | Expérimental | **[Sources Java](../minipdf-java/)** | **[XLSX](../artifacts/java-benchmark/issue/xlsx/report/comparison_report.md)** |
| Python | DOCX | Paquet, CLI | Expérimental | **[Guide Python](../minipdf-python/README.md)** | **[XLSX](../artifacts/python-benchmark/issue/xlsx/report/comparison_report.md)** |
| Node.js | XLSX, DOCX, PPTX | Paquet natif | Expérimental | **[Guide Node.js](../minipdf-node/README.md)** | **[XLSX](../artifacts/node-benchmark/issue/xlsx/report/comparison_report.md)** |
| Go | XLSX, DOCX, PPTX | Paquet, CLI | Expérimental | **[Guide Go](../minipdf-go/README.md)** | **[XLSX](../artifacts/go-benchmark/issue/xlsx/report/comparison_report.md)** |

## Démarrage rapide

### .NET

```bash
dotnet add package MiniPdf
```

```csharp
using MiniSoftware;

MiniPdf.ConvertToPdf("report.docx", "report.pdf");
```

Pour la ligne de commande :

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
```

Le [guide .NET](README.nuget.md) présente la conversion, les polices personnalisées, les options CLI et le déploiement.

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

Le [guide Rust](../minipdf-rs/README.md) décrit l'API du crate, la CLI, les fonctions prises en charge, les limites connues et le flux de développement.

### Java

```xml
<dependency>
	<groupId>io.github.mini-software</groupId>
	<artifactId>minipdf</artifactId>
	<version>0.1.4</version>
</dependency>
```

Les `groupId` Maven peuvent contenir des traits d’union, contrairement aux noms
de packages Java. La dépendance utilise donc `io.github.mini-software`, tandis
que les imports utilisent `io.github.minisoftware.minipdf`.

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

Le [guide Python](../minipdf-python/README.md) décrit le périmètre DOCX actuel et les options CLI.

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

Le [guide Node.js](../minipdf-node/README.md) décrit la conversion en mémoire, les tailles de page, l'enregistrement des polices et les plateformes natives prises en charge.

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

Le [guide Go](../minipdf-go/README.md) décrit le paquet, la CLI native, le périmètre de rendu actuel et les tags de version.

## Pourquoi MiniPdf

- **Aucune suite Office requise** : la conversion s'exécute dans l'application ou la CLI.
- **Déploiement réduit** : peu de dépendances et aucun processus externe.
- **Adapté aux serveurs et à la CI** : fonctionne dans les conteneurs, services cloud et pipelines.
- **Plusieurs langages disponibles** : utilisez MiniPdf depuis .NET, Rust, Java, Python, Node.js ou Go.
- **Options natives en ligne de commande** : disponibles pour .NET, Rust, Java, Python et Go.

MiniPdf vise une conversion pratique des documents, et non une compatibilité totale avec la mise en page de Microsoft Office. Les modèles complexes peuvent être rendus différemment ; utilisez la démo en ligne ou les rapports de benchmark pour évaluer des fichiers représentatifs.

<a id="quick-contribution"></a>

## Contribuer rapidement

Ouvrez un fork ou clone propre dans n'importe quel agent de programmation, puis collez cette instruction dans le chat de l'Agent :

```text
Read CONTRIBUTING.md and run the MiniPdf contribution loop from start to finish. Detect the installed supported language toolchains, randomly choose one available implementation, diagnose and improve the automatically selected benchmark cases, validate all changes, and prepare the pull request. Do not commit, push, fork, or open a pull request without my explicit approval.
```

## Ressources du projet

| Ressource | Description |
|---|---|
| [Démo en ligne](https://mini-software.github.io/MiniPdf/) | Tester la conversion dans le navigateur |
| [Documentation .NET](README.nuget.md) | Utilisation de la bibliothèque stable et de la CLI |
| [Documentation Rust](../minipdf-rs/README.md) | Utilisation du crate expérimental et de la CLI |
| [Implémentation Java](../minipdf-java/) | Sources de la bibliothèque Maven et de la CLI expérimentales |
| [Documentation Python](../minipdf-python/README.md) | Utilisation du paquet expérimental et de la CLI |
| [Documentation Node.js](../minipdf-node/README.md) | Utilisation du paquet natif expérimental |
| [Documentation Go](../minipdf-go/README.md) | Utilisation du paquet expérimental et de la CLI |
| [Benchmark XLSX .NET](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | Résultats visuels pour les feuilles de calcul |
| [Benchmark DOCX .NET](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | Résultats visuels pour les documents |
| [Benchmark XLSX Rust](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Résultats de comparaison visuelle Rust des feuilles de calcul |
| [Benchmark DOCX Rust](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Résultats de comparaison visuelle Rust des documents |
| [Procédure de benchmark Rust](../scripts/Run-Rust-Benchmark.ps1) | Génère la couverture des tests et les rapports comparatifs |
| [Gouvernance communautaire](../GOVERNANCE.md) | Décisions, rôles, votes et sélection des mainteneurs |
| [Feuille de route](../ROADMAP.md) | Périmètre du projet, état des implémentations et priorités |
| [Sécurité](../SECURITY.md) | Signalement privé des vulnérabilités et versions prises en charge |
| [Contribuer](../CONTRIBUTING.md) | Environnement, tests, revues et exigences de provenance |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | Paquets et binaires autonomes |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bogues, rapports de compatibilité et demandes de fonctionnalités |

## Licence

[Apache License 2.0](../LICENSE). L'utilisation commerciale est autorisée en conservant les avis et attributions requis.
