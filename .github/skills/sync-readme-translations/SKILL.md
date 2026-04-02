---
name: sync-readme-translations
description: "Sync changes from the English README.md to all translated README files. Use when: updating README translations, syncing README across languages, propagating README changes to other locales."
argument-hint: "Describe what changed in README.md that needs syncing"
---

# Sync README Translations

Propagate content changes from the English `README.md` to all translated versions, preserving each language's natural phrasing.

## Target Files

| File | Language |
|------|----------|
| `README.zh-CN.md` | 简体中文 (Simplified Chinese) |
| `documents/README.zh-TW.md` | 繁體中文 (Traditional Chinese) |
| `documents/README.ja.md` | 日本語 (Japanese) |
| `documents/README.ko.md` | 한국어 (Korean) |
| `documents/README.it.md` | Italiano (Italian) |
| `documents/README.fr.md` | Français (French) |

## Procedure

1. **Read the English README.md** to identify the changed sections.
2. **For each translated file**, read the corresponding section and apply the same change in that language.
3. **Preserve localized phrasing** — translate new/changed text naturally; do not produce literal word-for-word translations.
4. **Keep shared elements untranslated** — badges, image paths, links, HTML tags, table structure, and code blocks must stay identical across all files.
5. **Use `multi_replace_string_in_file`** to batch all edits efficiently when multiple files need the same structural change.
6. **Verify** that the number of edits matches the number of target files × changed sections.

## Rules

- Never remove or reorder sections that exist in a translated file but not in English — the structures are kept in sync.
- Benchmark numbers (scores, case counts) are language-neutral; copy them verbatim.
- If a section doesn't exist yet in a translated file, add it at the same position as in the English README.
- All paths in links/images are relative to the repo root and must not change.
