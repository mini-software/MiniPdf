import fitz

path = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf"
doc = fitz.open(path)
page = doc[0]
contents = page.get_contents()
for c in contents:
    stream = doc.xref_stream(c)
    text = stream.decode('latin-1')
    lines = text.split('\n')
    # Print lines 56-75 (around the 12pt row)
    for i in range(55, min(75, len(lines))):
        print(f"{i:3d}: {lines[i]}")
