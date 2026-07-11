# Lookup Workflow

Step-by-step procedure for answering "how does LibreOffice / POI handle X?" using local source first, then cached Obsidian notes.

## 1. Pick the source

| Question shape | Source |
| --- | --- |
| "What does LibreOffice produce / how does it render?" | `LibreOffice` source at `D:\git\libreoffice-core` |
| "What does this OOXML element mean / how is it modeled?" | `POI` source at `D:\git\poi` |
| "Has this been researched already?" | Obsidian LLM WIKI or supplementary vaults |

## 2. Search source

Prefer a scoped source search:

```powershell
.\.github\skills\libreoffice-reference\scripts\search-source.ps1 -Source LibreOffice -Pattern "ScaleToPagesX|fitToPage" -Folder sc
.\.github\skills\libreoffice-reference\scripts\search-source.ps1 -Source POI -Pattern "XWPFStyle|styleId" -Folder poi-ooxml\src\main\java
```

Useful LibreOffice folders:

- `sc/` - Calc / XLSX import, print, PDF export, spreadsheet layout.
- `sw/` - Writer / DOCX layout and import behavior.
- `oox/`, `xmloff/`, `filter/` - OOXML and import/export plumbing.
- `vcl/`, `svx/`, `editeng/` - drawing, fonts, graphics, and text layout helpers.

Useful POI folders:

- `poi-ooxml/src/main/java/` - XWPF/XSSF usermodel and OOXML implementations.
- `poi-ooxml/src/test/` - behavior tests and edge-case examples.
- `poi/src/main/java/` - shared SS/HSSF model details.

## 3. Read the owning implementation

Use `read_file` on the source file with one wide nearby range. Prefer the function/class that directly computes the behavior over callers that only forward data.

## 4. Check cached notes when useful

The MOC may link planned notes that don't exist on disk. List the folder first if using supplementary vaults:

```powershell
.\.github\skills\libreoffice-reference\scripts\list-vault.ps1 -Folder 13-Page-Layout
.\.github\skills\libreoffice-reference\scripts\list-vault.ps1 -Vault POI -Folder 05-XWPF-Styles

.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Pattern "sectPr|orientation" -Folder 13-Page-Layout
.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Vault POI -Pattern "XWPFStyle|styleId"
```

## 5. Save reusable findings

If the result is likely to be useful again, save a concise LLM WIKI note:

```powershell
@"
## Finding

LibreOffice imports `<pageSetUpPr fitToPage="1"/>` into fit-to-pages mode and writes `ScaleToPagesX` from `fitToWidth`.

## Evidence

- `sc/source/filter/oox/pagesettings.cxx`: writes `PROP_ScaleToPagesX` from `mnFitToWidth` when fit-to-pages mode is enabled.

## MiniPdf implication

Apply the effective print scale to grid geometry and anchored drawing positions.
"@ | .\.github\skills\libreoffice-reference\scripts\save-wiki-note.ps1 -Title "Calc fitToPage import" -Source LibreOffice -Topic XLSX
```

## 6. Cite

Reference source-relative paths so the user can find them, e.g.

> Per `sc/source/filter/oox/pagesettings.cxx`, ...
> Per `poi-ooxml/src/main/java/org/apache/poi/xwpf/usermodel/XWPFStyle.java`, ...

## 7. Disagreement protocol

If source, wiki notes, vault notes, or observed MiniPdf / soffice behavior disagree:

- Do not silently pick one.
- Quote both, flag the discrepancy, and run a quick `soffice` repro when practical before changing code.
- Apache POI describes the spec; LibreOffice describes one implementation - they can legitimately differ.
