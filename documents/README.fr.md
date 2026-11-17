<div align="center">

**Bibliothèques et outils en ligne de commande Office-vers-PDF légers, rapides et natifs pour .NET.**

<p>
<a href="https://www.nuget.org/packages/MiniPdf"><img src="https://img.shields.io/nuget/v/MiniPdf.svg" alt="NuGet"></a>
<a href="https://github.com/mini-software/MiniPdf"><img src="https://img.shields.io/github/stars/mini-software/MiniPdf?logo=github" alt="GitHub stars"></a>
<a href="https://doi.org/10.5281/zenodo.22057294"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.22057294.svg" alt="DOI"></a>
<a href="../LICENSE"><img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="License"></a>
</p>

<a href="../README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a> | <a href="README.it.md">Italiano</a> | Français

**[Démo en ligne](https://mini-software.github.io/MiniPdf/)** · **[Versions](https://github.com/mini-software/MiniPdf/releases)** · **[Signaler un problème](https://github.com/mini-software/MiniPdf/issues)**

Votre étoile ou votre don peut améliorer MiniPdf.

🤝 **Nous recherchons des co-développeurs :** [Contribuer rapidement](#quick-contribution)

</div>

MiniPdf provides a .NET library, command-line tool, and standalone Native AOT binaries for XLSX, DOCX, and PPTX conversion.

MiniPdf convertit directement les documents Office en PDF sans nécessiter Microsoft Office, LibreOffice, Adobe Acrobat ni automatisation COM à l'exécution.

## .NET

| Implémentation | Entrées | Interfaces | Maturité | Documentation | Résultats visuels |
|---|---|---|---|---|---|
| .NET | XLSX, DOCX, PPTX | Bibliothèque, CLI, binaires Native AOT | Stable | **[Guide .NET](README.nuget.md)** | **[XLSX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx)**<br>**[DOCX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx)**<br>**[PPTX](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=issue&format=pptx)** |

## Démarrage rapide

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

Pour la ligne de commande :

```bash
dotnet tool install --global MiniPdf.Cli
minipdf report.docx -o report.pdf
minipdf workbook.xlsx -o workbook.pdf --compress
minipdf slides.pptx -o slides.pdf
minipdf --help
```

Pour l'utilisation, consultez le [guide .NET](README.nuget.md).

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

## Pourquoi MiniPdf

- **Aucune suite Office requise** : la conversion s'exécute dans l'application ou la CLI.
- **Déploiement réduit** : peu de dépendances et aucun processus externe.
- **Adapté aux serveurs et à la CI** : fonctionne dans les conteneurs, services cloud et pipelines.

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
| [Benchmark XLSX .NET](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=xlsx) | Résultats visuels pour les feuilles de calcul |
| [Benchmark DOCX .NET](https://mini-software.github.io/minipdf-web-page/?language=dotnet&suite=classic&format=docx) | Résultats visuels pour les documents |
| [Gouvernance communautaire](../GOVERNANCE.md) | Décisions, rôles, votes et sélection des mainteneurs |
| [Feuille de route](../ROADMAP.md) | Périmètre du projet, état des implémentations et priorités |
| [Sécurité](../SECURITY.md) | Signalement privé des vulnérabilités et versions prises en charge |
| [Contribuer](../CONTRIBUTING.md) | Environnement, tests, revues et exigences de provenance |
| [GitHub Releases](https://github.com/mini-software/MiniPdf/releases) | Paquets et binaires autonomes |
| [Issues](https://github.com/mini-software/MiniPdf/issues) | Bogues, rapports de compatibilité et demandes de fonctionnalités |

## Licence

[Apache License 2.0](../LICENSE). L'utilisation commerciale est la bienvenue.
