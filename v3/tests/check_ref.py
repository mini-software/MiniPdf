import fitz
doc = fitz.open(r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic09_long_text.pdf')
for i in range(min(doc.page_count, 3)):
    page = doc[i]
    blocks = page.get_text('dict')['blocks']
    spans = [s for b in blocks if 'lines' in b for l in b['lines'] for s in l['spans']]
    print(f'Page {i+1}: {len(spans)} spans')
    for s in spans[:8]:
        text = s['text']
        bbox = s['bbox']
        print(f'  len={len(text)} y={bbox[1]:.0f} x={bbox[0]:.0f} text={text[:60]}')
    if len(spans) > 8:
        print(f'  ... and {len(spans)-8} more')
