import fitz
doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic95_area_chart.pdf')
page = doc[0]
blocks = page.get_text('dict', sort=True)['blocks']
for b in blocks:
    for l in b.get('lines', []):
        for s in l['spans']:
            t = s['text'].strip()
            if t and len(t) > 1:
                orig = s['origin']
                sz = s['size']
                bbox = s['bbox']
                print(f"  x={orig[0]:7.1f} y={orig[1]:7.1f} size={sz:.1f} w={bbox[2]-bbox[0]:.1f} text={t[:80]}")
doc.close()
