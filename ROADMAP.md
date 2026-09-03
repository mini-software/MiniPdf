# MiniPdf Roadmap

This roadmap describes current priorities, not a promise of delivery dates.
Priorities are refined through public issues and discussions under
`GOVERNANCE.md`.

MiniPdf is not an Apache Software Foundation project. The project is adopting
open, independent governance practices and may seek an Apache Incubator
readiness review after demonstrating that those practices work in public.

## Project Scope

The core project provides embeddable, open-source Office-to-PDF conversion
engines, public library APIs, command-line interfaces, and a shared conformance
corpus. Conversion must not require Microsoft Office, LibreOffice, Adobe
Acrobat, or COM automation at runtime.

The GUI, web demo, hosted API, deployment examples, benchmarks, and coding-agent
automation are convenience tooling. They may move to separate repositories when
that improves ownership, release clarity, or repository size.

MiniExcel is a separate project and is not part of MiniPdf governance or any
future MiniPdf incubation proposal.

## Implementation Status

| Area | .NET | Rust |
|---|---|---|
| XLSX to PDF | Stable, with documented compatibility limits | Experimental |
| DOCX to PDF | Stable, with documented compatibility limits | Experimental |
| PPTX to PDF | Basic support | Unsupported |
| Library API | Stable release line | Experimental pre-1.0 release line |
| CLI | Supported | Experimental |

Stable means the implementation is released and covered by the project's normal
compatibility process. It does not mean complete Microsoft Office layout
fidelity. Experimental areas may change incompatibly and must identify known
gaps in their release notes.

## Near-Term Priorities

### Open Governance and Community

- Apply the public decision process to significant technical and governance
  changes.
- Expand the active maintainer group to at least three legally independent
  participants through demonstrated merit, not honorary appointments.
- Enable contributors other than the founder to lead triage, reviews, design
  discussions, and release preparation.
- Publish community-health evidence without counting bots or mechanical
  artifact refreshes as independent participation.

### Security and Provenance

- Inventory the origin and licensing of source files, dependencies, fonts,
  images, Office fixtures, PDFs, and generated artifacts.
- Add malformed ZIP/XML, resource-limit, path-handling, concurrency, and fuzz
  coverage for untrusted inputs.
- Generate dependency license reports and software bills of materials for
  release candidates.

### Reproducible Releases

- Produce source-first release candidates from immutable commits.
- Add checksums, signatures, and source-to-binary manifests.
- Require independent build and test verification before publication.
- Rehearse at least two public, multi-person releases before requesting an
  external governance-readiness review.

### Maintainable Implementations

- Run .NET and Rust validation on every relevant pull request.
- Maintain a versioned format and feature support matrix.
- Use shared conformance fixtures where behavior should match while allowing
  implementation-specific architecture.
- Move large generated benchmark outputs out of the main source history while
  retaining compact, reviewable summaries.

## Non-Goals

- Claiming complete Microsoft Office rendering compatibility.
- Requiring the .NET and Rust implementations to share an architecture.
- Promoting experimental features to stable to meet a date.
- Increasing contributor counts through nominal or inactive roles.
- Treating stars, downloads, or generated commits as substitutes for a
  sustainable contributor community.

## Readiness Review Gate

The project should postpone an Apache Incubator proposal until it can show:

- at least three active and legally independent maintainers with real decision
  and review responsibility;
- sustained external contribution and independent review during the previous
  90 days;
- two reproducible source-first release rehearsals, each independently verified
  by three participants;
- public records for consequential decisions and maintainer selection;
- no unresolved high-risk provenance or licensing findings; and
- a clear willingness to transfer required code and trademark rights if an
  incubation proposal is accepted.