"""
Visual difference analysis between MiniPdf output and LibreOffice reference PDFs.
Analyzes drawings (paths) on specific pages to identify structural differences.
"""
import fitz  # PyMuPDF
import sys
from collections import Counter, defaultdict

MINIPDF_DIR = r"..\MiniPdf.Scripts\pdf_output"
REF_DIR = r"reference_pdfs"

CASES = [
    {
        "name": "classic18_large_dataset",
        "page": 0,  # page 1 (0-indexed)
        "description": "~20 pages, text_sim=1.0, visual_avg=0.8934",
    },
    {
        "name": "classic148_frozen_styled_grid",
        "page": 0,
        "description": "single page, text_sim=1.0, visual_avg=0.9339",
    },
]


def classify_drawing(item):
    """Classify a drawing item from page.get_drawings()."""
    items = item.get("items", [])
    fill = item.get("fill")        # fill color (tuple or None)
    color = item.get("color")      # stroke color (tuple or None)
    width = item.get("width") or 0  # stroke width
    rect = item.get("rect")        # bounding rect

    has_fill = fill is not None
    has_stroke = color is not None and width > 0

    # Count sub-path operations
    ops = [op[0] for op in items]
    n_lines = ops.count("l")
    n_rects = ops.count("re")
    n_curves = ops.count("c")
    n_quads = ops.count("qu")

    if n_rects > 0:
        if has_fill and has_stroke:
            return "filled_stroked_rect"
        elif has_fill:
            return "filled_rect"
        elif has_stroke:
            return "stroked_rect"
        else:
            return "rect_no_style"
    elif n_lines > 0 and n_curves == 0:
        if has_fill and has_stroke:
            return "filled_stroked_path"
        elif has_fill:
            return "filled_path"
        elif has_stroke:
            return "stroked_line"
        else:
            return "line_no_style"
    elif n_curves > 0:
        return "curve"
    else:
        return "other"


def color_key(c):
    """Round color tuple for grouping."""
    if c is None:
        return None
    return tuple(round(x, 3) for x in c)


def analyze_page(doc, page_idx):
    """Analyze all drawings on a single page."""
    page = doc[page_idx]
    drawings = page.get_drawings()

    result = {
        "page_width": page.rect.width,
        "page_height": page.rect.height,
        "total_drawings": len(drawings),
        "type_counts": Counter(),
        "fill_colors": Counter(),
        "stroke_colors": Counter(),
        "stroke_widths": Counter(),
        "fill_rects": [],         # (x0, y0, x1, y1, fill_color)
        "horiz_lines": [],        # (x0, y0, x1, y1, color, width)
        "vert_lines": [],         # (x0, y0, x1, y1, color, width)
        "other_lines": [],
        "op_counts": Counter(),   # raw operation counts
        "drawings_raw": drawings,
    }

    for d in drawings:
        dtype = classify_drawing(d)
        result["type_counts"][dtype] += 1

        fill = d.get("fill")
        color = d.get("color")
        width = d.get("width") or 0

        if fill is not None:
            result["fill_colors"][color_key(fill)] += 1
        if color is not None:
            result["stroke_colors"][color_key(color)] += 1
        if width > 0:
            result["stroke_widths"][round(width, 3)] += 1

        items = d.get("items", [])
        for op in items:
            result["op_counts"][op[0]] += 1

        rect = d.get("rect")
        # Collect filled rects
        if "rect" in dtype and fill is not None:
            result["fill_rects"].append((
                round(rect.x0, 2), round(rect.y0, 2),
                round(rect.x1, 2), round(rect.y1, 2),
                color_key(fill),
            ))

        # Collect lines
        if "line" in dtype or "stroked" in dtype:
            for op in items:
                if op[0] == "l":
                    p1, p2 = op[1], op[2]
                    x0, y0 = round(p1.x, 2), round(p1.y, 2)
                    x1, y1 = round(p2.x, 2), round(p2.y, 2)
                    if abs(y0 - y1) < 0.5:
                        result["horiz_lines"].append((x0, y0, x1, y1, color_key(color), round(width, 3)))
                    elif abs(x0 - x1) < 0.5:
                        result["vert_lines"].append((x0, y0, x1, y1, color_key(color), round(width, 3)))
                    else:
                        result["other_lines"].append((x0, y0, x1, y1, color_key(color), round(width, 3)))

    return result


