import fitz, os

# Compare exact text positions between MiniPdf and reference
cases = [
    ("docx_classic02_multiple_paragraphs", 0),
    ("docx_classic69_blockquote_styling", 0),
    ("docx_classic15_indentation", 0),
]

mini_dir = "../MiniPdf.Scripts/pdf_output_docx"
ref_dir = "reference_pdfs_docx"

for name, page_idx in cases:
    pdf_name = name + ".pdf"
    mini_doc = fitz.open(os.path.join(mini_dir, pdf_name))
    ref_doc = fitz.open(os.path.join(ref_dir, pdf_name))
    
    print(f"\n{'='*70}")
    print(f"{name} (page {page_idx})")
    print(f"Page size: mini={mini_doc[page_idx].rect} ref={ref_doc[page_idx].rect}")
    
    for label, doc in [("MINI", mini_doc), ("REF ", ref_doc)]:
        page = doc[page_idx]
        blocks = page.get_text("dict", sort=True)["blocks"]
        text_blocks = [b for b in blocks if b["type"] == 0]
        
        print(f"\n  {label} text spans (first 15):")
        count = 0
        for b in text_blocks:
            for l in b.get("lines", []):
                for s in l.get("spans", []):
                    if count >= 15: break
                    text = s["text"][:40]
                    origin = s.get("origin", (0,0))
                    bbox = s["bbox"]
                    print(f"    baseline_y={origin[1]:7.2f} bbox_top={bbox[1]:7.2f} size={s['size']:5.1f} font={s['font']:<20s} '{text}'")
                    count += 1
                if count >= 15: break
            if count >= 15: break
    
    mini_doc.close()
    ref_doc.close()
