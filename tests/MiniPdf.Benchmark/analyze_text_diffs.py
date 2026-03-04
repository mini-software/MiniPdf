#!/usr/bin/env python3
"""Analyze text differences between MiniPdf and reference PDFs for near-threshold cases."""
import fitz
import sys
import os
from difflib import SequenceMatcher, unified_diff

MINIPDF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"

# Near-threshold cases (0.97-0.99 range, text-limited)
CASES = [
    "classic49_contact_list",
    "classic78_small_icon_per_row",
    "classic66_invoice_with_logo",
    "classic86_software_screenshot_features",
    "classic51_product_catalog",
    "classic90_project_status_with_milestones",
    "classic44_employee_roster",
    "classic13_date_strings",
    "classic68_restaurant_menu",
    "classic83_color_swatch_palette",
]

def extract_text_blocks(pdf_path):
    """Extract text blocks grouped by page, same method as compare_pdfs.py."""
    doc = fitz.open(pdf_path)
    pages = []
    for page in doc:
        blocks = page.get_text("dict")["blocks"]
        spans = []
        for b in blocks:
            if "lines" not in b:
                continue
            for line in b["lines"]:
                for span in line["spans"]:
                    text = span["text"].strip()
                    if text:
                        spans.append((span["origin"][0], span["origin"][1], text))
        # Group by Y (tolerance 1.0)
        spans.sort(key=lambda s: (s[1], s[0]))
        rows = []
        current_row = []
        current_y = None
        for x, y, text in spans:
            if current_y is None or abs(y - current_y) > 1.0:
                if current_row:
                    rows.append(current_row)
                current_row = [(x, text)]
                current_y = y
            else:
                current_row.append((x, text))
        if current_row:
            rows.append(current_row)
        # Merge row texts
        row_texts = []
        for row in rows:
            row.sort(key=lambda r: r[0])
            row_texts.append(" ".join(t for _, t in row))
        pages.append(row_texts)
    doc.close()
    return pages

def compare_case(name):
    mini_path = os.path.join(MINIPDF_DIR, name + ".pdf")
    ref_path = os.path.join(REF_DIR, name + ".pdf")
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        print(f"  SKIPPED: missing PDF")
        return

    mini_pages = extract_text_blocks(mini_path)
    ref_pages = extract_text_blocks(ref_path)

    mini_flat = []
    for p in mini_pages:
        mini_flat.extend(p)
    ref_flat = []
    for p in ref_pages:
        ref_flat.extend(p)

    # Show unified diff
    diff = list(unified_diff(ref_flat, mini_flat, lineterm='',
                             fromfile='REFERENCE', tofile='MINIPDF', n=0))
    if not diff:
        print("  No text differences!")
        return

    # Count differences
    adds = [l for l in diff if l.startswith('+') and not l.startswith('+++')]
    removes = [l for l in diff if l.startswith('-') and not l.startswith('---')]

    print(f"  Ref lines: {len(ref_flat)}, Mini lines: {len(mini_flat)}")
    print(f"  Diff: {len(removes)} removed, {len(adds)} added")

    # Show first 30 diff lines
    shown = 0
    for line in diff:
        if line.startswith('@@') or line.startswith('---') or line.startswith('+++'):
            continue
        print(f"    {line}")
        shown += 1
        if shown >= 30:
            remaining = len([l for l in diff if not l.startswith('@@') and not l.startswith('---') and not l.startswith('+++')]) - shown
            if remaining > 0:
                print(f"    ... ({remaining} more diff lines)")
            break

    # Identify patterns
    truncations = []
    for r in removes:
        r_text = r[1:].strip()
        for a in adds:
            a_text = a[1:].strip()
            if r_text.startswith(a_text) or a_text.startswith(r_text):
                shorter = min(r_text, a_text, key=len)
                longer = max(r_text, a_text, key=len)
                if len(shorter) >= 3 and len(longer) - len(shorter) <= 5:
                    truncations.append((r_text, a_text))
                    break

    if truncations:
        print(f"\n  Truncation patterns ({len(truncations)}):")
        for ref_t, mini_t in truncations[:10]:
            print(f"    REF: '{ref_t}' -> MINI: '{mini_t}'")

if __name__ == "__main__":
    for name in CASES:
        print(f"\n{'='*60}")
        print(f"Case: {name}")
        print(f"{'='*60}")
        compare_case(name)
