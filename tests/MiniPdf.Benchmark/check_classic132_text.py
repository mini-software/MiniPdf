"""Dump classic132 PDF stream - text parts"""
import fitz

doc = fitz.open("../MiniPdf.Scripts/pdf_output/classic132_striped_table.pdf")
page = doc[0]
content_bytes = page.read_contents()
content = content_bytes.decode('latin-1')
lines = content.split('\n')

# Find BT/ET blocks
in_text = False
for i, line in enumerate(lines):
    s = line.strip()
    if s == 'BT' or s.startswith('q') or s.startswith('Q') or 'Tz' in s or 'Td' in s or 'Tj' in s or s == 'ET' or 're W' in s:
        print(f"{i:3d}: {line}")
    if i > 500:
        break
doc.close()