def print_analysis(label, a):
    print(f"\n{'='*70}")
    print(f"  {label}")
    print(f"{'='*70}")
    print(f"  Page size: {a['page_width']:.2f} x {a['page_height']:.2f} pts")
    print(f"  Total drawing objects: {a['total_drawings']}")

    print(f"\n  --- Drawing type counts ---")
    for t, c in sorted(a["type_counts"].items(), key=lambda x: -x[1]):
        print(f"    {t:30s} : {c}")

    print(f"\n  --- Raw operation counts ---")
    for t, c in sorted(a["op_counts"].items(), key=lambda x: -x[1]):
        print(f"    {t:10s} : {c}")

    print(f"\n  --- Fill color distribution (top 15) ---")
    for col, c in a["fill_colors"].most_common(15):
        print(f"    {str(col):40s} : {c}")

    print(f"\n  --- Stroke color distribution (top 15) ---")
    for col, c in a["stroke_colors"].most_common(15):
        print(f"    {str(col):40s} : {c}")

    print(f"\n  --- Stroke width distribution ---")
    for w, c in sorted(a["stroke_widths"].items()):
        print(f"    width={w:<8} : {c}")

    print(f"\n  --- Line counts ---")
    print(f"    Horizontal lines: {len(a['horiz_lines'])}")
    print(f"    Vertical lines:   {len(a['vert_lines'])}")
    print(f"    Other lines:      {len(a['other_lines'])}")

    print(f"\n  --- Fill rects count: {len(a['fill_rects'])} ---")
    # Show first 10 fill rects
    if a["fill_rects"]:
        print(f"    First 10 fill rects (x0, y0, x1, y1, color):")
        for r in a["fill_rects"][:10]:
            print(f"      {r}")


def compare_fill_rects(mini_a, ref_a, label):
    """Compare fill rectangle patterns between MiniPdf and reference."""
    print(f"\n{'='*70}")
    print(f"  FILL RECT COMPARISON: {label}")
    print(f"{'='*70}")

    mini_rects = mini_a["fill_rects"]
    ref_rects = ref_a["fill_rects"]

    print(f"  MiniPdf fill rects: {len(mini_rects)}")
    print(f"  Reference fill rects: {len(ref_rects)}")

    # Group by color
    mini_by_color = defaultdict(list)
    ref_by_color = defaultdict(list)
    for r in mini_rects:
        mini_by_color[r[4]].append(r[:4])
    for r in ref_rects:
        ref_by_color[r[4]].append(r[:4])

    all_colors = set(mini_by_color.keys()) | set(ref_by_color.keys())
    print(f"\n  Fill rects grouped by color:")
    for col in sorted(all_colors, key=str):
        mc = len(mini_by_color.get(col, []))
        rc = len(ref_by_color.get(col, []))
        diff_str = ""
        if mc != rc:
            diff_str = f"  <-- DIFF (delta={mc - rc})"
        print(f"    color={str(col):40s}  MiniPdf={mc:5d}  Ref={rc:5d}{diff_str}")

    # Check if reference uses wider/row-spanning rects
    if ref_rects and mini_rects:
        mini_widths = [r[2] - r[0] for r in mini_rects]
        ref_widths = [r[2] - r[0] for r in ref_rects]
        mini_heights = [r[3] - r[1] for r in mini_rects]
        ref_heights = [r[3] - r[1] for r in ref_rects]

        print(f"\n  Fill rect width stats:")
        print(f"    MiniPdf: min={min(mini_widths):.2f}  max={max(mini_widths):.2f}  avg={sum(mini_widths)/len(mini_widths):.2f}")
        print(f"    Reference: min={min(ref_widths):.2f}  max={max(ref_widths):.2f}  avg={sum(ref_widths)/len(ref_widths):.2f}")
        print(f"\n  Fill rect height stats:")
        print(f"    MiniPdf: min={min(mini_heights):.2f}  max={max(mini_heights):.2f}  avg={sum(mini_heights)/len(mini_heights):.2f}")
        print(f"    Reference: min={min(ref_heights):.2f}  max={max(ref_heights):.2f}  avg={sum(ref_heights)/len(ref_heights):.2f}")

        # Check for row-wide fills in reference
        if len(ref_rects) < len(mini_rects):
            print(f"\n  Reference has FEWER fill rects ({len(ref_rects)} vs {len(mini_rects)}).")
            print(f"  This suggests reference may use row-wide fills instead of per-cell fills.")

            # Show unique Y positions (rows) for reference fills
            ref_y_positions = sorted(set(r[1] for r in ref_rects))
            mini_y_positions = sorted(set(r[1] for r in mini_rects))
            print(f"    Reference unique Y start positions: {len(ref_y_positions)}")
            print(f"    MiniPdf unique Y start positions: {len(mini_y_positions)}")


