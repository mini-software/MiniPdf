import fitz

# Check Y coordinates of text spans for paragraph 1 on page 1
doc = fitz.open('minipdf_docx/SA8000 ch sample.pdf')
page = doc[0]
blocks = page.get_text('dict')['blocks']
for b in blocks:
    if 'lines' not in b:
        continue
    for line in b['lines']:
        for span in line['spans']:
            t = span['text']
            if '签定' in t or t.strip() in ['√', '）', '（']:
                bbox = span['bbox']
                print(f"y0={bbox[1]:.1f} y1={bbox[3]:.1f} x0={bbox[0]:.1f} x1={bbox[2]:.1f} fs={span['size']:.1f} text={repr(t)}")
