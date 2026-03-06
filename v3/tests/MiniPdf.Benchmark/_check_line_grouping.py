import fitz, json

doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic128_font_sizes.pdf')
page = doc[0]
data = page.get_text('rawdict', sort=False)
for bi, block in enumerate(data['blocks']):
    if block.get('type', 0) != 0:
        continue
    for li, line in enumerate(block['lines']):
        spans_text = []
        for span in line['spans']:
            chars = span.get('chars', [])
            text = ''.join(c.get('c', '') for c in chars).strip()
            if text:
                spans_text.append((text, span.get('origin'), span['bbox'], span['size']))
        has_12 = any('12' in t for t, _, _, _ in spans_text)
        if has_12:
            print(f"Block {bi}, Line {li}:")
            print(f"  line wmode={line.get('wmode')}, dir={line.get('dir')}")
            for text, origin, bbox, size in spans_text:
                print(f"  span: origin={origin}, bbox_top={bbox[1]:.4f}, size={size}, text=[{text}]")
            print()
