"""Quick benchmark for a single case. Compare text similarity and visual score."""
import fitz
import sys
from difflib import SequenceMatcher

try:
    from skimage.metrics import structural_similarity as ssim
    import numpy as np
    HAS_SSIM = True
except:
    HAS_SSIM = False

target = sys.argv[1] if len(sys.argv) > 1 else 'classic09_long_text'

mini_path = f'../MiniPdf.Scripts/pdf_output/{target}.pdf'
ref_path = f'reference_pdfs/{target}.pdf'

mini_doc = fitz.open(mini_path)
ref_doc = fitz.open(ref_path)

# Text similarity
m_text = ''
r_text = ''
for p in range(len(mini_doc)):
    m_text += mini_doc[p].get_text() + '\n'
for p in range(len(ref_doc)):
    r_text += ref_doc[p].get_text() + '\n'

text_sim = SequenceMatcher(None, m_text, r_text).ratio()
print(f'Text similarity: {text_sim:.4f} (M={len(m_text)} chars, R={len(r_text)} chars)')

# Visual score (SSIM per page)
vis_scores = []
if HAS_SSIM:
    for p in range(min(len(mini_doc), len(ref_doc))):
        mat = fitz.Matrix(2, 2)
        mp = mini_doc[p].get_pixmap(matrix=mat)
        rp = ref_doc[p].get_pixmap(matrix=mat)
        
        if mp.width != rp.width or mp.height != rp.height:
            w = min(mp.width, rp.width)
            h = min(mp.height, rp.height)
            m_arr = np.frombuffer(mp.samples, dtype=np.uint8).reshape(mp.height, mp.width, mp.n)[:h, :w, :3]
            r_arr = np.frombuffer(rp.samples, dtype=np.uint8).reshape(rp.height, rp.width, rp.n)[:h, :w, :3]
        else:
            m_arr = np.frombuffer(mp.samples, dtype=np.uint8).reshape(mp.height, mp.width, mp.n)[:, :, :3]
            r_arr = np.frombuffer(rp.samples, dtype=np.uint8).reshape(rp.height, rp.width, rp.n)[:, :, :3]
        
        score = ssim(m_arr, r_arr, channel_axis=2)
        vis_scores.append(score)
        if p < 3 or score < 0.95:
            print(f'  Page {p+1}: vis={score:.4f}')
    
    vis_avg = sum(vis_scores) / len(vis_scores)
    print(f'Visual avg: {vis_avg:.4f}')
else:
    vis_avg = 0
    print('  (skimage not available, skipping visual score)')
pm = 1.0 if len(mini_doc) == len(ref_doc) else 0.0
overall = text_sim * 0.4 + vis_avg * 0.4 + pm * 0.2
print(f'Visual avg: {vis_avg:.4f}')
print(f'Overall: {overall:.4f}')
print(f'Pages: M={len(mini_doc)} R={len(ref_doc)}')

mini_doc.close()
ref_doc.close()
