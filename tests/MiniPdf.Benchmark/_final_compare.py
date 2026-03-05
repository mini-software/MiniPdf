"""Final precise comparison of positions for each near-miss file."""
import fitz

MINI_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"

import re

def extract_rects_from_stream(stream_text):
    """Extract rect+fill_color from raw stream."""
    rects = []
    current_color = None
    lines = stream_text.split('\n')
    for line in lines:
        line = line.strip()
        m = re.match(r'^([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+rg$', line)
        if m:
            current_color = (float(m.group(1)), float(m.group(2)), float(m.group(3)))
            continue
        # Combined rect+fill: "x y w h re f*"
        m = re.match(r'^([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+re\s+f\*?$', line)
        if m:
            rects.append({
                'x': float(m.group(1)), 'y': float(m.group(2)),
                'w': float(m.group(3)), 'h': float(m.group(4)),
                'color': current_color
            })
            continue
        # Separate rect then fill
        m = re.match(r'^([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+re$', line)
        if m:
            pending = (float(m.group(1)), float(m.group(2)), float(m.group(3)), float(m.group(4)))
            continue
        if line in ('f', 'f*'):
            try:
                rects.append({
                    'x': pending[0], 'y': pending[1],
                    'w': pending[2], 'h': pending[3],
                    'color': current_color
                })
            except:
                pass
    return rects


def get_stream(doc, page_idx=0):
    page = doc[page_idx]
    parts = []
    for c in page.get_contents():
        parts.append(doc.xref_stream(c).decode('latin-1', errors='replace'))
    return "\n".join(parts)


# ============================================================
# classic134_heatmap: Compare fill positions precisely
# ============================================================
print("=" * 70)
print("classic134_heatmap: Position Comparison")
print("=" * 70)

ref = fitz.open(f"{REF_DIR}\\classic134_heatmap.pdf")
mini = fitz.open(f"{MINI_DIR}\\classic134_heatmap.pdf")

ref_rects = extract_rects_from_stream(get_stream(ref))
mini_rects = extract_rects_from_stream(get_stream(mini))

# Non-white rects only   
ref_fills = [r for r in ref_rects if r['color'] != (1.0, 1.0, 1.0) and r['color'] is not None]
mini_fills = [r for r in mini_rects if r['color'] != (1.0, 1.0, 1.0) and r['color'] is not None]

print(f"REF fill rects: {len(ref_fills)}")
print(f"MINI fill rects: {len(mini_fills)}")

# Compare rect by rect (they should be in same order)
if len(ref_fills) == len(mini_fills):
    for i in range(min(6, len(ref_fills))):
        rr = ref_fills[i]
        mr = mini_fills[i]
        dx = mr['x'] - rr['x']
        dy = mr['y'] - rr['y']
        dw = mr['w'] - rr['w']
        dh = mr['h'] - rr['h']
        # Color diff
        cr = rr['color']
        cm = mr['color']
        dc = tuple(round(cm[j] - cr[j], 4) for j in range(3))
        print(f"  Rect {i}: REF({rr['x']:.3f},{rr['y']:.3f},{rr['w']:.3f},{rr['h']:.3f}) "
              f"MINI({mr['x']:.3f},{mr['y']:.3f},{mr['w']:.3f},{mr['h']:.3f}) "
              f"Δ(x={dx:+.3f},y={dy:+.3f},w={dw:+.3f},h={dh:+.3f}) "
              f"ColorΔ={dc}")
    
    # Summarize X offsets
    x_offsets = [mini_fills[i]['x'] - ref_fills[i]['x'] for i in range(len(ref_fills))]
    print(f"\n  X offsets (MINI-REF): min={min(x_offsets):.3f} max={max(x_offsets):.3f} "
          f"avg={sum(x_offsets)/len(x_offsets):.3f}")
    
    # Unique X positions
    ref_xs = sorted(set(round(r['x'], 1) for r in ref_fills))
    mini_xs = sorted(set(round(r['x'], 1) for r in mini_fills))
    print(f"  REF column X positions: {ref_xs}")
    print(f"  MINI column X positions: {mini_xs}")
    
    # Column spacings
    ref_spacings = [ref_xs[i+1] - ref_xs[i] for i in range(len(ref_xs)-1)]
    mini_spacings = [mini_xs[i+1] - mini_xs[i] for i in range(len(mini_xs)-1)]
    print(f"  REF column spacings: {[round(s, 1) for s in ref_spacings]}")
    print(f"  MINI column spacings: {[round(s, 1) for s in mini_spacings]}")

