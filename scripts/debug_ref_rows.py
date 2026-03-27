import fitz

# Check reference PDF table row heights on page 3
doc = fitz.open(r'tests/Issue_Files/reference_docx/20260318_issue.pdf')
page = doc[2]  # page 3 (0-indexed)
blocks = page.get_text('dict')['blocks']
items = []
for b in blocks:
    if 'lines' in b:
        for l in b['lines']:
            for s in l['spans']:
                if s['text'].strip():
                    items.append((l['bbox'][1], l['bbox'][3], s['text'][:50], s['size']))
items.sort(key=lambda x: x[0])

# Show all items
for y1, y2, t, sz in items:
    print(f'  y={y1:7.1f}-{y2:7.1f} sz={sz:5.1f} [{t}]')
