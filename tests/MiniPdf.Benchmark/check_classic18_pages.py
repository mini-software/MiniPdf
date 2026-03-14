"""Compare classic18 text positions page-by-page to see if drift accumulates."""
import fitz

mini_doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic18_large_dataset.pdf')
ref_doc = fitz.open('reference_pdfs/classic18_large_dataset.pdf')

for p in range(min(len(mini_doc), len(ref_doc))):
    mp = mini_doc[p]
    rp = ref_doc[p]
    
    # Get first text span on each page
    mblocks = mp.get_text("dict")["blocks"]
    rblocks = rp.get_text("dict")["blocks"]
    
    m_spans = []
    for b in mblocks:
        if b['type'] == 0:
            for l in b['lines']:
                for s in l['spans']:
                    m_spans.append(s)
    
    r_spans = []
    for b in rblocks:
        if b['type'] == 0:
            for l in b['lines']:
                for s in l['spans']:
                    r_spans.append(s)
    
    if m_spans and r_spans:
        # First text Y position
        my = m_spans[0]['bbox'][1]
        ry = r_spans[0]['bbox'][1]
        # Last text Y position
        my_last = m_spans[-1]['bbox'][1]
        ry_last = r_spans[-1]['bbox'][1]
        # First X
        mx = m_spans[0]['bbox'][0]
        rx = r_spans[0]['bbox'][0]
        # Last X
        mx_last = max(s['bbox'][2] for s in m_spans)
        rx_last = max(s['bbox'][2] for s in r_spans)
        
        print(f'Page {p+1:2d}: spans M={len(m_spans):3d} R={len(r_spans):3d} | '
              f'firstY M={my:.1f} R={ry:.1f} dY={my-ry:.1f} | '
              f'lastY M={my_last:.1f} R={ry_last:.1f} dY={my_last-ry_last:.1f} | '
              f'maxX M={mx_last:.1f} R={rx_last:.1f} dX={mx_last-rx_last:.1f}')

mini_doc.close()
ref_doc.close()
