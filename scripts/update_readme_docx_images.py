#!/usr/bin/env python3
"""
update_readme_docx_images.py
Generates the DOCX Visual Comparison table and inserts/updates it in README.md.
Reads reports_docx/comparison_report.json for scores and image filenames (page 1 only).

Usage:
    python scripts/update_readme_docx_images.py
"""

import json
import re
from pathlib import Path

REPO_ROOT = Path(__file__).parent.parent
README_PATH = REPO_ROOT / "README.md"
REPORT_JSON = REPO_ROOT / "tests/MiniPdf.Benchmark/reports_docx/comparison_report.json"
IMAGE_DIR_REL = "tests/MiniPdf.Benchmark/reports_docx/images"
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
def pretty_name(raw_name: str) -> str:
    """docx_classic01_single_paragraph → Single paragraph"""
    # strip leading "docx_"
    name = raw_name
    if name.startswith("docx_"):
        name = name[5:]
    m = re.match(r"^classic\d+_(.*)", name)
    if m:
        return m.group(1).replace("_", " ").capitalize()
    return name

# ── build one test-case block (2 rows, 2 columns) ─────────────────────────────
def build_row(entry: dict) -> str:
    case_name = entry["name"]
    score     = entry.get("overall_score", 0.0)
    diff_imgs = entry.get("diff_images", [])

    # pick page 1 images; fall back to first available
    p1 = next((d for d in diff_imgs if d.get("page") == 1), diff_imgs[0] if diff_imgs else None)

    display  = pretty_name(case_name)
    # extract "classic01" etc. from "docx_classic01_single_paragraph"
    m = re.match(r"docx_(classic\d+)", case_name)
    num_code = m.group(1) if m else case_name.split("_")[0]
    emoji    = score_emoji(score)
    pct      = f"{score * 100:.1f}%"

    if p1:
        mini_src = f"{IMAGE_DIR_REL}/{p1['minipdf_img']}"
        ref_src  = f"{IMAGE_DIR_REL}/{p1['reference_img']}"
        td_mini = f'  <td><img src="{mini_src}" width="{IMG_WIDTH}"/></td>'
        td_ref  = f'  <td><img src="{ref_src}" width="{IMG_WIDTH}"/></td>'
    else:
        td_mini = "  <td><i>no image</i></td>"
        td_ref  = "  <td><i>no image</i></td>"

    lines = [
        "<tr>",
        f"  <td><b>{num_code}</b></td>",
        f"  <td>{display} {emoji} {pct}</td>",
        "</tr>",
        "<tr>",
        td_mini,
        td_ref,
        "</tr>",
    ]
    return "\n".join(lines)


# ── build summary table ───────────────────────────────────────────────────────
def build_summary(entries: list) -> str:
    scores = [e.get("overall_score", 0) for e in entries]
    avg = sum(scores) / len(scores) if scores else 0
    excellent = sum(1 for s in scores if s >= 0.90)
    acceptable = sum(1 for s in scores if 0.70 <= s < 0.90)
    needs_improvement = sum(1 for s in scores if s < 0.70)

    lines = [
        f"MiniPdf DOCX output is compared against LibreOffice as the reference renderer across **{len(entries)} classic test cases**.",
        "",
        "| Category | Count | Threshold |",
        "|---|---|---|",
        f"| 🟢 Excellent | {excellent} | ≥ 90% |",
        f"| 🟡 Acceptable | {acceptable} | 70% – 90% |",
        f"| 🔴 Needs Improvement | {needs_improvement} | < 70% |",
        "",
        f"**Average overall score: {avg * 100:.1f}%** (text similarity 40% + visual similarity 40% + page count 20%)",
    ]
    return "\n".join(lines)


# ── build full visual comparison table ─────────────────────────────────────────
def build_visual_table(entries: list) -> str:
    header = (
        "All DOCX test cases comparing MiniPdf output vs LibreOffice reference. "
        "Page 1 shown for multi-page results.\n\n"
        "<table>\n"
        "<tr><th>MiniPdf</th><th>LibreOffice (Reference)</th></tr>"
    )
    # Sort by numeric id
    def _sort_key(e):
        m = re.match(r"docx_classic(\d+)", e["name"])
        return int(m.group(1)) if m else 0
    sorted_entries = sorted(entries, key=_sort_key)
    rows = "\n".join(build_row(e) for e in sorted_entries)
    return f"{header}\n{rows}\n</table>"


# ── section markers ────────────────────────────────────────────────────────────
SECTION_HEADER = "### DOCX Benchmark"
SECTION_START = "<!-- DOCX_BENCHMARK_START -->"
SECTION_END = "<!-- DOCX_BENCHMARK_END -->"


def build_full_section(entries: list) -> str:
    summary = build_summary(entries)
    visual  = build_visual_table(entries)
    return f"""{SECTION_START}

{SECTION_HEADER}

{summary}

#### DOCX Visual Comparison

{visual}

{SECTION_END}"""


def update_readme(readme_path: Path, new_section: str) -> None:
    text = readme_path.read_text(encoding="utf-8")

    # If markers already exist, replace between them
    start_idx = text.find(SECTION_START)
    end_idx = text.find(SECTION_END)

    if start_idx != -1 and end_idx != -1:
        before = text[:start_idx]
        after  = text[end_idx + len(SECTION_END):]
        new_text = before + new_section + after
    else:
        # Insert before the last line (or at the end of the benchmark section)
        # Find the end of the existing xlsx </table> section
        last_table = text.rfind("</table>")
        if last_table != -1:
            insert_pos = last_table + len("</table>")
            before = text[:insert_pos]
            after  = text[insert_pos:]
            new_text = before + "\n\n" + new_section + after
        else:
            new_text = text + "\n\n" + new_section + "\n"

    readme_path.write_text(new_text, encoding="utf-8")
    print(f"README updated: {readme_path}")


# ── main ───────────────────────────────────────────────────────────────────────
def main():
    if not REPORT_JSON.exists():
        raise FileNotFoundError(f"Report not found: {REPORT_JSON}")

    entries = json.loads(REPORT_JSON.read_text(encoding="utf-8"))
    print(f"Loaded {len(entries)} DOCX test cases from report.")

    new_section = build_full_section(entries)
    update_readme(README_PATH, new_section)

    lines = README_PATH.read_text(encoding="utf-8").splitlines()
    print(f"README now has {len(lines)} lines.")


if __name__ == "__main__":
    main()
