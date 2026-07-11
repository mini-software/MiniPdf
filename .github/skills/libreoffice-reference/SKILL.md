---
name: libreoffice-reference
description: Look up Office -> PDF processing logic from local LibreOffice and Apache POI source trees, with Obsidian LLM WIKI notes as reusable cache. Use when: investigating how LibreOffice renders a document, how Apache POI models OOXML, aligning MiniPdf output with LibreOffice behavior, debugging a conversion mismatch, choosing a heuristic for docx/xlsx -> PDF, or any time the user asks "how does LibreOffice / POI handle X".
---

# Office Conversion Reference (Local Source + LLM WIKI)

Local LibreOffice and Apache POI source trees are the primary references for Office conversion behavior. Obsidian notes are a reusable LLM WIKI cache for distilled findings, not a substitute for checking source when behavior matters.

## Source Roots

| Source | Default path | Environment override | Use for |
| --- | --- | --- | --- |
| LibreOffice | `D:\git\libreoffice-core` | `LIBREOFFICE_SOURCE_ROOT` | Renderer behavior, import/export filters, page layout, print/PDF output, font/layout decisions. |
| Apache POI | `D:\git\poi` | `POI_SOURCE_ROOT` | OOXML object model, schema interpretation, XWPF/XSSF implementation details. |

When the question is "what does LibreOffice produce / how does it render", inspect LibreOffice source first. When the question is "what does this OOXML element mean / how is it modeled", inspect Apache POI source first.

## Research Order

1. Search the relevant local source tree with `scripts/search-source.ps1` or `rg`.
2. Read the smallest owning implementation or nearby test that controls the behavior.
3. Use existing Obsidian notes only as a cache or topic map, then verify important details against source.
4. If the finding is reusable, save a short LLM WIKI note with exact source file paths and a concise conclusion.

## LLM WIKI

Reusable research notes should go under the Obsidian LLM WIKI so future sessions can avoid repeating expensive source reads.

Resolution order:

1. `-WikiRoot <path>` on `scripts/save-wiki-note.ps1`.
2. `$env:OBSIDIAN_LLM_WIKI_ROOT`.
3. Fallback: `<MyDocuments>\Obsidian Vault\LLM WIKI`.

Write a wiki note when the result is likely to be reused, for example a confirmed LibreOffice import path, a print-scaling rule, a POI schema interpretation, or a non-obvious mismatch between source and observed output. Keep notes short and cite source-relative files such as `sc/source/filter/oox/pagesettings.cxx` or `poi-ooxml/src/main/java/...`.

## Supplementary Vaults

Local Obsidian vaults contain structured notes on how Office documents are processed into PDF. Use them as cached context and topic maps.

| Vault | Use for |
| --- | --- |
| `LibreOffice-to-PDF` | Cached notes about how LibreOffice renders DOCX/XLSX -> PDF (CLI, UNO, PDF export filter, layout, fonts). |
| `Apache-POI-Word` | Cached OOXML notes about Apache POI XWPF/HWPF behavior. |
| `Office-to-PDF` | Cross-tool comparisons / generic conversion notes, when this optional vault exists locally. |

## When to consult this skill

Load and consult this skill BEFORE designing a fix or heuristic when the task involves:

- DOCX or XLSX -> PDF rendering parity with LibreOffice
- Page layout, margins, page size, scaling, headers/footers
- Styles, fonts, font fallback, font metrics
- Tables, images, drawings, anchors, wrapping
- PDF export options / behavior
- Any conversion bug where the expected output is "what LibreOffice produces"

Skip when the task is unrelated to Office -> PDF rendering (build scripts, CI, README, etc.).

## Vault Locations

Vaults live under a single base directory. Resolution order:

1. `-VaultRoot <path>` parameter on a script (explicit override; may be the base directory or a concrete vault directory).
2. `$env:OBSIDIAN_VAULT_ROOT` environment variable.
3. Fallback: `<MyDocuments>\Obsidian Vault` (i.e. `[Environment]::GetFolderPath('MyDocuments')` + `Obsidian Vault`).

Under the resolved base, the vault subfolders are:

```text
<base>\LibreOffice-to-PDF\
<base>\Apache-POI-Word\
<base>\Office-to-PDF\
```

For compatibility, the scripts also recognize the older LibreOffice folder name `LibreOffice-Word-to-PDF` if it exists instead.

## Resources in this skill

- [scripts/search-source.ps1](scripts/search-source.ps1) - regex search in local LibreOffice or Apache POI source. Supports `-Source LibreOffice|POI`.
- [scripts/save-wiki-note.ps1](scripts/save-wiki-note.ps1) - save distilled reusable findings into the Obsidian LLM WIKI.
- [reference/topic-map.md](reference/topic-map.md) - LibreOffice vault: folder -> topic table.
- [reference/apache-poi-topic-map.md](reference/apache-poi-topic-map.md) - Apache POI vault: XWPF / HWPF folder -> topic table.
- [reference/lookup-workflow.md](reference/lookup-workflow.md) - step-by-step procedure (source -> implementation/test -> optional wiki note).
- [scripts/list-vault.ps1](scripts/list-vault.ps1) - list notes that actually exist on disk (the MOC links many that do not). Supports `-Vault LibreOffice|POI|OfficeToPdf`.
- [scripts/search-vault.ps1](scripts/search-vault.ps1) - regex search with optional folder scoping. Supports `-Vault LibreOffice|POI|OfficeToPdf`.

## Quick start

```powershell
# Search LibreOffice source for Calc fit-to-page behavior
.\.github\skills\libreoffice-reference\scripts\search-source.ps1 -Source LibreOffice -Pattern "ScaleToPagesX|fitToPage" -Folder sc

# Search Apache POI source for OOXML model behavior
.\.github\skills\libreoffice-reference\scripts\search-source.ps1 -Source POI -Pattern "XWPFStyle|styleId" -Folder poi-ooxml\src\main\java

# Save a reusable finding to Obsidian LLM WIKI
"Short markdown finding with source file citations" | .\.github\skills\libreoffice-reference\scripts\save-wiki-note.ps1 -Title "Calc fitToPage import" -Source LibreOffice -Topic XLSX

# Search cached LibreOffice notes when useful
.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Pattern "sectPr|orientation" -Folder 13-Page-Layout

# Search cached Apache POI notes when useful
.\.github\skills\libreoffice-reference\scripts\list-vault.ps1 -Vault POI -Folder 05-XWPF-Styles
.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Vault POI -Pattern "XWPFStyle|styleId"
```

Then `read_file` the matching source file or note with the full path. Cite source-relative paths in conclusions, for example `sc/source/filter/oox/pagesettings.cxx` or `poi-ooxml/src/main/java/org/apache/poi/xwpf/usermodel/XWPFStyle.java`.

## Optional: Obsidian CLI

If the user explicitly wants to interact with the live Obsidian app (open the note, append, search via the app), use the `obsidian-cli` skill. It needs `OBSIDIAN_API_KEY` from the Local REST API plugin. For read-only lookups during coding, direct file access is faster and does not require the app to be running.

## Output guidance

- Quote the relevant snippet (short) and cite the source-relative file path.
- Keep MiniPdf changes aligned with what LibreOffice source and observed output show. If source, wiki notes, and observed behavior disagree, flag it instead of silently choosing one.
- Do not copy large sections of source or vault notes into the repo; distill findings into the LLM WIKI when they are reusable.
