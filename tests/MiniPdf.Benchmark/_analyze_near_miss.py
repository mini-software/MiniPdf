"""Analyze near-miss PDF files: extract and compare PDF content streams, fills, borders, text."""
import fitz
import sys
import re
from collections import Counter

MINI_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"


def extract_stream_ops(doc, page_idx=0):
    """Extract raw content stream operations from a PDF page."""
    page = doc[page_idx]
    contents = page.get_contents()
    all_ops = []
    for c in contents:
        stream = doc.xref_stream(c)
        text = stream.decode('latin-1', errors='replace')
        all_ops.append(text)
    return "\n".join(all_ops)


def extract_fill_rects(stream_text):
    """Parse PDF stream to find filled rectangles with their colors."""
    fills = []
    current_fill_color = None
    current_stroke_color = None
    lines = stream_text.split('\n')
    for line in lines:
        line = line.strip()
        # Non-stroking color (fill) - rg for RGB
        m = re.match(r'^([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+rg$', line)
        if m:
            current_fill_color = (float(m.group(1)), float(m.group(2)), float(m.group(3)))
            continue
        # Stroking color - RG for RGB
        m = re.match(r'^([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+RG$', line)
        if m:
            current_stroke_color = (float(m.group(1)), float(m.group(2)), float(m.group(3)))
            continue
        # Gray fill
        m = re.match(r'^([\d.]+)\s+g$', line)
        if m:
            v = float(m.group(1))
            current_fill_color = (v, v, v)
            continue
        # Gray stroke
        m = re.match(r'^([\d.]+)\s+G$', line)
        if m:
            v = float(m.group(1))
            current_stroke_color = (v, v, v)
            continue
        # Rectangle: x y w h re
        m = re.match(r'^([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+re$', line)
        if m:
            x, y, w, h = float(m.group(1)), float(m.group(2)), float(m.group(3)), float(m.group(4))
            # Next line should be f (fill), S (stroke), or B (both)
            continue
        # Fill operator
        if line in ('f', 'f*', 'F'):
            fills.append({
                'type': 'fill',
                'color': current_fill_color
            })
            continue
        # Stroke
        if line == 'S':
            fills.append({
                'type': 'stroke',
                'color': current_stroke_color
            })
            continue
        # Both fill and stroke
        if line in ('B', 'B*'):
            fills.append({
                'type': 'fill+stroke',
                'fill_color': current_fill_color,
                'stroke_color': current_stroke_color
            })
    return fills


def extract_detailed_rects(stream_text):
    """Parse PDF stream to find rectangles with position, size, and colors."""
    rects = []
    current_fill_color = None
    current_stroke_color = None
    current_line_width = None
    pending_rect = None
    
    lines = stream_text.split('\n')
    for line in lines:
        line = line.strip()
        if not line:
            continue
            
        # Non-stroking color (fill) - rg for RGB
        m = re.match(r'^([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+rg$', line)
        if m:
            current_fill_color = (round(float(m.group(1)), 4), round(float(m.group(2)), 4), round(float(m.group(3)), 4))
            continue
        # Stroking color - RG for RGB
        m = re.match(r'^([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+RG$', line)
        if m:
            current_stroke_color = (round(float(m.group(1)), 4), round(float(m.group(2)), 4), round(float(m.group(3)), 4))
            continue
        # Gray fill
        m = re.match(r'^([\d.]+)\s+g$', line)
        if m:
            v = round(float(m.group(1)), 4)
            current_fill_color = (v, v, v)
            continue
        # Gray stroke
        m = re.match(r'^([\d.]+)\s+G$', line)
        if m:
            v = round(float(m.group(1)), 4)
            current_stroke_color = (v, v, v)
            continue
        # Line width
        m = re.match(r'^([\d.]+)\s+w$', line)
        if m:
            current_line_width = float(m.group(1))
            continue
        # Rectangle
        m = re.match(r'^([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+re$', line)
        if m:
            pending_rect = (float(m.group(1)), float(m.group(2)), float(m.group(3)), float(m.group(4)))
            continue
        # Fill operator
        if line in ('f', 'f*', 'F') and pending_rect:
            rects.append({
                'op': 'fill',
                'x': pending_rect[0], 'y': pending_rect[1],
                'w': pending_rect[2], 'h': pending_rect[3],
                'fill_color': current_fill_color,
                'line_width': current_line_width
            })
            pending_rect = None
            continue
        # Stroke
        if line == 'S' and pending_rect:
            rects.append({
                'op': 'stroke',
                'x': pending_rect[0], 'y': pending_rect[1],
                'w': pending_rect[2], 'h': pending_rect[3],
                'stroke_color': current_stroke_color,
                'line_width': current_line_width
            })
            pending_rect = None
            continue
        # Both
        if line in ('B', 'B*') and pending_rect:
            rects.append({
                'op': 'fill+stroke',
                'x': pending_rect[0], 'y': pending_rect[1],
                'w': pending_rect[2], 'h': pending_rect[3],
                'fill_color': current_fill_color,
                'stroke_color': current_stroke_color,
                'line_width': current_line_width
            })
            pending_rect = None
            continue
        # Line operations (m, l, re sequences for borders)
        if line in ('f', 'f*', 'F', 'S', 'B', 'B*'):
            pending_rect = None
    return rects


