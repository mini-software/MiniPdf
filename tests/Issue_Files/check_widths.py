import zipfile, xml.etree.ElementTree as ET
zf = zipfile.ZipFile('tests/Issue_Files/xlsx/payroll-calculator_f.xlsx')
ns = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'

# Sheet3 = Paystubs (print area A1:J37)
root = ET.fromstring(zf.read('xl/worksheets/sheet3.xml'))

# Get column widths
print("=== Sheet3 (Paystubs) Column Widths ===")
for col in root.iter(f'{ns}col'):
    min_c = col.get('min')
    max_c = col.get('max')
    width = col.get('width')
    custom = col.get('customWidth', '0')
    print(f'  Cols {min_c}-{max_c}: width={width}, custom={custom}')

# Also check sheetFormatPr default
fmt = root.find(f'.//{ns}sheetFormatPr')
if fmt is not None:
    print(f'  Default col width: {fmt.get("defaultColWidth", "N/A")}')
    print(f'  Default row height: {fmt.get("defaultRowHeight", "N/A")}')

# Calculate total width for columns A-J (1-10)
total_pts = 0
for col in root.iter(f'{ns}col'):
    min_c = int(col.get('min'))
    max_c = int(col.get('max'))
    width = float(col.get('width'))
    for c in range(min_c, max_c + 1):
        if 1 <= c <= 10:  # A-J
            pts = width * 5.62
            total_pts += pts
            print(f'    Col {c}: {width:.2f} chars = {pts:.1f}pt')

print(f'\n  Total for A-J: {total_pts:.1f}pt')
print(f'  At 79% scale: {total_pts * 0.79:.1f}pt')
print(f'  Usable width (Letter portrait): {612 - 54 - 54:.0f}pt')

# Now check Employee Register (sheet1) columns
print("\n=== Sheet1 (Employee Register) Column Widths ===")
root1 = ET.fromstring(zf.read('xl/worksheets/sheet1.xml'))
total_pts1 = 0
for col in root1.iter(f'{ns}col'):
    min_c = int(col.get('min'))
    max_c = int(col.get('max'))
    width = float(col.get('width'))
    for c in range(min_c, max_c + 1):
        if c <= 32:
            pts = width * 5.62
            total_pts1 += pts
total_pts_scaled = total_pts1 * 0.65
print(f'  Total: {total_pts1:.1f}pt, at 65% scale: {total_pts_scaled:.1f}pt')
print(f'  Usable width (Letter landscape): {792 - 54 - 54:.0f}pt')
