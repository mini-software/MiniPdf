"""Compare chart label text positioning between MiniPdf and reference for chart cases."""
import fitz, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

cases = [
    'classic113_chart_sheet',
    'classic118_bar_chart_custom_colors', 
    'classic106_3d_pie_chart',
    'classic91_simple_bar_chart',
]

for case in cases:
    print(f'\n=== {case} ===')
    for src_label, src_path in [('MiniPdf', f'../MiniPdf.Scripts/pdf_output/{case}.pdf'), 
                                 ('Ref', f'reference_pdfs/{case}.pdf')]:
        doc = fitz.open(src_path)
        p = doc[0]
        blocks = p.get_text('dict', sort=True)['blocks']
        print(f'\n  {src_label} (page {p.rect.width:.0f}x{p.rect.height:.0f}):')
        for b in blocks:
            if 'lines' in b:
                for line in b['lines']:
                    texts = []
                    for span in line['spans']:
                        t = span['text'].strip()
                        if t:
                            bb = span['bbox']
                            texts.append(f'{t}@y={bb[1]:.1f}')
                    if texts:
                        full = ' | '.join(texts)
                        if len(full) > 120:
                            full = full[:120] + '...'
                        print(f'    {full}')
        doc.close()
