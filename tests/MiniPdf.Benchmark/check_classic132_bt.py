"""Find text blocks in classic132 PDF stream"""
import fitz

doc = fitz.open("../MiniPdf.Scripts/pdf_output/classic132_striped_table.pdf")
page = doc[0]
content_bytes = page.read_contents()
content = content_bytes.decode('latin-1')
lines = content.split('\n')

# Find first BT
for i, line in enumerate(lines):
    if 'BT' in line or 'Tj' in line or 'Tz' in line:
        # Print context
        start = max(0, i-2)
        end = min(len(lines), i+5)
        for j in range(start, end):
            print(f"{j:4d}: {lines[j]}")
        print("---")
        if i > 800:
            break

doc.close()
