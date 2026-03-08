"""Compare text extraction from MiniPdf vs Reference PDFs for key XLSX cases."""
import json
import os
import sys

os.chdir(os.path.dirname(os.path.abspath(__file__)))
sys.stdout.reconfigure(encoding='utf-8')

MINI_DIR = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
REF_DIR = 'reference_pdfs'

with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

interesting = [
    'classic91_simple_bar_chart',
    'classic93_line_chart', 
    'classic94_pie_chart',
    'classic09_long_text',
    'classic44_employee_roster',
    'classic140_rotated_text',
    'classic49_contact_list',
    'classic57_cjk_only',
    'classic23_unicode_text',
    'classic103_pie_chart_with_labels',
    'classic118_bar_chart_custom_colors',
]

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

for name in interesting:
    mini_path = os.path.join(MINI_DIR, f"{name}.pdf")
    ref_path = os.path.join(REF_DIR, f"{name}.pdf")
    
    mini_text = extract_text(mini_path)
    ref_text = extract_text(ref_path)
    
    case = [c for c in data if c['name'] == name]
    score = f"{case[0]['text_similarity']:.4f}" if case else '?'
    
    print(f"\n=== {name} (text={score}) ===")
    print(f"MiniPdf text len: {len(mini_text)}, Reference text len: {len(ref_text)}")
    
    # Show first 20 lines of each
    mini_lines = [l for l in mini_text.split('\n') if l.strip()][:20]
    ref_lines = [l for l in ref_text.split('\n') if l.strip()][:20]
    
    print("MiniPdf lines:")
    for l in mini_lines:
        print(f"  M: {l[:80]}")
    print("Reference lines:")
    for l in ref_lines:
        print(f"  R: {l[:80]}")
