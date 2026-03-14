"""Analyze pixel differences for classic18 to find specific visual issues."""
import fitz
import os

def get_pixel_data(pdf_path, page=0, dpi=72):
    doc = fitz.open(pdf_path)
    if page >= len(doc):
        return None
    pg = doc[page]
    mat = fitz.Matrix(dpi/72, dpi/72)
    pix = pg.get_pixmap(matrix=mat)
    return pix.width, pix.height, pix.samples

ref_path = r'reference_pdfs\classic18_large_dataset.pdf'
mini_path = r'..\MiniPdf.Scripts\pdf_output\classic18_large_dataset.pdf'

# Get page 1 pixels at 72dpi (1:1)
w1, h1, s1 = get_pixel_data(ref_path, 0)
w2, h2, s2 = get_pixel_data(mini_path, 0)

print(f'Size: {w1}x{h1} vs {w2}x{h2}')

# Find regions with differences
channels = 3
diff_pixels = 0
total_pixels = w1 * h1

# Analyze by rows
row_diffs = []
for y in range(h1):
    row_diff = 0
    for x in range(w1):
        off = (y * w1 + x) * channels
        if s1[off] != s2[off] or s1[off+1] != s2[off+1] or s1[off+2] != s2[off+2]:
            row_diff += 1
            diff_pixels += 1
    row_diffs.append(row_diff)

print(f'Total different pixels: {diff_pixels} / {total_pixels} ({diff_pixels/total_pixels*100:.2f}%)')

# Find top 20 rows with most differences  
sorted_rows = sorted(enumerate(row_diffs), key=lambda x: -x[1])
print('\nTop 20 rows with most pixel differences:')
for y, count in sorted_rows[:20]:
    print(f'  Row y={y}: {count} different pixels ({count/w1*100:.1f}%)')

# Analyze by columns to see X drift
col_diffs = [0] * w1
for y in range(h1):
    for x in range(w1):
        off = (y * w1 + x) * channels
        if s1[off] != s2[off] or s1[off+1] != s2[off+1] or s1[off+2] != s2[off+2]:
            col_diffs[x] += 1

# Find column ranges with high diff
print('\nColumn ranges with high pixel differences:')
in_diff = False
start_x = 0
for x in range(w1):
    if col_diffs[x] > h1 * 0.01 and not in_diff:
        in_diff = True
        start_x = x
    elif col_diffs[x] <= h1 * 0.01 and in_diff:
        in_diff = False
        print(f'  x={start_x}-{x}: max={max(col_diffs[start_x:x])} pixels')
if in_diff:
    print(f'  x={start_x}-{w1}: max={max(col_diffs[start_x:])} pixels')
