"""Measure row heights in classic18 reference vs MiniPdf output."""
import subprocess, json, re, sys

def get_text_positions(pdf_path, page=0):
    """Extract text Y positions from a PDF page using pdftotext or similar."""
    # Use PyMuPDF (fitz) to extract text positions
    try:
        import fitz
    except ImportError:
        print("Need PyMuPDF: pip install PyMuPDF")
        sys.exit(1)
    
    doc = fitz.open(pdf_path)
    pg = doc[page]
    blocks = pg.get_text("dict")["blocks"]
    
    # Get all text line Y positions
    y_positions = []
    for block in blocks:
        if "lines" not in block:
            continue
        for line in block["lines"]:
            for span in line["spans"]:
                y_positions.append(span["origin"][1])
    
    return sorted(set(round(y, 2) for y in y_positions))

def get_drawing_lines(pdf_path, page=0):
    """Extract horizontal lines (borders/gridlines) from a PDF page."""
    import fitz
    doc = fitz.open(pdf_path)
    pg = doc[page]
    drawings = pg.get_drawings()
    
    h_lines = []
    for d in drawings:
        for item in d["items"]:
            if item[0] == "l":  # line
                p1, p2 = item[1], item[2]
                if abs(p1.y - p2.y) < 0.5:  # horizontal
                    h_lines.append(round(p1.y, 2))
    
    return sorted(set(h_lines))

ref_path = r"..\MiniPdf.Benchmark\reference_pdfs\classic18_large_dataset.pdf"
mini_path = r"..\MiniPdf.Scripts\pdf_output\classic18_large_dataset.pdf"

print("=== classic18 Page 1 Text Y Positions ===")
print("\nReference:")
ref_y = get_text_positions(ref_path, 0)
print(f"  Count: {len(ref_y)}")
if len(ref_y) > 3:
    gaps = [round(ref_y[i+1] - ref_y[i], 2) for i in range(min(20, len(ref_y)-1))]
    print(f"  First 20 Y: {ref_y[:20]}")
    print(f"  First 20 gaps: {gaps}")
    if len(ref_y) > 2:
        avg_gap = round(sum(ref_y[i+1] - ref_y[i] for i in range(len(ref_y)-1)) / (len(ref_y)-1), 4)
        print(f"  Avg gap: {avg_gap}")

print("\nMiniPdf:")
mini_y = get_text_positions(mini_path, 0)
print(f"  Count: {len(mini_y)}")
if len(mini_y) > 3:
    gaps = [round(mini_y[i+1] - mini_y[i], 2) for i in range(min(20, len(mini_y)-1))]
    print(f"  First 20 Y: {mini_y[:20]}")
    print(f"  First 20 gaps: {gaps}")
    if len(mini_y) > 2:
        avg_gap = round(sum(mini_y[i+1] - mini_y[i] for i in range(len(mini_y)-1)) / (len(mini_y)-1), 4)
        print(f"  Avg gap: {avg_gap}")

# Also check page sizes
import fitz
ref_doc = fitz.open(ref_path)
mini_doc = fitz.open(mini_path)
print(f"\nPage sizes: ref={ref_doc[0].rect}, mini={mini_doc[0].rect}")
print(f"Page counts: ref={len(ref_doc)}, mini={len(mini_doc)}")

# Check gridlines too
print("\n=== Horizontal Gridlines Page 1 ===")
ref_lines = get_drawing_lines(ref_path, 0)
mini_lines = get_drawing_lines(mini_path, 0)
print(f"Reference lines: {len(ref_lines)}")
if ref_lines:
    print(f"  First 10: {ref_lines[:10]}")
    if len(ref_lines) > 1:
        ref_lgaps = [round(ref_lines[i+1] - ref_lines[i], 2) for i in range(min(10, len(ref_lines)-1))]
        print(f"  First 10 gaps: {ref_lgaps}")
print(f"MiniPdf lines: {len(mini_lines)}")
if mini_lines:
    print(f"  First 10: {mini_lines[:10]}")
    if len(mini_lines) > 1:
        mini_lgaps = [round(mini_lines[i+1] - mini_lines[i], 2) for i in range(min(10, len(mini_lines)-1))]
        print(f"  First 10 gaps: {mini_lgaps}")
