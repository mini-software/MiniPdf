# MiniPdf Web Converter

An online document-to-PDF converter powered by [MiniPdf](https://github.com/mini-software/MiniPdf) — a zero-dependency .NET library.

**Live demo:** https://mini-software.github.io/MiniPdf/

## Features

- **Convert Word (.docx) → PDF** — preserves text, tables, images
- **Convert Excel (.xlsx) → PDF** — renders all sheets
- **Pure client-side conversion** — MiniPdf runs entirely in the browser via WebAssembly
- **No server upload** — files never leave your machine
- **Drag-and-drop UI** — responsive single-page interface
- **Max 50 MB** file size

## Architecture

```
Browser (Standalone Blazor WebAssembly)
┌──────────────────────────────────────────┐
│  Converter.razor                          │
│  - Drag & drop / file picker              │
│  - MiniPdf.ConvertDocxToPdf() in WASM     │
│  - MiniPdf.ConvertToPdf() in WASM         │
│  - JS interop downloads PDF locally       │
└──────────────────────────────────────────┘
Hosted as static files on GitHub Pages
```

All conversion happens in WebAssembly — no server required. The site is pure static HTML/JS/WASM.

## Run Locally

```bash
cd MiniPdf.Web
dotnet run --project MiniPdf.Web.Client/MiniPdf.Web.Client.csproj
```

## Deploy to GitHub Pages

Push to `main` — the GitHub Actions workflow (`.github/workflows/pages.yml`) will publish automatically.

Ensure **GitHub Pages** is configured to deploy from **GitHub Actions** in the repository settings.

## Tech Stack

- .NET 9 Standalone Blazor WebAssembly
- [MiniPdf](https://www.nuget.org/packages/MiniPdf) NuGet package — runs in WASM, no server dependency
- GitHub Pages (static hosting)

## License

Apache-2.0 — same as MiniPdf.
