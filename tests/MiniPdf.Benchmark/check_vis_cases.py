"""Compare visual structure of classic191_payroll_calculator and classic148_frozen_styled_grid."""
import fitz, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

for case in ['classic191_payroll_calculator', 'classic148_frozen_styled_grid', 'classic06_tall_table']:
    print(f'\n=== {case} ===')
    for label, path in [('MiniPdf', f'../MiniPdf.Scripts/pdf_output/{case}.pdf'),
                         ('Ref', f'reference_pdfs/{case}.pdf')]:
        doc = fitz.open(path)
        p = doc[0]
        print(f'  {label}: pages={doc.page_count}, size={p.rect.width:.0f}x{p.rect.height:.0f}')
        
        # Check lines/paths (borders, grids)
        paths = p.get_drawings()
        print(f'    drawings: {len(paths)}')
        
        # Check text block count
        blocks = p.get_text('dict', sort=True)['blocks']
        text_blocks = [b for b in blocks if 'lines' in b]
        print(f'    text blocks: {len(text_blocks)}')
        
        # First few text lines
        count = 0
        for b in text_blocks:
            if 'lines' in b:
                for line in b['lines']:
                    texts = ' '.join(s['text'].strip() for s in line['spans'] if s['text'].strip())
                    if texts and count < 5:
                        bb = line['bbox']
                        print(f'    y={bb[1]:.1f} text="{texts[:60]}"')
                        count += 1
        doc.close()
