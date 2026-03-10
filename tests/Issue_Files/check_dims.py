import zipfile, xml.etree.ElementTree as ET
zf = zipfile.ZipFile('tests/Issue_Files/xlsx/payroll-calculator_f.xlsx')
ns = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'

# Check workbook for defined names (print areas, print titles)
wb = ET.fromstring(zf.read('xl/workbook.xml'))
print("=== Defined Names ===")
for dn in wb.iter(f'{ns}definedName'):
    print(f"  {dn.get('name')}: {dn.text}")

# Check sheet dimensions and print area refs
for n in sorted(zf.namelist()):
    if 'sheet' in n.lower() and n.endswith('.xml'):
        root = ET.fromstring(zf.read(n))
        dim = root.find(f'.//{ns}dimension')
        pa = root.find(f'.//{ns}printArea')  
        print(f"\n{n}:")
        if dim is not None:
            print(f"  dimension: {dim.get('ref')}")
        # Count rows
        rows = root.findall(f'.//{ns}row')
        max_r = max(int(r.get('r','0')) for r in rows) if rows else 0
        max_c = 0
        for r in rows:
            for c in r.findall(f'{ns}c'):
                ref = c.get('r','')
                col = ''.join(ch for ch in ref if ch.isalpha())
                val = 0
                for ch in col:
                    val = val * 26 + (ord(ch) - ord('A') + 1)
                max_c = max(max_c, val)
        print(f"  rows: {len(rows)} (max row={max_r}), max col={max_c}")
