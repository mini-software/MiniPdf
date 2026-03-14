"""Check classic09 reference PDF text positions across multiple pages to understand the column layout."""
import fitz

ref_doc = fitz.open('reference_pdfs/classic09_long_text.pdf')

for p in range(min(3, len(ref_doc))):
    page = ref_doc[p]
    print(f'\n=== Ref Page {p+1} ===')
    
    blocks = page.get_text("dict")["blocks"]
    for b in blocks:
        if b['type'] == 0:
            for line in b['lines']:
                for span in line['spans']:
                    text = span['text']
                    bbox = span['bbox']
                    print(f'  "{text[:40]}..." len={len(text):3d} x=({bbox[0]:.1f}-{bbox[2]:.1f}) y={bbox[1]:.1f} w={bbox[2]-bbox[0]:.1f}' if len(text) > 10 else
                          f'  "{text}" x=({bbox[0]:.1f}-{bbox[2]:.1f}) y={bbox[1]:.1f}')
    
    # Check draws/fills
    draws = page.get_drawings()
    fills = [d for d in draws if d.get('fill')]
    lines_n = [d for d in draws if not d.get('fill') and d.get('items')]
    print(f'  Drawings: {len(draws)} fills={len(fills)} lines={len(lines_n)}')

ref_doc.close()

# Also check MiniPdf
mini_doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf')
for p in range(min(3, len(mini_doc))):
    page = mini_doc[p]
    print(f'\n=== MiniPdf Page {p+1} ===')
    blocks = page.get_text("dict")["blocks"]
    for b in blocks:
        if b['type'] == 0:
            for line in b['lines']:
                for span in line['spans']:
                    text = span['text']
                    bbox = span['bbox']
                    print(f'  "{text[:40]}..." len={len(text):3d} x=({bbox[0]:.1f}-{bbox[2]:.1f}) y={bbox[1]:.1f} w={bbox[2]-bbox[0]:.1f}' if len(text) > 10 else
                          f'  "{text}" x=({bbox[0]:.1f}-{bbox[2]:.1f}) y={bbox[1]:.1f}')
mini_doc.close()
