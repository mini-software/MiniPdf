# LibreOffice Vault — Topic Map

Snapshot of the structure of the local Obsidian vault `LibreOffice-Word-to-PDF`. Use this as a quick lookup before searching the vault.

> Note: The Map of Content (`00-Index/MOC - LibreOffice Word to PDF.md`) lists ~200 planned notes, but only a subset exist as real files on disk. Always verify with `scripts/list-vault.ps1` (or `grep_search`) before assuming a note is present.

## Vault root

`<OBSIDIAN_VAULT_ROOT>\LibreOffice-Word-to-PDF\`

The base is resolved by `scripts/_common.ps1` (see [SKILL.md](../SKILL.md#vault-locations) for details). Default: `<MyDocuments>\Obsidian Vault`.

## Folder -> topic mapping

| Folder | When to look here |
| --- | --- |
| `00-Index/` | Map of Content; entry point for browsing |
| `01-Introduction/` | What LibreOffice is, format support, conversion overview, key concepts |
| `02-Installation/` | Install on Linux/Windows/macOS, headless setup, version compatibility |
| `03-CLI/` | `soffice` / `libreoffice` CLI: `--headless`, `--convert-to pdf`, `--outdir`, batch, exit codes, env vars |
| `04-UNO-API/` | UNO architecture, ServiceManager, Desktop, XComponent, XStorable, PropertyValue, filter names, URL conventions |
| `05-PDF-Export/` | PDF export filter, FilterData properties, PDF version, PDF/A, encryption, bookmarks, hyperlinks, embedded fonts, image compression, watermarks |
| `06-Python/` | python-docx, subprocess to soffice, `uno` module, FastAPI service, unoconv |
| `07-Java/` | JODConverter, OfficeManager, Spring Boot, Maven, ODF Toolkit, Java UNO bridge |
| `08-Basic-Macros/` | LibreOffice Basic, `CreateUnoService`, headless macros, security |
| `09-Batch/` | Shell/Python batch, parallelism, queues, resume after failure |
| `10-DOCX-Internals/` | OOXML structures as LibreOffice interprets them (relevant for MiniPdf parity) |
| `11-Styles/` | Paragraph / character / table style resolution |
| `12-Fonts/` | Font matching, embedding, fallback |
| `13-Page-Layout/` | Sections, margins, page size, orientation, columns |
| `14-Tables-Images/` | Table layout, image anchoring / wrapping |
| `15-Error-Handling/` | Common failure modes |
| `16-Performance/` | Performance characteristics |
| `17-Docker/` | Containerized deployment |
| `18-REST-API/` | REST wrappers around LibreOffice |
| `19-Testing/` | Conversion testing approaches |
| `20-Recipes/` | End-to-end recipes |

## High-relevance folders for MiniPdf

When the task is MiniPdf docx/xlsx -> PDF parity, prioritize in this order:

1. `10-DOCX-Internals/` — what the OOXML actually means
2. `13-Page-Layout/` — page setup, sections, margins
3. `11-Styles/` — style cascade
4. `14-Tables-Images/` — tables, image anchors, wrapping
5. `12-Fonts/` — font fallback / metrics
6. `05-PDF-Export/` — output side (filter options, embedded fonts)
7. `15-Error-Handling/` — known gotchas

## Search tips

- Always scope `grep_search` with `includePattern` to a folder (or the whole vault) — see `scripts/search-vault.ps1`.
- The MOC uses Obsidian wikilinks (`[[NN - Topic]]`); the on-disk filename matches the link text plus `.md`.
- File numbering (`NN - Title`) is global; the same number appears under exactly one folder.
