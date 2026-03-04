"""Check column widths and cell values in xlsx files."""
import zipfile
import xml.etree.ElementTree as ET

files = [
    '../MiniPdf.Scripts/output/classic40_scientific_notation.xlsx',
    '../MiniPdf.Scripts/output/classic41_integer_vs_float.xlsx', 
    '../MiniPdf.Scripts/output/classic58_mixed_numeric_formats.xlsx',
]

for path in files:
    print(f"=== {path.split('/')[-1]} ===")
    with zipfile.ZipFile(path) as zf:
        # Check column widths
        tree = ET.parse(zf.open('xl/worksheets/sheet1.xml'))
        ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
        cols = tree.findall('.//s:col', ns)
        if cols:
            for c in cols:
                print(f"  Col {c.get('min')}-{c.get('max')}: width={c.get('width')} customWidth={c.get('customWidth')}")
        else:
            print("  No explicit column widths (default 8.43)")
        
        # Check cell values (first 12 rows)
        rows = tree.findall('.//s:sheetData/s:row', ns)
        for row in rows[:12]:
            for cell in row.findall('s:c', ns):
                ref = cell.get('r')
                val = cell.findtext('s:v', '', ns)
                t = cell.get('t', 'n')
                s = cell.get('s', '0')
                print(f"  {ref}: val={val} type={t} style={s}")
    print()
