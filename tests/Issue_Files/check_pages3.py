import fitz

for label, path in [("MiniPdf", "minipdf_xlsx/Small business cash flow forecast1.pdf"),
                     ("Reference", "reference_xlsx/Small business cash flow forecast1.pdf")]:
    doc = fitz.open(path)
    print(f"\n{label} ({doc.page_count} pages):")
    for i in range(doc.page_count):
        p = doc[i]
        text = p.get_text().strip()
        lines = [l for l in text.split("\n") if l.strip()]
        w, h = p.rect.width, p.rect.height
        print(f"  Page {i+1} ({w:.0f}x{h:.0f}): {len(lines)} lines")
        # Show drawings/images info
        imgs = p.get_images(full=True)
        drawings = p.get_drawings()
        print(f"    images={len(imgs)}, drawings={len(drawings)}")
        if lines:
            for ln in lines[:3]:
                print(f"    > {ln[:70]}")
            if len(lines) > 3:
                print(f"    ...")
                for ln in lines[-2:]:
                    print(f"    > {ln[:70]}")
    doc.close()
