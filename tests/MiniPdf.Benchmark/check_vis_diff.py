"""Analyze visual differences for top improvement targets.
For each target, render both PDFs and find the regions with biggest differences."""
import fitz
import struct

targets = [
    'classic18_large_dataset',
    'classic148_frozen_styled_grid',
    'classic60_large_wide_table',
    'classic142_styled_invoice',
    'classic137_checkerboard',
    'classic06_tall_table',
    'classic187_bug_report_with_screenshots',
]

for target in targets:
    mini_path = f'../MiniPdf.Scripts/pdf_output/{target}.pdf'
    ref_path = f'reference_pdfs/{target}.pdf'
    
    try:
        mini_doc = fitz.open(mini_path)
        ref_doc = fitz.open(ref_path)
    except Exception as e:
        print(f'{target}: ERROR {e}')
        continue
    
    mp = len(mini_doc)
    rp = len(ref_doc)
    print(f'\n=== {target}: {mp} pages MiniPdf, {rp} pages Reference ===')
    
    # Only check page 1 for quick analysis
    for p in range(min(1, min(mp, rp))):
        mat = fitz.Matrix(2, 2)  # 144 DPI
        mp_pix = mini_doc[p].get_pixmap(matrix=mat)
        rp_pix = ref_doc[p].get_pixmap(matrix=mat)
        
        if mp_pix.width != rp_pix.width or mp_pix.height != rp_pix.height:
            print(f'  Page {p+1}: SIZE MISMATCH mini={mp_pix.width}x{mp_pix.height} ref={rp_pix.width}x{rp_pix.height}')
            continue
        
        w, h = mp_pix.width, mp_pix.height
        n = mp_pix.n  # components per pixel
        ms = mp_pix.samples
        rs = rp_pix.samples
        
        # Divide into 8x8 grid, compute diff per cell
        gw, gh = 8, 8
        cw = w // gw
        ch = h // gh
        
        diffs = []
        for gy in range(gh):
            for gx in range(gw):
                total_diff = 0
                count = 0
                for y in range(gy * ch, min((gy+1) * ch, h), 4):
                    for x in range(gx * cw, min((gx+1) * cw, w), 4):
                        idx = (y * w + x) * n
                        for c in range(min(n, 3)):
                            total_diff += abs(ms[idx+c] - rs[idx+c])
                            count += 1
                avg = total_diff / count if count > 0 else 0
                diffs.append((avg, gx, gy))
        
        diffs.sort(reverse=True)
        print(f'  Page {p+1} top diff regions (grid 8x8):')
        for diff, gx, gy in diffs[:5]:
            x1 = gx * cw // 2  # convert back to 72dpi coords
            y1 = gy * ch // 2
            x2 = (gx + 1) * cw // 2
            y2 = (gy + 1) * ch // 2
            print(f'    ({gx},{gy}) diff={diff:.1f}/255 region=({x1},{y1})-({x2},{y2})pt')
    
    mini_doc.close()
    ref_doc.close()
