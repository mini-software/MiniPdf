import fitz

ref = fitz.open('../MiniPdf.Benchmark/reference_pdfs/classic18_large_dataset.pdf')
mp = fitz.open('pdf_output/classic18_large_dataset.pdf')
print(f'Ref pages: {len(ref)}, MiniPdf pages: {len(mp)}')

for i in [0, 1, 23]:
    rp = ref[i]
    mp_p = mp[i]
    r_items = []
    for b in rp.get_text('dict')['blocks']:
        if 'lines' in b:
            for l in b['lines']:
                for s in l['spans']:
                    if s['text'].strip():
                        r_items.append((round(s['origin'][0],1), round(s['origin'][1],1), s['text'][:30], round(s['size'],1)))
    m_items = []
    for b in mp_p.get_text('dict')['blocks']:
        if 'lines' in b:
            for l in b['lines']:
                for s in l['spans']:
                    if s['text'].strip():
                        m_items.append((round(s['origin'][0],1), round(s['origin'][1],1), s['text'][:30], round(s['size'],1)))
    print(f'\nPage {i+1}: ref={len(r_items)} items, mp={len(m_items)} items')
    if r_items and m_items:
        print(f'  Ref first:  x={r_items[0][0]} y={r_items[0][1]} sz={r_items[0][3]} "{r_items[0][2]}"')
        print(f'  MP  first:  x={m_items[0][0]} y={m_items[0][1]} sz={m_items[0][3]} "{m_items[0][2]}"')
        print(f'  Ref last:   x={r_items[-1][0]} y={r_items[-1][1]} sz={r_items[-1][3]} "{r_items[-1][2]}"')
        print(f'  MP  last:   x={m_items[-1][0]} y={m_items[-1][1]} sz={m_items[-1][3]} "{m_items[-1][2]}"')
        # Check y range
        r_ys = [it[1] for it in r_items]
        m_ys = [it[1] for it in m_items]
        print(f'  Ref Y range: {min(r_ys):.1f} - {max(r_ys):.1f}')
        print(f'  MP  Y range: {min(m_ys):.1f} - {max(m_ys):.1f}')
        # Check x range  
        r_xs = [it[0] for it in r_items]
        m_xs = [it[0] for it in m_items]
        print(f'  Ref X range: {min(r_xs):.1f} - {max(r_xs):.1f}')
        print(f'  MP  X range: {min(m_xs):.1f} - {max(m_xs):.1f}')
