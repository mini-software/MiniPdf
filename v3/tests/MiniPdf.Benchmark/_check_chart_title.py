"""Check chart title details in reference vs MINI."""
import fitz, sys

name = sys.argv[1] if len(sys.argv) > 1 else "classic107_multi_series_line"
keywords = sys.argv[2].split(',') if len(sys.argv) > 2 else ['Stock','Price','Trend','Revenue','Browser','Market']

for label, path in [("REF", f"reference_pdfs/{name}.pdf"),
                     ("MINI", f"../MiniPdf.Scripts/pdf_output/{name}.pdf")]:
    doc = fitz.open(path)
    for pi in range(doc.page_count):
        data = doc[pi].get_text("dict")
        for b in data["blocks"]:
            if b.get("type", 0) == 0:
                for l in b["lines"]:
                    for s in l["spans"]:
                        t = s.get("text", "").strip()
                        if t and any(k in t for k in keywords):
                            print(f'{label} p{pi+1} x={s["origin"][0]:.0f} y={s["origin"][1]:.0f} sz={s["size"]:.1f} font={s["font"]} "{t}"')
