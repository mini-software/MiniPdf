"""Compare visual scores per page for classic18, classic06, classic148, classic60, classic191
to identify which pages cause the biggest visual gap."""
import json
import fitz  # PyMuPDF

targets = ['classic18', 'classic06', 'classic148', 'classic60', 'classic191', 'classic142', 'classic137', 'classic187']

for target in targets:
    mini_path = f'../MiniPdf.Scripts/pdf_output/{target}.pdf'
    ref_path = f'reference_pdfs/{target}.pdf'
    
    try:
        mini_doc = fitz.open(mini_path)
        ref_doc = fitz.open(ref_path)
    except:
        print(f'{target}: MISSING')
        continue
    
    mp = len(mini_doc)
    rp = len(ref_doc)
    
    print(f'\n{target}: {mp} pages MiniPdf, {rp} pages Reference')
    
    # Compare page by page (up to min)
    for p in range(min(mp, rp)):
        # Get page dimensions
        mp_page = mini_doc[p]
        rp_page = ref_doc[p]
        
        mp_rect = mp_page.rect
        rp_rect = rp_page.rect
        
        # Render at 150 DPI for reasonable speed
        mat = fitz.Matrix(150/72, 150/72)
        mp_pix = mp_page.get_pixmap(matrix=mat)
        rp_pix = rp_page.get_pixmap(matrix=mat)
        
        # Compare using simple pixel difference
        mp_samples = mp_pix.samples
        rp_samples = rp_pix.samples
        
        # If dimensions differ, note it
        if mp_pix.width != rp_pix.width or mp_pix.height != rp_pix.height:
            print(f'  Page {p+1}: DIMENSION MISMATCH mini={mp_pix.width}x{mp_pix.height} ref={rp_pix.width}x{rp_pix.height}')
        else:
            # Compute SSIM-like metric (simple: mean absolute diff)
            total = len(mp_samples)
            diff_sum = 0
            for j in range(0, total, 100):  # sample every 100th byte for speed
                diff_sum += abs(mp_samples[j] - rp_samples[j])
            sampled = total // 100
            avg_diff = diff_sum / sampled if sampled > 0 else 0
            similarity = 1.0 - avg_diff / 255.0
            print(f'  Page {p+1}: approx_sim={similarity:.4f} (sampled pixel diff)')
    
    mini_doc.close()
    ref_doc.close()
