import fitz

# Compare specific page 3 to find missing items
ref = fitz.open('../MiniPdf.Benchmark/reference_pdfs/classic06_tall_table.pdf')
mp = fitz.open('pdf_output/classic06_tall_table.pdf')

page_idx = 2  # Page 3

def get_items(page):
    items = []
    for b in page.get_text('dict')['blocks']:
        if 'lines' in b:
            for l in b['lines']:
                for s in l['spans']:
                    if s['text'].strip():
                        items.append((round(s['origin'][0], 1), round(s['origin'][1], 1), s['text'][:50]))
    items.sort(key=lambda t: (t[1], t[0]))
    return items

r_items = get_items(ref[page_idx])
m_items = get_items(mp[page_idx])

# Group items by row (y position)
def group_by_y(items, threshold=2):
    rows = {}
    for x, y, t in items:
        # Find existing row key
        matched = False
        for key in rows:
            if abs(y - key) < threshold:
                rows[key].append((x, t))
                matched = True
                break
        if not matched:
            rows[y] = [(x, t)]
    return rows

r_rows = group_by_y(r_items)
m_rows = group_by_y(m_items)

# Show first few rows with differences
r_keys = sorted(r_rows.keys())
m_keys = sorted(m_rows.keys())

for i in range(min(len(r_keys), len(m_keys))):
    ry = r_keys[i]
    my = m_keys[i]
    r_texts = sorted(r_rows[ry])
    m_texts = sorted(m_rows[my])
    if len(r_texts) != len(m_texts):
        print(f'Row {i} y~{ry:.0f}: ref={len(r_texts)} vs mp={len(m_texts)} items')
        # Show what's in ref but not mp
        r_set = set(t for _, t in r_texts)
        m_set = set(t for _, t in m_texts)
        missing = r_set - m_set
        if missing:
            print(f'  Missing in MP: {[t for t in missing][:5]}')
        extra = m_set - r_set
        if extra:
            print(f'  Extra in MP: {[t for t in extra][:5]}')

print(f'\nTotal: ref rows={len(r_keys)}, mp rows={len(m_keys)}')
