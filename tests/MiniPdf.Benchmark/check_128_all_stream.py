"""Dump ALL Td commands from classic128 to see baseline positions."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf'
doc = fitz.open(path)
page = doc[0]

contents = page.get_contents()
for xref_id in contents:
    stream = doc.xref_stream(xref_id).decode('latin-1')
    lines = stream.split('\n')
    for i, line in enumerate(lines):
        line = line.strip()
        if 'Td' in line or 'Tf' in line or 'Tj' in line:
            print(line)
doc.close()
