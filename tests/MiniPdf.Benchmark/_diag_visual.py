"""Diagnostic script to analyze visual differences between minipdf and reference PDFs."""
from PIL import Image
import numpy as np

def analyze_file(name):
    print(f"\n{'='*60}")
    print(f"Analyzing: {name}")
    print(f"{'='*60}")
    m = Image.open(f'reports/images/{name}_p1_minipdf.png').convert('RGB')
    r = Image.open(f'reports/images/{name}_p1_reference.png').convert('RGB')
    w = min(m.width, r.width)
    h = min(m.height, r.height)
    m = m.crop((0, 0, w, h))
    r = r.crop((0, 0, w, h))
    ma = np.array(m, dtype=float)
    ra = np.array(r, dtype=float)
    
    # Get significant differences
    diff = np.abs(ma - ra)
    sig = diff.max(axis=2) > 30
    
    # For each row with diffs, categorize: is the diff in a "fill area" (colored bg)?
    # Check: in reference, is there a colored fill at that pixel? 
    # If reference has color but minipdf has white → missing fill
    # If minipdf has color but reference has white → extra fill
    
    ref_has_fill = (ra < 250).any(axis=2) & ~((ra > 200).all(axis=2))  # has some non-white, non-gray color
    mini_has_fill = (ma < 250).any(axis=2) & ~((ma > 200).all(axis=2))
    
    # Areas where reference has fill but minipdf doesn't (within diff areas)
    missing_fill = sig & ref_has_fill & ~mini_has_fill
    extra_fill = sig & mini_has_fill & ~ref_has_fill
    
    # Check column alignment - look at vertical lines in the reference
    # Find the vertical transition points (left edges of fills/borders)
    print(f"\nImage size: {w}x{h}")
    print(f"Total diff pixels (>30): {sig.sum()} ({sig.sum()/(w*h)*100:.2f}%)")
    
    # Sample a data row and analyze horizontal color transitions
    rows_with_diff = np.where(sig.any(axis=1))[0]
    if len(rows_with_diff) == 0:
        print("No significant differences found")
        return
    
    # Sample 3 rows
    samples = [rows_with_diff[len(rows_with_diff)//4], 
               rows_with_diff[len(rows_with_diff)//2],
               rows_with_diff[3*len(rows_with_diff)//4]]
    
    for sy in samples:
        print(f"\n--- Row y={sy} horizontal transitions ---")
        # Find columns where diff exists
        diff_cols = np.where(sig[sy])[0]
        if len(diff_cols) == 0:
            continue
        
        # Show diff regions
        regions = []
        start = diff_cols[0]
        prev = diff_cols[0]
        for c in diff_cols[1:]:
            if c > prev + 3:
                regions.append((start, prev))
                start = c
            prev = c
        regions.append((start, prev))
        
        for rx, (rs, re) in enumerate(regions[:8]):
            mid = (rs + re) // 2
            mp = ma[sy, mid].astype(int)
            rp = ra[sy, mid].astype(int)
            print(f"  x={rs}-{re} ({re-rs+1}px): mini=RGB({mp[0]},{mp[1]},{mp[2]}) ref=RGB({rp[0]},{rp[1]},{rp[2]})")
    
    # Analyze: what percentage of diffs are "fill gaps" vs "positional shifts"?
    # Fill gap: reference has a colored fill, we have white
    print(f"\nDiff category analysis:")
    print(f"  Missing fill (ref has color, mini white): {missing_fill.sum()} px")
    print(f"  Extra fill (mini has color, ref white): {extra_fill.sum()} px")
    print(f"  Other diffs (position/shade): {sig.sum() - missing_fill.sum() - extra_fill.sum()} px")
    
    # Check if fills are shifted horizontally
    # For each row, find the leftmost fill pixel in both
    data_rows = range(rows_with_diff[0], min(rows_with_diff[-1]+1, h), 3)
    x_shifts = []
    for y in data_rows:
        m_fills = np.where(ma[y, :, :].max(axis=1) < 240)[0]  # darker than near-white
        r_fills = np.where(ra[y, :, :].max(axis=1) < 240)[0]
        if len(m_fills) > 0 and len(r_fills) > 0:
            x_shifts.append(m_fills[0] - r_fills[0])
    if x_shifts:
        shifts = np.array(x_shifts)
        print(f"\nFill X-offset (mini-ref left edge): mean={shifts.mean():.1f}, median={np.median(shifts):.0f}, range=[{shifts.min()}, {shifts.max()}]")

    # Check vertical position offset
    # Find first filled row in both
    for x_test in range(150, 500, 100):
        m_col = ma[:, x_test, :].max(axis=1)
        r_col = ra[:, x_test, :].max(axis=1)
        m_filled_rows = np.where(m_col < 240)[0]
        r_filled_rows = np.where(r_col < 240)[0]
        if len(m_filled_rows) > 0 and len(r_filled_rows) > 0:
            y_shift = m_filled_rows[0] - r_filled_rows[0]
            if abs(y_shift) > 2:
                print(f"  Y-offset at x={x_test}: {y_shift}px (mini starts at y={m_filled_rows[0]}, ref at y={r_filled_rows[0]})")


for name in [
    'classic132_striped_table',
    'classic134_heatmap', 
    'classic137_checkerboard',
    'classic148_frozen_styled_grid',
    'classic142_styled_invoice',
    'classic149_merged_styled_sections',
    'classic128_font_sizes',
    'classic44_employee_roster',
    'classic150_kitchen_sink_styles',
]:
    try:
        analyze_file(name)
    except Exception as e:
        print(f"ERROR analyzing {name}: {e}")
