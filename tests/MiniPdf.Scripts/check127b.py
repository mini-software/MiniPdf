import fitz

for label, path in [
    ("REF", "../MiniPdf.Benchmark/reference_pdfs/classic127_font_styles.pdf"),
    ("MP",  "pdf_output/classic127_font_styles.pdf"),
]:
    doc = fitz.open(path)
    p = doc[0]
    print(f"\n=== {label} ===")
    for b in p.get_text("dict")["blocks"]:
        if "lines" in b:
            for l in b["lines"]:
                texts = []
                for s in l["spans"]:
                    texts.append(f"{s['text'][:30]}(x={s['origin'][0]:.1f})")
                print(f"  {' | '.join(texts)}")
