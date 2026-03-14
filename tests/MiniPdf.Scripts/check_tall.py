import fitz

# Compare row-by-row position drift for classic06_tall_table
ref = fitz.open('../MiniPdf.Benchmark/reference_pdfs/classic06_tall_table.pdf')
mp = fitz.open('pdf_output/classic06_tall_table.pdf')

print(f'Ref pages: {len(ref)}, MiniPdf pages: {len(mp)}')

for page_idx in range(min(len(ref), len(mp))):
    rp = ref[page_idx]
    mp_p = mp[page_idx]
    
    # Get text items sorted by y position
    def get_items(page):
        items = []
        for b in page.get_text('dict')['blocks']:
            if 'lines' in b:
                for l in b['lines']:
                    for s in l['spans']:
                        if s['text'].strip():
                            items.append((round(s['origin'][0], 1), round(s['origin'][1], 1), s['text'][:30]))
        items.sort(key=lambda t: (t[1], t[0]))
        return items
    
    r_items = get_items(rp)
    m_items = get_items(mp_p)
    
    print(f'\nPage {page_idx+1}: ref={len(r_items)}, mp={len(m_items)} items')
    if r_items and m_items:
        # get Y positions of first item in each "row" (group by unique Y)
        def get_row_ys(items):
            ys = []
            prev_y = -999
            for x, y, t in items:
                if y - prev_y > 1:  # new row
                    ys.append((y, t))
                    prev_y = y
            return ys
        
        r_ys = get_row_ys(r_items)
        m_ys = get_row_ys(m_items)
        
        # Compare row positions
        max_diff = 0
        for i in range(min(len(r_ys), len(m_ys))):
            diff = m_ys[i][0] - r_ys[i][0]
            if abs(diff) > abs(max_diff):
                max_diff = diff
        
        print(f'  Rows: ref={len(r_ys)}, mp={len(m_ys)}')
        if r_ys and m_ys:
            print(f'  First row: ref_y={r_ys[0][0]}, mp_y={m_ys[0][0]}, diff={m_ys[0][0]-r_ys[0][0]:.1f}')
            print(f'  Last row:  ref_y={r_ys[-1][0]}, mp_y={m_ys[-1][0]}, diff={m_ys[-1][0]-r_ys[-1][0]:.1f}')
            print(f'  Max y diff: {max_diff:.1f}')
