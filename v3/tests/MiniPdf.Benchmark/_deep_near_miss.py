"""Deep analysis: examine REF PDF streams to understand how LibreOffice renders fills,
and analyze exact text differences for classic51."""
import fitz
import re
import sys

MINI_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\pdf_output"
REF_DIR = r"D:\git\MiniPdf\tests\MiniPdf.Benchmark\reference_pdfs"


def dump_stream_summary(doc, page_idx=0, label=""):
    """Dump a summary of stream operators."""
    page = doc[page_idx]
    contents = page.get_contents()
    all_text = []
    for c in contents:
        stream = doc.xref_stream(c)
        text = stream.decode('latin-1', errors='replace')
        all_text.append(text)
    full = "\n".join(all_text)
    lines = full.split('\n')
    
    # Count operator types
    ops = {}
    for line in lines:
        line = line.strip()
        if not line:
            continue
        # Get operator (last word)
        parts = line.split()
        if parts:
            op = parts[-1]
            ops[op] = ops.get(op, 0) + 1
    
    print(f"\n  {label} Stream operators ({len(lines)} lines):")
    for op, count in sorted(ops.items(), key=lambda x: -x[1])[:30]:
        print(f"    {op}: {count}")
    
    return full


def analyze_ref_fills(stream_text, label):
    """Look for how the reference PDF renders colored areas."""
    lines = stream_text.split('\n')
    
    # Find all color setting operations
    colors_rg = []
    colors_RG = []
    colors_g = []
    colors_scn = []
    colors_sc = []
    
    for i, line in enumerate(lines):
        line = line.strip()
        m = re.match(r'^([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+rg$', line)
        if m:
            colors_rg.append((i, float(m.group(1)), float(m.group(2)), float(m.group(3))))
        m = re.match(r'^([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+RG$', line)
        if m:
            colors_RG.append((i, float(m.group(1)), float(m.group(2)), float(m.group(3))))
        m = re.match(r'^([\d.]+)\s+g$', line)
        if m:
            colors_g.append((i, float(m.group(1))))
        if 'scn' in line.lower() or 'SCN' in line:
            colors_scn.append((i, line))
        if line.endswith(' sc') or line.endswith(' SC'):
            colors_sc.append((i, line))
    
    print(f"\n  {label} Color operations:")
    print(f"    rg (fill RGB): {len(colors_rg)}")
    if colors_rg:
        for idx, r, g, b in colors_rg[:10]:
            print(f"      line {idx}: ({r:.3f},{g:.3f},{b:.3f})")
    print(f"    RG (stroke RGB): {len(colors_RG)}")
    print(f"    g (fill gray): {len(colors_g)}")
    print(f"    scn/SCN: {len(colors_scn)}")
    if colors_scn:
        for idx, line in colors_scn[:10]:
            print(f"      line {idx}: {line}")
    print(f"    sc/SC: {len(colors_sc)}")
    if colors_sc:
        for idx, line in colors_sc[:10]:
            print(f"      line {idx}: {line}")
    
    # Find path operations (m, l, c) - LibreOffice may use paths instead of re
    path_ops = {'m': 0, 'l': 0, 'c': 0, 're': 0, 'h': 0}
    for line in lines:
        line = line.strip()
        parts = line.split()
        if parts:
            op = parts[-1]
            if op in path_ops:
                path_ops[op] += 1
    print(f"    Path ops: {path_ops}")
    
    # Find fill/stroke after paths
    draw_ops = {'f': 0, 'f*': 0, 'F': 0, 'S': 0, 'B': 0, 'B*': 0, 'n': 0, 'W': 0, 'W*': 0}
    for line in lines:
        line = line.strip()
        if line in draw_ops:
            draw_ops[line] += 1
    print(f"    Draw ops: {draw_ops}")
    
    # Show all 're' (rectangle) operations with context
    re_count = 0
    for i, line in enumerate(lines):
        line = line.strip()
        if re.match(r'^[\d.-]+\s+[\d.-]+\s+[\d.-]+\s+[\d.-]+\s+re$', line):
            re_count += 1
            if re_count <= 20:
                # Show context: 3 lines before and 3 after
                start = max(0, i-3)
                end = min(len(lines), i+4)
                ctx = [f"      {'>' if j==i else ' '} {lines[j].strip()}" for j in range(start, end)]
                print(f"    rect #{re_count} at line {i}:")
                print('\n'.join(ctx))
    print(f"    Total re ops: {re_count}")


