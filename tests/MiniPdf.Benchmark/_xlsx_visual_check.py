import fitz, os

# Compare text positions in closest XLSX visual-only cases
cases = [
    "classic134_heatmap",
    "classic149_merged_styled_sections",
    "classic137_checkerboard",
    "classic06_tall_table",
]

mini_dir = "../MiniPdf.Scripts/pdf_output"
ref_dir = "reference_pdfs"

for name in cases:
    pdf = name + ".pdf"
    mini_doc = fitz.open(os.path.join(mini_dir, pdf))
    ref_doc = fitz.open(os.path.join(ref_dir, pdf))
    
    print(f"\n{'='*60}")
    print(f"{name}")
    print(f"  Pages: mini={len(mini_doc)} ref={len(ref_doc)}")
    
    mp = mini_doc[0]
    rp = ref_doc[0]
    
    # Compare page size
    print(f"  Page size: mini={mp.rect.width:.1f}x{mp.rect.height:.1f} ref={rp.rect.width:.1f}x{rp.rect.height:.1f}")
    
    # Compare first few text spans
    for label, page in [("MINI", mp), ("REF ", rp)]:
        blocks = page.get_text("dict", sort=True)["blocks"]
        text_blocks = [b for b in blocks if b["type"] == 0]
        
        spans = []
        for b in text_blocks:
            for l in b.get("lines", []):
                for s in l.get("spans", []):
                    spans.append(s)
        
        print(f"  {label} first 5 spans:")
        for s in spans[:5]:
            origin = s.get("origin", (0,0))
            text = s["text"][:30]
            print(f"    x={origin[0]:7.1f} y={origin[1]:7.1f} size={s['size']:5.1f} '{text}'")
        
        if spans:
            # Check last span too
            last = spans[-1]
            origin = last.get("origin", (0,0))
            print(f"    ... last: x={origin[0]:7.1f} y={origin[1]:7.1f} '{last['text'][:30]}'")
    
    # Compare drawings
    md = mp.get_drawings()
    rd = rp.get_drawings()
    print(f"  Drawings: mini={len(md)} ref={len(rd)}")
    
    mini_doc.close()
    ref_doc.close()