def compare_lines(mini_a, ref_a, label):
    """Compare line patterns between MiniPdf and reference."""
    print(f"\n{'='*70}")
    print(f"  LINE COMPARISON: {label}")
    print(f"{'='*70}")

    print(f"  Horizontal lines:  MiniPdf={len(mini_a['horiz_lines'])}  Ref={len(ref_a['horiz_lines'])}")
    print(f"  Vertical lines:    MiniPdf={len(mini_a['vert_lines'])}  Ref={len(ref_a['vert_lines'])}")
    print(f"  Other lines:       MiniPdf={len(mini_a['other_lines'])}  Ref={len(ref_a['other_lines'])}")

    # Compare line colors
    mini_hline_colors = Counter(l[4] for l in mini_a["horiz_lines"])
    ref_hline_colors = Counter(l[4] for l in ref_a["horiz_lines"])
    mini_vline_colors = Counter(l[4] for l in mini_a["vert_lines"])
    ref_vline_colors = Counter(l[4] for l in ref_a["vert_lines"])

    all_hcolors = set(mini_hline_colors.keys()) | set(ref_hline_colors.keys())
    if all_hcolors:
        print(f"\n  Horizontal line colors:")
        for col in sorted(all_hcolors, key=str):
            mc = mini_hline_colors.get(col, 0)
            rc = ref_hline_colors.get(col, 0)
            diff_str = ""
            if mc != rc:
                diff_str = f"  <-- DIFF"
            print(f"    color={str(col):40s}  MiniPdf={mc:5d}  Ref={rc:5d}{diff_str}")

    all_vcolors = set(mini_vline_colors.keys()) | set(ref_vline_colors.keys())
    if all_vcolors:
        print(f"\n  Vertical line colors:")
        for col in sorted(all_vcolors, key=str):
            mc = mini_vline_colors.get(col, 0)
            rc = ref_vline_colors.get(col, 0)
            diff_str = ""
            if mc != rc:
                diff_str = f"  <-- DIFF"
            print(f"    color={str(col):40s}  MiniPdf={mc:5d}  Ref={rc:5d}{diff_str}")

    # Compare line stroke widths
    mini_hline_widths = Counter(l[5] for l in mini_a["horiz_lines"])
    ref_hline_widths = Counter(l[5] for l in ref_a["horiz_lines"])
    mini_vline_widths = Counter(l[5] for l in mini_a["vert_lines"])
    ref_vline_widths = Counter(l[5] for l in ref_a["vert_lines"])

    all_hw = set(mini_hline_widths.keys()) | set(ref_hline_widths.keys())
    if all_hw:
        print(f"\n  Horizontal line widths:")
        for w in sorted(all_hw):
            mc = mini_hline_widths.get(w, 0)
            rc = ref_hline_widths.get(w, 0)
            diff_str = ""
            if mc != rc:
                diff_str = f"  <-- DIFF"
            print(f"    width={w:<8}  MiniPdf={mc:5d}  Ref={rc:5d}{diff_str}")

    all_vw = set(mini_vline_widths.keys()) | set(ref_vline_widths.keys())
    if all_vw:
        print(f"\n  Vertical line widths:")
        for w in sorted(all_vw):
            mc = mini_vline_widths.get(w, 0)
            rc = ref_vline_widths.get(w, 0)
            diff_str = ""
            if mc != rc:
                diff_str = f"  <-- DIFF"
            print(f"    width={w:<8}  MiniPdf={mc:5d}  Ref={rc:5d}{diff_str}")


