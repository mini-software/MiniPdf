"""Compare chart drawing elements between our PDF and reference PDF."""
import fitz
import os

MINIPDF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REFERENCE_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"

def get_drawings(pdf_path, page_idx=1):
    """Get drawings from a specific page."""
    doc = fitz.open(pdf_path)
    if page_idx >= len(doc):
        doc.close()
        return []
    page = doc[page_idx]
    drawings = page.get_drawings()
    doc.close()
    return drawings

def analyze_rects(drawings):
    """Analyze filled rectangles (bars, grid cells)."""
    rects = []
    lines = []
    for d in drawings:
        fill = d.get("fill")
        color = d.get("color")
        rect = d.get("rect")
        if not rect:
            continue
        
        w = abs(rect.x1 - rect.x0)
        h = abs(rect.y1 - rect.y0)
        
        if fill and len(fill) >= 3 and (fill[0] != fill[1] or fill[1] != fill[2] or fill[0] < 0.8):
            # Colored filled rectangle (bar or chart element)
            if w > 2 and h > 2:  # Skip tiny elements
                rects.append({
                    'x': round(rect.x0, 1),
                    'y': round(rect.y0, 1),
                    'w': round(w, 1),
                    'h': round(h, 1),
                    'fill': tuple(round(c, 3) for c in fill[:3]),
                })
        
        if color and not fill:
            # Line (axis, gridline)
            lines.append({
                'x0': round(rect.x0, 1),
                'y0': round(rect.y0, 1),
                'x1': round(rect.x1, 1),
                'y1': round(rect.y1, 1),
                'color': tuple(round(c, 3) for c in color[:3]) if color else None,
            })
    
    return rects, lines

case = "classic91_simple_bar_chart"
mini_path = os.path.join(MINIPDF_DIR, case + ".pdf")
ref_path = os.path.join(REFERENCE_DIR, case + ".pdf")

# Check page count
mini_doc = fitz.open(mini_path)
ref_doc = fitz.open(ref_path)
print(f"Our pages: {len(mini_doc)}, Ref pages: {len(ref_doc)}")
print(f"Our page 1 size: {mini_doc[1].rect.width} x {mini_doc[1].rect.height}")
print(f"Ref page 1 size: {ref_doc[1].rect.width} x {ref_doc[1].rect.height}")
mini_doc.close()
ref_doc.close()

# Get chart drawings from page 1 (index 1)
mini_draws = get_drawings(mini_path, 1)
ref_draws = get_drawings(ref_path, 1)
print(f"\nOur drawings: {len(mini_draws)}, Ref drawings: {len(ref_draws)}")

mini_rects, mini_lines = analyze_rects(mini_draws)
ref_rects, ref_lines = analyze_rects(ref_draws)

print(f"\nOur colored rects: {len(mini_rects)}")
for r in sorted(mini_rects, key=lambda x: x['x'])[:15]:
    print(f"  x={r['x']:7.1f} y={r['y']:7.1f} w={r['w']:7.1f} h={r['h']:7.1f} fill={r['fill']}")

print(f"\nRef colored rects: {len(ref_rects)}")
for r in sorted(ref_rects, key=lambda x: x['x'])[:15]:
    print(f"  x={r['x']:7.1f} y={r['y']:7.1f} w={r['w']:7.1f} h={r['h']:7.1f} fill={r['fill']}")

# Also show page 1 text positions
mini_doc = fitz.open(mini_path)
page = mini_doc[1]
data = page.get_text("dict", sort=True)
print(f"\nOur page 1 text spans (first 10):")
spans = []
for block in data.get("blocks", []):
    if block.get("type", 0) != 0:
        continue
    for line in block.get("lines", []):
        for span in line.get("spans", []):
            text = span.get("text", "").strip()
            if text:
                bbox = span["bbox"]
                spans.append((bbox[1], bbox[0], text, span.get("size", 0)))
spans.sort()
for y, x, text, size in spans[:15]:
    print(f"  y={y:7.1f} x={x:7.1f} size={size:5.1f} '{text}'")
mini_doc.close()
