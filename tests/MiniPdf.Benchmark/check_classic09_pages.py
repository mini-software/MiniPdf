"""Check page content counts for classic09."""
import fitz

for label, path in [('Ref', 'reference_pdfs/classic09_long_text.pdf'),
                     ('MiniPdf', '../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf')]:
    doc = fitz.open(path)
    print(f'\n=== {label}: {len(doc)} pages ===')
    for p in range(len(doc)):
        text = doc[p].get_text().strip()
        blocks = len(doc[p].get_text("dict")["blocks"])
        print(f'  Page {p+1:2d}: {blocks} blocks, text={len(text)} chars, first: {repr(text[:40])}')
    doc.close()
