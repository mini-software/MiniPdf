"""Look for easy text_sim wins where MiniPdf text is very close to reference."""
import json, fitz, os
from difflib import SequenceMatcher

d = json.load(open(r'reports\comparison_report.json', encoding='utf-8'))

# Focus on cases where text_sim is 0.98-0.99 (close to 1.0, single fix could push to 1.0)
near_perfect = []
for x in d:
    ts = x.get('text_similarity', 0)
    if 0.98 <= ts < 1.0:
        near_perfect.append((ts, x['name'], x['overall_score']))

near_perfect.sort()
print(f"Cases with text_sim 0.98-0.99 ({len(near_perfect)} cases):")
for ts, name, score in near_perfect:
    # Get actual text to inspect diff
    mini = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', f'{name}.pdf')
    ref = os.path.join('reference_pdfs', f'{name}.pdf')
    if not os.path.exists(mini) or not os.path.exists(ref):
        continue
    
    md = fitz.open(mini)
    rd = fitz.open(ref)
    
    # Use full multi-page comparison like benchmark
    m_text = '\n'.join(md[i].get_text("text") for i in range(len(md)))
    r_text = '\n'.join(rd[i].get_text("text") for i in range(len(rd)))
    
    # Find first diff
    sm = SequenceMatcher(None, m_text.split(), r_text.split())
    first_diff = None
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag != 'equal':
            mw = ' '.join(m_text.split()[i1:i2])[:40]
            rw = ' '.join(r_text.split()[j1:j2])[:40]
            first_diff = (tag, mw, rw)
            break
    
    diff_desc = f"  [{first_diff[0]}] M:{repr(first_diff[1])} R:{repr(first_diff[2])}" if first_diff else "  (no word-level diff?)"
    print(f"  {name:48s}  ts={ts:.4f}  score={score:.4f}")
    print(f"  {diff_desc}")
