# Office Reference Scripts

Helper PowerShell scripts for the `libreoffice-reference` skill. They wrap access to local Obsidian vaults.

| Script | Purpose |
| --- | --- |
| `list-vault.ps1` | List notes that actually exist on disk (optionally scoped to a folder). |
| `search-vault.ps1` | Regex search across the vault (optionally scoped to a folder). |

## Vault root resolution

Resolved by `_common.ps1` in this order:

1. `-VaultRoot <path>` parameter on the script.
2. `$env:OBSIDIAN_VAULT_ROOT`.
3. Fallback: `[Environment]::GetFolderPath('MyDocuments')` + `Obsidian Vault`.

All scripts accept:

- `-Vault LibreOffice|POI|OfficeToPdf` (default `LibreOffice`) — selects the vault subfolder.
- `-VaultRoot <path>` — explicit base override.

Vault aliases (subfolder under the resolved base):

| Alias | Subfolder |
| --- | --- |
| `LibreOffice` | `LibreOffice-Word-to-PDF` |
| `POI` | `Apache-POI-Word` |
| `OfficeToPdf` | `Office-to-PDF` |
