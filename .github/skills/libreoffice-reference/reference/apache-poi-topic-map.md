# Apache POI (XWPF / HWPF) — Topic Map

Snapshot of `Apache-POI-Word` vault. Use POI notes as the reference Java implementation of OOXML — useful when you need to know what an OOXML element *means* (independent of any renderer).

## Vault root

`<OBSIDIAN_VAULT_ROOT>\Apache-POI-Word\`

The base is resolved by `scripts/_common.ps1` (see [SKILL.md](../SKILL.md#vault-locations) for details). Default: `<MyDocuments>\Obsidian Vault`.

## Folder -> topic mapping

| Folder | When to look here |
| --- | --- |
| `00-Index/` | MOC / map of content |
| `01-Overview/` | POI architecture overview, packages |
| `02-XWPF-Document/` | `XWPFDocument` lifecycle, package parts |
| `03-XWPF-Paragraphs-Runs/` | Paragraph / run model, properties cascade |
| `04-XWPF-Tables/` | Tables, rows, cells, table properties |
| `05-XWPF-Styles/` | Style definitions, character / paragraph / table styles |
| `06-XWPF-Headers-Footers/` | Headers, footers, sections |
| `07-XWPF-Numbering/` | Numbering definitions, abstract / concrete num |
| `08-XWPF-Images/` | Inline / anchored images, drawingML |
| `09-XWPF-Comments/` | Comments part |
| `10-XWPF-Footnotes-Endnotes/` | Footnotes / endnotes parts |
| `11-XWPF-Hyperlinks/` | Hyperlinks |
| `12-XWPF-SDT/` | Structured Document Tags (content controls) |
| `13-XWPF-Settings/` | Document settings |
| `14-XWPF-Extractors/` | Text extraction |
| `15-HWPF-Document/` | Legacy `.doc` document |
| `16-HWPF-Model/` | HWPF binary model |
| `17-HWPF-Usermodel/` | Usermodel API |
| `18-HWPF-SPRM/` | SPRM property model |
| `19-HWPF-Converters/` | HWPF -> HTML / FO converters |
| `20-Common-Concepts/` | Shared concepts across XWPF/HWPF |
| `21-Code-Examples/` | Worked examples |
| `22-Architecture/` | Cross-cutting architecture |

## High-relevance folders for MiniPdf

For DOCX parsing and OOXML interpretation:

1. `03-XWPF-Paragraphs-Runs/` — paragraph / run property cascade
2. `05-XWPF-Styles/` — style resolution
3. `04-XWPF-Tables/` — table layout properties
4. `07-XWPF-Numbering/` — list numbering
5. `06-XWPF-Headers-Footers/` — section / header / footer model
6. `08-XWPF-Images/` — image anchoring
7. `12-XWPF-SDT/` — content controls
