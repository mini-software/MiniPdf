"""Compare row heights in reference PDFs vs MINI PDFs for key near-miss files."""
import fitz
import os

def get_text_y_positions(path):
    """Extract Y positions of text lines from a PDF."""
    doc = fitz.open(path)
    positions = []
    for page_num, page in enumerate(doc):
        blocks = page.get_text("dict")["blocks"]
        for b in blocks:
            if "lines" not in b:
                continue
            for l in b["lines"]:
                for s in l["spans"]:
                    if s["text"].strip():
                        positions.append((page_num, round(s["origin"][1], 2), s["text"][:30]))
    doc.close()
    return positions

files = [
    "classic01_basic_table_with_headers",
    "classic13_date_strings",
    "classic134_heatmap",
    "classic132_striped_table",
    "classic121_thin_borders",
    "classic125_solid_fills",
    "classic131_number_formats",
    "classic137_checkerboard",
    "classic06_tall_table",
]

for name in files:
    mini_path = f"../MiniPdf.Scripts/pdf_output/{name}.pdf"
    ref_path = f"reference_pdfs/{name}.pdf"
    
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        continue
    
    mini_pos = get_text_y_positions(mini_path)
    ref_pos = get_text_y_positions(ref_path)
    
    print(f"\n{'='*60}")
    print(f"  {name}")
    print(f"{'='*60}")
    
    # Find Y positions on page 0
    mini_ys = sorted(set(y for p, y, t in mini_pos if p == 0))
    ref_ys = sorted(set(y for p, y, t in ref_pos if p == 0))
    
    print(f"  MINI Y count: {len(mini_ys)}, REF Y count: {len(ref_ys)}")
    
    # Show first few Y positions and row gaps
    n = min(10, len(mini_ys), len(ref_ys))
    print(f"  {'Row':>4} {'MINI Y':>10} {'REF Y':>10} {'MINI gap':>10} {'REF gap':>10}")
    for i in range(n):
        mini_gap = mini_ys[i] - mini_ys[i-1] if i > 0 else 0
        ref_gap = ref_ys[i] - ref_ys[i-1] if i > 0 else 0
        print(f"  {i:>4} {mini_ys[i]:>10.2f} {ref_ys[i]:>10.2f} {mini_gap:>10.2f} {ref_gap:>10.2f}")
