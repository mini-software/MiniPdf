#!/usr/bin/env python3
"""
Compare MiniPdf vs Reference PDF for 'Cooperation Agreement Template'.
Extracts per-span font, size, color, position and shows differences.
"""
import fitz  # PyMuPDF
import sys, os

MINI_PATH = "tests/Issue_Files/minipdf_docx/Cooperation Agreement Template.pdf"
REF_PATH  = "tests/Issue_Files/reference_docx/Cooperation Agreement Template.pdf"

def extract_spans(doc, page_idx):
    """Extract all text spans with full metadata from a page."""
    pg = doc[page_idx]
    data = pg.get_text("dict", sort=True)
    spans = []
    for block in data["blocks"]:
        if block["type"] != 0:
            continue
        for line in block["lines"]:
            for span in line["spans"]:
                text = span["text"]
                if not text.strip():
                    continue
                bbox = span["bbox"]  # (x0, y0, x1, y1)
                origin = span.get("origin", (bbox[0], bbox[1]))
                flags = span.get("flags", 0)
                spans.append({
                    "text": text,
                    "font": span.get("font", "?"),
                    "size": round(span.get("size", 0), 2),
                    "color": span.get("color", 0),
                    "x0": round(bbox[0], 1),
                    "y0": round(bbox[1], 1),
                    "x1": round(bbox[2], 1),
                    "y1": round(bbox[3], 1),
                    "origin_x": round(origin[0], 1),
                    "origin_y": round(origin[1], 1),
                    "width": round(bbox[2] - bbox[0], 1),
                    "bold": bool(flags & (1 << 4)),
                    "italic": bool(flags & (1 << 1)),
                    "flags": flags,
                })
    return spans


def color_hex(c):
    """Convert fitz color int to #RRGGBB."""
    if isinstance(c, int):
        return f"#{c:06X}"
    return str(c)


def group_by_y(spans, tol=2.0):
    """Group spans into visual rows by Y position."""
    rows = []
    current_y = None
    current_row = []
    for s in sorted(spans, key=lambda s: (s["origin_y"], s["x0"])):
        if current_y is None or abs(s["origin_y"] - current_y) > tol:
            if current_row:
                rows.append(current_row)
            current_row = [s]
            current_y = s["origin_y"]
        else:
            current_row.append(s)
    if current_row:
        rows.append(current_row)
    return rows


def row_text(row):
    return " ".join(s["text"] for s in row)


def print_page_comparison(mini_doc, ref_doc, page_idx):
    print(f"\n{'='*80}")
    print(f"  PAGE {page_idx+1}")
    print(f"{'='*80}")

    mini_spans = extract_spans(mini_doc, page_idx)
    ref_spans  = extract_spans(ref_doc, page_idx)

    mini_rows = group_by_y(mini_spans)
    ref_rows  = group_by_y(ref_spans)

    # --- Font/size/color summary ---
    print(f"\n  Font/Size Summary:")
    for label, spans in [("MiniPdf", mini_spans), ("Reference", ref_spans)]:
        fonts = {}
        for s in spans:
            key = (s["font"], s["size"], s["bold"])
            fonts[key] = fonts.get(key, 0) + 1
        print(f"    {label}:")
        for (font, size, bold), count in sorted(fonts.items(), key=lambda x: -x[1]):
            b = " BOLD" if bold else ""
            print(f"      {font} {size}pt{b} — {count} spans")

    # --- Color summary ---
    print(f"\n  Color Summary:")
    for label, spans in [("MiniPdf", mini_spans), ("Reference", ref_spans)]:
        colors = {}
        for s in spans:
            c = color_hex(s["color"])
            colors[c] = colors.get(c, 0) + 1
        print(f"    {label}: {colors}")

    # --- Row-by-row content comparison ---
    print(f"\n  Row-by-row comparison (first 40 rows):")
    print(f"  {'Row':>3} | {'Y(M)':>6} {'Y(R)':>6} {'dY':>5} | MiniPdf text → Reference text")
    print(f"  {'-'*3}-+-{'-'*6}-{'-'*6}-{'-'*5}-+-{'-'*60}")

    max_rows = max(len(mini_rows), len(ref_rows))
    for i in range(min(max_rows, 40)):
        m_row = mini_rows[i] if i < len(mini_rows) else []
        r_row = ref_rows[i] if i < len(ref_rows) else []
        m_y = m_row[0]["origin_y"] if m_row else 0
        r_y = r_row[0]["origin_y"] if r_row else 0
        m_text = row_text(m_row)
        r_text = row_text(r_row)
        dy = m_y - r_y if m_row and r_row else 0
        marker = " " if m_text == r_text else "*"
        if m_text != r_text:
            print(f"{marker} {i+1:>3} | {m_y:6.1f} {r_y:6.1f} {dy:+5.1f} | M: {m_text[:70]}")
            print(f"  {'':>3} | {'':>6} {'':>6} {'':>5} | R: {r_text[:70]}")
        else:
            print(f"  {i+1:>3} | {m_y:6.1f} {r_y:6.1f} {dy:+5.1f} | {m_text[:70]}")

    print(f"\n  Total rows: MiniPdf={len(mini_rows)}, Reference={len(ref_rows)}")


def print_span_detail(mini_doc, ref_doc, page_idx, y_range=None):
    """Print detailed per-span info for a page, optionally filtered by Y range."""
    print(f"\n  Detailed spans page {page_idx+1}" +
          (f" (Y {y_range[0]}-{y_range[1]})" if y_range else ""))

    for label, doc in [("MiniPdf", mini_doc), ("Reference", ref_doc)]:
        spans = extract_spans(doc, page_idx)
        if y_range:
            spans = [s for s in spans if y_range[0] <= s["origin_y"] <= y_range[1]]
        print(f"\n    {label} ({len(spans)} spans):")
        print(f"    {'X':>6} {'Y':>6} {'W':>6} | {'Size':>5} {'Bold':>4} | Font             | Text")
        print(f"    {'-'*6}-{'-'*6}-{'-'*6}-+-{'-'*5}-{'-'*4}-+-{'-'*17}+-{'-'*40}")
        for s in sorted(spans, key=lambda s: (s["origin_y"], s["x0"])):
            b = "B" if s["bold"] else " "
            print(f"    {s['x0']:6.1f} {s['origin_y']:6.1f} {s['width']:6.1f} | {s['size']:5.1f} {b:>4} | {s['font']:17s}| {s['text'][:40]}")


def main():
    if not os.path.exists(MINI_PATH):
        print(f"ERROR: {MINI_PATH} not found"); sys.exit(1)
    if not os.path.exists(REF_PATH):
        print(f"ERROR: {REF_PATH} not found"); sys.exit(1)

    mini = fitz.open(MINI_PATH)
    ref  = fitz.open(REF_PATH)
    print(f"MiniPdf pages: {len(mini)}, Reference pages: {len(ref)}")

    # Compare all pages (summary)
    for pg in range(min(len(mini), len(ref))):
        print_page_comparison(mini, ref, pg)

    # Detailed spans for pages 1-2 (where the image diff is most visible)
    # Page 2 (index 1) — the "合作模式" paragraph and underline blanks
    print_span_detail(mini, ref, 1)
    # Page 3 (index 2) — where content shifts
    print_span_detail(mini, ref, 2)

    # Focus on the underline area on page 2 (Y ~350-550 approx)
    print("\n\n=== UNDERLINE AREA FOCUS (Page 2, Y 300-600) ===")
    print_span_detail(mini, ref, 1, y_range=(300, 600))

    mini.close()
    ref.close()


if __name__ == "__main__":
    main()
