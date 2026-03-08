"""Compare classic66_colored_title_page PDFs — see why visual is so low"""
import fitz

ref = fitz.open('reference_pdfs_docx/docx_classic66_colored_title_page.pdf')
mini = fitz.open('../MiniPdf.Scripts/pdf_output_docx/docx_classic66_colored_title_page.pdf')

for label, doc in [('REF', ref), ('MINI', mini)]:
    p = doc[0]
    print(f'\n{label}: {p.rect.width:.0f}x{p.rect.height:.0f}')
    
    # Images
    images = p.get_images()
    print(f'  Images: {len(images)}')
    for img in images:
        xref = img[0]
        w, h = img[2], img[3]
        print(f'    xref={xref} w={w} h={h}')
    
    # Colored rectangles
    draws = p.get_drawings()
    fills = [d for d in draws if d.get('fill')]
    print(f'  Fills: {len(fills)}')
    for d in fills[:5]:
        r = d['rect']
        c = d['fill']
        print(f'    ({r.x0:.1f},{r.y0:.1f})-({r.x1:.1f},{r.y1:.1f}) color=({c[0]:.3f},{c[1]:.3f},{c[2]:.3f})')
    
    # Text
    text_data = p.get_text('dict', sort=True)
    lines_shown = 0
    for b in text_data.get('blocks', []):
        if b.get('type', 0) != 0: continue
        for ln in b.get('lines', []):
            for s in ln.get('spans', []):
                t = s.get('text', '').strip()
                if t and lines_shown < 6:
                    fs = s.get('size', 0)
                    print(f'    text @y={s["bbox"][1]:.1f} size={fs:.1f}: {t[:50]}')
                    lines_shown += 1
                    break
    doc.close()
