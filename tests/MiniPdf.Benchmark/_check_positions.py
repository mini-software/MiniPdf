import fitz
doc = fitz.open(r'..\MiniPdf.Scripts\pdf_output\classic42_boolean_values.pdf')
page = doc[0]
blocks = page.get_text('dict')['blocks']
for b in blocks:
    if 'lines' in b:
        for line in b['lines']:
            for span in line['spans']:
                t = span['text'].strip()
                if t:
                    x0 = span['bbox'][0]
                    y0 = span['bbox'][1]
                    x1 = span['bbox'][2]
                    print(f'  x={x0:.1f}-{x1:.1f} y={y0:.1f} "{t}"')
doc.close()