def analyze_text(doc, page_idx):
    """Extract text block info for comparison."""
    page = doc[page_idx]
    blocks = page.get_text("dict", flags=fitz.TEXT_PRESERVE_WHITESPACE)["blocks"]
    text_blocks = [b for b in blocks if b["type"] == 0]
    total_spans = 0
    font_sizes = Counter()
    fonts = Counter()
    span_details = []  # (x0, y0, x1, y1, text, font, size)
    for block in text_blocks:
        for line in block["lines"]:
            for span in line["spans"]:
                total_spans += 1
                font_sizes[round(span["size"], 2)] += 1
                fonts[span["font"]] += 1
                bbox = span["bbox"]
                span_details.append((
                    round(bbox[0], 2), round(bbox[1], 2),
                    round(bbox[2], 2), round(bbox[3], 2),
                    span["text"][:30], span["font"], round(span["size"], 2)
                ))
    return {
        "text_blocks": len(text_blocks),
        "total_spans": total_spans,
        "font_sizes": font_sizes,
        "fonts": fonts,
        "span_details": span_details,
    }


def print_text_analysis(label, t):
    print(f"\n  --- Text info: {label} ---")
    print(f"    Text blocks: {t['text_blocks']}")
    print(f"    Total spans: {t['total_spans']}")
    print(f"    Font sizes (top 10):")
    for sz, c in t["font_sizes"].most_common(10):
        print(f"      {sz} pt : {c}")
    print(f"    Fonts used:")
    for f, c in t["fonts"].most_common(10):
        print(f"      {f} : {c}")


def additional_rect_analysis(mini_a, ref_a, label):
    """Deeper analysis of rectangles: are MiniPdf rects per-cell vs reference per-row?"""
    print(f"\n{'='*70}")
    print(f"  RECT PATTERN ANALYSIS: {label}")
    print(f"{'='*70}")

    for tag, rects in [("MiniPdf", mini_a["fill_rects"]), ("Reference", ref_a["fill_rects"])]:
        if not rects:
            print(f"  {tag}: no fill rects")
            continue
        # Group by Y coordinate (rows)
        by_y = defaultdict(list)
        for r in rects:
            by_y[r[1]].append(r)

        widths = [r[2] - r[0] for r in rects]
        print(f"\n  {tag}:")
        print(f"    Total fill rects: {len(rects)}")
        print(f"    Unique Y positions (rows): {len(by_y)}")
        print(f"    Avg rects per row: {len(rects)/max(len(by_y),1):.2f}")
        print(f"    Width range: {min(widths):.2f} - {max(widths):.2f}")

        # Show first 5 rows
        print(f"    First 5 row samples:")
        for i, (y, row_rects) in enumerate(sorted(by_y.items())[:5]):
            row_widths = [r[2] - r[0] for r in row_rects]
            row_colors = set(r[4] for r in row_rects)
            x_range = f"x=[{min(r[0] for r in row_rects):.1f}..{max(r[2] for r in row_rects):.1f}]"
            print(f"      y={y:.2f}: {len(row_rects)} rects, widths={[round(w,1) for w in row_widths]}, {x_range}, colors={row_colors}")


