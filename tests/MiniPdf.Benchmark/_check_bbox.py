import fitz, sys

name = sys.argv[1] if len(sys.argv) > 1 else "classic128_font_sizes"
path = f"../MiniPdf.Scripts/pdf_output/{name}.pdf"
doc = fitz.open(path)
page = doc[0]
data = page.get_text("dict", sort=True)

for block in data.get("blocks", []):
    if block.get("type", 0) != 0:
        continue
    for line in block.get("lines", []):
        for span in line.get("spans", []):
            text = span.get("text", "").strip()
            if text:
                bbox = span["bbox"]
                origin = span.get("origin", (0,0))
                size = span.get("size", 0)
                print(f"  bbox_top={bbox[1]:7.2f} origin=({origin[0]:7.2f},{origin[1]:7.2f}) size={size:5.1f} [{text}]")
