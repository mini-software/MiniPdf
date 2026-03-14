"""Check Y offset across multiple cases."""
import fitz

cases = [
    'classic01_basic_table_with_headers',
    'classic06_tall_table', 
    'classic18_large_dataset',
    'classic37_freeze_panes',
    'classic60_large_wide_table',
    'classic148_frozen_styled_grid',
    'classic134_heatmap',
    'classic142_styled_invoice',
]

for case in cases:
    ref_path = f'reference_pdfs/{case}.pdf'
    mini_path = f'..\\MiniPdf.Scripts\\pdf_output\\{case}.pdf'
    try:
        ref = fitz.open(ref_path)
        mini = fitz.open(mini_path)
    except:
        print(f'{case}: FILE NOT FOUND')
        continue
    
    ref_pg = ref[0]
    mini_pg = mini[0]
    
    def first_text_y(pg):
        blocks = pg.get_text('dict')['blocks']
        ys = []
        for b in blocks:
            if 'lines' not in b: continue
            for l in b['lines']:
                for s in l['spans']:
                    ys.append(s['origin'][1])
        return min(ys) if ys else None
    
    ref_y = first_text_y(ref_pg)
    mini_y = first_text_y(mini_pg)
    if ref_y and mini_y:
        dy = mini_y - ref_y
        print(f'{case:50s} ref_y={ref_y:.2f} mini_y={mini_y:.2f} dy={dy:+.2f}')
    else:
        print(f'{case}: no text found')
