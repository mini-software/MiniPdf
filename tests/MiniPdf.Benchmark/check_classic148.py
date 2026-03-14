"""Compare classic148 (frozen_styled_grid) row heights and column widths to understand visual gap."""
import fitz

for label, path in [('MiniPdf', '../MiniPdf.Scripts/pdf_output/classic148_frozen_styled_grid.pdf'),
                     ('Ref', 'reference_pdfs/classic148_frozen_styled_grid.pdf')]:
    doc = fitz.open(path)
    page = doc[0]
    print(f'\n=== {label} ===')
    print(f'Page size: {page.rect}')
    
    # Get all text spans
    blocks = page.get_text("dict")["blocks"]
    text_spans = []
    for b in blocks:
        if b['type'] == 0:
            for line in b['lines']:
                for span in line['spans']:
                    text_spans.append(span)
    
    print(f'Total text spans: {len(text_spans)}')
    
    # Group by Y position (rows)
    y_groups = {}
    for s in text_spans:
        y = round(s['bbox'][1], 0)
        if y not in y_groups:
            y_groups[y] = []
        y_groups[y].append(s)
    
    print(f'Distinct Y positions (rows): {len(y_groups)}')
    ys = sorted(y_groups.keys())
    for y in ys[:15]:
        texts = [s['text'][:15] for s in y_groups[y]]
        xs = [f"{s['bbox'][0]:.1f}" for s in y_groups[y]]
        print(f'  y={y:.0f}: {len(y_groups[y])} spans, first_x=[{",".join(xs[:5])}] texts={texts[:5]}')
    
    # Get fill rects
    draws = page.get_drawings()
    fills = [d for d in draws if d.get('fill') and len(d.get('items',[])) == 1 and d['items'][0][0] == 're']
    
    # Row heights from fills
    y_vals = set()
    for d in fills:
        r = d['items'][0][1]
        y_vals.add(round(r.y0, 1))
        y_vals.add(round(r.y1, 1))
    y_sorted = sorted(y_vals)
    row_heights = [y_sorted[i+1] - y_sorted[i] for i in range(min(15, len(y_sorted)-1))]
    print(f'Row height pattern from fills: {[f"{h:.1f}" for h in row_heights]}')
    
    # Column positions from fills in first row
    first_row_fills = [d for d in fills if abs(d['items'][0][1].y0 - y_sorted[0]) < 1]
    col_xs = sorted(set(round(d['items'][0][1].x0, 1) for d in first_row_fills))
    col_widths = [col_xs[i+1] - col_xs[i] for i in range(min(10, len(col_xs)-1))]
    print(f'Column X positions: {[f"{x:.1f}" for x in col_xs[:10]]}')
    print(f'Column widths from fills: {[f"{w:.1f}" for w in col_widths]}')
    
    # Table extent
    all_x0 = min(d['items'][0][1].x0 for d in fills)
    all_x1 = max(d['items'][0][1].x1 for d in fills)
    all_y0 = min(d['items'][0][1].y0 for d in fills)
    all_y1 = max(d['items'][0][1].y1 for d in fills)
    print(f'Table extent: ({all_x0:.1f},{all_y0:.1f})-({all_x1:.1f},{all_y1:.1f}) = {all_x1-all_x0:.1f}w x {all_y1-all_y0:.1f}h')
    
    doc.close()
