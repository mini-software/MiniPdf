"""Check column positions in reference vs MiniPdf PDFs."""
import fitz, sys

name = sys.argv[1] if len(sys.argv) > 1 else "classic42_boolean_values"

for label, path in [("REF", f"reference_pdfs/{name}.pdf"),
                     ("MINI", f"../MiniPdf.Scripts/pdf_output/{name}.pdf")]:
    doc = fitz.open(path)
    page = doc[0]
    print(f"=== {label} ===")
    blocks = page.get_text("dict")["blocks"]
    for b in blocks:
        if "lines" in b:
            for l in b["lines"]:
                for s in l["spans"]:
                    t = s["text"].strip()
                    if t:
                        x0 = s["origin"][0]
                        bbox = s.get("bbox", (0,0,0,0))
                        x1 = bbox[2] if bbox else x0
                        print(f"  x={x0:.1f} x1={x1:.1f} font={s['font']} sz={s['size']:.1f} \"{t}\"")
    doc.close()
    print()
