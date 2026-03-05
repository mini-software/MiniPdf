"""Check raw PDF content stream for classic13."""
import fitz

path = "../MiniPdf.Scripts/pdf_output/classic13_date_strings.pdf"
doc = fitz.open(path)
page = doc[0]

# Get the raw content stream
xref = page.xref
content = page.get_contents()[0]  # xref of first content stream object
stream = doc.xref_stream(content).decode('latin-1')

print("=== MINI PDF content stream (text operations only) ===")
for line in stream.split('\n'):
    line = line.strip()
    if any(op in line for op in ['BT', 'ET', 'Tj', 'Td', 'Tf', ' rg']):
        print(f"  {line}")
