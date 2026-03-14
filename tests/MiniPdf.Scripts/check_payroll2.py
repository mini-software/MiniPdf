import fitz

# Check reference PDF page font sizes
doc = fitz.open('../MiniPdf.Benchmark/reference_pdfs/classic191_payroll_calculator.pdf')
for i in range(min(4, len(doc))):
    page = doc[i]
    print(f'=== Page {i+1} ({page.rect.width:.0f}x{page.rect.height:.0f}) ===')
    blocks = page.get_text('dict')['blocks']
    sizes = set()
    min_x = 999
    max_x = 0
    for b in blocks:
        if 'lines' in b:
            for line in b['lines']:
                for span in line['spans']:
                    if span['text'].strip():
                        x, y = span['origin']
                        min_x = min(min_x, x)
                        # Find rightmost extent
                        max_x = max(max_x, span['bbox'][2])
                        sizes.add(round(span['size'], 1))
    print(f'  x range: {min_x:.1f} - {max_x:.1f}  (width used: {max_x - min_x:.1f})')
    print(f'  font sizes: {sorted(sizes)}')

# Now check MiniPdf PDF
print()
doc2 = fitz.open('pdf_output/classic191_payroll_calculator.pdf')
for i in range(min(4, len(doc2))):
    page = doc2[i]
    print(f'=== MiniPdf Page {i+1} ({page.rect.width:.0f}x{page.rect.height:.0f}) ===')
    blocks = page.get_text('dict')['blocks']
    sizes = set()
    min_x = 999
    max_x = 0
    for b in blocks:
        if 'lines' in b:
            for line in b['lines']:
                for span in line['spans']:
                    if span['text'].strip():
                        x, y = span['origin']
                        min_x = min(min_x, x)
                        max_x = max(max_x, span['bbox'][2])
                        sizes.add(round(span['size'], 1))
    print(f'  x range: {min_x:.1f} - {max_x:.1f}  (width used: {max_x - min_x:.1f})')
    print(f'  font sizes: {sorted(sizes)}')
