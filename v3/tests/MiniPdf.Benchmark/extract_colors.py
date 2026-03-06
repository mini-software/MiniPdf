"""Extract dominant colors from chart areas in reference PDFs."""
import fitz
import os
from collections import Counter

REFERENCE_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"

def get_chart_colors(pdf_path, dpi=150):
    """Extract colored rectangles from chart area of a PDF."""
    doc = fitz.open(pdf_path)
    colors = Counter()
    
    for page in doc:
        # Render at low DPI
        mat = fitz.Matrix(dpi/72, dpi/72)
        pix = page.get_pixmap(matrix=mat, alpha=False)
        w, h = pix.width, pix.height
        samples = pix.samples
        
        # Look at bottom half of page (where chart typically is)
        # Sample every 10th pixel
        for y in range(h//3, h*2//3):
            for x in range(w//4, w*3//4, 5):
                idx = (y * w + x) * 3
                r, g, b = samples[idx], samples[idx+1], samples[idx+2]
                # Skip white, near-white, black, and gray
                if r > 230 and g > 230 and b > 230:
                    continue
                if r < 30 and g < 30 and b < 30:
                    continue
                if abs(r-g) < 15 and abs(g-b) < 15 and abs(r-b) < 15:
                    continue
                # Quantize to reduce noise
                r = (r // 16) * 16
                g = (g // 16) * 16
                b = (b // 16) * 16
                colors[(r, g, b)] += 1
    
    doc.close()
    return colors

# Analyze a few reference chart PDFs
cases = [
    "classic91_simple_bar_chart",
    "classic94_pie_chart", 
    "classic93_line_chart",
    "classic100_stacked_bar_chart",
    "classic92_horizontal_bar_chart",
]

for case in cases:
    pdf_path = os.path.join(REFERENCE_DIR, case + ".pdf")
    if not os.path.exists(pdf_path):
        continue
    
    colors = get_chart_colors(pdf_path)
    print(f"\n{case}:")
    for (r, g, b), count in colors.most_common(10):
        print(f"  RGB({r:3d}, {g:3d}, {b:3d}) = #{r:02x}{g:02x}{b:02x}  count={count}")
