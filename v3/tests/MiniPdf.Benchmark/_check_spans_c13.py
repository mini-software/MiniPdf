"""Check raw PDF spans for classic13 to understand text placement."""
import fitz

for label, path in [("MINI", "../MiniPdf.Scripts/pdf_output/classic13_date_strings.pdf"),
                     ("REF", "reference_pdfs/classic13_date_strings.pdf")]:
    doc = fitz.open(path)
    page = doc[0]
    blocks = page.get_text("dict")["blocks"]
    print(f"\n=== {label} ===")
    for b in blocks:
        if "lines" not in b:
            continue
        for l in b["lines"]:
            for s in l["spans"]:
                if s["text"].strip():
                    bbox = s["bbox"]
                    origin = s["origin"]
                    print(f"  origin=({origin[0]:7.2f}, {origin[1]:7.2f}) "
                          f"bbox=({bbox[0]:7.2f}, {bbox[1]:7.2f}, {bbox[2]:7.2f}, {bbox[3]:7.2f}) "
                          f"size={s['size']:5.2f} text='{s['text']}'")
    doc.close()
