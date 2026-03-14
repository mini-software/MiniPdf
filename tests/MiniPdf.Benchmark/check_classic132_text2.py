"""Dump classic132 PDF stream - find text"""
import fitz

doc = fitz.open("../MiniPdf.Scripts/pdf_output/classic132_striped_table.pdf")
page = doc[0]
content_bytes = page.read_contents()
content = content_bytes.decode('latin-1')
lines = content.split('\n')

# Show lines from 200 onwards (text usually after fills/lines)
for i, line in enumerate(lines):
    if i >= 200 and i < 320:
        print(f"{i:3d}: {line}")
    
print(f"\nTotal lines: {len(lines)}")
doc.close()
