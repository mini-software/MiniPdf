import fitz

doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic128_font_sizes.pdf')
page = doc[0]
data = page.get_text('rawdict', sort=False)
for block in data['blocks']:
    if block.get('type', 0) != 0:
        continue
    for line in block['lines']:
        for span in line['spans']:
            text = span.get('text', '').strip()
            if not text:
                # rawdict uses 'chars' instead of 'text'
                chars = span.get('chars', [])
                text = ''.join(c.get('c', '') for c in chars).strip()
            if '12' in text or 'Font size' in text:
                print(f"origin={span.get('origin')}, bbox={span['bbox']}, size={span['size']}, text=[{text}]")
