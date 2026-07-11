# Office Reference Scripts

Helper PowerShell scripts for the `libreoffice-reference` skill. They search local LibreOffice / Apache POI source trees and manage cached Obsidian notes.

| Script | Purpose |
| --- | --- |
| `search-source.ps1` | Regex search in the local LibreOffice or Apache POI source tree. |
| `save-wiki-note.ps1` | Save distilled reusable findings into the Obsidian LLM WIKI. |
| `list-vault.ps1` | List notes that actually exist on disk (optionally scoped to a folder). |
| `search-vault.ps1` | Regex search across the vault (optionally scoped to a folder). |

## Source root resolution

Resolved by `_common.ps1` in this order:

| Source | Environment variable | Default |
| --- | --- | --- |
| `LibreOffice` | `LIBREOFFICE_SOURCE_ROOT` | `D:\git\libreoffice-core` |
| `POI` | `POI_SOURCE_ROOT` | `D:\git\poi` |

Examples:

```powershell
.\search-source.ps1 -Source LibreOffice -Pattern "ScaleToPagesX" -Folder sc
.\search-source.ps1 -Source POI -Pattern "XWPFStyle" -Folder poi-ooxml\src\main\java
```

## LLM WIKI resolution

Reusable notes are saved under `<wiki root>\Office-Conversion\<Source>\...`.

Resolution order:

1. `-WikiRoot <path>` parameter on `save-wiki-note.ps1`.
2. `$env:OBSIDIAN_LLM_WIKI_ROOT`.
3. Fallback: `[Environment]::GetFolderPath('MyDocuments')` + `Obsidian Vault\LLM WIKI`.

## Vault root resolution

Resolved by `_common.ps1` in this order:

1. `-VaultRoot <path>` parameter on the script. This may be the base directory or a concrete vault directory.
2. `$env:OBSIDIAN_VAULT_ROOT`.
3. Fallback: `[Environment]::GetFolderPath('MyDocuments')` + `Obsidian Vault`.

All vault scripts accept:

- `-Vault LibreOffice|POI|OfficeToPdf` (default `LibreOffice`) - selects the vault subfolder.
- `-VaultRoot <path>` - explicit base or vault override.

Vault aliases (subfolder under the resolved base):

| Alias | Subfolder |
| --- | --- |
| `LibreOffice` | `LibreOffice-to-PDF` (`LibreOffice-Word-to-PDF` is also recognized for compatibility) |
| `POI` | `Apache-POI-Word` |
| `OfficeToPdf` | `Office-to-PDF` |
