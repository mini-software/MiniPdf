"""Show text blocks in classic132"""
import fitz

doc = fitz.open("../MiniPdf.Scripts/pdf_output/classic132_striped_table.pdf")
page = doc[0]
content_bytes = page.read_contents()
content = content_bytes.decode('latin-1')
lines = content.split('\n')

for i in range(950, min(1100, len(lines))):
    print(f"{i:4d}: {lines[i]}")

doc.close()
