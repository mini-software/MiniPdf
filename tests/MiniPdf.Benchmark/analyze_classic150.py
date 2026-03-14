"""Investigate classic150_kitchen_sink_styles - text identical but text_sim < 1."""
import fitz
import os
import difflib

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'
name = 'classic150_kitchen_sink_styles'

mini_doc = fitz.open(os.path.join(mini_dir, f'{name}.pdf'))
ref_doc = fitz.open(os.path.join(ref_dir, f'{name}.pdf'))

print(f'Pages: mini={len(mini_doc)}, ref={len(ref_doc)}')

# Page-aware comparison
for pi in range(max(len(mini_doc), len(ref_doc))):
    mt = mini_doc[pi].get_text() if pi < len(mini_doc) else ''
    rt = ref_doc[pi].get_text() if pi < len(ref_doc) else ''
    seq = difflib.SequenceMatcher(None, mt, rt)
    print(f'\nPage {pi+1}: mini_len={len(mt)}, ref_len={len(rt)}, ratio={seq.ratio():.4f}')
    
    if seq.ratio() < 1.0:
        for tag, i1, i2, j1, j2 in seq.get_opcodes():
            if tag == 'equal':
                continue
            print(f'  {tag}: mini={repr(mt[i1:i2][:60])} ref={repr(rt[j1:j2][:60])}')

# Compare flat text
mini_flat = '\n'.join(p.get_text() for p in mini_doc)
ref_flat = '\n'.join(p.get_text() for p in ref_doc)
seq = difflib.SequenceMatcher(None, mini_flat, ref_flat)
print(f'\nFlat text ratio: {seq.ratio():.4f}')
print(f'Mini chars: {len(mini_flat)}, Ref chars: {len(ref_flat)}')

# Word-level comparison
mini_words = mini_flat.split()
ref_words = ref_flat.split()
wseq = difflib.SequenceMatcher(None, mini_words, ref_words)
print(f'Word ratio: {wseq.ratio():.4f}')
print(f'Mini words: {len(mini_words)}, Ref words: {len(ref_words)}')

# Page-aware combined check
page_ratios = []
for pi in range(max(len(mini_doc), len(ref_doc))):
    mt = mini_doc[pi].get_text() if pi < len(mini_doc) else ''
    rt = ref_doc[pi].get_text() if pi < len(ref_doc) else ''
    seq = difflib.SequenceMatcher(None, mt, rt)
    page_ratios.append(seq.ratio())
print(f'\nPage ratios: {page_ratios}')
print(f'Page avg: {sum(page_ratios)/len(page_ratios):.4f}')

# text_similarity = max(page_aware, flat, word_level)
# page_aware = avg of per-page ratios
page_aware = sum(page_ratios)/len(page_ratios) if page_ratios else 0
print(f'\ntext_similarity = max(page_aware={page_aware:.4f}, flat={seq.ratio():.4f}, word={wseq.ratio():.4f})')
print(f'  → {max(page_aware, seq.ratio(), wseq.ratio()):.4f}')

mini_doc.close()
ref_doc.close()
