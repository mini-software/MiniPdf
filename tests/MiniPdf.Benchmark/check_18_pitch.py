"""Check column width calculations for classic18."""
import fitz, os

# Check MiniPdf output for column widths
mini = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', 'classic18_large_dataset.pdf')
doc = fitz.open(mini)
page = doc[0]

# Get the X positions of all spans in the first data row
blocks = page.get_text("dict", sort=True)
xs = []
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
            if y != prev_y:
                break  # only first row
            xs.append(span['bbox'][0])
        if prev_y is not None and y != prev_y:
            break
    if prev_y is not None and y != prev_y:
        break

print(f"MiniPdf X positions: {[f'{x:.1f}' for x in xs]}")
if len(xs) >= 2:
    pitch = xs[1] - xs[0]
    print(f"Column pitch: {pitch:.2f}pt")
    # Expected: 8.43 char units = 8.43 * 5.62 = 47.37pt
    print(f"Expected from 8.43 charUnits @ 5.62: {8.43 * 5.62:.2f}pt")
    # With padding
    print(f"With ColumnPadding 3pt: pitch includes padding")

# Same for reference
ref = os.path.join('reference_pdfs', 'classic18_large_dataset.pdf')
rdoc = fitz.open(ref)
rpage = rdoc[0]
rxs = []
prev_y = None
for block in rpage.get_text("dict", sort=True)['blocks']:
    if block['type'] != 0:
        continue
    for line in block['lines']:
        for span in line['spans']:
            if not span['text'].strip():
                continue
            y = round(span['bbox'][1])
            if prev_y is None:
                prev_y = y
            if y != prev_y:
                break
            rxs.append(span['bbox'][0])
        if prev_y is not None and y != prev_y:
            break
    if prev_y is not None and y != prev_y:
        break

print(f"\nRef X positions: {[f'{x:.1f}' for x in rxs]}")
if len(rxs) >= 2:
    print(f"Ref pitch: {rxs[1] - rxs[0]:.2f}pt")
