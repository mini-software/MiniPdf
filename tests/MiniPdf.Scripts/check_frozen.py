import fitz

# Compare page 1 of frozen_styled_grid
ref = fitz.open('../MiniPdf.Benchmark/reference_pdfs/classic148_frozen_styled_grid.pdf')
mp = fitz.open('pdf_output/classic148_frozen_styled_grid.pdf')

print(f'Ref pages: {len(ref)}, size: {ref[0].rect}')
print(f'MP pages:  {len(mp)},  size: {mp[0].rect}')

# Render both to images and compare pixel by pixel
for i in range(min(len(ref), len(mp))):
    r = ref[i]
    m = mp[i]
    
    # Get text extents
    def get_extents(page):
        min_x, min_y = 999, 999
        max_x, max_y = 0, 0
        count = 0
        for b in page.get_text('dict')['blocks']:
            if 'lines' in b:
                for l in b['lines']:
                    for s in l['spans']:
                        if s['text'].strip():
                            count += 1
                            x, y = s['origin']
                            min_x = min(min_x, x)
                            min_y = min(min_y, y)
                            max_x = max(max_x, s['bbox'][2])
                            max_y = max(max_y, s['bbox'][3])
        return count, min_x, min_y, max_x, max_y
    
    rc, rx1, ry1, rx2, ry2 = get_extents(r)
    mc, mx1, my1, mx2, my2 = get_extents(m)
    print(f'\nPage {i+1}:')
    print(f'  Ref: {rc} items, x=[{rx1:.1f}, {rx2:.1f}], y=[{ry1:.1f}, {ry2:.1f}]')
    print(f'  MP:  {mc} items, x=[{mx1:.1f}, {mx2:.1f}], y=[{my1:.1f}, {my2:.1f}]')
    print(f'  X shift: {mx1-rx1:.1f}, Y shift: {my1-ry1:.1f}')
    print(f'  Width diff: {(mx2-mx1)-(rx2-rx1):.1f}')