def analyze_content_stream(doc, page_idx):
    """Parse raw PDF content stream to find graphics operations."""
    page = doc[page_idx]
    # Get raw page content bytes
    xref = page.xref
    content = page.read_contents()
    if not content:
        return {"raw_length": 0, "ops": Counter(), "re_count": 0, "m_count": 0, "l_count": 0}

    text = content.decode("latin-1", errors="replace")
    lines = text.split("\n")

    ops = Counter()
    re_count = 0
    rg_colors = Counter()
    RG_colors = Counter()
    m_count = 0
    l_count = 0
    w_values = Counter()
    current_fill = None
    current_stroke = None
    fills = []
    strokes = []

    for line in lines:
        line = line.strip()
        if not line:
            continue
        parts = line.split()
        if not parts:
            continue
        op = parts[-1]
        ops[op] += 1

        if op == "re":
            re_count += 1
        elif op == "m":
            m_count += 1
        elif op == "l":
            l_count += 1
        elif op == "rg" and len(parts) >= 4:
            try:
                r, g, b = float(parts[0]), float(parts[1]), float(parts[2])
                rg_colors[(round(r,3), round(g,3), round(b,3))] += 1
                current_fill = (round(r,3), round(g,3), round(b,3))
            except:
                pass
        elif op == "RG" and len(parts) >= 4:
            try:
                r, g, b = float(parts[0]), float(parts[1]), float(parts[2])
                RG_colors[(round(r,3), round(g,3), round(b,3))] += 1
                current_stroke = (round(r,3), round(g,3), round(b,3))
            except:
                pass
        elif op == "w" and len(parts) >= 2:
            try:
                w_values[round(float(parts[0]), 3)] += 1
            except:
                pass
        elif op == "f" or op == "F" or op == "f*":
            if current_fill is not None:
                fills.append(current_fill)
        elif op == "S":
            if current_stroke is not None:
                strokes.append(current_stroke)

    return {
        "raw_length": len(content),
        "ops": ops,
        "re_count": re_count,
        "m_count": m_count,
        "l_count": l_count,
        "rg_colors": rg_colors,  # fill colors (non-stroking)
        "RG_colors": RG_colors,  # stroke colors
        "w_values": w_values,
        "fill_ops": Counter(fills),
        "stroke_ops": Counter(strokes),
    }


def print_content_stream_analysis(label, cs):
    print(f"\n{'='*70}")
    print(f"  CONTENT STREAM: {label}")
    print(f"{'='*70}")
    print(f"  Raw content length: {cs['raw_length']} bytes")
    print(f"  re (rect) ops: {cs['re_count']}")
    print(f"  m (moveto) ops: {cs['m_count']}")
    print(f"  l (lineto) ops: {cs['l_count']}")

    print(f"\n  Top 20 PDF operators:")
    for op, c in cs["ops"].most_common(20):
        print(f"    {op:10s} : {c}")

    print(f"\n  Fill colors (rg) set:")
    for col, c in cs["rg_colors"].most_common(20):
        print(f"    {str(col):40s} : {c}")

    print(f"\n  Stroke colors (RG) set:")
    for col, c in cs["RG_colors"].most_common(20):
        print(f"    {str(col):40s} : {c}")

    print(f"\n  Line widths (w) set:")
    for w, c in sorted(cs["w_values"].items()):
        print(f"    w={w:<8} : {c}")

    print(f"\n  Fill operations by color:")
    for col, c in cs["fill_ops"].most_common(20):
        print(f"    {str(col):40s} : {c}")

    print(f"\n  Stroke operations by color:")
    for col, c in cs["stroke_ops"].most_common(20):
        print(f"    {str(col):40s} : {c}")


