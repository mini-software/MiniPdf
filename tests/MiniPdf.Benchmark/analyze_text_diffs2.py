"""Analyze text differences for key TEXT-bottleneck cases."""
import fitz  # PyMuPDF
import os
import difflib

def extract_text_pages(pdf_path):
    doc = fitz.open(pdf_path)
    pages = []
    for page in doc:
        pages.append(page.get_text())
    doc.close()
    return pages

targets = [
    'classic189_alternating_image_text_rows',
    'classic182_dense_long_text_columns', 
    'classic49_contact_list',
    'classic73_event_flyer_with_banner',
    'classic74_dashboard_with_kpi_image',
    'classic140_rotated_text',
    'classic13_date_strings',
    'classic51_product_catalog',
    'classic68_restaurant_menu',
    'classic150_kitchen_sink_styles',
]

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')
ref_dir = 'reference_pdfs'

for name in targets:
    mini_path = os.path.join(mini_dir, f'{name}.pdf')
    ref_path = os.path.join(ref_dir, f'{name}.pdf')
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        print(f'\n=== {name}: MISSING ===')
        continue
    
    mini_pages = extract_text_pages(mini_path)
    ref_pages = extract_text_pages(ref_path)
    
    mini_flat = '\n'.join(mini_pages)
    ref_flat = '\n'.join(ref_pages)
    
    seq = difflib.SequenceMatcher(None, mini_flat, ref_flat)
    ratio = seq.ratio()
    
    print(f'\n=== {name} ===')
    print(f'  Pages: mini={len(mini_pages)}, ref={len(ref_pages)}')
    print(f'  Chars: mini={len(mini_flat)}, ref={len(ref_flat)}')
    print(f'  Flat ratio: {ratio:.4f}')
    
    # Show first few differences
    diff_count = 0
    for tag, i1, i2, j1, j2 in seq.get_opcodes():
        if tag == 'equal':
            continue
        if diff_count >= 5:
            print(f'  ... (more differences)')
            break
        mini_snippet = repr(mini_flat[i1:i2][:60])
        ref_snippet = repr(ref_flat[j1:j2][:60])
        print(f'  {tag}: mini[{i1}:{i2}]={mini_snippet}')
        print(f'         ref[{j1}:{j2}]={ref_snippet}')
        diff_count += 1
