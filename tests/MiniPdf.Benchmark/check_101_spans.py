"""Show MiniPdf raw spans for classic101 chart area."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic101_percent_stacked_bar.pdf'
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
            sz = s["size"]
            print(f"y={b[1]:7.1f} x=({b[0]:6.1f}-{b[2]:6.1f}) sz={sz:5.1f} [{text}]")
doc.close()