def compare_text_positions(mini_t, ref_t, label):
    """Compare text span positions to find layout differences."""
    print(f"\n{'='*70}")
    print(f"  TEXT POSITION COMPARISON: {label}")
    print(f"{'='*70}")

    mini_spans = mini_t["span_details"]
    ref_spans = ref_t["span_details"]

    print(f"  MiniPdf spans: {len(mini_spans)}")
    print(f"  Reference spans: {len(ref_spans)}")

    # Compare first 20 spans by position
    n = min(20, len(mini_spans), len(ref_spans))
    if n > 0:
        print(f"\n  First {n} span positions (x0, y0, x1, y1, text...):")
        diffs = []
        for i in range(n):
            ms = mini_spans[i]
            rs = ref_spans[i]
            dx = ms[0] - rs[0]
            dy = ms[1] - rs[1]
            dw = (ms[2] - ms[0]) - (rs[2] - rs[0])
            if abs(dx) > 0.5 or abs(dy) > 0.5 or abs(dw) > 1.0:
                diffs.append(i)
                print(f"    [{i}] MiniPdf: ({ms[0]:7.2f},{ms[1]:7.2f},{ms[2]:7.2f},{ms[3]:7.2f}) '{ms[4]}' {ms[5]} {ms[6]}pt")
                print(f"    [{i}] Ref:     ({rs[0]:7.2f},{rs[1]:7.2f},{rs[2]:7.2f},{rs[3]:7.2f}) '{rs[4]}' {rs[5]} {rs[6]}pt")
                print(f"         dx={dx:.2f} dy={dy:.2f} dw={dw:.2f}")

        if not diffs:
            print(f"    All first {n} spans have similar positions (dx<0.5, dy<0.5)")

        # Overall position drift stats
        all_dx = []
        all_dy = []
        for i in range(min(len(mini_spans), len(ref_spans))):
            ms = mini_spans[i]
            rs = ref_spans[i]
            all_dx.append(ms[0] - rs[0])
            all_dy.append(ms[1] - rs[1])

        if all_dx:
            print(f"\n  Overall X drift: min={min(all_dx):.2f} max={max(all_dx):.2f} avg={sum(all_dx)/len(all_dx):.2f}")
            print(f"  Overall Y drift: min={min(all_dy):.2f} max={max(all_dy):.2f} avg={sum(all_dy)/len(all_dy):.2f}")


def main():
    import os
    os.chdir(os.path.dirname(os.path.abspath(__file__)))

    for case in CASES:
        name = case["name"]
        page_idx = case["page"]

        mini_path = os.path.join(MINIPDF_DIR, f"{name}.pdf")
        ref_path = os.path.join(REF_DIR, f"{name}.pdf")

        print(f"\n{'#'*70}")
        print(f"# CASE: {name}")
        print(f"# {case['description']}")
        print(f"{'#'*70}")

        mini_doc = fitz.open(mini_path)
        ref_doc = fitz.open(ref_path)

        print(f"\n  MiniPdf pages: {len(mini_doc)}")
        print(f"  Reference pages: {len(ref_doc)}")

        # Analyze drawings
        mini_a = analyze_page(mini_doc, page_idx)
        ref_a = analyze_page(ref_doc, page_idx)

        print_analysis(f"MiniPdf - {name} page {page_idx+1}", mini_a)
        print_analysis(f"Reference - {name} page {page_idx+1}", ref_a)

        # Comparisons
        if mini_a["page_width"] != ref_a["page_width"] or mini_a["page_height"] != ref_a["page_height"]:
            print(f"\n  *** PAGE SIZE DIFFERS ***")
            print(f"    MiniPdf:   {mini_a['page_width']:.2f} x {mini_a['page_height']:.2f}")
            print(f"    Reference: {ref_a['page_width']:.2f} x {ref_a['page_height']:.2f}")

        compare_fill_rects(mini_a, ref_a, name)
        compare_lines(mini_a, ref_a, name)
        additional_rect_analysis(mini_a, ref_a, name)

        # Content stream analysis (raw PDF operators)
        mini_cs = analyze_content_stream(mini_doc, page_idx)
        ref_cs = analyze_content_stream(ref_doc, page_idx)
        print_content_stream_analysis(f"MiniPdf - {name}", mini_cs)
        print_content_stream_analysis(f"Reference - {name}", ref_cs)

        # Text analysis
        mini_t = analyze_text(mini_doc, page_idx)
        ref_t = analyze_text(ref_doc, page_idx)
        print_text_analysis(f"MiniPdf - {name}", mini_t)
        print_text_analysis(f"Reference - {name}", ref_t)

        # Text position comparison
        compare_text_positions(mini_t, ref_t, name)

        mini_doc.close()
        ref_doc.close()


if __name__ == "__main__":
    main()
