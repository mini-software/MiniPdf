import zipfile, xml.etree.ElementTree as ET, sys, os

xlsx_dir = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output"

# Check a few files for page margins
for name in ["classic132_striped_table", "classic01_basic_table_with_headers", "classic44_employee_roster"]:
    path = os.path.join(xlsx_dir, f"{name}.xlsx")
    if not os.path.exists(path):
        print(f"{name}: NOT FOUND")
        continue
    zf = zipfile.ZipFile(path)
    data = zf.read('xl/worksheets/sheet1.xml')
    root = ET.fromstring(data)
    ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
    pm = root.find(f'.//{ns}pageMargins')
    if pm is not None:
        parts = []
        for attr in ['left','right','top','bottom','header','footer']:
            val = pm.attrib.get(attr, '?')
            try:
                pts = float(val) * 72
                parts.append(f"{attr}={val}({pts:.1f}pt)")
            except:
                parts.append(f"{attr}={val}")
        print(f"{name}: {', '.join(parts)}")
    else:
        print(f"{name}: No pageMargins")
