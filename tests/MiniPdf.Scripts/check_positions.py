import fitz

cases = [
    ('classic01_basic_table_with_headers', 0.9988),
    ('classic34_explicit_column_widths', 0.9984),
    ('classic56_alternating_row_colors', 0.9961),
    ('classic132_striped_table', 0.9913),
]

for case, _ in cases:
    ref_path = f'../MiniPdf.Benchmark/reference_pdfs/{case}.pdf'
    mp_path = f'pdf_output/{case}.pdf'
    try:
        ref = fitz.open(ref_path)
        mp = fitz.open(mp_path)
    except:
        print(f'SKIP {case}')
        continue
    
    def get_first_items(page, n=6):
        items = []
        for b in page.get_text('dict')['blocks']:
            if 'lines' in b:
                for l in b['lines']:
                    for s in l['spans']:
                        if s['text'].strip():
                            items.append((s['origin'][0], s['origin'][1], s['text'][:30], s['size']))
        items.sort(key=lambda t: (t[1], t[0]))
        return items[:n]
    
    ri = get_first_items(ref[0])
    mi = get_first_items(mp[0])
    
    print(f'\n=== {case} ===')
    for j in range(min(len(ri), len(mi))):
        dx = mi[j][0] - ri[j][0]
        dy = mi[j][1] - ri[j][1]
        print(f'  "{ri[j][2]:20s}" ref=({ri[j][0]:6.1f},{ri[j][1]:6.1f}) sz={ri[j][3]:.1f}  mp=({mi[j][0]:6.1f},{mi[j][1]:6.1f}) sz={mi[j][3]:.1f}  dx={dx:+.1f} dy={dy:+.1f}')
