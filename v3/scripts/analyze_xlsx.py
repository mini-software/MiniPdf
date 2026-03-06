#!/usr/bin/env python3
"""Analyze xlsx files to understand column width requirements."""
import zipfile, re, os, sys

_script_dir = os.path.dirname(os.path.abspath(__file__))
_repo_root = os.path.dirname(_script_dir)
output_dir = os.path.join(_repo_root, 'tests', 'MiniPdf.Scripts', 'output')

def get_col_content_widths(xlsx_path):
    """Get max content length per column and SheetFormatPr info."""
    with zipfile.ZipFile(xlsx_path) as z:
        # Get sheets
        names = [n for n in z.namelist() if n.startswith('xl/worksheets/sheet') and n.endswith('.xml')]
        if not names:
            return {}, None
        with z.open(names[0]) as f:
            sheet_xml = f.read().decode('utf-8')
        
        # Get shared strings
        try:
            with z.open('xl/sharedStrings.xml') as f:
                sst_xml = f.read().decode('utf-8')
            strings = re.findall(r'<t[^>]*>([^<]+)</t>', sst_xml)
        except:
            strings = []
        
        # Get col widths
        cols = re.findall(r'<col\s[^/]*/>', sheet_xml)
        custom_cols = [c for c in cols if 'customWidth="1"' in c]
        
        # Get sheetFormatPr
        fmt = re.search(r'<sheetFormatPr[^/]*/>', sheet_xml)
        fmt_str = fmt.group(0) if fmt else None
        
        return custom_cols, fmt_str, strings

target_files = [
    # Tests that reference clips but we used to NOT clip -> now clipping correctly
    'classic35_explicit_row_heights.xlsx',
    'classic44_employee_roster.xlsx',
    'classic49_contact_list.xlsx',
    # Tests that now WRONGLY clip (regressions)
    'classic06_tall_table.xlsx',
    'classic18_large_dataset.xlsx',
    'classic60_large_wide_table.xlsx',
    'classic12_sparse_columns.xlsx',
    'classic05_wide_table.xlsx',
    'classic36_merged_cells.xlsx',
    'classic24_red_text.xlsx',
    'classic38_hyperlink_cell.xlsx',
    'classic01_basic_table_with_headers.xlsx',
]

for fname in target_files:
    path = os.path.join(output_dir, fname)
    if not os.path.exists(path):
        print(f"{fname}: NOT FOUND")
        continue
    cols, fmt, strings = get_col_content_widths(path)
    max_len = max((len(s) for s in strings), default=0)
    print(f"{fname}")
    print(f"  custom_cols: {cols[:3]}")
    print(f"  sheetFormatPr: {fmt}")
    print(f"  max_string_len: {max_len}")
    if strings:
        long_strings = [s for s in strings if len(s) > 10][:3]
        if long_strings:
            print(f"  long strings (>10): {long_strings}")
    print()
