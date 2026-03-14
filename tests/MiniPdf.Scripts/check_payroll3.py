import fitz

def get_page_columns(page):
    """Get unique x positions to estimate column count"""
    x_positions = set()
    blocks = page.get_text('dict')['blocks']
    all_text = []
    for b in blocks:
        if 'lines' in b:
            for line in b['lines']:
                for span in line['spans']:
                    if span['text'].strip():
                        x = round(span['origin'][0], 0)
                        y = round(span['origin'][1], 0)
                        x_positions.add(x)
                        all_text.append((x, y, span['text'][:40]))
    return sorted(x_positions), all_text

# Reference
print('=== REFERENCE ===')
doc = fitz.open('../MiniPdf.Benchmark/reference_pdfs/classic191_payroll_calculator.pdf')
for i in range(min(5, len(doc))):
    page = doc[i]
    xs, texts = get_page_columns(page)
    print(f'Page {i+1}: {len(xs)} unique x positions')
    # Show first row of data to understand column layout
    # Get first few lines
    texts.sort(key=lambda t: (t[1], t[0]))
    seen_y = set()
    row_count = 0
    for x, y, t in texts:
        if y not in seen_y:
            seen_y.add(y)
            row_count += 1
            if row_count <= 3:
                # Print all items at this y
                row_items = [(x2, t2) for x2, y2, t2 in texts if y2 == y]
                row_items.sort()
                print(f'  y={y}: {" | ".join(f"x={x2:.0f} {t2}" for x2, t2 in row_items)}')

print()
print('=== MINIPDF ===')
doc2 = fitz.open('pdf_output/classic191_payroll_calculator.pdf')
for i in range(min(5, len(doc2))):
    page = doc2[i]
    xs, texts = get_page_columns(page)
    print(f'Page {i+1}: {len(xs)} unique x positions')
    texts.sort(key=lambda t: (t[1], t[0]))
    seen_y = set()
    row_count = 0
    for x, y, t in texts:
        if y not in seen_y:
            seen_y.add(y)
            row_count += 1
            if row_count <= 3:
                row_items = [(x2, t2) for x2, y2, t2 in texts if y2 == y]
                row_items.sort()
                print(f'  y={y}: {" | ".join(f"x={x2:.0f} {t2}" for x2, t2 in row_items)}')
