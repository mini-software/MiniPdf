"""Check character-level positions for classic128."""
import fitz

path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output\classic128_font_sizes.pdf'
doc = fitz.open(path)
page = doc[0]

# Extract text using get_text("dict") which includes word-level info
data = page.get_text("dict", sort=True)
for block in data.get("blocks", []):
    if block.get("type", 0) != 0:
        continue
    for line in block.get("lines", []):
        line_bbox = line["bbox"]
        for span in line.get("spans", []):
            text = span.get("text", "").strip()
            if not text:
                continue
            sb = span["bbox"]
            sz = span["size"]
            origin = span.get("origin", (0, 0))
            print("origin=({:.3f},{:.3f}) bbox=({:.3f},{:.3f},{:.3f},{:.3f}) sz={:.0f} [{}]".format(
                origin[0], origin[1], sb[0], sb[1], sb[2], sb[3], sz, text))

doc.close()
