import fitz
doc = fitz.open(r'reference_pdfs/classic74_dashboard_with_kpi_image.pdf')
page = doc[0]
blocks = page.get_text('dict', sort=True)['blocks']
for b in blocks:
    if 'lines' in b:
        for l in b['lines']:
            for s in l['spans']:
                text = s.get('text', '').strip()
                if text:
                    print(f'y={s["bbox"][1]:.2f} font={s["font"]} text={repr(text[:40])}')
