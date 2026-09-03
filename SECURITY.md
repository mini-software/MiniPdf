# Security Policy

MiniPdf processes untrusted ZIP, XML, image, font, and document content. Security
reports are treated as a high priority even when the affected behavior is not
yet covered by an automated test.

## Supported Versions

Security fixes are provided for the latest published .NET release and the
latest published Rust release. Older versions may receive a fix when the impact
and backport risk justify it, but this is not guaranteed.

## Reporting a Vulnerability

Do not open a public issue for a vulnerability that has not been coordinated.
Use GitHub's private vulnerability reporting form:

https://github.com/mini-software/MiniPdf/security/advisories/new

Include, when possible:

- the affected package, version, platform, and document format;
- a minimal reproducer or proof of concept;
- the expected impact and required attacker capabilities;
- whether the issue is already public; and
- any suggested mitigation.

Do not include confidential production documents or personal data. Create a
small synthetic reproducer whenever possible.

If private vulnerability reporting is unavailable, open a public issue asking a
maintainer to establish private contact. Do not include vulnerability details in
that issue.

## Response Process

The project aims to acknowledge a report within seven days. A maintainer will
coordinate validation, affected-version analysis, a fix, release timing, and
credit with the reporter. Complex reports may take longer, but the reporter
should receive progress updates.

Security fixes should include a regression test when it can be added without
revealing an unpatched vulnerability. Advisories are published after a fix is
available or when disclosure is otherwise necessary to protect users.

## Scope

Examples of in-scope reports include arbitrary code execution, path traversal,
ZIP bombs or uncontrolled resource consumption, unsafe XML processing,
malformed-input crashes that enable denial of service, and disclosure of input
document content.

Reports that only compare rendering fidelity, without a security impact, should
use the conversion-quality issue form instead.