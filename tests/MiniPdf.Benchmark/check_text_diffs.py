"""Detailed text comparison for key cases to find fixable patterns."""
import fitz, os
from difflib import SequenceMatcher

cases = [
    'classic189_alternating_image_text_rows',
    'classic182_dense_long_text_columns', 
    'classic44_employee_roster',
    'classic49_contact_list',
    'classic09_long_text',
    'classic140_rotated_text',
    'classic191_payroll_calculator',
]

for case in cases:
    mini = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', f'{case}.pdf')
    ref = os.path.join('reference_pdfs', f'{case}.pdf')
    if not os.path.exists(mini) or not os.path.exists(ref):
        print(f"SKIP {case}")
        continue
    
    md = fitz.open(mini)
    rd = fitz.open(ref)
    
    # Compare page 1 text
    mt = md[0].get_text("text")
    rt = rd[0].get_text("text")
    
    ratio = SequenceMatcher(None, mt, rt).ratio()
    
    print(f"\n{'='*60}")
    print(f"{case}  text_ratio={ratio:.4f}  pages_m={len(md)}  pages_r={len(rd)}")
    
    # Find specific word-level diffs
    mwords = mt.split()
    rwords = rt.split()
    
    sm = SequenceMatcher(None, mwords, rwords)
    diffs = []
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag != 'equal':
            mpart = ' '.join(mwords[i1:i2])[:60]
            rpart = ' '.join(rwords[j1:j2])[:60]
            diffs.append((tag, mpart, rpart))
    
    print(f"  Word diffs: {len(diffs)}")
    for tag, mpart, rpart in diffs[:10]:
        print(f"  [{tag}] MINI: {repr(mpart)}")
        print(f"          REF:  {repr(rpart)}")
