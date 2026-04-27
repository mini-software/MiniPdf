---
name: libreoffice-reference
description: Look up the underlying processing logic of Office -> PDF tooling (LibreOffice, Apache POI, generic Office-to-PDF) from local Obsidian vaults. Use when: investigating how LibreOffice renders a document, how Apache POI models OOXML, aligning MiniPdf output with LibreOffice behavior, debugging a conversion mismatch, choosing a heuristic for docx/xlsx -> PDF, or any time the user asks "how does LibreOffice / POI handle X".
---

# Office Conversion Reference (Obsidian Vaults)

Local Obsidian vaults contain structured notes on how Office documents are processed into PDF. Treat them as the primary reference whenever underlying conversion logic matters for a MiniPdf change.

## Vaults

| Vault | Use for |
| --- | --- |
| `LibreOffice-Word-to-PDF` | Primary. How LibreOffice renders DOCX/XLSX -> PDF (CLI, UNO, PDF export filter, layout, fonts). |
| `Apache-POI-Word` | OOXML internals. How Apache POI models XWPF/HWPF (paragraphs, runs, tables, styles, numbering, headers/footers, SDT, settings). Reference Java implementation of the spec. |
| `Office-to-PDF` | Cross-tool comparisons / generic conversion notes. |

When the question is "what does the OOXML element actually mean / how should it be interpreted", prefer `Apache-POI-Word`. When the question is "what does LibreOffice produce", prefer `LibreOffice-Word-to-PDF`.

## When to consult this skill

Load and consult the vault BEFORE designing a fix or heuristic when the task involves:

- DOCX or XLSX -> PDF rendering parity with LibreOffice
- Page layout, margins, page size, scaling, headers/footers
- Styles, fonts, font fallback, font metrics
- Tables, images, drawings, anchors, wrapping
- PDF export options / behavior
- Any conversion bug where the expected output is "what LibreOffice produces"

Skip when the task is unrelated to Office -> PDF rendering (build scripts, CI, README, etc.).

## Vault locations

Vaults live under a single base directory. Resolution order:

1. `-VaultRoot <path>` parameter on a script (explicit override).
2. `$env:OBSIDIAN_VAULT_ROOT` environment variable.
3. Fallback: `<MyDocuments>\Obsidian Vault` (i.e. `[Environment]::GetFolderPath('MyDocuments')` + `Obsidian Vault`).

Under the resolved base, the three vault subfolders are:

```
<base>\LibreOffice-Word-to-PDF\
<base>\Apache-POI-Word\
<base>\Office-to-PDF\
```

Notes are plain Markdown files on disk. Read them directly with `read_file` / `grep_search` — no Obsidian app or API key required. To point at a different machine, set `OBSIDIAN_VAULT_ROOT` once:

```powershell
[Environment]::SetEnvironmentVariable('OBSIDIAN_VAULT_ROOT', 'D:\notes\ObsidianVault', 'User')
```

## Resources in this skill

- [reference/topic-map.md](reference/topic-map.md) — LibreOffice vault: folder -> topic table.
- [reference/apache-poi-topic-map.md](reference/apache-poi-topic-map.md) — Apache POI vault: XWPF / HWPF folder -> topic table.
- [reference/lookup-workflow.md](reference/lookup-workflow.md) — step-by-step procedure (vault -> folder -> list -> search -> read -> cite).
- [scripts/list-vault.ps1](scripts/list-vault.ps1) — list notes that actually exist on disk (the MOC links many that do not). Supports `-Vault LibreOffice|POI|OfficeToPdf`.
- [scripts/search-vault.ps1](scripts/search-vault.ps1) — regex search with optional folder scoping. Supports `-Vault LibreOffice|POI|OfficeToPdf`.

## Quick start

```powershell
# Default vault (LibreOffice): list notes under page layout
.\.github\skills\libreoffice-reference\scripts\list-vault.ps1 -Folder 13-Page-Layout

# Search a concept in LibreOffice vault
.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Pattern "sectPr|orientation" -Folder 13-Page-Layout

# Switch to Apache POI vault (OOXML internals)
.\.github\skills\libreoffice-reference\scripts\list-vault.ps1 -Vault POI -Folder 05-XWPF-Styles
.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Vault POI -Pattern "XWPFStyle|styleId"

# Cross-tool generic notes
.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Vault OfficeToPdf -Pattern "font embed"
```

Then `read_file` the matching note(s) with the full path and cite the vault-relative path in your reply (e.g. `LibreOffice-Word-to-PDF/13-Page-Layout/Sections.md` or `Apache-POI-Word/05-XWPF-Styles/XWPFStyle-Definition.md`).

## Optional: Obsidian CLI

If the user explicitly wants to interact with the live Obsidian app (open the note, append, search via the app), use the `obsidian-cli` skill. It needs `OBSIDIAN_API_KEY` from the Local REST API plugin. For read-only lookups during coding, direct file access is faster and does not require the app to be running.

## Output guidance

- Quote the relevant snippet (short) and link the source note.
- Keep MiniPdf changes aligned with what the vault states LibreOffice does. If the vault and observed behavior disagree, flag it instead of silently choosing one.
- Do not copy large sections of the vault into the repo; reference them.
