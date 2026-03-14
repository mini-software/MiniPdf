"""Check classic128_font_sizes span Y positions."""
import fitz
import os

for label, d in [('MiniPdf', os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')), 
                  ('Reference', 'reference_pdfs')]:
    doc = fitz.open(os.path.join(d, 'classic128_font_sizes.pdf'))
    page = doc[0]
    data = page.get_text("dict", sort=True)
    
    print(f'\n=== {label} ===')
    for block in data.get("blocks", []):
        if block.get("type", 0) != 0:
            continue
        for line in block.get("lines", []):
            for span in line.get("spans", []):
                text = span.get("text", "").strip()
                if not text:
                    continue
                bbox = span["bbox"]
                size = span.get("size", 0)
                print(f'  y={bbox[1]:.2f} x={bbox[0]:.1f} sz={size:.0f} "{text[:40]}"')
    doc.close()
