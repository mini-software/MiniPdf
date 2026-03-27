import fitz
doc = fitz.open(r'tests/Issue_Files/minipdf_docx/20260318_issue.pdf')
for pi in range(min(4, doc.page_count)):
    page = doc[pi]
    blocks = page.get_text('dict')['blocks']
    first_y = None
    last_y = None
    items = []
    for b in blocks:
        if 'lines' in b:
            for l in b['lines']:
                for s in l['spans']:
                    if s['text'].strip():
                        y = l['bbox'][1]
                        yb = l['bbox'][3]
                        if first_y is None: first_y = y
                        last_y = yb
                        items.append((y, yb, s['text'][:40]))
    print(f'Page {pi+1}: first_y={first_y:.1f} last_y={last_y:.1f} range={last_y-first_y:.1f}')
    if pi == 2:
        # Show first and last items on page 3
        items.sort(key=lambda x: x[0])
        print('  First 3:')
        for y1, y2, t in items[:3]:
            print(f'    y={y1:.1f}-{y2:.1f} [{t}]')
        print('  Last 5:')
        for y1, y2, t in items[-5:]:
            print(f'    y={y1:.1f}-{y2:.1f} [{t}]')
