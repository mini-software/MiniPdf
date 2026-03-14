import fitz

for name, pdfdir in [
    ("classic148_frozen_styled_grid", "../MiniPdf.Benchmark/reference_pdfs"),
    ("classic148_frozen_styled_grid", "pdf_output"),
]:
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
                    if len(spans) >= 14:
                        break
                if len(spans) >= 14: break
        if len(spans) >= 14: break
    
    label = "REF" if "reference" in pdfdir else " MP"
    print(f"\n=== {label} {name} ===")
    prev_x = None
    for s in spans:
        x = s["origin"][0]
        gap = f"  gap={x - prev_x:.2f}" if prev_x is not None else ""
        prev_x = x
        print(f"  {s['text'][:25]:25s} x={x:7.2f} y={s['origin'][1]:7.2f}{gap}")
