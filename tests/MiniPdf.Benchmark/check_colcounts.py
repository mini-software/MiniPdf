"""Check column counts for top-changed cases."""
import fitz, os

cases = ['classic191_payroll_calculator', 'classic60_large_wide_table', 'classic18_large_dataset']
for case in cases:
    path = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', f'{case}.pdf')
    if not os.path.exists(path):
        continue
    doc = fitz.open(path)
    page = doc[0]
    blocks = page.get_text("dict", sort=True)
    
    # Count unique X positions in first row
    xs = set()
    prev_y = None
    for block in blocks['blocks']:
        if block['type'] != 0:
            continue
        for line in block['lines']:
            for span in line['spans']:
                if not span['text'].strip():
                    continue
                y = round(span['bbox'][1])
                if prev_y is None:
                    prev_y = y
                if y == prev_y:
                    xs.add(round(span['bbox'][0]))
    
    print(f"{case}: ~{len(xs)} columns (from first row)")
