"""Compare code_block_styling PDFs to understand shading difference"""
import fitz

ref = fitz.open('reference_pdfs_docx/docx_classic65_code_block_styling.pdf')
mini = fitz.open('../MiniPdf.Scripts/pdf_output_docx/docx_classic65_code_block_styling.pdf')

for label, doc in [('REF', ref), ('MINI', mini)]:
    p = doc[0]
    draws = p.get_drawings()
    fills = [d for d in draws if d.get('fill')]
    print(f'\n{label}: page size={p.rect.width:.0f}x{p.rect.height:.0f}, fills={len(fills)}')
    for d in fills[:8]:
        r = d['rect']
        c = d['fill']
        print(f'  fill at ({r.x0:.1f},{r.y0:.1f})-({r.x1:.1f},{r.y1:.1f}) w={r.x1-r.x0:.1f} h={r.y1-r.y0:.1f} color=({c[0]:.3f},{c[1]:.3f},{c[2]:.3f})')
    
    # Show first few text positions
    text_data = p.get_text('dict', sort=True)
    lines_shown = 0
    for b in text_data.get('blocks', []):
        if b.get('type', 0) != 0: continue
        for ln in b.get('lines', []):
            for s in ln.get('spans', []):
                t = s.get('text', '').strip()
                if t and lines_shown < 8:
                    bbox = s['bbox']
                    print(f'  text @y={bbox[1]:.1f} x={bbox[0]:.1f}: {t[:50]}')
                    lines_shown += 1
                    break
    doc.close()
