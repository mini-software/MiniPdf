"""Check column definitions for key test cases to understand padding behavior"""
import zipfile
import xml.etree.ElementTree as ET
import os
import glob

test_dir = "../MiniPdf.Scripts/output"
xlsx_files = glob.glob(os.path.join(test_dir, "classic*.xlsx"))

cases = {}
for f in sorted(xlsx_files):
    name = os.path.splitext(os.path.basename(f))[0]
    try:
        with zipfile.ZipFile(f) as z:
            has_explicit_widths = False
            col_count = 0
            has_custom_width = False
            ns = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'
            
            # Check sheet1 for column definitions
            for sheet_name in ['xl/worksheets/sheet1.xml']:
                if sheet_name in z.namelist():
                    data = z.read(sheet_name)
                    root = ET.fromstring(data)
                    cols = root.findall(f'.//{ns}col')
                    if cols:
                        has_explicit_widths = True
                        for col in cols:
                            cw = col.get('customWidth')
                            if cw == '1':
                                has_custom_width = True
                            # Count columns from min/max
                            min_c = int(col.get('min', '1'))
                            max_c = int(col.get('max', '1'))
                            col_count = max(col_count, max_c)
                    # Count actual data columns from sheetData
                    sheet_data = root.find(f'.//{ns}sheetData')
                    if sheet_data is not None:
                        first_row = sheet_data.find(f'{ns}row')
                        if first_row is not None:
                            data_cols = len(first_row.findall(f'{ns}c'))
                            col_count = max(col_count, data_cols)
            
            cases[name] = {
                'has_explicit_widths': has_explicit_widths,
                'has_custom_width': has_custom_width,
                'col_count': col_count,
            }
    except Exception as e:
        pass

# Print cases without explicit column widths (default width)
no_explicit = [(name, info) for name, info in cases.items() if not info['has_explicit_widths']]
print(f"Cases WITHOUT explicit column widths ({len(no_explicit)}):")
for name, info in sorted(no_explicit):
    print(f"  {name}: cols={info['col_count']}")

print()

# Print cases that had regressions before: classic132, classic127
for case in ['classic132_striped_table', 'classic127_font_styles', 'classic06_tall_table', 'classic18_large_dataset', 'classic60_large_wide_table']:
    if case in cases:
        info = cases[case]
        print(f"{case}: explicit_widths={info['has_explicit_widths']}, custom_width={info['has_custom_width']}, cols={info['col_count']}")
