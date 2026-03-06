"""Compare extracted text using SAME method as compare_pdfs.py (PyMuPDF)."""
import fitz
import difflib
import os
import json

MINIPDF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REFERENCE_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"

def extract_text_pymupdf(pdf_path):
    pages = []
    doc = fitz.open(pdf_path)
    for page in doc:
        data = page.get_text("dict", sort=True)
        spans = []
        for block in data.get("blocks", []):
            if block.get("type", 0) != 0:
                continue
            for line in block.get("lines", []):
                for span in line.get("spans", []):
                    text = span.get("text", "").strip()
                    if text:
                        spans.append((round(span["bbox"][1], 1), span["bbox"][0], text))
        spans.sort()
        lines = []
        current_y = None
        current_tokens = []
        for y, x, text in spans:
            if current_y is None or abs(y - current_y) > 1.0:
                if current_tokens:
                    current_tokens.sort()
                    lines.append(" ".join(t for _, t in current_tokens))
                current_y = y
                current_tokens = [(x, text)]
            else:
                current_tokens.append((x, text))
        if current_tokens:
            current_tokens.sort()
            lines.append(" ".join(t for _, t in current_tokens))
        pages.append("\n".join(lines))
    doc.close()
    return pages

# All chart cases
cases = [
    "classic91_simple_bar_chart",
    "classic92_horizontal_bar_chart",
    "classic93_line_chart",
    "classic94_pie_chart",
    "classic95_area_chart",
    "classic96_scatter_chart",
    "classic97_doughnut_chart",
    "classic98_radar_chart",
    "classic100_stacked_bar_chart",
    "classic101_percent_stacked_bar",
    "classic102_line_chart_with_markers",
    "classic111_chart_with_axis_labels",
    "classic115_chart_negative_values",
    "classic118_bar_chart_custom_colors",
]

for case in cases:
    mini_path = os.path.join(MINIPDF_DIR, case + ".pdf")
    ref_path = os.path.join(REFERENCE_DIR, case + ".pdf")
    
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        continue
    
    mini_pages = extract_text_pymupdf(mini_path)
    ref_pages = extract_text_pymupdf(ref_path)
    
    # Flatten all pages
    mini_lines = []
    for p in mini_pages:
        mini_lines.extend(p.split('\n'))
    ref_lines = []
    for p in ref_pages:
        ref_lines.extend(p.split('\n'))
    
    print(f"\n{'='*70}")
    print(f"CASE: {case}")
    print(f"Our pages: {len(mini_pages)}, Ref pages: {len(ref_pages)}")
    print(f"Our lines: {len(mini_lines)}, Ref lines: {len(ref_lines)}")
    print(f"{'='*70}")
    
    # Show actual lines side by side  
    print("--- REFERENCE ---")
    for i, line in enumerate(ref_lines):
        print(f"  R{i:2d}: {line}")
    print("--- MINIPDF ---")
    for i, line in enumerate(mini_lines):
        print(f"  M{i:2d}: {line}")
    
    # Show diff
    diff = list(difflib.unified_diff(ref_lines, mini_lines, lineterm='', n=0))
    if diff:
        print("--- DIFF ---")
        for line in diff:
            print(f"  {line}")
