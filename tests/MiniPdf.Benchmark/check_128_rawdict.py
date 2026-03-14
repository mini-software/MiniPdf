"""Check classic128 text positions using raw PDF content stream parsing, 
not PyMuPDF's get_text which may do grouping/adjustments."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf'
doc = fitz.open(path)
page = doc[0]

# Use get_texttrace or rawdict to get raw span positions
data = page.get_text('rawdict')
for block in data.get('blocks', []):
    if block.get('type', 0) != 0: continue
    for line in block.get('lines', []):
        for span in line.get('spans', []):
            t = span.get('text', '').strip()
            if not t: continue
            chars = span.get('chars', [])
            if chars:
                c0 = chars[0]
                print(f'  span_bbox={span["bbox"]}, char0_bbox={c0["bbox"]}, text={t!r}, size={span["size"]}')
            else:
                print(f'  span_bbox={span["bbox"]}, NO_CHARS, text={t!r}, size={span["size"]}')
doc.close()
