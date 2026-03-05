import fitz, sys
doc = fitz.open(r'..\MiniPdf.Scripts\pdf_output\classic104_combo_bar_line_chart.pdf')
page = doc[0]
blocks = page.get_text('dict')['blocks']
for b in blocks:
    if 'lines' in b:
        for line in b['lines']:
            for span in line['spans']:
                x = span['bbox'][0]
                y = span['bbox'][1]
                t = span['text'].strip()
                if t and ('Sales' in t or 'Target' in t):
                    print(f'  ({x:.1f},{y:.1f}) size={span["size"]:.1f} "{t}"')
doc.close()
print('---')
# Also check classic107
doc2 = fitz.open(r'..\MiniPdf.Scripts\pdf_output\classic107_multi_series_line.pdf')
page2 = doc2[0]
blocks2 = page2.get_text('dict')['blocks']
for b in blocks2:
    if 'lines' in b:
        for line in b['lines']:
            for span in line['spans']:
                t = span['text'].strip()
                if t and ('AAPL' in t or 'GOOG' in t or 'MSFT' in t):
                    x = span['bbox'][0]
                    y = span['bbox'][1]
                    print(f'  ({x:.1f},{y:.1f}) size={span["size"]:.1f} "{t}"')
doc2.close()
