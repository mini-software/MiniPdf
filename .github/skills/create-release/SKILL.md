---
name: create-release
description: "Create a new GitHub release for MiniPdf with CLI and GUI AOT binaries. Use when: publishing a release, creating a new version, bumping version, tagging a release, generating release notes from git diff."
argument-hint: "Optionally specify the new version number (e.g. v0.27.0)"
---

# Create MiniPdf Release

Automate the full release cycle: compute version, commit, tag, generate release notes from git diff, and publish via `gh release create` so that CLI + GUI AOT workflows build and upload binaries automatically.

## Prerequisites

- All changes committed and pushed to `upstream main`
- `gh` CLI authenticated with repo write access
- Repository remote: `upstream` -> `mini-software/MiniPdf`

## Version Convention

| Component | Tag pattern | Trigger workflow |
|-----------|-------------|-----------------|
| Library + CLI + GUI | `v*` (e.g. `v0.26.0`) | `release-aot.yml` (CLI) + `release-aot-gui.yml` (GUI) + `nuget-publish.yml` |

Version bumps follow `v{major}.{minor}.{patch}` — default increment is `+0.1.0` on the minor.

## Procedure

### 1. Determine Version

```powershell
git tag --sort=-v:refname | Select-Object -First 1
```

Parse the last tag, increment minor by 1, e.g. `v0.25.0` -> `v0.26.0`.
If user specifies a version, use that instead.

### 2. Ensure Changes Are Committed and Pushed

```powershell
git status --short
# If uncommitted changes exist, stage and commit them first
git push upstream main
```

### 3. Generate Diff Summary

```powershell
git log {LAST_TAG}..HEAD --oneline
git diff {LAST_TAG} HEAD --stat
```

### 4. Create Tag

```powershell
git tag {NEW_TAG}
git push upstream {NEW_TAG}
```

### 5. Create Release via `gh`

Write release notes to a temp file, then create:

```powershell
gh release create {NEW_TAG} --repo mini-software/MiniPdf --title "{NEW_TAG}" --notes-file release-notes.tmp
Remove-Item release-notes.tmp -Force
```

**IMPORTANT**: Do NOT use PowerShell here-strings (`@"..."@`) for multi-line notes -- they have escaping issues in the terminal. Instead, use `create_file` to write the notes file, then pass `--notes-file`.

### 6. Verify Workflows Triggered

```powershell
Start-Sleep 5
gh run list --repo mini-software/MiniPdf --limit 4 --json name,status -q ".[] | {name,status}"
```

Expect to see:
- `Release AOT Binaries` — builds CLI for 6 platforms
- `Release GUI AOT Binaries` — builds GUI for 6 platforms
- `NuGet Publish` — publishes library to NuGet

## Release Notes Template

Use this structure for the release body (write via `create_file` to a temp file):

```markdown
## What's New in {NEW_TAG}

### Highlights

- **Feature A** -- description
- **Fix B** -- description

### Downloads

| Platform | CLI | GUI |
|----------|-----|-----|
| Windows x64 | `minipdf-win-x64.zip` | `minipdf-gui-win-x64.zip` |
| Windows ARM64 | `minipdf-win-arm64.zip` | `minipdf-gui-win-arm64.zip` |
| Linux x64 | `minipdf-linux-x64.tar.gz` | `minipdf-gui-linux-x64.tar.gz` |
| Linux ARM64 | `minipdf-linux-arm64.tar.gz` | `minipdf-gui-linux-arm64.tar.gz` |
| macOS x64 | `minipdf-osx-x64.tar.gz` | `minipdf-gui-osx-x64.tar.gz` |
| macOS ARM64 | `minipdf-osx-arm64.tar.gz` | `minipdf-gui-osx-arm64.tar.gz` |

### Changes since {LAST_TAG}

{GIT_LOG_ONELINE}

```
{GIT_DIFF_STAT}
```

**Full Changelog**: https://github.com/mini-software/MiniPdf/compare/{LAST_TAG}...{NEW_TAG}
```

## Release Assets (auto-built)

The following assets are built and uploaded automatically by GitHub Actions:

### CLI (`release-aot.yml`)
| Artifact | Platform |
|----------|----------|
| `minipdf-win-x64.zip` | Windows x64 |
| `minipdf-win-arm64.zip` | Windows ARM64 |
| `minipdf-linux-x64.tar.gz` | Linux x64 |
| `minipdf-linux-arm64.tar.gz` | Linux ARM64 |
| `minipdf-osx-x64.tar.gz` | macOS x64 |
| `minipdf-osx-arm64.tar.gz` | macOS ARM64 |

### GUI (`release-aot-gui.yml`)
| Artifact | Platform | Contents |
|----------|----------|----------|
| `minipdf-gui-win-x64.zip` | Windows x64 | exe + native DLLs |
| `minipdf-gui-win-arm64.zip` | Windows ARM64 | exe + native DLLs |
| `minipdf-gui-linux-x64.tar.gz` | Linux x64 | binary + .so libs |
| `minipdf-gui-linux-arm64.tar.gz` | Linux ARM64 | binary + .so libs |
| `minipdf-gui-osx-x64.tar.gz` | macOS x64 | binary + .dylib libs |
| `minipdf-gui-osx-arm64.tar.gz` | macOS ARM64 | binary + .dylib libs |

**GUI packages must include native libraries** (libSkiaSharp, libHarfBuzzSharp, av_libglesv2) alongside the executable. The exe alone will not launch.

## Fixing a Bad Release

If a release needs to be recreated (e.g. workflow bug, wrong packaging):

```powershell
# Delete remote release and tag
gh release delete {TAG} --repo mini-software/MiniPdf --yes --cleanup-tag

# Delete and recreate local tag on latest commit
git tag -d {TAG}
git tag {TAG}
git push upstream {TAG}

# Recreate release
gh release create {TAG} --repo mini-software/MiniPdf --title "{TAG}" --notes-file release-notes.tmp
```

## Rules

- Never use `--force` on tags that other people may have fetched -- delete and recreate instead.
- Always verify both CLI and GUI workflows are triggered after creating a release.
- The system cannot auto `git push` per project policy -- always push explicitly.
- Clean up temp files (`release-notes.tmp`) after release creation.
