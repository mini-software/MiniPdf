"""Compare text extraction from MiniPdf vs Reference PDFs for chart cases to identify missing text."""
import json
import os

with open('reports/comparison_report.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

# Look at specific cases to understand what text is missing
interesting = [
    'classic91_simple_bar_chart',
    'classic93_line_chart',
    'classic94_pie_chart',
    'classic95_area_chart',
    'classic09_long_text',
    'classic44_employee_roster',
    'classic140_rotated_text',
    'classic49_contact_list',
    'classic57_cjk_only',
    'classic23_unicode_text',
    'classic152_emoji_sampler',
]

# Text extraction
try:
    import fitz
except Exception:
    print("PyMuPDF not available, using stored diffs")
    # Instead just look at stored extraction diffs
    import sys
    sys.exit(0)

def extract_text(pdf_path):
    """Extract text from PDF using PyMuPDF."""
    if not os.path.exists(pdf_path):
        return ""
    doc = fitz.open(pdf_path)
    text = ""
    for page in doc:
        text += page.get_text("text") + "\n---PAGE---\n"
    doc.close()
    return text.strip()

for name in interesting:
    mini_path = f"pdf_output/{name}.pdf"
    ref_path = f"reference_pdfs/{name}.pdf"
    
    mini_text = extract_text(mini_path)
    ref_text = extract_text(ref_path)
    
    # Find what's in reference but not in minipdf
    mini_words = set(mini_text.split())
    ref_words = set(ref_text.split())
    missing = ref_words - mini_words
    extra = mini_words - ref_words
    
    case = [c for c in data if c['name'] == name]
    score = case[0]['text_similarity'] if case else '?'
    
    print(f"\n=== {name} (text={score}) ===")
    print(f"MiniPdf text length: {len(mini_text)}, Reference text length: {len(ref_text)}")
    if missing:
        print(f"Missing words ({len(missing)}): {list(missing)[:30]}")
    if extra:
        print(f"Extra words ({len(extra)}): {list(extra)[:20]}")
    
    # Show first differences
    mini_lines = mini_text.split('\n')
    ref_lines = ref_text.split('\n')
    print(f"MiniPdf first 15 lines:")
    for line in mini_lines[:15]:
        print(f"  M: {line}")
    print(f"Reference first 15 lines:")
    for line in ref_lines[:15]:
        print(f"  R: {line}")
