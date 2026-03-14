import fitz

# Check what happened with classic37_freeze_panes
ref = fitz.open(r'..\MiniPdf.Benchmark\reference_pdfs\classic37_freeze_panes.pdf')
mini = fitz.open(r'..\MiniPdf.Scripts\pdf_output\classic37_freeze_panes.pdf')

print(f'Ref pages: {len(ref)}, Mini pages: {len(mini)}')
print(f'Ref page size: {ref[0].rect}')
print(f'Mini page size: {mini[0].rect}')

# Check text
ref_pg = ref[0]
mini_pg = mini[0]

def get_spans(pg):
    blocks = pg.get_text('dict')['blocks']
    spans = []
    for b in blocks:
        if 'lines' not in b: continue
        for l in b['lines']:
            for s in l['spans']:
                spans.append((round(s['origin'][0],2), round(s['origin'][1],2), s['text'].strip()))
    return spans

ref_s = get_spans(ref_pg)
mini_s = get_spans(mini_pg)
print(f'\nRef spans: {len(ref_s)}')
for s in ref_s[:10]:
    print(f'  x={s[0]:7.2f} y={s[1]:7.2f} text={s[2][:30]}')
print(f'\nMini spans: {len(mini_s)}')
for s in mini_s[:10]:
    print(f'  x={s[0]:7.2f} y={s[1]:7.2f} text={s[2][:30]}')