def extract_text_spans(doc, page_idx=0):
    """Extract text spans with position, font, size."""
    page = doc[page_idx]
    data = page.get_text("dict", sort=True)
    spans = []
    for block in data.get("blocks", []):
        if block.get("type", 0) != 0:
            continue
        for line in block.get("lines", []):
            for span in line.get("spans", []):
                text = span.get("text", "").strip()
                if text:
                    spans.append({
                        'text': text,
                        'bbox': span["bbox"],
                        'font': span.get("font", ""),
                        'size': span.get("size", 0),
                        'color': span.get("color", 0),
                        'flags': span.get("flags", 0),
                    })
    return spans


def color_to_hex(c):
    if c is None:
        return "None"
    return f"({c[0]:.3f},{c[1]:.3f},{c[2]:.3f})"


def compare_fills(mini_rects, ref_rects, label):
    """Compare fill rectangles between mini and ref."""
    print(f"\n  --- {label}: Fill Rectangles ---")
    
    # Count fill colors
    mini_fill_colors = Counter()
    ref_fill_colors = Counter()
    
    for r in mini_rects:
        if r['op'] in ('fill', 'fill+stroke'):
            c = r.get('fill_color')
            if c and c != (1.0, 1.0, 1.0):  # skip white
                mini_fill_colors[color_to_hex(c)] += 1
    
    for r in ref_rects:
        if r['op'] in ('fill', 'fill+stroke'):
            c = r.get('fill_color')
            if c and c != (1.0, 1.0, 1.0):
                ref_fill_colors[color_to_hex(c)] += 1
    
    print(f"  MINI non-white fill colors: {dict(mini_fill_colors.most_common(20))}")
    print(f"  REF  non-white fill colors: {dict(ref_fill_colors.most_common(20))}")
    
    # Colors in REF but not MINI
    ref_only = set(ref_fill_colors.keys()) - set(mini_fill_colors.keys())
    mini_only = set(mini_fill_colors.keys()) - set(ref_fill_colors.keys())
    if ref_only:
        print(f"  MISSING in MINI (REF-only colors): {ref_only}")
    if mini_only:
        print(f"  EXTRA in MINI (MINI-only colors): {mini_only}")
    
    # Compare stroke colors
    mini_stroke_colors = Counter()
    ref_stroke_colors = Counter()
    for r in mini_rects:
        if r['op'] in ('stroke', 'fill+stroke'):
            c = r.get('stroke_color')
            if c:
                mini_stroke_colors[color_to_hex(c)] += 1
    for r in ref_rects:
        if r['op'] in ('stroke', 'fill+stroke'):
            c = r.get('stroke_color')
            if c:
                ref_stroke_colors[color_to_hex(c)] += 1
    
    if mini_stroke_colors or ref_stroke_colors:
        print(f"  MINI stroke colors: {dict(mini_stroke_colors.most_common(10))}")
        print(f"  REF  stroke colors: {dict(ref_stroke_colors.most_common(10))}")
    
    # Compare rect positions (fill rects)
    mini_fills = [r for r in mini_rects if r['op'] in ('fill', 'fill+stroke') and r.get('fill_color') != (1.0, 1.0, 1.0)]
    ref_fills = [r for r in ref_rects if r['op'] in ('fill', 'fill+stroke') and r.get('fill_color') != (1.0, 1.0, 1.0)]
    print(f"  MINI fill rect count: {len(mini_fills)}")
    print(f"  REF  fill rect count: {len(ref_fills)}")
    
    # Compare line widths used
    mini_lw = Counter(r.get('line_width') for r in mini_rects if r.get('line_width'))
    ref_lw = Counter(r.get('line_width') for r in ref_rects if r.get('line_width'))
    if mini_lw or ref_lw:
        print(f"  MINI line widths: {dict(mini_lw)}")
        print(f"  REF  line widths: {dict(ref_lw)}")


