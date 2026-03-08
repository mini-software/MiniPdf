"""Check what text classic93 MiniPdf PDF actually contains - detailed extraction."""
import fitz
import os
import sys

sys.stdout.reconfigure(encoding='utf-8')

MINI_DIR = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
REF_DIR = 'reference_pdfs'

def extract_detailed(pdf_path):
    """Extract text with position info."""
    doc = fitz.open(pdf_path)
    result = []
    for pi, page in enumerate(doc):
        blocks = page.get_text("dict", flags=fitz.TEXT_PRESERVE_WHITESPACE)["blocks"]
        for block in blocks:
            if "lines" not in block:
                continue
            for line in block["lines"]:
                for span in line["spans"]:
                    text = span["text"].strip()
                    if text:
                        bbox = span["bbox"]
                        result.append((pi, bbox[0], bbox[1], bbox[2], bbox[3], span["size"], text))
    doc.close()
    return result

for name in ['classic93_line_chart', 'classic91_simple_bar_chart']:
    print(f"\n=== {name} ===")
    print("--- MiniPdf ---")
    m_spans = extract_detailed(os.path.join(MINI_DIR, f"{name}.pdf"))
    for pi, x0, y0, x1, y1, sz, text in m_spans:
        print(f"  p{pi} ({x0:.0f},{y0:.0f}) sz={sz:.0f}: {text[:60]}")
    
    print("--- Reference ---")
    r_spans = extract_detailed(os.path.join(REF_DIR, f"{name}.pdf"))
    for pi, x0, y0, x1, y1, sz, text in r_spans:
        print(f"  p{pi} ({x0:.0f},{y0:.0f}) sz={sz:.0f}: {text[:60]}")
