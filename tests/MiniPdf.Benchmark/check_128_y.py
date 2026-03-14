import fitz, os
doc = fitz.open(os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', 'classic128_font_sizes.pdf'))
page = doc[0]
data = page.get_text('dict', sort=True)
for block in data.get('blocks', []):
    if block.get('type', 0) != 0: continue
    for line in block.get('lines', []):
        for span in line.get('spans', []):
            text = span.get('text', '').strip()
            if not text: continue
            bbox = span['bbox']
            size = span.get('size', 0)
            if 8 <= size <= 14:
                print(f'  y={bbox[1]:.4f} x={bbox[0]:.1f} sz={size:.0f} "{text[:40]}"')
doc.close()
