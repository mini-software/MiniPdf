import fitz
doc = fitz.open(r'tests/Issue_Files/minipdf_docx/20260318_issue.pdf')
for pi in [2, 3, 4, 5]:
    if pi >= doc.page_count:
        break
    page = doc[pi]
    blocks = page.get_text('dict')['blocks']
    items = []
    for b in blocks:
        if 'lines' in b:
            for l in b['lines']:
                for s in l['spans']:
                    if s['text'].strip():
                        items.append((l['bbox'][1], l['bbox'][3], s['text'][:50]))
    items.sort(key=lambda x: x[0])
    print(f'\n=== Page {pi+1} ({len(items)} items) ===')
    for y1, y2, t in items[:8]:
        print(f'  y={y1:7.1f}-{y2:7.1f} [{t}]')
    if len(items) > 16:
        print(f'  ... ({len(items)-16} more)')
    for y1, y2, t in items[-8:]:
        print(f'  y={y1:7.1f}-{y2:7.1f} [{t}]')