ref.close()
mini.close()


# ============================================================
# classic149_merged_styled_sections: Position + border width comparison
# ============================================================
print("\n" + "=" * 70)
print("classic149_merged_styled_sections: Position & Border Comparison")
print("=" * 70)

ref = fitz.open(f"{REF_DIR}\\classic149_merged_styled_sections.pdf")
mini = fitz.open(f"{MINI_DIR}\\classic149_merged_styled_sections.pdf")

ref_rects = extract_rects_from_stream(get_stream(ref))
mini_rects = extract_rects_from_stream(get_stream(mini))

ref_fills = [r for r in ref_rects if r['color'] is not None]
mini_fills = [r for r in mini_rects if r['color'] is not None]

print(f"REF fills: {len(ref_fills)}")
for r in ref_fills:
    print(f"  x={r['x']:.3f} y={r['y']:.3f} w={r['w']:.3f} h={r['h']:.3f} color=({r['color'][0]:.4f},{r['color'][1]:.4f},{r['color'][2]:.4f})")
print(f"MINI fills: {len(mini_fills)}")
for r in mini_fills:
    print(f"  x={r['x']:.3f} y={r['y']:.3f} w={r['w']:.3f} h={r['h']:.3f} color=({r['color'][0]:.4f},{r['color'][1]:.4f},{r['color'][2]:.4f})")

# Position diffs for matching fills (same color)
for i in range(min(len(ref_fills), len(mini_fills))):
    rr = ref_fills[i]
    mr = mini_fills[i]
    print(f"  Fill {i}: Δx={mr['x']-rr['x']:+.3f} Δy={mr['y']-rr['y']:+.3f} "
          f"Δw={mr['w']-rr['w']:+.3f} Δh={mr['h']-rr['h']:+.3f}")

# Extract border line widths from stream
ref_str = get_stream(ref)
ref_lw = re.findall(r'([\d.]+)\s+w\b', ref_str)
mini_str = get_stream(mini)
mini_lw = re.findall(r'([\d.]+)\s+w\b', mini_str)
print(f"\nBorder line widths:")
print(f"  REF: {set(ref_lw)}")  
print(f"  MINI: {set(mini_lw)}")

ref.close()
mini.close()


# ============================================================
# classic142_styled_invoice: Fill rect analysis
# ============================================================
print("\n" + "=" * 70)
print("classic142_styled_invoice: Fill & Layout Comparison")
print("=" * 70)

ref = fitz.open(f"{REF_DIR}\\classic142_styled_invoice.pdf")
mini = fitz.open(f"{MINI_DIR}\\classic142_styled_invoice.pdf")

ref_rects = extract_rects_from_stream(get_stream(ref))
mini_rects = extract_rects_from_stream(get_stream(mini))

ref_fills = [r for r in ref_rects if r['color'] is not None]
mini_fills = [r for r in mini_rects if r['color'] is not None]

print(f"REF fill rects: {len(ref_fills)}")
for r in ref_fills:
    cstr = f"({r['color'][0]:.3f},{r['color'][1]:.3f},{r['color'][2]:.3f})" if r['color'] else "None"
    print(f"  x={r['x']:.3f} y={r['y']:.3f} w={r['w']:.3f} h={r['h']:.3f} color={cstr}")

print(f"\nMINI fill rects: {len(mini_fills)}")
for r in mini_fills:
    cstr = f"({r['color'][0]:.3f},{r['color'][1]:.3f},{r['color'][2]:.3f})" if r['color'] else "None"
    print(f"  x={r['x']:.3f} y={r['y']:.3f} w={r['w']:.3f} h={r['h']:.3f} color={cstr}")

