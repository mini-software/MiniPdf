"""Dump classic132 PDF stream"""
import fitz

doc = fitz.open("../MiniPdf.Scripts/pdf_output/classic132_striped_table.pdf")
page = doc[0]
content_bytes = page.read_contents()
content = content_bytes.decode('latin-1')
lines = content.split('\n')
for i, line in enumerate(lines[:80]):
    print(f"{i:3d}: {line}")
doc.close()
