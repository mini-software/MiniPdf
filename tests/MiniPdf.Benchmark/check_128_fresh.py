"""Re-read classic128 fresh, bypassing any cache."""
import fitz
import os

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf'
print(f'File size: {os.path.getsize(path)} bytes')
print(f'Modified: {os.path.getmtime(path)}')

doc = fitz.open(path)
page = doc[0]

# Get raw content to verify
contents = page.get_contents()
for xref_id in contents:
    stream = doc.xref_stream(xref_id).decode('latin-1')
    for line in stream.split('\n'):
        if 'Font size 12' in line or ('632' in line and 'Td' in line):
            print(f'  STREAM: {line}')

# Now get text dict
data = page.get_text('dict', sort=True)
for block in data.get('blocks', []):
    if block.get('type', 0) != 0: continue
    for line in block.get('lines', []):
        for span in line.get('spans', []):
            text = span.get('text', '').strip()
            if not text: continue
            bbox = span['bbox']
            size = span.get('size', 0)
            if size in (11, 12) and bbox[1] > 130 and bbox[1] < 155:
                print(f'  SPAN: y={bbox[1]:.6f} x={bbox[0]:.1f} sz={size:.0f} "{text}"')
doc.close()
