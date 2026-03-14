"""Check how Tz scaling works in MiniPdf PDF and if text extraction gets full text."""
import fitz, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf')
p = doc[0]

# Simple text extraction
text = p.get_text()
for line in text.split('\n'):
    if len(line) > 20:
        print(f"extracted len={len(line)}: {line[:80]}...")

# Raw PDF stream - look for Tz near X text
xref_list = p.get_contents()
for xref in xref_list:
    content = doc.xref_stream(xref).decode('latin-1')
    lines = content.split('\n')
    for i, line in enumerate(lines):
        if 'Tz' in line or ('Tj' in line and len(line) > 30):
            print(f"  stream[{i}]: {line[:150]}")

doc.close()
