<div align="center">

# MiniPdf

**Bibliothèques et outils en ligne de commande légers pour convertir des documents Office en PDF avec .NET et Rust.**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://crates.io/crates/minipdf"><img src="https://img.shields.io/crates/v/minipdf.svg" alt="crates.io"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | Français

**[Démo en ligne](https://mini-software.github.io/MiniPdf/)** · **[Versions](https://github.com/mini-software/MiniPdf/releases)** · **[Signaler un problème](https://github.com/mini-software/MiniPdf/issues)**

</div>

MiniPdf convertit directement les documents Office en PDF sans nécessiter Microsoft Office, LibreOffice, Adobe Acrobat ni automatisation COM à l'exécution. Choisissez l'implémentation adaptée à votre projet.

## Choisir une implémentation

| | .NET | Rust |
|---|---|---|
| État | Stable et complète | Expérimentale, en développement actif |
| Entrées | XLSX, DOCX, PPTX, fusion PDF | XLSX, DOCX |
| Interfaces | Bibliothèque .NET, CLI, binaires Native AOT autonomes | Crate Rust, CLI |
| Documentation | **[Ouvrir le guide .NET](README.nuget.md)** | **[Ouvrir le guide Rust](../minipdf-rs/README.md)** |

Pour la production et la prise en charge documentaire la plus large, commencez par **.NET**. Choisissez **Rust** si vous souhaitez une dépendance Rust native et que le périmètre de conversion actuel vous convient.

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

Le [guide .NET](README.nuget.md) présente la conversion, la fusion PDF, les polices personnalisées, les options CLI et le déploiement.

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

## Pourquoi MiniPdf

- **Aucune suite Office requise** : la conversion s'exécute dans l'application ou la CLI.
- **Déploiement réduit** : peu de dépendances et aucun processus externe.
- **Adapté aux serveurs et à la CI** : fonctionne dans les conteneurs, services cloud et pipelines.
- **Options natives en ligne de commande** : versions .NET Native AOT et CLI Rust.

MiniPdf vise une conversion pratique des documents, et non une compatibilité totale avec la mise en page de Microsoft Office. Les modèles complexes peuvent être rendus différemment ; utilisez la démo en ligne ou les rapports de benchmark pour évaluer des fichiers représentatifs.

## Contribuer avec de la puissance de calcul

Ouvrez un fork ou clone propre dans GitHub Copilot, Claude Code, Cursor, Codex ou tout agent de programmation capable de modifier des fichiers et d'exécuter PowerShell. Le point d'entrée commun est :

```powershell
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation dotnet
.\scripts\Invoke-MiniPdfContributionLoop.ps1 -Action Start -Implementation rust
```

La boucle vérifie les outils, installe les paquets Python du benchmark, crée une branche locale et sélectionne les deux écarts visuels ayant les scores les plus faibles pour le moteur choisi. L'Agent effectue jusqu'à trois tentatives par cas ; sans amélioration du score, les modifications sont automatiquement annulées avant de passer au cas suivant. Une PR n'est autorisée qu'après la réussite de tous les tests de l'implémentation choisie et des benchmarks visuels XLSX/DOCX, sans régression significative.

Les deux parcours nécessitent Git, Python 3.10+ et LibreOffice. .NET nécessite le SDK .NET 9 ; Rust nécessite Cargo ainsi que les versions bureau d'Excel et Word pour produire les références visuelles principales actuelles. Si GitHub CLI n'est pas installée et authentifiée, le workflow génère le titre, le corps, la commande push et l'URL de création de la PR dans le navigateur. Consultez le [guide de contribution](../CONTRIBUTING.md) pour les raccourcis Copilot, Claude Code, Cursor, Codex et terminal, ainsi que les règles de sécurité. Aucun commit, push ou ouverture de PR n'est effectué sans approbation.

## Ressources du projet

| Ressource | Description |
|---|---|
| [Démo en ligne](https://mini-software.github.io/MiniPdf/) | Tester la conversion dans le navigateur |
| [Documentation .NET](README.nuget.md) | Utilisation de la bibliothèque stable et de la CLI |
| [Documentation Rust](../minipdf-rs/README.md) | Utilisation du crate expérimental et de la CLI |
| [Benchmark XLSX .NET](../tests/MiniPdf.Benchmark/reports/comparison_report.md) | Résultats visuels pour les feuilles de calcul |
| [Benchmark DOCX .NET](../tests/MiniPdf.Benchmark/reports_docx/comparison_report.md) | Résultats visuels pour les documents |
| [Benchmark XLSX Rust](../artifacts/rust-benchmark/classic/xlsx/report/comparison_report.md) | Résultats de comparaison visuelle Rust des feuilles de calcul |
| [Benchmark DOCX Rust](../artifacts/rust-benchmark/classic/docx/report/comparison_report.md) | Résultats de comparaison visuelle Rust des documents |
| [Procédure de benchmark Rust](../scripts/Run-Rust-Benchmark.ps1) | Génère la couverture des tests et les rapports comparatifs |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | Paquets et binaires autonomes |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bogues, rapports de compatibilité et demandes de fonctionnalités |

## Licence

[Apache License 2.0](../LICENSE). L'utilisation commerciale est autorisée en conservant les avis et attributions requis.

Une [étoile](https://github.com/mini-software/MiniPdf) ou un [don](https://mini-software.github.io/) contribue à soutenir le projet.
