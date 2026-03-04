import fitz
import re

# Compare Y-axis tick values between MiniPdf and Reference
for name in ['classic95_area_chart', 'classic118_bar_chart_custom_colors']:
    print(f'\n=== {name} ===')
    for label, path in [('Mini', f'D:/git/MiniPdf/tests/MiniPdf.Scripts/pdf_output/{name}.pdf'),
                        ('Ref ', f'reference_pdfs/{name}.pdf')]:
        doc = fitz.open(path)
        page = doc[0]
        blocks = page.get_text('dict', sort=True)['blocks']
        numbers = []
        for b in blocks:
            if 'lines' in b:
                for l in b['lines']:
                    for s in l['spans']:
                        text = s.get('text', '').strip()
                        y = round(s["bbox"][1], 1)
                        x = s["bbox"][0]
                        if text and re.match(r'^-?\d+\.?\d*$', text) and y > 150:
                            numbers.append((y, x, text))
        numbers.sort()
        print(f'  {label} ticks: {[(t, round(x,0)) for y, x, t in numbers]}')
        doc.close()
