<div align="center">

**Bibliothèques et outils en ligne de commande Office-vers-PDF légers, rapides et natifs pour Rust, .NET, Java, Python, JS et Go.**

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

Votre étoile ou votre don peut améliorer MiniPdf.

🤝 **Nous recherchons des co-développeurs :** [Contribuer rapidement](#quick-contribution)

</div>

MiniPdf convertit directement les documents Office en PDF sans nécessiter Microsoft Office, LibreOffice, Adobe Acrobat ni automatisation COM à l'exécution.

## Choisir une implémentation

| Implémentation | Entrées | Interfaces | Maturité | Documentation | Résultats visuels |
|---|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | Bibliothèque, CLI, binaires Native AOT | Stable | **[Guide .NET](README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |
| Rust | XLSX, DOCX, PPTX | Crate, CLI | Stable | **[Guide Rust](../minipdf-rs/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=docx)** |
| Java | XLSX, DOCX, PPTX | Bibliothèque, CLI | Expérimental | **[Sources Java](../minipdf-java/)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=java&suite=issue&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=java&suite=issue&format=docx)** |
| Python | DOCX | Paquet, CLI | Stable | **[Guide Python](../minipdf-python/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=python&suite=issue&format=xlsx)** |
| Node.js | XLSX, DOCX, PPTX | Paquet natif | Stable | **[Guide Node.js](../minipdf-node/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=node&suite=issue&format=xlsx)** |
| Go | XLSX, DOCX, PPTX | Paquet, CLI | Expérimental | **[Guide Go](../minipdf-go/README.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=go&suite=issue&format=xlsx)** |

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

Pour l'utilisation, consultez le [guide .NET](README.nuget.md).

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

Pour l'utilisation, consultez le [guide Rust](../minipdf-rs/README.md).

### Java

[Maven Central artifact](https://central.sonatype.com/artifact/io.github.mini-software/minipdf)

```java
import io.github.minisoftware.minipdf.MiniPdf;
import java.nio.file.Files;
import java.nio.file.Path;

try {
	MiniPdf.registerFont(
			"Noto Sans",
			Files.readAllBytes(Path.of("fonts/NotoSans-Regular.ttf")));
	MiniPdf.convertToPdf(Path.of("report.docx"), Path.of("report.pdf"));
} finally {
	MiniPdf.clearRegisteredFonts();
}
```

Pour l'utilisation, consultez le [guide Java](../minipdf-java/README.md).

### Python

```bash
pip install minipdf
```

```python
import minipdf

minipdf.convert_to_pdf("report.docx", "report.pdf")
```

Pour l'utilisation, consultez le [guide Python](../minipdf-python/README.md).

### Node.js

```bash
npm install minipdf
```

```javascript
const minipdf = require('minipdf')

minipdf.convertToPdf('report.docx', 'report.pdf')
```

Pour l'utilisation, consultez le [guide Node.js](../minipdf-node/README.md).

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

Pour l'utilisation, consultez le [guide Go](../minipdf-go/README.md).

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
Run the minipdf-contribution skill.
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
| [Benchmark XLSX .NET](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx) | Résultats visuels pour les feuilles de calcul |
| [Benchmark DOCX .NET](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx) | Résultats visuels pour les documents |
| [Benchmark XLSX Rust](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=xlsx) | Résultats de comparaison visuelle Rust des feuilles de calcul |
| [Benchmark DOCX Rust](https://mini-software.github.io/minipdf-web-page/?language=rust&suite=classic&format=docx) | Résultats de comparaison visuelle Rust des documents |
| [Procédure de benchmark Rust](../scripts/Run-Rust-Benchmark.ps1) | Génère la couverture des tests et les rapports comparatifs |
| [Gouvernance communautaire](../GOVERNANCE.md) | Décisions, rôles, votes et sélection des mainteneurs |
| [Feuille de route](../ROADMAP.md) | Périmètre du projet, état des implémentations et priorités |
| [Sécurité](../SECURITY.md) | Signalement privé des vulnérabilités et versions prises en charge |
| [Contribuer](../CONTRIBUTING.md) | Environnement, tests, revues et exigences de provenance |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | Paquets et binaires autonomes |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bogues, rapports de compatibilité et demandes de fonctionnalités |

## Licence

[Apache License 2.0](../LICENSE). L'utilisation commerciale est la bienvenue.
