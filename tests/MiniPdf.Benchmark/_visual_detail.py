"""Check visual score components for a specific file."""
import subprocess, sys, json

name = sys.argv[1] if len(sys.argv) > 1 else "classic132_striped_table"
mini = f"../MiniPdf.Scripts/pdf_output/{name}.pdf"
ref = f"reference_pdfs/{name}.pdf"

# Run compare on single file and capture components
import fitz
from PIL import Image
import io

def render_page(pdf_path, page_num=0, dpi=150):
    doc = fitz.open(pdf_path)
    page = doc[page_num]
    mat = fitz.Matrix(dpi/72, dpi/72)
    pix = page.get_pixmap(matrix=mat)
    img = Image.open(io.BytesIO(pix.tobytes("png")))
    return img

img_mini = render_page(mini)
img_ref = render_page(ref)

# Ensure same size
w = min(img_mini.width, img_ref.width)
h = min(img_mini.height, img_ref.height)
img_mini = img_mini.crop((0, 0, w, h))
img_ref = img_ref.crop((0, 0, w, h))

import numpy as np
a = np.array(img_mini)
b = np.array(img_ref)

# Raw byte match
raw = np.mean(a == b)

# Grid density (divide into grid)
grid_size = 8
scores = []
for gy in range(0, h, grid_size):
    for gx in range(0, w, grid_size):
        block_a = a[gy:gy+grid_size, gx:gx+grid_size]
        block_b = b[gy:gy+grid_size, gx:gx+grid_size]
        scores.append(np.mean(block_a == block_b))
grid = np.mean(scores)

# Top strip (top 15% of the page)
top_h = int(h * 0.15)
top_a = a[:top_h]
top_b = b[:top_h]
top_strip = np.mean(top_a == top_b)

vis = 0.40 * raw + 0.40 * grid + 0.20 * top_strip
print(f"raw_byte={raw:.4f} grid_density={grid:.4f} top_strip={top_strip:.4f}")
print(f"visual = 0.40*{raw:.4f} + 0.40*{grid:.4f} + 0.20*{top_strip:.4f} = {vis:.4f}")
print(f"Need vis >= 0.975 to pass. Current = {vis:.4f}, gap = {0.975 - vis:.4f}")

# Find worst regions
print(f"\nImage sizes: MINI={img_mini.size} REF={img_ref.size}")
diff = np.any(a != b, axis=2)  # per-pixel difference
print(f"Total different pixels: {np.sum(diff)}/{diff.size} ({np.mean(diff)*100:.1f}%)")

# Show where differences are vertically
for y_start in range(0, h, h//10):
    y_end = min(y_start + h//10, h)
    region_diff = np.mean(diff[y_start:y_end])
    print(f"  Y {y_start:4d}-{y_end:4d}: {region_diff*100:.1f}% different")
