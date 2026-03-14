"""Check classic09 XLSX structure."""
import zipfile
import xml.etree.ElementTree as ET

xlsx = '../MiniPdf.Scripts/output/classic09_long_text.xlsx'
with zipfile.ZipFile(xlsx) as z:
    with z.open('xl/worksheets/sheet1.xml') as f:
        tree = ET.parse(f)
        root = tree.getroot()
        ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
        
        # Check columns
        cols = root.findall(f'.//{ns}col')
        print(f'Column definitions: {len(cols)}')
        for col in cols:
            print(f'  min={col.get("min")} max={col.get("max")} width={col.get("width")} customWidth={col.get("customWidth")} bestFit={col.get("bestFit")}')
        
        # Check rows  
        rows = root.findall(f'.//{ns}row')
        print(f'\nTotal rows: {len(rows)}')
        for row in rows[:5]:
            print(f'  row {row.get("r")}: ht={row.get("ht")} customHeight={row.get("customHeight")}')
            for cell in row:
                ref = cell.get('r')
                val = cell.find(f'{ns}v')
                val_text = val.text[:30] if val is not None and val.text else '(none)'
                print(f'    cell {ref}: type={cell.get("t")} val={val_text}')
        
    # Check sharedStrings for the long texts
    try:
        with z.open('xl/sharedStrings.xml') as f:
            tree = ET.parse(f)
            root = tree.getroot()
            ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
            strings = root.findall(f'{ns}si')
            print(f'\nShared strings: {len(strings)}')
            for i, si in enumerate(strings[:10]):
                t = si.find(f'.//{ns}t')
                text = t.text if t is not None else '(none)'
                print(f'  [{i}] len={len(text)} text="{text[:60]}"')
    except:
        print('\nNo sharedStrings.xml')
