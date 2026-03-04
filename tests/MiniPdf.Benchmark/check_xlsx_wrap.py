import zipfile, xml.etree.ElementTree as ET

zf = zipfile.ZipFile(r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic09_long_text.xlsx')
# Check styles.xml for wrapText
styles = ET.parse(zf.open('xl/styles.xml'))
ns = {'x': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
print("=== Cell Formats (cellXfs) ===")
for i, xf in enumerate(styles.findall('.//x:cellXfs/x:xf', ns)):
    align = xf.find('x:alignment', ns)
    if align is not None:
        wt = align.get('wrapText', '0')
        horiz = align.get('horizontal', '')
        vert = align.get('vertical', '')
        print(f'  Style {i}: wrapText={wt} h={horiz} v={vert}')
    else:
        print(f'  Style {i}: no alignment')

# Check sheet1.xml for cells and their styles
sheet = ET.parse(zf.open('xl/worksheets/sheet1.xml'))
print("\n=== Cells ===")
for row in sheet.findall('.//x:sheetData/x:row', ns)[:10]:
    r = row.get('r')
    for c in row.findall('x:c', ns):
        cr = c.get('r')
        s = c.get('s', '0')
        t = c.get('t', '')
        v = c.find('x:v', ns)
        vt = v.text[:30] if v is not None and v.text else ''
        print(f'  Cell {cr}: style={s} type={t} val={vt}...')

# Check for sheetFormatPr and cols
fmt = sheet.find('.//x:sheetFormatPr', ns)
if fmt is not None:
    print(f"\n=== sheetFormatPr === defaultColWidth={fmt.get('defaultColWidth','N/A')} defaultRowHeight={fmt.get('defaultRowHeight','N/A')}")
cols = sheet.findall('.//x:col', ns)
print(f"\n=== cols count: {len(cols)} ===")
for col in cols:
    print(f"  col min={col.get('min')} max={col.get('max')} width={col.get('width')} customWidth={col.get('customWidth')}")
