import zipfile, xml.etree.ElementTree as ET

z = zipfile.ZipFile(r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic09_long_text.xlsx')
data = z.read('xl/worksheets/sheet1.xml')
root = ET.fromstring(data)
ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}

# Check for column widths
for col in root.findall('.//s:col', ns):
    print(f'Column: min={col.get("min")} max={col.get("max")} width={col.get("width")} customWidth={col.get("customWidth")}')

# Check for row heights
for row in root.findall('.//s:row', ns):
    r = row.get('r')
    ht = row.get('ht')
    customHeight = row.get('customHeight')
    spans = row.get('spans')
    print(f'Row {r}: ht={ht} customHeight={customHeight} spans={spans}')

# Check for sheet format properties
fmt = root.find('.//s:sheetFormatPr', ns)
if fmt is not None:
    print(f'sheetFormatPr: defaultColWidth={fmt.get("defaultColWidth")} defaultRowHeight={fmt.get("defaultRowHeight")} customHeight={fmt.get("customHeight")}')

# Check for page setup
ps = root.find('.//s:pageSetup', ns)
if ps is not None:
    attrs = {k: v for k, v in ps.attrib.items()}
    print(f'pageSetup: {attrs}')
