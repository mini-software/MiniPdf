import json, fitz, os

# Analyze visual features for closest DOCX cases  
cases = [
    "docx_classic32_company_logo_header",
    "docx_classic129_alignment_combinations",
    "docx_classic127_font_styles_showcase",
    "docx_classic15_indentation",
    "docx_classic69_blockquote_styling",
    "docx_classic113_address_labels",
    "docx_classic124_colored_border_table",
    "docx_classic36_invoice_with_logo",
    "docx_classic131_number_format_table",
    "docx_classic48_multi_level_headings",
    "docx_classic16_line_spacing",
    "docx_classic02_multiple_paragraphs",
    "docx_classic52_spacing_before_after",
    "docx_classic86_checklist_document",
]

mini_dir = "../MiniPdf.Scripts/pdf_output_docx"
ref_dir = "reference_pdfs_docx"

for name in cases:
    pdf_name = name + ".pdf"
    mini_path = os.path.join(mini_dir, pdf_name)
    ref_path = os.path.join(ref_dir, pdf_name)
    
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        print(f"\n{name}: File not found")
        continue
    
    mini_doc = fitz.open(mini_path)
    ref_doc = fitz.open(ref_path)
    
    print(f"\n{'='*70}")
    print(f"{name}")
    print(f"  Pages: mini={len(mini_doc)} ref={len(ref_doc)}")
    print(f"  Size: mini={os.path.getsize(mini_path)} ref={os.path.getsize(ref_path)}")
    
    # Compare page dimensions
    for i in range(min(len(mini_doc), len(ref_doc))):
        mp = mini_doc[i]
        rp = ref_doc[i]
        mr = mp.rect
        rr = rp.rect
        if abs(mr.width - rr.width) > 1 or abs(mr.height - rr.height) > 1:
            print(f"  Page {i}: DIFFERENT SIZE mini={mr.width:.1f}x{mr.height:.1f} ref={rr.width:.1f}x{rr.height:.1f}")
    
    # Compare drawings on page 0
    mp = mini_doc[0]
    rp = ref_doc[0]
    
    md = mp.get_drawings()
    rd = rp.get_drawings()
    
    m_filled = [d for d in md if d.get("fill")]
    r_filled = [d for d in rd if d.get("fill")]
    m_stroked = [d for d in md if d.get("color") and not d.get("fill")]
    r_stroked = [d for d in rd if d.get("color") and not d.get("fill")]
    
    print(f"  P0 drawings: mini={len(md)} ref={len(rd)}")
    print(f"  P0 filled: mini={len(m_filled)} ref={len(r_filled)}")
    print(f"  P0 stroked: mini={len(m_stroked)} ref={len(r_stroked)}")
    
    # Compare images
    m_imgs = mp.get_images()
    r_imgs = rp.get_images()
    print(f"  P0 images: mini={len(m_imgs)} ref={len(r_imgs)}")
    
    # Compare text blocks - first and last position
    m_blocks = mp.get_text("dict", sort=True)["blocks"]
    r_blocks = rp.get_text("dict", sort=True)["blocks"]
    m_text_blocks = [b for b in m_blocks if b["type"] == 0]
    r_text_blocks = [b for b in r_blocks if b["type"] == 0]
    
    if m_text_blocks and r_text_blocks:
        # First text block position
        m_first = m_text_blocks[0]["bbox"]
        r_first = r_text_blocks[0]["bbox"]
        m_last = m_text_blocks[-1]["bbox"]
        r_last = r_text_blocks[-1]["bbox"]
        print(f"  P0 first text Y: mini={m_first[1]:.1f} ref={r_first[1]:.1f} diff={m_first[1]-r_first[1]:.1f}")
        print(f"  P0 last text Y: mini={m_last[1]:.1f} ref={r_last[1]:.1f} diff={m_last[1]-r_last[1]:.1f}")
        
        # Count text blocks
        print(f"  P0 text blocks: mini={len(m_text_blocks)} ref={len(r_text_blocks)}")
    
    # Check font sizes
    m_sizes = set()
    r_sizes = set()
    for b in m_text_blocks[:5]:
        for l in b.get("lines", []):
            for s in l.get("spans", []):
                m_sizes.add(round(s["size"], 1))
    for b in r_text_blocks[:5]:
        for l in b.get("lines", []):
            for s in l.get("spans", []):
                r_sizes.add(round(s["size"], 1))
    print(f"  P0 font sizes (first 5 blocks): mini={sorted(m_sizes)} ref={sorted(r_sizes)}")
    
    mini_doc.close()
    ref_doc.close()