def compare_text(mini_spans, ref_spans, label):
    """Compare text spans."""
    print(f"\n  --- {label}: Text Comparison ---")
    
    mini_texts = sorted([(s['text'], round(s['bbox'][0], 1), round(s['bbox'][1], 1)) for s in mini_spans])
    ref_texts = sorted([(s['text'], round(s['bbox'][0], 1), round(s['bbox'][1], 1)) for s in ref_spans])
    
    mini_text_set = set(s['text'] for s in mini_spans)
    ref_text_set = set(s['text'] for s in ref_spans)
    
    missing = ref_text_set - mini_text_set
    extra = mini_text_set - ref_text_set
    
    if missing:
        print(f"  MISSING text in MINI (in REF but not MINI): {sorted(missing)[:20]}")
    if extra:
        print(f"  EXTRA text in MINI (in MINI but not REF): {sorted(extra)[:20]}")
    if not missing and not extra:
        print(f"  All text content matches ({len(mini_text_set)} unique strings)")
    
    # Compare font sizes
    mini_sizes = Counter(round(s['size'], 1) for s in mini_spans)
    ref_sizes = Counter(round(s['size'], 1) for s in ref_spans)
    if mini_sizes != ref_sizes:
        print(f"  MINI font sizes: {dict(mini_sizes.most_common(10))}")
        print(f"  REF  font sizes: {dict(ref_sizes.most_common(10))}")


def analyze_file(name):
    print(f"\n{'='*70}")
    print(f"ANALYZING: {name}")
    print(f"{'='*70}")
    
    mini_path = f"{MINI_DIR}\\{name}.pdf"
    ref_path = f"{REF_DIR}\\{name}.pdf"
    
    try:
        mini_doc = fitz.open(mini_path)
        ref_doc = fitz.open(ref_path)
    except Exception as e:
        print(f"  ERROR opening files: {e}")
        return
    
    print(f"  MINI pages: {len(mini_doc)}, REF pages: {len(ref_doc)}")
    
    for pi in range(min(len(mini_doc), len(ref_doc))):
        print(f"\n  === Page {pi+1} ===")
        
        # Page dimensions
        mini_page = mini_doc[pi]
        ref_page = ref_doc[pi]
        print(f"  MINI page size: {mini_page.rect.width:.1f} x {mini_page.rect.height:.1f}")
        print(f"  REF  page size: {ref_page.rect.width:.1f} x {ref_page.rect.height:.1f}")
        
        # Extract streams
        mini_stream = extract_stream_ops(mini_doc, pi)
        ref_stream = extract_stream_ops(ref_doc, pi)
        
        # Extract detailed rects
        mini_rects = extract_detailed_rects(mini_stream)
        ref_rects = extract_detailed_rects(ref_stream)
        
        compare_fills(mini_rects, ref_rects, f"Page {pi+1}")
        
        # Extract and compare text
        mini_spans = extract_text_spans(mini_doc, pi)
        ref_spans = extract_text_spans(ref_doc, pi)
        compare_text(mini_spans, ref_spans, f"Page {pi+1}")
        
        # Show first few fill rects with positions for comparison
        mini_fills = [r for r in mini_rects if r['op'] in ('fill', 'fill+stroke') 
                      and r.get('fill_color') and r['fill_color'] != (1.0, 1.0, 1.0)]
        ref_fills = [r for r in ref_rects if r['op'] in ('fill', 'fill+stroke')
                     and r.get('fill_color') and r['fill_color'] != (1.0, 1.0, 1.0)]
        
        if mini_fills or ref_fills:
            print(f"\n  --- Sample MINI fill rects (first 15) ---")
            for r in mini_fills[:15]:
                print(f"    x={r['x']:.1f} y={r['y']:.1f} w={r['w']:.1f} h={r['h']:.1f} color={color_to_hex(r['fill_color'])}")
            print(f"  --- Sample REF fill rects (first 15) ---")
            for r in ref_fills[:15]:
                print(f"    x={r['x']:.1f} y={r['y']:.1f} w={r['w']:.1f} h={r['h']:.1f} color={color_to_hex(r['fill_color'])}")
    
    mini_doc.close()
    ref_doc.close()

