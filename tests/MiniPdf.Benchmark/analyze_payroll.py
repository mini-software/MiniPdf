"""Analyze classic191_payroll_calculator differences."""
import fitz
import os

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'
name = 'classic191_payroll_calculator'

for label, d in [('MiniPdf', mini_dir), ('Reference', ref_dir)]:
    doc = fitz.open(os.path.join(d, f'{name}.pdf'))
    print(f'\n=== {label}: {len(doc)} pages ===')
    for pi, page in enumerate(doc):
        text = page.get_text()
        lines = [l for l in text.split('\n') if l.strip()]
        rect = page.rect
        blocks = page.get_text("blocks")
        text_blocks = [b for b in blocks if b[6] == 0]
        img_blocks = [b for b in blocks if b[6] == 1]
        max_x = max((b[2] for b in text_blocks), default=0)
        max_y = max((b[3] for b in text_blocks), default=0)
        print(f'  P{pi+1}: {rect.width:.0f}x{rect.height:.0f}, {len(lines)} text lines, {len(img_blocks)} imgs, extent=({max_x:.0f},{max_y:.0f})')
        if pi < 3:
            for li, line in enumerate(lines[:10]):
                print(f'    {li}: {line[:100]}')
            if len(lines) > 10:
                print(f'    ... ({len(lines)} total lines)')
    doc.close()
