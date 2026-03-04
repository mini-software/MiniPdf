import fitz
import re

# Compare Y-axis tick values between MiniPdf and Reference for key chart cases
for name in ['classic95_area_chart', 'classic118_bar_chart_custom_colors', 'classic115_chart_negative_values']:
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
                        if text and re.match(r'^-?\d+\.?\d*$', text):
                            y = s["bbox"][1]
                            # Only look at Y-axis tick area (X < 150 typically)
                            x = s["bbox"][0]
                            if x < 100:
                                numbers.append((round(y, 1), text))
        numbers.sort()
        tick_vals = [t for _, t in numbers]
        print(f'  {label}: {tick_vals}')
        doc.close()