# Also do pixel-level analysis if images exist
def pixel_analysis(name):
    try:
        from PIL import Image
        import numpy as np
    except ImportError:
        print("  (PIL/numpy not available for pixel analysis)")
        return
    
    report_dir = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reports\images"
    mini_img = f"{report_dir}\\{name}_p1_minipdf.png"
    ref_img = f"{report_dir}\\{name}_p1_reference.png"
    
    try:
        m = Image.open(mini_img).convert('RGB')
        r = Image.open(ref_img).convert('RGB')
    except Exception:
        # Render from PDF using fitz
        print("  Rendering from PDF for pixel comparison...")
        mini_path = f"{MINI_DIR}\\{name}.pdf"
        ref_path = f"{REF_DIR}\\{name}.pdf"
        md = fitz.open(mini_path)
        rd = fitz.open(ref_path)
        mat = fitz.Matrix(2, 2)  # 144 DPI
        mp = md[0].get_pixmap(matrix=mat, alpha=False)
        rp = rd[0].get_pixmap(matrix=mat, alpha=False)
        mp.save("_tmp_mini.png")
        rp.save("_tmp_ref.png")
        m = Image.open("_tmp_mini.png").convert('RGB')
        r = Image.open("_tmp_ref.png").convert('RGB')
        md.close()
        rd.close()
    
    w = min(m.width, r.width)
    h = min(m.height, r.height)
    m = m.crop((0, 0, w, h))
    r = r.crop((0, 0, w, h))
    ma = np.array(m, dtype=float)
    ra = np.array(r, dtype=float)
    
    diff = np.abs(ma - ra)
    sig = diff.max(axis=2) > 30
    
    total_pixels = w * h
    diff_pixels = sig.sum()
    print(f"\n  --- Pixel Analysis ---")
    print(f"  Image size: {w}x{h} ({total_pixels} pixels)")
    print(f"  Diff pixels (>30): {diff_pixels} ({diff_pixels/total_pixels*100:.3f}%)")
    
    if diff_pixels == 0:
        return
    
    # Categorize diffs
    ref_colored = (ra < 245).any(axis=2)
    mini_colored = (ma < 245).any(axis=2)
    
    missing_color = sig & ref_colored & ~mini_colored
    extra_color = sig & mini_colored & ~ref_colored
    color_diff = sig & ref_colored & mini_colored
    
    print(f"  Missing color (ref has, mini doesn't): {missing_color.sum()} px")
    print(f"  Extra color (mini has, ref doesn't): {extra_color.sum()} px")
    print(f"  Color mismatch (both colored): {color_diff.sum()} px")
    
    # Find diff regions (bounding boxes)
    rows_with_diff = np.where(sig.any(axis=1))[0]
    cols_with_diff = np.where(sig.any(axis=0))[0]
    if len(rows_with_diff) > 0:
        print(f"  Diff Y range: {rows_with_diff[0]} - {rows_with_diff[-1]}")
        print(f"  Diff X range: {cols_with_diff[0]} - {cols_with_diff[-1]}")
    
    # Sample diff regions  
    rows_sample = rows_with_diff[::max(1, len(rows_with_diff)//5)][:5]
    for sy in rows_sample:
        diff_cols = np.where(sig[sy])[0]
        if len(diff_cols) == 0:
            continue
        regions = []
        start = diff_cols[0]
        prev = diff_cols[0]
        for c in diff_cols[1:]:
            if c > prev + 5:
                regions.append((start, prev))
                start = c
            prev = c
        regions.append((start, prev))
        
        for rs, re in regions[:5]:
            mid = (rs + re) // 2
            mp = ma[sy, mid].astype(int)
            rp = ra[sy, mid].astype(int)
            print(f"    y={sy} x={rs}-{re} ({re-rs+1}px): mini=RGB({mp[0]},{mp[1]},{mp[2]}) ref=RGB({rp[0]},{rp[1]},{rp[2]})")


FILES = [
    "classic134_heatmap",
    "classic149_merged_styled_sections",
    "classic142_styled_invoice",
    "classic51_product_catalog",
]

for name in FILES:
    analyze_file(name)
    pixel_analysis(name)
