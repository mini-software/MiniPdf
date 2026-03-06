"""Extract exact drawing colors from reference PDF chart pages."""
import fitz
import os

REFERENCE_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"

def extract_drawing_colors(pdf_path):
    """Extract fill colors from PDF drawings."""
    doc = fitz.open(pdf_path)
    colors_by_page = []
    
    for page_num, page in enumerate(doc):
        drawings = page.get_drawings()
        fill_colors = {}
        for d in drawings:
            fill = d.get("fill")
            if fill and len(fill) >= 3:
                r, g, b = round(fill[0], 3), round(fill[1], 3), round(fill[2], 3)
                # Skip white, near-white, black, gray
                if r > 0.9 and g > 0.9 and b > 0.9:
                    continue
                if r < 0.1 and g < 0.1 and b < 0.1:
                    continue
                if abs(r-g) < 0.05 and abs(g-b) < 0.05:
                    continue
                rgb = (r, g, b)
                rect = d.get("rect", fitz.Rect(0,0,0,0))
                area = abs(rect.width * rect.height)
                if rgb not in fill_colors:
                    fill_colors[rgb] = 0
                fill_colors[rgb] += area
        
        if fill_colors:
            colors_by_page.append((page_num, fill_colors))
    
    doc.close()
    return colors_by_page

cases = [
    "classic91_simple_bar_chart",
    "classic94_pie_chart",
    "classic100_stacked_bar_chart",
    "classic93_line_chart",
]

for case in cases:
    pdf_path = os.path.join(REFERENCE_DIR, case + ".pdf")
    if not os.path.exists(pdf_path):
        continue
    
    print(f"\n{'='*60}")
    print(f"CASE: {case}")
    print(f"{'='*60}")
    
    pages = extract_drawing_colors(pdf_path)
    for page_num, colors in pages:
        if colors:
            print(f"  Page {page_num}:")
            for (r, g, b), area in sorted(colors.items(), key=lambda x: -x[1])[:8]:
                r255, g255, b255 = int(r*255), int(g*255), int(b*255)
                print(f"    ({r:.3f}, {g:.3f}, {b:.3f}) = RGB({r255:3d},{g255:3d},{b255:3d}) = #{r255:02x}{g255:02x}{b255:02x}  area={area:.0f}")
