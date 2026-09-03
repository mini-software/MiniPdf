# MiniPdf Governance

MiniPdf is governed by its active contributors. The project aims to make
decisions openly, recognize sustained merit, and avoid dependence on any one
person or organization.

MiniPdf is not an Apache Software Foundation project. This policy adopts useful
open-source governance practices while the community evaluates its long-term
governance options.

## Principles

- Project decisions serve users and the long-term health of the project.
- Participants act as individuals. Employment or organizational affiliation
  does not grant additional authority.
- Technical and governance decisions are discussed in public and in writing.
- Contributions of code, documentation, testing, issue triage, reviews,
  security work, release verification, and community support all count as
  merit.
- Authority is earned through sustained, constructive participation and can be
  shared with any contributor who demonstrates the required judgment.

## Roles

### Contributors

Anyone who participates constructively is a contributor. Contributors may open
issues and discussions, submit pull requests, review work, improve
documentation, verify releases, and help other users.

### Maintainers

Maintainers are contributors trusted to merge changes and participate in
project decisions. Maintainers are expected to:

- review contributions fairly and promptly;
- explain consequential decisions in public;
- protect compatibility, security, licensing, and release quality;
- disclose relevant conflicts of interest; and
- help qualified contributors take on greater responsibility.

The current maintainers and their areas of responsibility are listed in
`MAINTAINERS.md`.

### Release Managers

A release manager prepares a release candidate and coordinates its review. The
role is temporary for each release and does not grant unilateral authority to
publish it. Release approval follows the process documented below.

## Decision Making

Routine, reversible changes use lazy consensus. A proposal may proceed after a
reasonable review period when there is support and no unresolved objection from
a maintainer. The expected review period is at least 72 hours for significant
changes and may be shorter for routine pull requests.

Significant decisions must begin as a public GitHub issue or discussion. These
include:

- changes to governance, licensing, project scope, or compatibility policy;
- removal or substantial redesign of a public API;
- adoption of a required runtime dependency;
- changes to the release process; and
- promotion or removal of a maintainer.

The proposer should describe the problem, alternatives, compatibility impact,
and a migration plan when applicable. Decisions reached in meetings or private
messages must be summarized in the public thread before implementation.

## Voting

When discussion does not produce a clear consensus, a maintainer may call a
vote in the relevant public issue or discussion. Votes remain open for at least
72 hours and use these values:

- `+1`: support
- `0`: no position
- `-1`: oppose, with an explanation and preferably an alternative

Unless a stricter rule is documented, a vote passes with at least three
maintainer votes, more `+1` than `-1`, and no unresolved technical veto on a
code change. If the project has fewer than three active maintainers, all active
maintainers must participate and the result must be unanimous. This fallback is
temporary and should not be treated as evidence of a diverse community.

Release candidates require at least three positive maintainer votes after each
voter independently verifies the source archive, checks its provenance and
licensing material, and builds and tests it. Until the project has three active
maintainers, releases continue under the existing process and must explicitly
record that governance limitation.

## Becoming a Maintainer

There is no fixed contribution count. A candidate should demonstrate sustained
participation, sound technical or community judgment, constructive review, and
an understanding of the project's security and licensing responsibilities.

Any maintainer may nominate a contributor in a private discussion when personal
information must be considered. The final invitation and its rationale are
announced publicly after approval by consensus of the active maintainers. A
candidate's employer, nationality, or use of a particular programming language
must not affect the decision.

## Inactive Maintainers

A maintainer who has not participated for six months may move to emeritus status
after a public check-in. Emeritus maintainers retain recognition and may return
to active status by resuming sustained participation. Access may be removed
immediately when necessary to protect the project or its users, followed by a
documented maintainer review.

## Conflicts of Interest

Participants must disclose financial, employment, or personal interests that a
reasonable observer could expect to affect a decision. A conflicted maintainer
may provide relevant facts but should abstain from the final decision. The
remaining maintainers decide whether recusal is necessary.

## Private Matters

Security reports, conduct reports, credentials, and sensitive personal matters
must use the private channels documented in `SECURITY.md` and
`CODE_OF_CONDUCT.md`. Technical outcomes should be summarized publicly once it
is safe and appropriate to do so.

## Changing This Policy

Changes to this policy require a public proposal, at least seven days of
discussion, and consensus of the active maintainers. The discussion must explain
how the change improves openness, independence, or project sustainability.