"""Compare classic137 checkerboard: text positions, fill rects, and overall structure."""
import fitz

for label, path in [('MiniPdf', '../MiniPdf.Scripts/pdf_output/classic137_checkerboard.pdf'),
                     ('Ref', 'reference_pdfs/classic137_checkerboard.pdf')]:
    doc = fitz.open(path)
    page = doc[0]
    print(f'\n=== {label} ===')
    print(f'Page size: {page.rect}')
    
    # Text spans
    blocks = page.get_text("dict")["blocks"]
    print(f'Text blocks: {len(blocks)}')
    for b in blocks:
        if b['type'] == 0:  # text
            for line in b['lines']:
                for span in line['spans']:
                    text = span['text'][:30]
                    bbox = span['bbox']
                    print(f'  text="{text}" bbox=({bbox[0]:.1f},{bbox[1]:.1f},{bbox[2]:.1f},{bbox[3]:.1f}) size={span["size"]:.1f}')
    
    # Drawings (fills)
    drawings = page.get_drawings()
    fills = [d for d in drawings if d.get('fill')]
    rects = [d for d in fills if len(d.get('items', [])) == 1 and d['items'][0][0] == 're']
    print(f'Fill rects: {len(rects)}')
    # Show first 10
    for d in rects[:10]:
        r = d['items'][0][1]
        c = d.get('fill')
        print(f'  rect=({r.x0:.1f},{r.y0:.1f},{r.x1:.1f},{r.y1:.1f}) fill={c}')
    if len(rects) > 10:
        print(f'  ... and {len(rects)-10} more')
    
    doc.close()
