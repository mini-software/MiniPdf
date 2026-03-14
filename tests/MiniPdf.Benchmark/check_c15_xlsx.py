"""Check classic15 XLSX structure."""
import zipfile, xml.etree.ElementTree as ET

zf = zipfile.ZipFile('../MiniPdf.Scripts/output/classic15_negative_numbers.xlsx')
sheet = ET.parse(zf.open('xl/worksheets/sheet1.xml'))
ns = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'

# Check for merged cells
mc = sheet.findall(f'.//{{{ns}}}mergeCells/{{{ns}}}mergeCell')
for m in mc:
    print('merge:', m.attrib)

# Get shared strings
try:
    ss = ET.parse(zf.open('xl/sharedStrings.xml'))
    strings = [si.findtext(f'{{{ns}}}t') or ''.join(t.text or '' for t in si.iter(f'{{{ns}}}t'))
               for si in ss.findall(f'{{{ns}}}si')]
except:
    strings = []

# Get all rows
rows = sheet.findall(f'.//{{{ns}}}sheetData/{{{ns}}}row')
for r in rows:
    print(f"row {r.get('r')}:")
    for c in r.findall(f'{{{ns}}}c'):
        ref = c.get('r')
        t = c.get('t', '')
        v = c.findtext(f'{{{ns}}}v') or ''
        text = v
        if t == 's' and v.isdigit():
            idx = int(v)
            text = strings[idx] if idx < len(strings) else v
        print(f'  {ref} type={t} value={v} text="{text}"')
