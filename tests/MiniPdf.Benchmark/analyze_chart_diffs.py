#!/usr/bin/env python3
"""Analyze text differences for chart cases to find fixable patterns."""
import fitz
import os

MINIPDF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"

CASES = [
    "classic103_pie_chart_with_labels",
    "classic110_chart_with_legend",
    "classic111_chart_with_axis_labels",
    "classic107_multi_series_line",
    "classic102_line_chart_with_markers",
    "classic93_line_chart",
    "classic109_scatter_with_trendline",
    "classic104_combo_bar_line_chart",
    "classic105_3d_bar_chart",
    "classic113_chart_sheet",
    "classic112_multiple_charts",
    "classic100_stacked_bar_chart",
    "classic101_percent_stacked_bar",
    "classic115_chart_negative_values",
    "classic91_simple_bar_chart",
    "classic117_stock_ohlc_chart",
]

def extract_text_lines(pdf_path):
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
        row_texts = []
        for row in rows:
            row.sort(key=lambda r: r[0])
            row_texts.append(" ".join(t for _, t in row))
        pages.append(row_texts)
    doc.close()
    return pages

for name in CASES:
    mini_path = os.path.join(MINIPDF_DIR, name + ".pdf")
    ref_path = os.path.join(REF_DIR, name + ".pdf")
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        continue
    mini = extract_text_lines(mini_path)
    ref = extract_text_lines(ref_path)
    
    print(f"\n{'='*70}")
    print(f"Case: {name}")
    print(f"{'='*70}")
    print(f"  Pages: ref={len(ref)}, mini={len(mini)}")
    
    for i in range(max(len(ref), len(mini))):
        r = ref[i] if i < len(ref) else []
        m = mini[i] if i < len(mini) else []
        if r != m:
            print(f"\n  ---- Page {i+1} differences ----")
            print(f"  REF ({len(r)} lines):")
            for line in r:
                print(f"    [{line}]")
            print(f"  MINI ({len(m)} lines):")
            for line in m:
                print(f"    [{line}]")
