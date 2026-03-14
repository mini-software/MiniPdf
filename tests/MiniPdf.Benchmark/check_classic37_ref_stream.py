"""Dump raw PDF content stream for reference classic37"""
import fitz

doc = fitz.open("reference_pdfs/classic37_freeze_panes.pdf")
page = doc[0]

content_bytes = page.read_contents()
content = content_bytes.decode('latin-1')

lines = content.split('\n')
for i, line in enumerate(lines[:150]):
    print(f"{i:3d}: {line}")

doc.close()
