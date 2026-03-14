"""Dump the raw PDF content stream to see Td positioning fully."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf'
doc = fitz.open(path)
page = doc[0]

contents = page.get_contents()
for xref_id in contents:
    stream = doc.xref_stream(xref_id).decode('latin-1')
    print(stream[:3000])
doc.close()
