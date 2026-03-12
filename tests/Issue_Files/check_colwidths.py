import zipfile, xml.etree.ElementTree as ET
ns = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
zf = zipfile.ZipFile('xlsx/Small business cash flow forecast1.xlsx')
tree = ET.parse(zf.open('xl/worksheets/sheet2.xml'))
root = tree.getroot()
fmt = root.find(f'{{{ns}}}sheetFormatPr')
print('defaultColWidth:', fmt.get('defaultColWidth') if fmt is not None else 'N/A')
print('defaultRowHeight:', fmt.get('defaultRowHeight') if fmt is not None else 'N/A')
cols = root.findall(f'.//{{{ns}}}col')
for c in cols:
    mn = c.get("min")
    mx = c.get("max")
    w = c.get("width")
    h = c.get("hidden", "0")
    cw = c.get("customWidth", "0")
    print(f'  col {mn}-{mx}: width={w}, hidden={h}, customWidth={cw}')
