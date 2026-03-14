"""Check Y positions of spans for classic150 row 2 to understand why text splits."""
import fitz
import os

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'
name = 'classic150_kitchen_sink_styles'

for label, d in [('MiniPdf', mini_dir), ('Reference', ref_dir)]:
    doc = fitz.open(os.path.join(d, f'{name}.pdf'))
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
                y_rounded = round(bbox[1], 1)
                print(f'  y={bbox[1]:.2f} yr={y_rounded} x={bbox[0]:.1f} sz={size:.1f} "{text[:40]}"')
    doc.close()
