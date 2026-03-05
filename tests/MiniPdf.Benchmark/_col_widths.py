"""Compare column X positions between MINI and REF to detect column width differences."""
import fitz

files = [
    "classic01_basic_table_with_headers",
    "classic13_date_strings",
    "classic132_striped_table",
    "classic134_heatmap",
    "classic131_number_formats",
    "classic51_product_catalog",
    "classic142_styled_invoice",
]

for name in files:
    mini_path = f"../MiniPdf.Scripts/pdf_output/{name}.pdf"
    ref_path = f"reference_pdfs/{name}.pdf"
    
    for label, path in [("MINI", mini_path), ("REF", ref_path)]:
        doc = fitz.open(path)
        page = doc[0]
        data = page.get_text("dict", sort=True)
        
        # Get X positions of first-row spans
        first_y = None
        spans_row1 = []
        for b in data["blocks"]:
            if b.get("type", 0) != 0:
                continue
            for l in b["lines"]:
                for s in l["spans"]:
                    if s["text"].strip():
                        y = round(s["origin"][1], 0)
                        if first_y is None:
                            first_y = y
                        if abs(y - first_y) < 2:
                            spans_row1.append((s["origin"][0], s["text"][:15]))
        
        spans_row1.sort()
        if label == "MINI":
            print(f"\n  {name}:")
        print(f"    {label} first row X positions: ", end="")
        for x, t in spans_row1:
            print(f"  {x:.2f}('{t}')", end="")
        print()
        doc.close()