def analyze_text_details(name):
    """Deep text comparison for classic51_product_catalog."""
    print(f"\n{'='*70}")
    print(f"TEXT DETAIL: {name}")
    print(f"{'='*70}")
    
    mini_doc = fitz.open(f"{MINI_DIR}\\{name}.pdf")
    ref_doc = fitz.open(f"{REF_DIR}\\{name}.pdf")
    
    mini_page = mini_doc[0]
    ref_page = ref_doc[0]
    
    # Get detailed text dict
    mini_data = mini_page.get_text("dict", sort=True)
    ref_data = ref_page.get_text("dict", sort=True)
    
    mini_spans = []
    ref_spans = []
    
    for block in mini_data.get("blocks", []):
        if block.get("type", 0) != 0:
            continue
        for line in block.get("lines", []):
            for span in line.get("spans", []):
                text = span.get("text", "").strip()
                if text:
                    mini_spans.append({
                        'text': text,
                        'x': round(span["bbox"][0], 1),
                        'y': round(span["bbox"][1], 1),
                        'x2': round(span["bbox"][2], 1),
                        'y2': round(span["bbox"][3], 1),
                        'font': span.get("font", ""),
                        'size': round(span.get("size", 0), 1),
                    })
    
    for block in ref_data.get("blocks", []):
        if block.get("type", 0) != 0:
            continue
        for line in block.get("lines", []):
            for span in line.get("spans", []):
                text = span.get("text", "").strip()
                if text:
                    ref_spans.append({
                        'text': text,
                        'x': round(span["bbox"][0], 1),
                        'y': round(span["bbox"][1], 1),
                        'x2': round(span["bbox"][2], 1),
                        'y2': round(span["bbox"][3], 1),
                        'font': span.get("font", ""),
                        'size': round(span.get("size", 0), 1),
                    })
    
    # Sort by Y then X
    mini_spans.sort(key=lambda s: (s['y'], s['x']))
    ref_spans.sort(key=lambda s: (s['y'], s['x']))
    
    print(f"\n  MINI spans: {len(mini_spans)}")
    print(f"  REF  spans: {len(ref_spans)}")
    
    # Group by approximate rows (Y within 2pt)
    def group_by_row(spans):
        rows = []
        current_y = None
        current_row = []
        for s in spans:
            if current_y is None or abs(s['y'] - current_y) > 2:
                if current_row:
                    rows.append((current_y, current_row))
                current_y = s['y']
                current_row = [s]
            else:
                current_row.append(s)
        if current_row:
            rows.append((current_y, current_row))
        return rows
    
    mini_rows = group_by_row(mini_spans)
    ref_rows = group_by_row(ref_spans)
    
    print(f"\n  MINI rows: {len(mini_rows)}")
    print(f"  REF  rows: {len(ref_rows)}")
    
    # Compare row by row
    min_rows = min(len(mini_rows), len(ref_rows))
    for i in range(min_rows):
        my, mspans = mini_rows[i]
        ry, rspans = ref_rows[i]
        
        m_texts = [s['text'] for s in mspans]
        r_texts = [s['text'] for s in rspans]
        
        m_joined = " | ".join(m_texts)
        r_joined = " | ".join(r_texts)
        
        if m_joined != r_joined:
            print(f"\n  Row {i} (MINI y={my:.1f}, REF y={ry:.1f}):")
            print(f"    MINI: {m_joined}")
            print(f"    REF:  {r_joined}")
            # Show per-span details
            for j, s in enumerate(mspans):
                print(f"    MINI span {j}: '{s['text']}' x=[{s['x']},{s['x2']}] w={s['x2']-s['x']:.1f}")
            for j, s in enumerate(rspans):
                print(f"    REF  span {j}: '{s['text']}' x=[{s['x']},{s['x2']}] w={s['x2']-s['x']:.1f}")
    
    mini_doc.close()
    ref_doc.close()


# ===== MAIN =====

# 1. Analyze classic134_heatmap REF stream
print("="*70)
print("DEEP ANALYSIS: classic134_heatmap REF PDF stream")
print("="*70)
ref_doc = fitz.open(f"{REF_DIR}\\classic134_heatmap.pdf")
ref_stream = dump_stream_summary(ref_doc, 0, "REF heatmap")
analyze_ref_fills(ref_stream, "REF heatmap")
ref_doc.close()

mini_doc = fitz.open(f"{MINI_DIR}\\classic134_heatmap.pdf")
mini_stream = dump_stream_summary(mini_doc, 0, "MINI heatmap")
mini_doc.close()

# 2. Analyze classic149_merged_styled_sections REF stream
print("\n" + "="*70)
print("DEEP ANALYSIS: classic149_merged_styled_sections REF PDF stream")
print("="*70)
ref_doc = fitz.open(f"{REF_DIR}\\classic149_merged_styled_sections.pdf")
ref_stream = dump_stream_summary(ref_doc, 0, "REF merged_styled")
analyze_ref_fills(ref_stream, "REF merged_styled")
ref_doc.close()

mini_doc = fitz.open(f"{MINI_DIR}\\classic149_merged_styled_sections.pdf")
mini_stream = dump_stream_summary(mini_doc, 0, "MINI merged_styled")
mini_doc.close()

# 3. Analyze classic142_styled_invoice REF stream
print("\n" + "="*70)
print("DEEP ANALYSIS: classic142_styled_invoice REF PDF stream")
print("="*70)
ref_doc = fitz.open(f"{REF_DIR}\\classic142_styled_invoice.pdf")
ref_stream = dump_stream_summary(ref_doc, 0, "REF styled_invoice")
analyze_ref_fills(ref_stream, "REF styled_invoice")
ref_doc.close()

mini_doc = fitz.open(f"{MINI_DIR}\\classic142_styled_invoice.pdf")
mini_stream = dump_stream_summary(mini_doc, 0, "MINI styled_invoice")
mini_doc.close()

# 4. Text detail for classic51_product_catalog
analyze_text_details("classic51_product_catalog")
