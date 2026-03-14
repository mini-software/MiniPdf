import fitz

def get_col_positions(page):
    """Get all text items with their x,y positions"""
    items = []
    blocks = page.get_text('dict')['blocks']
    for b in blocks:
        if 'lines' in b:
            for line in b['lines']:
                for span in line['spans']:
                    if span['text'].strip():
                        items.append((round(span['origin'][0], 1), round(span['origin'][1], 1), span['text'].strip()[:50]))
    items.sort(key=lambda t: (t[1], t[0]))
    return items

# Compare page 2 (Sheet 2 in payroll - salary info) columns
print('=== REFERENCE Page 2 (salary info) ===')
ref = fitz.open('../MiniPdf.Benchmark/reference_pdfs/classic191_payroll_calculator.pdf')
items = get_col_positions(ref[1])
# Get header row items (first data rows)
for x, y, t in items:
    if 140 < y < 170:  # Header area
        print(f'  x={x:6.1f} y={y:6.1f} "{t}"')

print()
print('=== MINIPDF Page 2 (salary info) ===')
mp = fitz.open('pdf_output/classic191_payroll_calculator.pdf')
items = get_col_positions(mp[1])
for x, y, t in items:
    if 140 < y < 170:
        print(f'  x={x:6.1f} y={y:6.1f} "{t}"')

print()
# Page 4 comparison
print('=== REFERENCE Page 4 (payroll data) ===')
items = get_col_positions(ref[3])
for x, y, t in items:
    if 140 < y < 180:
        print(f'  x={x:6.1f} y={y:6.1f} "{t}"')

print()
print('=== MINIPDF Page 4 (payroll data) ===')
items = get_col_positions(mp[3])
for x, y, t in items:
    if 140 < y < 180:
        print(f'  x={x:6.1f} y={y:6.1f} "{t}"')
