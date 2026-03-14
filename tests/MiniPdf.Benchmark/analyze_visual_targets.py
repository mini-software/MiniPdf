"""Check visual differences for key cases - what's different visually."""
import fitz
import os

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'

cases = [
    'classic187_bug_report_with_screenshots',
    'classic142_styled_invoice',
    'classic149_merged_styled_sections',
    'classic134_heatmap',
]

for name in cases:
    mini_path = os.path.join(mini_dir, f'{name}.pdf')
    ref_path = os.path.join(ref_dir, f'{name}.pdf')
    
    mini_doc = fitz.open(mini_path)
    ref_doc = fitz.open(ref_path)
    
    print(f'\n=== {name} ===')
    print(f'  Pages: mini={len(mini_doc)}, ref={len(ref_doc)}')
    
    for pi in range(min(len(mini_doc), len(ref_doc))):
        mp = mini_doc[pi]
        rp = ref_doc[pi]
        
        # Get page dimensions
        mini_rect = mp.rect
        ref_rect = rp.rect
        print(f'  Page {pi+1}: mini=({mini_rect.width:.0f}x{mini_rect.height:.0f}), ref=({ref_rect.width:.0f}x{ref_rect.height:.0f})')
        
        # Compare text blocks positions
        m_blocks = mp.get_text("blocks")
        r_blocks = rp.get_text("blocks")
        
        m_text_blocks = [b for b in m_blocks if b[6] == 0]
        r_text_blocks = [b for b in r_blocks if b[6] == 0]
        m_img_blocks = [b for b in m_blocks if b[6] == 1]
        r_img_blocks = [b for b in r_blocks if b[6] == 1]
        
        print(f'    Text blocks: mini={len(m_text_blocks)}, ref={len(r_text_blocks)}')
        print(f'    Image blocks: mini={len(m_img_blocks)}, ref={len(r_img_blocks)}')
        
        # Compare rightmost extent of text
        if m_text_blocks:
            m_max_x = max(b[2] for b in m_text_blocks)
            m_max_y = max(b[3] for b in m_text_blocks)
        else:
            m_max_x = m_max_y = 0
            
        if r_text_blocks:
            r_max_x = max(b[2] for b in r_text_blocks)
            r_max_y = max(b[3] for b in r_text_blocks)
        else:
            r_max_x = r_max_y = 0
        
        print(f'    Text extents: mini=({m_max_x:.0f},{m_max_y:.0f}), ref=({r_max_x:.0f},{r_max_y:.0f}), dx={m_max_x-r_max_x:.0f}')
        
        # Compare image positions if any
        for i, (mb, rb) in enumerate(zip(m_img_blocks, r_img_blocks)):
            m_rect = (mb[0], mb[1], mb[2], mb[3])
            r_rect = (rb[0], rb[1], rb[2], rb[3])
            dx = m_rect[0] - r_rect[0]
            dy = m_rect[1] - r_rect[1]
            dw = (m_rect[2]-m_rect[0]) - (r_rect[2]-r_rect[0])
            dh = (m_rect[3]-m_rect[1]) - (r_rect[3]-r_rect[1])
            if abs(dx) > 2 or abs(dy) > 2 or abs(dw) > 2 or abs(dh) > 2:
                print(f'    Img {i}: mini=({m_rect[0]:.0f},{m_rect[1]:.0f},{m_rect[2]:.0f},{m_rect[3]:.0f}), ref=({r_rect[0]:.0f},{r_rect[1]:.0f},{r_rect[2]:.0f},{r_rect[3]:.0f})')
                print(f'            dx={dx:.0f} dy={dy:.0f} dw={dw:.0f} dh={dh:.0f}')
    
    mini_doc.close()
    ref_doc.close()
