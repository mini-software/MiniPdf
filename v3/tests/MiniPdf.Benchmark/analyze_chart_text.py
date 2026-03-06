"""Compare extracted text from our PDFs vs reference PDFs for chart cases."""
import subprocess
import sys
import os
import difflib

MINIPDF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REFERENCE_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"

def extract_text(pdf_path):
    """Extract text using pdfplumber."""
    try:
        import pdfplumber
    except ImportError:
        subprocess.check_call([sys.executable, "-m", "pip", "install", "pdfplumber", "-q"])
        import pdfplumber
    
    texts = []
    with pdfplumber.open(pdf_path) as pdf:
        for page in pdf.pages:
            words = page.extract_words(x_tolerance=3, y_tolerance=3)
            # Group by y position
            lines = {}
            for w in words:
                y = round(float(w['top']), 0)
                if y not in lines:
                    lines[y] = []
                lines[y].append((float(w['x0']), w['text']))
            for y in sorted(lines.keys()):
                line_words = sorted(lines[y], key=lambda x: x[0])
                texts.append(' '.join(w[1] for w in line_words))
    return texts

# Chart cases to analyze
cases = [
    "classic91_simple_bar_chart",
    "classic92_horizontal_bar_chart", 
    "classic93_line_chart",
    "classic94_pie_chart",
    "classic95_area_chart",
    "classic100_stacked_bar_chart",
    "classic102_line_chart_with_markers",
    "classic111_chart_with_axis_labels",
]

for case in cases:
    mini_path = os.path.join(MINIPDF_DIR, case + ".pdf")
    ref_path = os.path.join(REFERENCE_DIR, case + ".pdf")
    
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        print(f"SKIP {case}: missing file")
        continue
    
    mini_text = extract_text(mini_path)
    ref_text = extract_text(ref_path)
    
    print(f"\n{'='*60}")
    print(f"CASE: {case}")
    print(f"{'='*60}")
    print(f"Our lines: {len(mini_text)}, Ref lines: {len(ref_text)}")
    
    # Show diff
    diff = list(difflib.unified_diff(ref_text, mini_text, lineterm='', fromfile='reference', tofile='minipdf', n=1))
    if diff:
        for line in diff[:40]:
            print(line)
    else:
        print("  TEXT IDENTICAL")
