"""Check reference classic49 spans."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs\classic49_contact_list.pdf'
doc = fitz.open(path)
page = doc[0]
data = page.get_text("dict", sort=True)

for block in data.get("blocks", []):
    if block.get("type", 0) != 0:
        continue
    for line in block.get("lines", []):
        for s in line.get("spans", []):
            text = s.get("text", "").strip()
            if not text:
                continue
            b = s["bbox"]
            if 86 < b[1] < 130:
                print(f"  x=({b[0]:6.1f}-{b[2]:6.1f}) y={b[1]:6.1f} [{text}]")
doc.close()
