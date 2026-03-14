"""Check line/block structure from PyMuPDF for row 12."""
import fitz, json

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf'
doc = fitz.open(path)
page = doc[0]

data = page.get_text("dict", sort=True)
for block in data.get("blocks", []):
    if block.get("type", 0) != 0:
        continue
    for line in block.get("lines", []):
        spans = line.get("spans", [])
        texts = [s["text"] for s in spans]
        combined = " ".join(texts)
        if "12" in combined or "Font size 1" in combined:
            print(f"LINE bbox={line['bbox']}")
            print(f"  wmode={line.get('wmode', '?')}, dir={line.get('dir', '?')}")
            for s in spans:
                print(f"  SPAN: origin={s.get('origin', '?')}, bbox={s['bbox']}, "
                      f"size={s['size']}, flags={s.get('flags', '?')}, text={s['text']!r}")
            print()

doc.close()
