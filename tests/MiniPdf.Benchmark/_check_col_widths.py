import json, os, zipfile
from xml.etree import ElementTree as ET

xlsx_dir = os.path.join('..', 'MiniPdf.Scripts', 'output')
count_with = 0
count_without = 0
for fn in sorted(os.listdir(xlsx_dir)):
    if not fn.endswith('.xlsx'):
        continue
    path = os.path.join(xlsx_dir, fn)
    try:
        with zipfile.ZipFile(path) as z:
            for name in z.namelist():
                if 'worksheets/sheet1.xml' in name.lower():
                    with z.open(name) as f:
                        tree = ET.parse(f)
                        root = tree.getroot()
                        ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
                        cols = root.findall(f'.//{ns}col')
                        custom_cols = [c for c in cols if c.get('customWidth') == '1']
                        fmt_pr = root.find(f'.//{ns}sheetFormatPr')
                        has_default = fmt_pr is not None and fmt_pr.get('defaultColWidth') is not None
                        has_explicit = len(custom_cols) > 0 or has_default
                        if has_explicit:
                            count_with += 1
                        else:
                            count_without += 1
                            if count_without <= 10:
                                print(f"  NO explicit widths: {fn}")
                    break
    except Exception as e:
        print(f"  ERROR: {fn}: {e}")

print(f"\nWith explicit widths: {count_with}")
print(f"Without explicit widths: {count_without}")
