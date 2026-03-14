import fitz

for name in ["classic01_basic_table_with_headers", "classic37_freeze_panes"]:
    ref = fitz.open(f"../MiniPdf.Benchmark/reference_pdfs/{name}.pdf")
    mp_path = f"temp_test/{name}.pdf"
    if not __import__('os').path.exists(mp_path):
        mp_path = f"pdf_output/{name}.pdf"
    mp  = fitz.open(mp_path)
    rp, mp2 = ref[0], mp[0]
    print(f"Ref page: {rp.rect.width} x {rp.rect.height}")
    print(f"MP  page: {mp2.rect.width} x {mp2.rect.height}")

    def get_spans(page, limit=8):
        items = []
        for b in page.get_text("dict")["blocks"]:
            if "lines" in b:
                for l in b["lines"]:
                    for s in l["spans"]:
                        items.append(s)
                        if len(items) >= limit:
                            return items
        return items

    ref_spans = get_spans(rp)
    mp_spans  = get_spans(mp2)
    print("\n--- Reference ---")
    for s in ref_spans:
        t = s["text"][:25]
        ox, oy = s["origin"]
        bx0, by0, bx1, by1 = s["bbox"]
        print(f"  {t:25s} origin=({ox:7.2f},{oy:7.2f}) bbox=({bx0:.2f},{by0:.2f},{bx1:.2f},{by1:.2f}) sz={s['size']:.1f}")

    print("\n--- MiniPdf ---")
    for s in mp_spans:
        t = s["text"][:25]
        ox, oy = s["origin"]
        bx0, by0, bx1, by1 = s["bbox"]
        print(f"  {t:25s} origin=({ox:7.2f},{oy:7.2f}) bbox=({bx0:.2f},{by0:.2f},{bx1:.2f},{by1:.2f}) sz={s['size']:.1f}")
