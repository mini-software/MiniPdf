import fitz, json

# Check classic09 structure
mp_path = '../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf'
ref_path = 'reference_pdfs/classic09_long_text.pdf'

for label, path in [("MiniPdf", mp_path), ("Reference", ref_path)]:
    doc = fitz.open(path)
    print(f"\n{label}: {len(doc)} pages, {doc.metadata}")
    for i in range(min(2, len(doc))):
        page = doc[i]
        text = page.get_text()
        lines = [l for l in text.split('\n') if l.strip()]
        print(f"  Page {i+1}: {len(lines)} text lines, first: {lines[0][:60] if lines else 'EMPTY'}")
    doc.close()

# Check XLSX structure
import zipfile
from xml.etree import ElementTree as ET

xlsx_path = '../MiniPdf.Scripts/output/classic09_long_text.xlsx'
with zipfile.ZipFile(xlsx_path) as z:
    for name in z.namelist():
        if 'sheet1.xml' in name.lower():
            with z.open(name) as f:
                tree = ET.parse(f)
                root = tree.getroot()
                ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
                rows = root.findall(f'.//{ns}row')
                print(f"\nXLSX: {len(rows)} rows")
                # Check columns per row
                for r in rows[:3]:
                    cells = r.findall(f'{ns}c')
                    print(f"  Row {r.get('r')}: {len(cells)} cells, refs: {[c.get('r') for c in cells]}")
            break
