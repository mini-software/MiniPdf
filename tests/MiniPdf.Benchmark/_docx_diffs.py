"""Analyze top DOCX text failures - show text diffs between MiniPdf and Reference."""
import json
import os
import sys
import difflib

os.chdir(os.path.dirname(os.path.abspath(__file__)))
sys.stdout.reconfigure(encoding='utf-8')

MINI_DIR = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output_docx')
REF_DIR = 'reference_pdfs_docx'

with open('reports_docx/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

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

# Top DOCX text failures  
cases = [
    'docx_classic08_bullet_list',
    'docx_classic21_nested_lists',
    'docx_classic50_long_table_with_formatting',
    'docx_classic64_multi_column_layout',
    'docx_classic49_cjk_document',
    'docx_classic44_memo',
    'docx_classic32_superscript_subscript',
    'docx_classic76_recipe_card',
    'docx_classic92_first_line_indent',
    'docx_classic59_numbered_and_bullet_mixed',
]

for name in cases:
    fn = name  # keep docx_ prefix for filenames
    mini_text = extract_text(os.path.join(MINI_DIR, f"{fn}.pdf"))
    ref_text = extract_text(os.path.join(REF_DIR, f"{fn}.pdf"))
    
    case = [c for c in data if c['name'] == name]
    score = f"{case[0]['text_similarity']:.4f}" if case else '?'
    
    print(f"\n=== {name} (text={score}) ===")
    print(f"MiniPdf len: {len(mini_text)}, Reference len: {len(ref_text)}")
    
    # Show unified diff (first 20 differences)
    m_lines = mini_text.split('\n')
    r_lines = ref_text.split('\n')
    diff = list(difflib.unified_diff(m_lines[:60], r_lines[:60], lineterm='', n=0))
    if diff:
        diffcount = 0
        for line in diff:
            if line.startswith('---') or line.startswith('+++') or line.startswith('@@'):
                continue
            if diffcount < 15:
                print(f"  {line[:80]}")
            diffcount += 1
        if diffcount > 15:
            print(f"  ... ({diffcount} total diff lines)")
