import fitz
import sys

# Check Y spacing for a chart case that regressed
for name in ['classic91_simple_bar_chart', 'classic118_bar_chart_custom_colors']:
    print(f'\n=== {name} (MiniPdf) ===')
    doc = fitz.open(f'D:/git/MiniPdf/tests/MiniPdf.Scripts/pdf_output/{name}.pdf')
    for pi, page in enumerate(doc):
        blocks = page.get_text('dict', sort=True)['blocks']
        ys = []
        for b in blocks:
            if 'lines' in b:
                for l in b['lines']:
                    for s in l['spans']:
                        text = s.get('text', '').strip()
                        if text:
                            y = round(s["bbox"][1], 1)
                            ys.append((y, s["font"], text[:30]))
        ys.sort()
        prev_y = None 
        for y, font, text in ys:
            gap = f' (+{y-prev_y:.1f})' if prev_y is not None and y != prev_y else ''
            prev_y = y
            print(f'  y={y:.1f}{gap} {font}: {repr(text)}')
    doc.close()

    print(f'\n=== {name} (Reference) ===')
    doc = fitz.open(f'reference_pdfs/{name}.pdf')
    for pi, page in enumerate(doc):
        blocks = page.get_text('dict', sort=True)['blocks']
        ys = []
        for b in blocks:
            if 'lines' in b:
                for l in b['lines']:
                    for s in l['spans']:
                        text = s.get('text', '').strip()
                        if text:
                            y = round(s["bbox"][1], 1)
                            ys.append((y, s["font"], text[:30]))
        ys.sort()
        prev_y = None
        for y, font, text in ys:
            gap = f' (+{y-prev_y:.1f})' if prev_y is not None and y != prev_y else ''
            prev_y = y
            print(f'  y={y:.1f}{gap} {font}: {repr(text)}')
    doc.close()