# REF column positions from border lines
ref_str = get_stream(ref)
# Extract vertical line x-positions from "X Y1 m X Y2 l S" pattern
vert_lines_ref = re.findall(r'([\d.]+)\s+[\d.]+\s+m\s*\n\s*\1\s+[\d.]+\s+l\s+S', ref_str)
vert_lines_mini = re.findall(r'([\d.]+)\s+[\d.]+\s+m\s*\n\s*\1\s+[\d.]+\s+l\s+S', get_stream(mini))

print(f"\nREF vertical border X positions: {sorted(set(round(float(x), 1) for x in vert_lines_ref))}")
print(f"MINI vertical border X positions: {sorted(set(round(float(x), 1) for x in vert_lines_mini))}")

# Border widths
ref_lw = re.findall(r'([\d.]+)\s+w\b', ref_str)
mini_lw = re.findall(r'([\d.]+)\s+w\b', get_stream(mini))
print(f"REF line widths: {set(ref_lw)}")
print(f"MINI line widths: {set(mini_lw)}")

ref.close()
mini.close()


# ============================================================
# classic51_product_catalog: Column width analysis
# ============================================================
print("\n" + "=" * 70)
print("classic51_product_catalog: Column Width Analysis")
print("=" * 70)

ref = fitz.open(f"{REF_DIR}\\classic51_product_catalog.pdf")
mini = fitz.open(f"{MINI_DIR}\\classic51_product_catalog.pdf")

# Extract text spans with bounding boxes
def get_spans(doc):
    page = doc[0]
    data = page.get_text("dict", sort=True)
    spans = []
    for block in data.get("blocks", []):
        if block.get("type", 0) != 0:
            continue
        for line in block.get("lines", []):
            for span in line.get("spans", []):
                text = span.get("text", "").strip()
                if text:
                    spans.append((text, span["bbox"]))
    return spans

ref_spans = get_spans(ref)
mini_spans = get_spans(mini)

# Group spans by Y
def group_by_y(spans, tol=2):
    rows = []
    current_y = None
    current_row = []
    for text, bbox in sorted(spans, key=lambda s: (round(s[1][1], 0), s[1][0])):
        y = bbox[1]
        if current_y is None or abs(y - current_y) > tol:
            if current_row:
                rows.append(current_row)
            current_y = y
            current_row = [(text, bbox)]
        else:
            current_row.append((text, bbox))
    if current_row:
        rows.append(current_row)
    return rows

ref_rows = group_by_y(ref_spans)
mini_rows = group_by_y(mini_spans)

# Show column boundaries (X start of each span in first row)
for label, rows in [("REF", ref_rows), ("MINI", mini_rows)]:
    if rows:
        first = rows[0]
        xs = [round(bbox[0], 1) for _, bbox in first]
        x2s = [round(bbox[2], 1) for _, bbox in first]
        print(f"  {label} row0 X starts: {xs}")
        print(f"  {label} row0 X ends: {x2s}")
        widths = [round(bbox[2]-bbox[0], 1) for _, bbox in first]
        print(f"  {label} row0 span widths: {widths}")
        gap = [round(xs[i+1]-x2s[i], 1) for i in range(len(xs)-1)]
        print(f"  {label} row0 gaps between spans: {gap}")

# Check each row's second span (Name+Description combined)
print("\nPer-row column-1 (Name) span comparison:")
min_rows_count = min(len(ref_rows), len(mini_rows))
for i in range(min_rows_count):
    if len(ref_rows[i]) >= 2 and len(mini_rows[i]) >= 2:
        r_text, r_bbox = ref_rows[i][1]
        m_text, m_bbox = mini_rows[i][1]
        r_width = r_bbox[2] - r_bbox[0]
        m_width = m_bbox[2] - m_bbox[0]
        print(f"  Row {i}: MINI '{m_text}' w={m_width:.1f} | REF '{r_text}' w={r_width:.1f} | Δw={m_width-r_width:+.1f}")

ref.close()
mini.close()
