# Contributing to MiniPdf

Thank you for helping MiniPdf. Contributions include code, documentation,
tests, issue triage, design discussion, release verification, and user support.

Please follow the `CODE_OF_CONDUCT.md`. Project decisions and the path to
maintainership are documented in `GOVERNANCE.md`; current priorities are in
`ROADMAP.md`.

## Before You Start

- Search existing issues and discussions before opening a new one.
- Use an issue or discussion for a significant API, architecture, dependency,
	compatibility, governance, or release-process change before implementation.
- Do not submit confidential documents, personal data, proprietary fonts, or
	test files that you do not have permission to redistribute.
- Report suspected vulnerabilities through `SECURITY.md`, not a public issue.

By submitting a contribution, you represent that you have the right to submit
it under the repository's Apache License 2.0. Clearly identify third-party code,
fixtures, images, fonts, or generated material and include their source and
license. MiniPdf does not currently require a separate contributor license
agreement.

## Development Setup

Clone your fork and create a focused branch. Keep unrelated formatting,
generated artifacts, and benchmark refreshes out of the change.

### .NET

Install a supported .NET SDK, then run:

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test tests/MiniPdf.Tests --configuration Release
```

DOCX tests that measure installed Cambria or Segoe fonts report an explicit skip
when their required fonts are unavailable. CI runs the portable suite on Ubuntu
and a separate DOCX suite on Windows, where it verifies the required fonts
before testing. Install these fonts through a licensed system/Office installation
to run the font-dependent checks locally; do not add font binaries to the repository.

### Rust

Install the stable Rust toolchain, then run:

```powershell
Set-Location minipdf-rs
cargo fmt --check
cargo clippy --workspace --all-targets -- -D warnings
cargo test --workspace
```

### Python

Install Python 3.10 or later, then run:

```powershell
python -m pip install -e ".\minipdf-python[dev]"
python -m pytest minipdf-python/tests
python -m ruff check minipdf-python
python -m ruff format --check minipdf-python
python -m mypy minipdf-python/src minipdf-python/tests
python -m build minipdf-python
python -m twine check minipdf-python/dist/*
```

### Documentation

Keep commands, APIs, benchmark results, and report links consistent. Changes to
the English `README.md` must also be applied to the corresponding translated
README files under `documents/`.

## Making a Change

1. Reproduce the behavior with the smallest useful test or fixture.
2. Make one focused change at the owning implementation boundary.
3. Add or update automated tests for behavior changes.
4. Run the relevant .NET or Rust checks above.
5. For rendering changes, run the narrowest applicable visual benchmark.
6. Explain compatibility, security, provenance, and documentation effects in
	 the pull request.

Useful focused benchmark commands include:

```powershell
scripts/Run-Benchmark.ps1 -Filter "border"
scripts/Run-Benchmark_docx.ps1 -Filter "heading"
scripts/Run-Benchmark_issues.ps1 -Filter "sa8000"
scripts/Run-Rust-Benchmark.ps1 -Filter "classic01" -MinimumScore 0.99
```

Generated PDFs, PNGs, heatmaps, and reports should only be committed when they
are canonical project evidence requested by the relevant workflow. Otherwise,
attach them to the pull request or retain them as CI artifacts.

## Pull Requests

Pull requests should be small enough to review and should include:

- the problem and intended behavior;
- linked issues or design discussions when applicable;
- tests and exact validation commands;
- before-and-after benchmark evidence for rendering changes;
- public API or compatibility impact;
- the source and license of new third-party material; and
- documentation updates for user-visible behavior.

The project aims to provide an initial review within seven days, but complex visual
rendering and security changes may take longer. Authors should respond to review
questions and avoid force-pushing after review begins unless rebasing is needed.

Maintainers may request that a large change be split, discussed publicly, or
reworked to preserve compatibility and project scope. Reviews assess the change,
not the contributor.

### Automatic Review Requests

PRs targeting `main` automatically request active human members of the
[`core-reviewers` team](https://github.com/orgs/mini-software/teams/core-reviewers)
when opened, reopened, marked ready for review, or updated. The PR author,
already requested reviewers, and anyone who has already submitted a review are
excluded. Draft PRs do not request reviews. Merge approval requirements are
configured separately from review invitations.

Maintainers can update the reviewer list by adding or removing team members;
pending organization invitations must be accepted before members are included.
The team must have Write or higher repository access. Removing a member does
not revoke existing review requests or their separately granted permissions.

Before enabling the [reviewer workflow](.github/workflows/auto-reviewers.yml),
configure a fine-grained personal access token with `mini-software` as the resource
owner and only the organization **Members: read** permission. Store it as the
repository Actions secret `CORE_REVIEWERS_TOKEN` and ensure any required
organization approval is complete. Rotate the token before it expires. Do not
reuse a local GitHub CLI login token. The dedicated token only reads team
membership; review requests use the workflow's `GITHUB_TOKEN` with
**Pull requests: write** permission.

After the workflow is merged into `main`, use its **Run workflow** action with
a PR number to refresh reviewers on an existing PR. Team changes are otherwise
picked up on the next supported PR event, not immediately when membership changes.
The workflow uses `pull_request_target` to support fork PRs without checking out
or executing PR code. Missing credentials, team access errors, and unconfirmed
review requests fail the workflow explicitly.

### Approval Requirements

The tiered review policy for PRs targeting `main`, once activated below, is:

- Ordinary changes: at least **one approval** from someone other than the author.
- Important changes with the `major-change` label: at least **three approvals**.

Approvals must come from users with Write or higher repository permissions.
Invitations, comments, author approvals, bot reviews, and dismissed approvals do
not count. A later approval or change request supersedes that user's earlier
decision; a comment does not cancel an approval. Existing native protections
still apply, including blocking unresolved change requests.

Maintainers should add `major-change` before review for public API changes,
architecture changes, breaking changes, governance changes, or other high-impact
modifications. Do not remove the label merely to bypass the three-approval
requirement. This is a project policy, not an ASF-wide requirement for every
GitHub PR.

The [approval workflow](.github/workflows/review-approval-policy.yml) publishes
the commit status **Review approval policy**. It refreshes all open, non-draft
PRs on PR/label changes and after the
[read-only review notification workflow](.github/workflows/review-changed.yml)
completes. This two-workflow design supports fork reviews without granting the
notification workflow secrets or write permissions, or running PR code in the
privileged workflow. Neither approval workflow requires `CORE_REVIEWERS_TOKEN`.

Changes are processed asynchronously. Wait for the approval workflow to finish
after changing labels or reviews, then check the status before merging.
Maintainers can manually run **Review approval policy** to refresh existing PRs.
PRs sharing a head commit also share its status, so all such open, non-draft PRs
must meet their individual approval requirements.

To activate enforcement without a gap:

1. Merge these workflows into `main` while retaining the current native
   **three-approval** requirement for every PR.
2. Run **Review approval policy** and verify its live commit status on open PRs.
3. In the **Copilot review for default branch** ruleset, require the
   **Review approval policy** status check from **GitHub Actions**, then lower the
   native required approvals to **one** in the same ruleset update. Preserve the
   empty bypass list, Copilot review, deletion protection, and force-push protection.

Until this activation is complete, the native three-approval requirement
continues to apply to every PR. Do not lower it before the required status check
is configured. Team-based automatic invitations additionally require the secret
described above, but tiered approval enforcement does not.

The automation and its tests are adapted from
[mini-software/MiniExcel#1040](https://github.com/mini-software/MiniExcel/pull/1040)
under Apache License 2.0. CI runs both test files; run them locally with Node.js
20 or later:

```powershell
node --test .github\tests\auto-reviewers.test.cjs .github\tests\review-approval-policy.test.cjs
```

## Earning More Responsibility

Sustained contributions and sound judgment can lead to maintainer
responsibility. Code volume is not the only form of merit: reviews,
documentation, issue triage, release verification, security work, and community
support all count. See `GOVERNANCE.md` for the selection process and
`MAINTAINERS.md` for areas that need additional ownership.