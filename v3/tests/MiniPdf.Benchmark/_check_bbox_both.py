import fitz, sys

name = sys.argv[1] if len(sys.argv) > 1 else "classic128_font_sizes"

for label, path in [("MINI", f"../MiniPdf.Scripts/pdf_output/{name}.pdf"),
                     ("REF", f"reference_pdfs/{name}.pdf")]:
    doc = fitz.open(path)
    page = doc[0]
    data = page.get_text("dict", sort=True)
    print(f"=== {label} ===")
    for block in data.get("blocks", []):
        if block.get("type", 0) != 0:
            continue
        for line in block.get("lines", []):
            for span in line.get("spans", []):
                text = span.get("text", "").strip()
                if '12' in text or 'Size' in text or 'Sample' in text:
                    bbox = span["bbox"]
                    size = span.get("size", 0)
                    print(f"  bbox_top={bbox[1]:7.2f} size={size:5.1f} [{text}]")
    print()
