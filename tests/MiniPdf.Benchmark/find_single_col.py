"""Find all single-column XLSX benchmark cases."""
import zipfile
import xml.etree.ElementTree as ET
import os, glob

xlsx_dir = '../MiniPdf.Scripts/output'
for path in sorted(glob.glob(os.path.join(xlsx_dir, 'classic*.xlsx'))):
    name = os.path.basename(path)
    try:
        with zipfile.ZipFile(path) as z:
            sheets = [n for n in z.namelist() if n.startswith('xl/worksheets/sheet')]
            # Check first sheet
            with z.open(sheets[0]) as f:
                tree = ET.parse(f)
                root = tree.getroot()
                ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
                rows = root.findall(f'.//{ns}row')
                max_col = 0
                for row in rows:
                    for cell in row:
                        ref = cell.get('r', 'A1')
                        col_str = ''.join(c for c in ref if c.isalpha())
                        col_num = 0
                        for c in col_str:
                            col_num = col_num * 26 + (ord(c.upper()) - ord('A') + 1)
                        max_col = max(max_col, col_num)
                if max_col == 1:
                    print(f'  {name}: single column ({max_col})')
    except Exception as e:
        pass
