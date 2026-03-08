"""Analyze near-miss XLSX cases (text 0.96-0.99) for quick fixes."""
import json
import os
import sys
import difflib

os.chdir(os.path.dirname(os.path.abspath(__file__)))
sys.stdout.reconfigure(encoding='utf-8')

MINI_DIR = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
REF_DIR = 'reference_pdfs'

with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

import fitz

def extract_text(pdf_path):
    if not os.path.exists(pdf_path):
        return ""
    doc = fitz.open(pdf_path)
    text = ""
    for page in doc:
        text += page.get_text("text") + "\n---PAGE---\n"
    doc.close()
    return text.strip()

# Near-miss cases with text 0.93-0.99
near_miss = [c for c in data if 0.93 < c['text_similarity'] < 0.99]
near_miss.sort(key=lambda c: c['text_similarity'], reverse=True)

for c in near_miss[:15]:
    name = c['name']
    mini = extract_text(os.path.join(MINI_DIR, f"{name}.pdf"))
    ref = extract_text(os.path.join(REF_DIR, f"{name}.pdf"))
    
    print(f"\n=== {name} (text={c['text_similarity']:.4f}) ===")
    print(f"  Lengths: mini={len(mini)}, ref={len(ref)}")
    
    # Show key differences
    m_lines = mini.split('\n')
    r_lines = ref.split('\n')
    
    sm = difflib.SequenceMatcher(None, mini, ref)
    ops = sm.get_opcodes()
    diffs = [(tag, mini[i1:i2], ref[j1:j2]) for tag, i1, i2, j1, j2 in ops if tag != 'equal']
    for tag, m_chunk, r_chunk in diffs[:5]:
        print(f"  {tag}: mini='{m_chunk[:50]}' ref='{r_chunk[:50]}'")
