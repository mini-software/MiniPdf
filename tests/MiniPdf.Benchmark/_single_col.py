"""Check which XLSX files have only 1 column with overflowing text"""
import zipfile, xml.etree.ElementTree as ET, os, glob

xlsx_dir = "../MiniPdf.Scripts/output"
results = []

for f in sorted(glob.glob(os.path.join(xlsx_dir, "*.xlsx"))):
    name = os.path.basename(f)
    try:
        zf = zipfile.ZipFile(f)
        tree = ET.parse(zf.open('xl/worksheets/sheet1.xml'))
        root = tree.getroot()
        ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
        rows = root.findall(f'.//{ns}row')
        
        # Count max columns used
        max_col = 0
        for r in rows:
            cells = r.findall(f'{ns}c')
            for c in cells:
                ref = c.attrib.get('r', '')
                # Parse column letter
                col_letter = ''.join(ch for ch in ref if ch.isalpha())
                col_num = 0
                for ch in col_letter:
                    col_num = col_num * 26 + (ord(ch.upper()) - ord('A') + 1)
                max_col = max(max_col, col_num)
        
        if max_col == 1:
            results.append(name)
        zf.close()
    except Exception as e:
        pass

print(f"Files with only 1 column: {len(results)}")
for r in results:
    print(f"  {r}")
