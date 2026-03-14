"""Dump raw PDF content stream for classic37"""
import fitz

doc = fitz.open("../MiniPdf.Scripts/pdf_output/classic37_freeze_panes.pdf")
page = doc[0]

# Get the content stream as bytes
xref = page.xref
content_bytes = page.read_contents()
content = content_bytes.decode('latin-1')

# Print first 2000 chars of content stream
lines = content.split('\n')
for i, line in enumerate(lines[:100]):
    print(f"{i:3d}: {line}")

doc.close()
