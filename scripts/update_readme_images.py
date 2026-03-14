#!/usr/bin/env python3
"""
update_readme_images.py
Updates the Visual Comparison table in README.md with all classic test cases.
Uses comparison_report.json to get scores and image filenames (page 1 only).

Usage:
    python scripts/update_readme_images.py
"""

import json
import re
from pathlib import Path

REPO_ROOT = Path(__file__).parent.parent
README_PATH = REPO_ROOT / "README.md"
REPORT_JSON = REPO_ROOT / "tests/MiniPdf.Benchmark/reports/comparison_report.json"
IMAGE_DIR_REL = "tests/MiniPdf.Benchmark/reports/images"
IMG_WIDTH = 320

# ── score → emoji ──────────────────────────────────────────────────────────────
def score_emoji(score: float) -> str:
    if score >= 0.90:
        return "🟢"
    elif score >= 0.70:
        return "🟡"
    else:
        return "🔴"

# ── human-readable test name ───────────────────────────────────────────────────
def pretty_name(name: str) -> str:
    """classic01_basic_table_with_headers → Basic table with headers"""
    m = re.match(r"^classic\d+_(.*)", name)
    if m:
        return m.group(1).replace("_", " ").capitalize()
    return name

# ── build one test-case block ──────────────────────────────────────────────────
def build_row(entry: dict, has_office: bool = False) -> str:
    case_name = entry["name"]
    score     = entry.get("overall_score", 0.0)
    diff_imgs = entry.get("diff_images", [])

    # pick page 1 images; fall back to first available
    p1 = next((d for d in diff_imgs if d.get("page") == 1), diff_imgs[0] if diff_imgs else None)

    display  = pretty_name(case_name)
    num_code = case_name.split("_")[0]          # e.g. classic01
    emoji    = score_emoji(score)
    pct      = f"{score * 100:.1f}%"
    img_w    = 220 if has_office else IMG_WIDTH
    num_cols = 3 if has_office else 2

    if p1:
        mini_src = f"{IMAGE_DIR_REL}/{p1['minipdf_img']}"
        ref_src  = f"{IMAGE_DIR_REL}/{p1['reference_img']}"
        td_mini = f'  <td><img src="{mini_src}" width="{img_w}"/></td>'
        td_ref  = f'  <td><img src="{ref_src}" width="{img_w}"/></td>'
        if has_office:
            office_img = p1.get('office_img')
            td_office = (f'  <td><img src="{IMAGE_DIR_REL}/{office_img}" width="{img_w}"/></td>'
                         if office_img else '  <td><i>no image</i></td>')
    else:
        td_mini = "  <td><i>no image</i></td>"
        td_ref  = "  <td><i>no image</i></td>"
        td_office = "  <td><i>no image</i></td>"

    lines = [
        "<tr>",
        f"  <td><b>{num_code}</b></td>",
        f"  <td colspan=\"{num_cols - 1}\">{display} {emoji} {pct}</td>",
        "</tr>",
        "<tr>",
        td_mini,
        td_ref,
    ]
    if has_office:
        lines.append(td_office)
    lines.append("</tr>")
    return "\n".join(lines)


# ── build full table ───────────────────────────────────────────────────────────
def build_table(entries: list) -> str:
    # Detect if any entry has office images
    has_office = any(
        pg.get("office_img")
        for e in entries
        for pg in e.get("diff_images", [])
    )
    if has_office:
        desc = ("All test cases comparing MiniPdf output vs LibreOffice reference vs Office (Excel). "
                "Page 1 shown for multi-page results.\n\n")
        header = (desc +
                  "<table>\n"
                  "<tr><th>MiniPdf</th><th>LibreOffice (Reference)</th><th>Office (Excel)</th></tr>")
    else:
        header = (
            "All test cases comparing MiniPdf output vs LibreOffice reference. "
            "Page 1 shown for multi-page results.\n\n"
            "<table>\n"
            "<tr><th>MiniPdf</th><th>LibreOffice (Reference)</th></tr>"
        )
    # Sort by numeric id so classic100 follows classic99, not classic09
    def _sort_key(e):
        m = re.match(r"classic(\d+)", e["name"])
        return int(m.group(1)) if m else 0
    sorted_entries = sorted(entries, key=_sort_key)
    rows = "\n".join(build_row(e, has_office=has_office) for e in sorted_entries)
    return f"{header}\n{rows}\n</table>"


# ── replace section in README ──────────────────────────────────────────────────
SECTION_START_RE = re.compile(
    r"(### Visual Comparison\n)",
    re.MULTILINE,
)
SECTION_END_RE = re.compile(
    r"</table>",
    re.MULTILINE,
)

def update_readme(readme_path: Path, new_table: str) -> None:
    text = readme_path.read_text(encoding="utf-8")

    # find "### Visual Comparison"
    m_start = SECTION_START_RE.search(text)
    if not m_start:
        raise ValueError("### Visual Comparison header not found in README.md")

    # find the LAST </table> after the section start
    search_from = m_start.end()
    m_end = None
    for m in SECTION_END_RE.finditer(text, search_from):
        m_end = m
    if not m_end:
        raise ValueError("</table> not found after ### Visual Comparison")

    before = text[: m_start.end()]          # up to and including the header line
    after  = text[m_end.end():]             # everything after </table>
    new_text = before + "\n" + new_table + "\n" + after

    readme_path.write_text(new_text, encoding="utf-8")
    print(f"README updated: {readme_path}")


# ── main ───────────────────────────────────────────────────────────────────────
def main():
    if not REPORT_JSON.exists():
        raise FileNotFoundError(f"Report not found: {REPORT_JSON}")

    entries = json.loads(REPORT_JSON.read_text(encoding="utf-8"))
    print(f"Loaded {len(entries)} test cases from report.")

    new_table = build_table(entries)
    update_readme(README_PATH, new_table)

    # quick sanity
    lines = README_PATH.read_text(encoding="utf-8").splitlines()
    print(f"README now has {len(lines)} lines.")


if __name__ == "__main__":
    main()
