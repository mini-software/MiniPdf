# Lookup Workflow

Step-by-step procedure for answering "how does LibreOffice / POI handle X?" using the vaults.

## 1. Pick the vault

| Question shape | Vault |
| --- | --- |
| "What does LibreOffice produce / how does it render?" | `LibreOffice` (default) |
| "What does this OOXML element mean / how is it modeled?" | `POI` |
| "How do tools generally convert Office -> PDF?" | `OfficeToPdf` |

## 2. Identify the topic folder

- LibreOffice: see [topic-map.md](topic-map.md). For MiniPdf parity start with `10-DOCX-Internals/`, `13-Page-Layout/`, `11-Styles/`, `14-Tables-Images/`.
- Apache POI: see [apache-poi-topic-map.md](apache-poi-topic-map.md). For DOCX parsing start with `03-XWPF-Paragraphs-Runs/`, `05-XWPF-Styles/`, `04-XWPF-Tables/`.

## 3. List what actually exists

The MOC may link planned notes that don't exist on disk. List the folder first:

```powershell
.\.github\skills\libreoffice-reference\scripts\list-vault.ps1 -Folder 13-Page-Layout
.\.github\skills\libreoffice-reference\scripts\list-vault.ps1 -Vault POI -Folder 05-XWPF-Styles
```

## 4. Search

```powershell
.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Pattern "sectPr|orientation" -Folder 13-Page-Layout
.\.github\skills\libreoffice-reference\scripts\search-vault.ps1 -Vault POI -Pattern "XWPFStyle|styleId"
```

Equivalent `grep_search` call (substitute `<OBSIDIAN_VAULT_ROOT>` with the value of `$env:OBSIDIAN_VAULT_ROOT`, or default `<MyDocuments>/Obsidian Vault`):

```
includePattern: <OBSIDIAN_VAULT_ROOT>/LibreOffice-Word-to-PDF/13-Page-Layout/**
query: sectPr|orientation
isRegexp: true
```

Tip: resolve once with PowerShell and reuse:

```powershell
$base = if ($env:OBSIDIAN_VAULT_ROOT) { $env:OBSIDIAN_VAULT_ROOT } else { Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Obsidian Vault' }
```

## 5. Read

Use `read_file` with the absolute path returned by the search. Prefer one wide read over many small reads.

## 6. Cite

Reference the note by its **vault-prefixed** relative path so the user can find it, e.g.

> Per `LibreOffice-Word-to-PDF/02-Installation/09 - Headless Mode Setup.md`, ...
>
> Per `Apache-POI-Word/05-XWPF-Styles/XWPFStyle-Definition.md`, ...

## 7. Disagreement protocol

If vault notes disagree with each other, or with observed MiniPdf / soffice behavior:

- Do not silently pick one.
- Quote both, flag the discrepancy, and ask the user (or run a quick `soffice` repro) before changing code.
- Apache POI describes the spec; LibreOffice describes one implementation — they can legitimately differ.
