"""Check the actual PDF stream for classic128 to see baseline positions."""
import fitz
import os

doc = fitz.open(os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', 'classic128_font_sizes.pdf'))
page = doc[0]

# Get the content stream
xref = page.xref
content = page.get_text("rawdict")

# Get raw page contents
contents = page.get_contents()
for xref_id in contents:
    stream = doc.xref_stream(xref_id).decode('latin-1')
    # Find the text rendering commands around "Font size 12"
    lines = stream.split('\n')
    for i, line in enumerate(lines):
        if 'Font size 1' in line or 'Tf' in line:
            start = max(0, i-3)
            end = min(len(lines), i+3)
            for j in range(start, end):
                print(f'  [{j:4d}] {lines[j]}')
            print()

doc.close()
