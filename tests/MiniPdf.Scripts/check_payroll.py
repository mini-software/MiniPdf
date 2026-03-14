from zipfile import ZipFile
import xml.etree.ElementTree as ET

z = ZipFile('output/classic191_payroll_calculator.xlsx')
ns = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
sheets = [n for n in z.namelist() if n.startswith('xl/worksheets/sheet') and n.endswith('.xml')]
for name in sorted(sheets):
    print(f'=== {name} ===')
    tree = ET.parse(z.open(name))
    root = tree.getroot()
    for el in root.iter():
        tag = el.tag.split('}')[1] if '}' in el.tag else el.tag
        if tag in ('sheetPr', 'pageSetUpPr', 'pageSetup', 'printOptions', 'sheetFormatPr'):
            print(f'  {tag}: {el.attrib}')
    # Count data columns
    max_col = 0
    for row in root.iter(f'{{{ns}}}row'):
        for c in row.iter(f'{{{ns}}}c'):
            ref = c.get('r', '')
            import re
            m = re.match(r'([A-Z]+)', ref)
            if m:
                col_letters = m.group(1)
                col_idx = sum((ord(ch) - 64) * (26 ** i) for i, ch in enumerate(reversed(col_letters)))
                max_col = max(max_col, col_idx)
    print(f'  max data column: {max_col}')
