#!/usr/bin/env python3
"""Check xlsx structure to understand overflow vs clip."""
import zipfile, re, os, sys
sys.stdout.reconfigure(encoding='utf-8')

_script_dir = os.path.dirname(os.path.abspath(__file__))
_repo_root = os.path.dirname(_script_dir)
output_dir = os.path.join(_repo_root, 'tests', 'MiniPdf.Scripts', 'output')

def analyze_xlsx(xlsx_path):
    with zipfile.ZipFile(xlsx_path) as z:
        names = [n for n in z.namelist() if n.startswith('xl/worksheets/sheet') and n.endswith('.xml')]
        if not names:
            return {}
        with z.open(names[0]) as f:
            sheet_xml = f.read().decode('utf-8')
        sharedstrings = []
        if 'xl/sharedStrings.xml' in z.namelist():
            with z.open('xl/sharedStrings.xml') as f:
                sst = f.read().decode('utf-8')
            sharedstrings = re.findall(r'<t[^>]*>([^<]*)</t>', sst)
    
    # Find cell references in first few rows to know which columns are occupied
    cell_refs = re.findall(r'<c r="([A-Z]+)(\d+)"', sheet_xml)
    # Group by row
    from collections import defaultdict
    row_cols = defaultdict(set)
    for col, row in cell_refs:
        row_cols[int(row)].add(col)
    
    return dict(list(row_cols.items())[:6]), sharedstrings[:10]

cases = [
    ('classic35_explicit_row_heights', 'CLIPS'),
    ('classic44_employee_roster', 'CLIPS'),
    ('classic49_contact_list', 'CLIPS'),
    ('classic24_red_text', 'NO CLIP'),
    ('classic38_hyperlink_cell', 'NO CLIP'),
    ('classic06_tall_table', 'NO CLIP'),
    ('classic01_basic_table_with_headers', 'NO CLIP'),
    ('classic36_merged_cells', 'NO CLIP - merged'),
]

for name, expected in cases:
    path = os.path.join(output_dir, name + '.xlsx')
    row_cols, strings = analyze_xlsx(path)
    print(f"{name} ({expected}):")
    for rnum, cols in row_cols.items():
        print(f"  row {rnum}: {sorted(cols)}")
    print(f"  strings: {strings[:5]}")
    print()
