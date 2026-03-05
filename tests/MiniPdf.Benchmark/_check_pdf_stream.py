import fitz
import sys

# Read the MINI PDF and extract all text operations from the content stream
path = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf"
doc = fitz.open(path)
page = doc[0]

# Get page content as text
xref = page.xref
contents = page.get_contents()
for c in contents:
    stream = doc.xref_stream(c)
    text = stream.decode('latin-1')
    # Find lines with "Tj" (text show operator)
    lines = text.split('\n')
    for i, line in enumerate(lines):
        if 'Tj' in line or 'Td' in line or 'Tf' in line:
            # Show context
            start = max(0, i-2)
            end = min(len(lines), i+1)
            for j in range(start, end):
                print(f"  {j}: {lines[j]}")
            print()
