"""Extract text around item 6 from MiniPdf PDF"""
import fitz

d = fitz.open('tests/Issue_Files/minipdf_docx/SA8000 ch sample.pdf')
page = d[0]

# Get all text lines on page 1
blocks = page.get_text('dict')['blocks']
for bi, b in enumerate(blocks):
    if 'lines' not in b:
        continue
    for li, line in enumerate(b['lines']):
        text = ''.join(s['text'] for s in line['spans'])
        y = line['bbox'][1]
        # Show lines in the area of items 5-7 (roughly middle of page)
        if 260 < y < 400:
            fs = line['spans'][0]['size'] if line['spans'] else 0
            x = line['bbox'][0]
            print(f"y={y:6.1f} x={x:6.1f} fs={fs:4.1f} text={repr(text[:100])}")
