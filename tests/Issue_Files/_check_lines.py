import fitz

# Detailed extraction of line grouping for first few lines
doc = fitz.open('minipdf_docx/SA8000 ch sample.pdf')
page = doc[0]
blocks = page.get_text('dict')['blocks']
for bi, b in enumerate(blocks[:8]):
    if 'lines' not in b:
        continue
    for li, line in enumerate(b['lines']):
        spans_txt = []
        for span in line['spans']:
            spans_txt.append(f"({span['bbox'][0]:.0f}-{span['bbox'][2]:.0f})'{span['text'][:30]}'")
        y0 = line['bbox'][1]
        print(f"B{bi} L{li} y={y0:.1f}: {' | '.join(spans_txt)}")

print("\n--- Reference ---")
doc2 = fitz.open('reference_docx/SA8000 ch sample.pdf')
page2 = doc2[0]
blocks2 = page2.get_text('dict')['blocks']
for bi, b in enumerate(blocks2[:8]):
    if 'lines' not in b:
        continue
    for li, line in enumerate(b['lines']):
        spans_txt = []
        for span in line['spans']:
            spans_txt.append(f"({span['bbox'][0]:.0f}-{span['bbox'][2]:.0f})'{span['text'][:30]}'")
        y0 = line['bbox'][1]
        print(f"B{bi} L{li} y={y0:.1f}: {' | '.join(spans_txt)}")
