import fitz

# Check pie chart text in reference pdfs
for name in ['classic94_pie_chart', 'classic103_pie_chart_with_labels', 'classic106_3d_pie_chart', 'classic97_doughnut_chart']:
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
                            ys.append((y, text))
        ys.sort()
        for y, text in ys:
            print(f'  y={y:.1f}: {repr(text)}')
    doc.close()
    
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
                            ys.append((y, text))
        ys.sort()
        for y, text in ys:
            print(f'  y={y:.1f}: {repr(text)}')
    doc.close()
