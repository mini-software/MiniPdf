#!/usr/bin/env python3
"""Extract text from reference PDFs to understand what LibreOffice renders."""
import os, sys
_script_dir = os.path.dirname(os.path.abspath(__file__))
_repo_root = os.path.dirname(_script_dir)
sys.path.insert(0, os.path.join(_repo_root, 'tests', 'MiniPdf.Benchmark'))

try:
    import fitz  # pymupdf
except ImportError:
    print("pymupdf not installed")
    sys.exit(1)

ref_dir = os.path.join(_repo_root, 'tests', 'MiniPdf.Benchmark', 'reference_pdfs')

tests = [
    ('classic35_explicit_row_heights', ['Tall Header', 'Tall HeadeValue', 'Tall Heade']),
    ('classic44_employee_roster', ['Engineering', 'Engineerin']),
    ('classic49_contact_list', ['alice@example.com', 'alice@exam']),
    ('classic24_red_text', ['Something went wrong']),
    ('classic38_hyperlink_cell', ['https://github.com', 'https://gi']),
    ('classic06_tall_table', ['Contact', 'Customer']),
    ('classic01_basic_table_with_headers', ['Product', 'Description']),
    ('classic36_merged_cells', ['Merged Header', 'Merged Hea']),
]

for name, look_for in tests:
    path = os.path.join(ref_dir, name + '.pdf')
    if not os.path.exists(path):
        print(f"{name}: PDF not found")
        continue
    doc = fitz.open(path)
    text = doc[0].get_text()
    print(f"\n{name}:")
    for s in look_for:
        found = s in text
        print(f"  '{s}': {'FOUND' if found else 'NOT FOUND'}")
    # Print all words from text
    words = text.strip().split()
    print(f"  First 20 words: {words[:20]}")
