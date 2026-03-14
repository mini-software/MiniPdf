"""Look for cases where text_sim could be improved by fixing specific issues.
Focus on cases where the text is close but has small systematic errors."""
import json
import os
import fitz
import difflib

with open('reports/comparison_report.json', encoding='utf-8') as f:
    results = json.load(f)

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'

# Cases with text_sim between 0.95 and 1.0 (almost perfect but not quite)
near_perfect = [(r['name'], r.get('text_similarity', 1.0)) for r in results 
                if 0.95 <= r.get('text_similarity', 1.0) < 1.0]
near_perfect.sort(key=lambda x: x[1])

print(f'Cases with 0.95 <= text_sim < 1.0: {len(near_perfect)}')
for name, ts in near_perfect:
    mini_path = os.path.join(mini_dir, f'{name}.pdf')
    ref_path = os.path.join(ref_dir, f'{name}.pdf')
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        continue
    
    mini_doc = fitz.open(mini_path)
    ref_doc = fitz.open(ref_path)
    
    mini_text = '\n'.join(p.get_text() for p in mini_doc)
    ref_text = '\n'.join(p.get_text() for p in ref_doc)
    
    # Count specific types of differences
    seq = difflib.SequenceMatcher(None, mini_text, ref_text)
    
    space_diffs = 0
    merge_diffs = 0
    trunc_diffs = 0
    other_diffs = 0
    
    for tag, i1, i2, j1, j2 in seq.get_opcodes():
        if tag == 'equal':
            continue
        mini_part = mini_text[i1:i2]
        ref_part = ref_text[j1:j2]
        
        if mini_part.strip() == '' and ref_part.strip() == '':
            space_diffs += 1
        elif ref_part.strip() == '' or mini_part.strip() == '':
            space_diffs += 1
        elif '\n' in ref_part and '\n' not in mini_part:
            merge_diffs += 1
        elif '\n' in mini_part and '\n' not in ref_part:
            merge_diffs += 1
        elif len(mini_part) != len(ref_part) and (len(mini_part) <= 3 or len(ref_part) <= 3):
            trunc_diffs += 1
        else:
            other_diffs += 1
    
    total = space_diffs + merge_diffs + trunc_diffs + other_diffs
    print(f'  {name:45s} ts={ts:.4f} diffs={total} space={space_diffs} merge={merge_diffs} trunc={trunc_diffs} other={other_diffs}')
    
    mini_doc.close()
    ref_doc.close()
