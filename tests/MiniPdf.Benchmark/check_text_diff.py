"""Check text differences in non-chart/non-CJK cases."""
import fitz

def get_page_text(pdf_path, page=0):
    doc = fitz.open(pdf_path)
    return doc[page].get_text()

def get_text_words(pdf_path, page=0):
    doc = fitz.open(pdf_path)
    pg = doc[page]
    words = pg.get_text("words")
    return words

cases = [
    ('classic09_long_text', 0.9614),
    ('classic44_employee_roster', 0.9652),
    ('classic182_dense_long_text_columns', 0.9286),
    ('classic140_rotated_text', 0.9583),
]

for case, tsim in cases:
    ref_path = f'reference_pdfs/{case}.pdf'
    mini_path = f'..\\MiniPdf.Scripts\\pdf_output\\{case}.pdf'
    try:
        ref_doc = fitz.open(ref_path)
        mini_doc = fitz.open(mini_path)
    except:
        print(f'{case}: MISSING')
        continue
    
    print(f'\n=== {case} (text_sim={tsim}) ===')
    print(f'  Pages: ref={len(ref_doc)}, mini={len(mini_doc)}')
    
    # Compare text per page
    for p in range(min(len(ref_doc), len(mini_doc), 3)):
        ref_text = ref_doc[p].get_text().strip()
        mini_text = mini_doc[p].get_text().strip()
        
        ref_words = set(ref_text.split())
        mini_words = set(mini_text.split())
        
        only_ref = ref_words - mini_words
        only_mini = mini_words - ref_words
        
        print(f'  Page {p}: ref_words={len(ref_words)}, mini_words={len(mini_words)}')
        if only_ref:
            print(f'    In ref only ({len(only_ref)}): {list(only_ref)[:10]}')
        if only_mini:
            print(f'    In mini only ({len(only_mini)}): {list(only_mini)[:10]}')
