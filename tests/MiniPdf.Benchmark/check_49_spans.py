"""Check how PyMuPDF extracts clipped text in classic49_contact_list."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic49_contact_list.pdf'
doc = fitz.open(path)
page = doc[0]
data = page.get_text("dict", sort=True)

# Show all spans with bbox for the first few data rows
for block in data.get("blocks", []):
    if block.get("type", 0) != 0:
        continue
    for line in block.get("lines", []):
        for s in line.get("spans", []):
            text = s.get("text", "").strip()
            if not text:
                continue
            b = s["bbox"]
            if 86 < b[1] < 130:  # Data rows near top
                print(f"  x=({b[0]:6.1f}-{b[2]:6.1f}) y={b[1]:6.1f} [{text}]")
doc.close()
