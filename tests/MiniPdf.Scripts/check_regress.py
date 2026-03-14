import fitz

cases = [
    "classic37_freeze_panes",
    "classic184_wide_narrow_columns",
]

for name in cases:
    for label, pdfdir in [("REF", "../MiniPdf.Benchmark/reference_pdfs"), ("MP", "pdf_output")]:
        path = f"{pdfdir}/{name}.pdf"
        try:
            doc = fitz.open(path)
        except:
            print(f"SKIP {path}")
            continue
        p = doc[0]
        spans = []
        for b in p.get_text("dict")["blocks"]:
            if "lines" in b:
                for l in b["lines"]:
                    for s in l["spans"]:
                        spans.append(s)
                        if len(spans) >= 8:
                            break
                    if len(spans) >= 8: break
            if len(spans) >= 8: break
        
        print(f"\n=== {label} {name} ===")
        for s in spans:
            t = s["text"][:25]
            x = s["origin"][0]
            y = s["origin"][1]
            print(f"  {t:25s} x={x:7.2f} y={y:7.2f}")
