"""Compare text extraction more carefully for chart cases."""
import json
import os
import sys
import difflib

os.chdir(os.path.dirname(os.path.abspath(__file__)))
sys.stdout.reconfigure(encoding='utf-8')

MINI_DIR = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
REF_DIR = 'reference_pdfs'

import fitz

def extract_text(pdf_path):
    if not os.path.exists(pdf_path):
        return f"[FILE NOT FOUND: {pdf_path}]"
    doc = fitz.open(pdf_path)
    text = ""
    for page in doc:
        text += page.get_text("text") + "\n---PAGE---\n"
    doc.close()
    return text.strip()

cases = [
    'classic91_simple_bar_chart',
    'classic93_line_chart',
    'classic94_pie_chart',
    'classic95_area_chart',
    'classic118_bar_chart_custom_colors',
    'classic97_doughnut_chart',
    'classic96_scatter_chart',
    'classic102_line_chart_with_markers',
]

with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

for name in cases:
    mini_text = extract_text(os.path.join(MINI_DIR, f"{name}.pdf"))
    ref_text = extract_text(os.path.join(REF_DIR, f"{name}.pdf"))
    
    case = [c for c in data if c['name'] == name]
    score = f"{case[0]['text_similarity']:.4f}" if case else '?'
    
    # Compute character-level diff
    sm = difflib.SequenceMatcher(None, mini_text, ref_text)
    ratio = sm.ratio()
    
    print(f"\n=== {name} (text={score}, char_ratio={ratio:.4f}) ===")
    print(f"MiniPdf len: {len(mini_text)}, Reference len: {len(ref_text)}")
    
    # Show what's in reference but not MiniPdf (added parts)
    opcodes = sm.get_opcodes()
    for tag, i1, i2, j1, j2 in opcodes:
        if tag in ('replace', 'insert'):
            ref_chunk = ref_text[j1:j2][:80]
            mini_chunk = mini_text[i1:i2][:80] if tag == 'replace' else ''
            if tag == 'insert':
                print(f"  MISSING: '{ref_chunk}'")
            else:
                print(f"  DIFF: mini='{mini_chunk}' ref='{ref_chunk}'")
