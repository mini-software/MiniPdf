"""Compare text positions between MiniPdf and reference PDFs."""
import fitz
import sys

cases = [
    "classic01_basic_table_with_headers",
    "classic06_tall_table",
]

for case in cases:
    mini_path = f"../MiniPdf.Scripts/pdf_output/{case}.pdf"
    ref_path = f"reference_pdfs/{case}.pdf"
    
    for label, path in [("MINI", mini_path), ("REF", ref_path)]:
        doc = fitz.open(path)
        page = doc[0]
        print(f"=== {label} {case} (page: {page.rect.width:.1f} x {page.rect.height:.1f}) ===")
        blocks = page.get_text("dict")
        count = 0
        for block in blocks.get("blocks", []):
            if block.get("type") != 0:
                continue
            for line in block.get("lines", []):
                for span in line.get("spans", []):
                    x = span["origin"][0]
                    y = span["origin"][1]
                    text = span["text"].strip()[:25]
                    size = span["size"]
                    font = span.get("font", "?")
                    if text and count < 15:
                        print(f"  x={x:6.1f} y={y:6.1f} size={size:4.1f} font={font:20s} \"{text}\"")
                        count += 1
        doc.close()
        print()
